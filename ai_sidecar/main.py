import os
import json
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import httpx
from dotenv import load_dotenv

from agent import workflow
from langchain_core.messages import HumanMessage, AIMessage
from langgraph.checkpoint.sqlite.aio import AsyncSqliteSaver

load_dotenv()

app = FastAPI(title="LMS Admin AI Sidecar")

BACKEND_URL = os.getenv("BACKEND_URL", "http://localhost:5000/api")
API_BASE = os.getenv("OPENAI_API_BASE")
PORT = os.getenv("PORT", "8000")

print(f"--- AI Sidecar Configuration ---")
print(f"Backend URL: {BACKEND_URL}")
print(f"API Base (LLM): {API_BASE}")
print(f"Local Port: {PORT}")
print(f"-------------------------------")

class ChatRequest(BaseModel):
    message: str
    adminId: int
    threadId: str

async def notify_backend_progress(thread_id: str, step: str, status: str, progress: int, detail: str = None):
    async with httpx.AsyncClient() as client:
        try:
            await client.post(
                f"{BACKEND_URL}/admin-ai/notify-progress",
                json={
                    "threadId": thread_id,
                    "step": step,
                    "status": status,
                    "progress": progress,
                    "detail": detail
                }
            )
        except Exception as e:
            print(f"Error notifying progress: {e}")

@app.post("/chat")
async def chat(request: ChatRequest):
    config = {"configurable": {"thread_id": request.threadId}}
    
    print(f"DEBUG: Using API_BASE={os.getenv('OPENAI_API_BASE')}")
    print(f"DEBUG: adminId={request.adminId}, threadId={request.threadId}")

    await notify_backend_progress(request.threadId, "Planning", "Running", 10, "Đang phân tích yêu cầu...")
    
    try:
        db_path = os.path.join(os.path.dirname(__file__), "checkpoints.db")
        async with AsyncSqliteSaver.from_conn_string(db_path) as saver:
            available_tool_names = []
            async with httpx.AsyncClient() as client:
                try:
                    headers = {"X-Internal-Secret": os.getenv("BACKEND_INTERNAL_SECRET")}
                    t_resp = await client.get(
                        f"{BACKEND_URL}/admin-ai/available-tools",
                        headers=headers,
                        params={
                            "query": request.message,
                            "adminId": request.adminId
                        }
                    )
                    if t_resp.status_code == 200:
                        tools_data = t_resp.json()
                        available_tool_names = [t["name"] for t in tools_data]
                        print(f"DEBUG: Dynamic tools count: {len(available_tool_names)}")
                except Exception as te:
                    print(f"Error fetching tools: {te}")

            agent_app = workflow.compile(checkpointer=saver)
            
            inputs = {
                "messages": [HumanMessage(content=request.message)],
                "admin_id": request.adminId,
                "thread_id": request.threadId,
                "available_tools": available_tool_names
            }
            
            total_tool_calls = {}
            
            final_response = ""
            async for event in agent_app.astream(inputs, config=config, stream_mode="updates"):
                for node_name, updates in event.items():
                    if node_name.startswith("__"): continue
                    
                    if "messages" in updates:
                        msgs = updates["messages"]
                        if not isinstance(msgs, list): msgs = [msgs]
                        
                        for msg in msgs:
                            from langchain_core.messages import ToolMessage
                            
                            if isinstance(msg, AIMessage) and msg.tool_calls:
                                tool_names_vi = {
                                    "analyze_performance": "Phân tích hiệu suất",
                                    "get_user_ai_learning_history": "Xem lịch sử học tập AI",
                                    "search_entities": "Tìm kiếm dữ liệu",
                                    "search_vector_content": "Tra cứu kiến thức hệ thống",
                                    "get_course_details": "Lấy thông tin khóa học",
                                    "update_course": "Cập nhật khóa học",
                                    "generate_lesson_content": "Sinh nội dung bài giảng",
                                    "mass_enroll_users": "Ghi danh hàng loạt",
                                    "create_new_course": "Tạo khóa học mới",
                                    "assign_users_to_group": "Phân nhóm học viên",
                                    "send_email_report": "Gửi báo cáo qua Email",
                                    "register_tasks": "Đăng ký lộ trình",
                                    "generate_infographic": "Tạo Infographic"
                                }
                                
                                for tc in msg.tool_calls:
                                    tool_name_raw = tc['name']
                                    t_name = tool_names_vi.get(tool_name_raw, tool_name_raw)
                                    total_tool_calls[tc['id']] = t_name
                                    
                                    try:
                                        args = json.loads(tc['arguments'])
                                        important_val = args.get('query') or args.get('keyword') or args.get('topic') or args.get('title') or ""
                                        detail_msg = f"{t_name}: {important_val}" if important_val else t_name
                                    except:
                                        detail_msg = t_name

                                    await notify_backend_progress(request.threadId, t_name, "running", 50, detail_msg)
                            
                            elif isinstance(msg, ToolMessage):
                                t_name = total_tool_calls.get(msg.tool_call_id)
                                if t_name:
                                    status = "completed"
                                    detail = "Đã hoàn thành"
                                    if "lỗi" in str(msg.content).lower() or "error" in str(msg.content).lower():
                                        status = "error"
                                        detail = str(msg.content)[:100]
                                    
                                    await notify_backend_progress(request.threadId, t_name, status, 100, detail)

                            elif isinstance(msg, AIMessage) and msg.content:
                                final_response = msg.content
                            
        await notify_backend_progress(request.threadId, "Planning", "completed", 100, "Phân tích hoàn tất.")
        await notify_backend_progress(request.threadId, "Hoàn tất", "completed", 100, "Đang trả phản hồi...")
        return {"response": final_response}
        
    except Exception as e:
        import traceback
        traceback.print_exc()
        await notify_backend_progress(request.threadId, "Error", "Failed", 0, str(e))
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/health")
async def health():
    return {"status": "healthy"}

if __name__ == "__main__":
    import uvicorn
    port = int(os.getenv("PORT", 8000))
    uvicorn.run(app, host="0.0.0.0", port=port)
