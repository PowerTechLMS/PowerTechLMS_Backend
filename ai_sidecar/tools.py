import httpx
import os
import json
from dotenv import load_dotenv
from langchain_core.tools import tool

load_dotenv()

INTERNAL_SECRET = os.getenv("BACKEND_INTERNAL_SECRET")
BACKEND_URL = os.getenv("BACKEND_URL", "http://localhost:5000/api")

async def call_backend_tool(tool_name: str, args: dict, admin_id: int):
    headers = {
        "X-Internal-Secret": INTERNAL_SECRET
    } if INTERNAL_SECRET else {}
    
    async with httpx.AsyncClient(timeout=60.0) as client:
        try:
            response = await client.post(
                f"{BACKEND_URL}/admin-ai/execute-tool",
                headers=headers,
                json={
                    "toolName": tool_name,
                    "argumentsJson": json.dumps(args),
                    "adminId": admin_id
                }
            )
            response.raise_for_status()
            return response.json()
        except Exception as e:
            return {"success": False, "error": str(e)}

@tool
async def analyze_performance(query: str, admin_id: int):
    """
    Phân tích hiệu suất học tập của người dùng. 
    Ví dụ: 'Tìm top 5 nhân viên học tốt nhất về ASP.NET Core'.
    """
    return await call_backend_tool("analyze_performance", {"query": query}, admin_id)

@tool
async def search_courses(keyword: str, admin_id: int):
    """
    Tìm kiếm các Khóa học trong hệ thống theo từ khóa.
    """
    return await call_backend_tool("search_courses", {"keyword": keyword}, admin_id)

@tool
async def search_users_departments(keyword: str, admin_id: int, entity_type: str = "user"):
    """
    Tìm kiếm Thành viên (User) hoặc Phòng ban/Nhóm (Department/Group).
    entity_type có thể là: 'User', 'Department'.
    """
    return await call_backend_tool("search_users_departments", {"keyword": keyword, "entity_type": entity_type}, admin_id)

@tool
async def mass_enroll_users(user_ids: list[int], course_id: int, admin_id: int):
    """
    Ghi danh hàng loạt danh sách học viên (user_ids) vào một khóa học (course_id).
    """
    return await call_backend_tool("mass_enroll_users", {"userIds": user_ids, "courseId": course_id}, admin_id)

@tool
async def create_new_course(title: str, category_id: int = None, level: int = 3, admin_id: int = 1):
    """
    Tạo một khóa học mới dưới dạng bản nháp.
    """
    return await call_backend_tool("create_new_course", {"title": title, "categoryId": category_id, "level": level}, admin_id)

@tool
async def assign_users_to_group(user_ids: list[int], group_id: int, admin_id: int):
    """
    Gán danh sách học viên vào một Phòng ban/Nhóm người dùng.
    """
    return await call_backend_tool("assign_users_to_group", {"userIds": user_ids, "groupId": group_id}, admin_id)

@tool
async def get_user_ai_learning_history(user_id: int, admin_id: int):
    """
    Lấy lịch sử học tập AI của người dùng bao gồm các phiên Role-play và bài tự luận (Essay).
    Kết quả trả về bao gồm nội dung hội thoại, điểm số và feedback từ AI.
    """
    return await call_backend_tool("get_user_ai_learning_history", {"userId": user_id}, admin_id)

@tool
async def search_vector_content(query: str, admin_id: int, top_k: int = 5):
    """
    Tìm kiếm nội dung ngữ nghĩa trong Vector Database (tài liệu, bài giảng).
    Sử dụng khi cần tìm thông tin chi tiết không có trong tìm kiếm từ khóa thông thường.
    """
    return await call_backend_tool("search_vector_content", {"query": query, "topK": top_k}, admin_id)

@tool
async def get_course_details(course_id: int, admin_id: int):
    """
    Lấy thông tin chi tiết của một khóa học bao gồm danh sách Module và các bài học (Lessons).
    """
    return await call_backend_tool("get_course_details", {"courseId": course_id}, admin_id)

@tool
async def get_course_students(course_id: int, admin_id: int):
    """
    Lấy danh sách tất cả học viên đã đăng ký/tham gia một khóa học dựa trên ID khóa học.
    Hãy sử dụng tool này sau khi đã xác định được course_id chính xác.
    """
    return await call_backend_tool("get_course_students", {"courseId": course_id}, admin_id)

@tool
async def update_course(course_id: int, admin_id: int, title: str = None, description: str = None):
    """
    Cập nhật tiêu đề hoặc mô tả của một khóa học hiện có.
    """
    return await call_backend_tool("update_course", {"courseId": course_id, "title": title, "description": description}, admin_id)

@tool
async def generate_lesson_content(module_id: int, topic: str, lesson_type: str, admin_id: int):
    """
    Yêu cầu AI sinh nội dung bài giảng mới dựa trên chủ đề và loại bài học.
    lesson_type có thể là: 'Video', 'Text', 'RolePlay', 'Essay'.
    """
    return await call_backend_tool("generate_lesson_content", {"moduleId": module_id, "topic": topic, "type": lesson_type}, admin_id)

@tool
async def send_email_report(subject: str, body: str, admin_id: int, thread_id: str, to_email: str = None):
    """
    Gửi email báo cáo kết quả thực hiện hoặc tổng hợp thông tin.
    Sử dụng sau khi hoàn thành các tác vụ quan trọng hoặc khi Admin yêu cầu báo cáo.
    thread_id: Mã thread_id của phiên chat hiện tại (lấy từ state).
    to_email: Email của người nhận (phải là email tồn tại trong hệ thống). Nếu không cung cấp, báo cáo sẽ được gửi tới Admin.
    """
    payload = {"subject": subject, "body": body, "threadId": thread_id}
    if to_email:
        payload["toEmail"] = to_email
    return await call_backend_tool("send_email_report", payload, admin_id)

@tool
async def register_tasks(tasks: list[str], admin_id: int, thread_id: str):
    """
    Đăng ký danh sách các tác vụ dự kiến sẽ thực hiện (Plan) để hiển thị lên giao diện cho Admin theo dõi.
    Sử dụng ngay khi vừa đề xuất Kế hoạch (Plan) cho Admin.
    tasks: Danh sách tiêu đề các tác vụ (ví dụ: ["Ghi danh học viên", "Mở khóa học"]).
    thread_id: Mã thread_id của phiên chat hiện tại (lấy từ state).
    """
    for task_name in tasks:
        await call_backend_tool("NotifyProgress", {
            "threadId": thread_id,
            "step": task_name,
            "status": "planned",
            "progress": 0,
            "detail": "Đang chờ phê duyệt"
        }, admin_id)
    return "Đã đăng ký danh sách tác vụ lên giao diện."

TOOLS = [
    analyze_performance, 
    get_user_ai_learning_history,
    search_courses,
    search_users_departments, 
    search_vector_content,
    get_course_students,
    get_course_details,
    update_course,
    generate_lesson_content,
    mass_enroll_users, 
    create_new_course, 
    assign_users_to_group,
    send_email_report,
    register_tasks
]
