import os
from typing import Annotated, List, TypedDict, Union
from dotenv import load_dotenv

from langchain_openai import ChatOpenAI
from langchain_core.messages import BaseMessage, HumanMessage, AIMessage
from langchain_core.prompts import ChatPromptTemplate, MessagesPlaceholder
from langgraph.graph import StateGraph, START, END
from langgraph.graph.message import add_messages
from langgraph.checkpoint.sqlite import SqliteSaver
from langgraph.prebuilt import ToolNode

from tools import TOOLS

load_dotenv()

class AgentState(TypedDict):
    messages: Annotated[List[BaseMessage], add_messages]
    admin_id: int
    thread_id: str
    available_tools: List[str]
    is_planning: bool

model = ChatOpenAI(
    model="gemini-3-flash", 
    temperature=0,
    base_url=os.getenv("OPENAI_API_BASE")
)
model_with_tools = model.bind_tools(TOOLS)

async def call_model(state: AgentState):
    available_tool_names = state.get("available_tools", [])
    
    current_tools = [t for t in TOOLS if t.name in available_tool_names] if available_tool_names else TOOLS
    
    model_with_tools = model.bind_tools(current_tools)
    
    system_instructions = [
        "Bạn là trợ lý AI quản trị hệ thống LMS PowerTech. Nhiệm vụ của bạn tùy thuộc vào danh sách công cụ (tools) được cấp:"
    ]
    
    if "search_entities" in available_tool_names or "search_vector_content" in available_tool_names:
        system_instructions.append("- Tìm kiếm & Tra cứu: Khám phá dữ liệu khóa học, thành viên và kiến nghị nội dung từ Vector DB.")
    
    if "get_course_details" in available_tool_names or "update_course" in available_tool_names or "generate_lesson_content" in available_tool_names or "get_course_students" in available_tool_names:
        system_instructions.append("- Quản lý Khóa học: Xem chi tiết bài giảng, cập nhật nội dung, sinh bài giảng mới và TRUY VẤN danh sách học viên theo ID khóa học.")
        
    if "mass_enroll_users" in available_tool_names or "assign_users_to_group" in available_tool_names:
        system_instructions.append("- Quản lý Thành viên: Ghi danh hàng loạt học viên và phân nhóm người dùng.")
        
    if "get_user_ai_learning_history" in available_tool_names or "analyze_performance" in available_tool_names:
        system_instructions.append("- Đánh giá Năng lực: Xem lịch sử học tập AI và phân tích hiệu suất học viên.")
        
    if "send_email_report" in available_tool_names:
        system_instructions.append("- Báo cáo: Sau khi hoàn thành các tác vụ quan trọng, hãy LUÔN LUÔN gọi `send_email_report` để tóm tắt kết quả gửi cho Admin.")

    system_instructions.append("\nQUY TRÌNH PHÊ DUYỆT (BẮT BUỘC):")
    system_instructions.append("- Đối với yêu cầu phức tạp (trên 2 bước), bạn PHẢI gọi `register_tasks` để đăng ký kế hoạch lên UI.")
    system_instructions.append("- Sau khi gọi `register_tasks`, bạn PHẢI LIỆT KÊ rõ danh sách các bước đó cho Admin thấy rồi mới hỏi: 'Bạn có phê duyệt kế hoạch này không?'.")
    system_instructions.append("- Tuyệt đối không được hỏi phê duyệt mà không liệt kê nội dung kế hoạch.")
    system_instructions.append("- Chỉ thực thi các bước tiếp theo khi Admin đồng ý (ví dụ: 'Làm đi', 'Phê duyệt').")
    
    system_instructions.append(f"\nAdmin ID hiện tại: {state.get('admin_id', 1)}")
    system_instructions.append(f"Thread ID hiện tại: {state.get('thread_id', 'unknown')}")

    prompt = ChatPromptTemplate.from_messages([
        ("system", "\n".join(system_instructions)),
        MessagesPlaceholder(variable_name="messages"),
    ])
    
    chain = prompt | model_with_tools
    response = await chain.ainvoke(state)
    return {"messages": [response]}

workflow = StateGraph(AgentState)

workflow.add_node("agent", call_model)
tool_node = ToolNode(TOOLS)
workflow.add_node("tools", tool_node)

workflow.add_edge(START, "agent")

def route_tools(state: AgentState):
    if isinstance(state["messages"][-1], AIMessage) and state["messages"][-1].tool_calls:
        return "tools"
    return END

workflow.add_conditional_edges("agent", route_tools, ["tools", END])
workflow.add_edge("tools", "agent")
