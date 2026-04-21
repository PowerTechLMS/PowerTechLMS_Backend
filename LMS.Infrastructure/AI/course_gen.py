import sys
import json
import asyncio
import os
import io
import operator
import uuid
from typing import TypedDict, List, Dict, Any, Annotated, Optional, Literal

from langgraph.graph import StateGraph, END, START
from langgraph.types import Send
from langgraph.types import RetryPolicy
from langchain_openai import ChatOpenAI
from langchain_core.messages import HumanMessage, SystemMessage
from pydantic import BaseModel, Field, model_validator
import re

import aiosqlite
from langgraph.checkpoint.sqlite.aio import AsyncSqliteSaver

# Tương thích UTF-8 cho Windows (tránh lỗi UnicodeEncodeError)
if sys.stdout.encoding != 'utf-8':
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

LLM_BASE_URL = os.getenv("LLM_BASE_URL")

def repair_json(text: str) -> str:
    """Cố gắng sửa các lỗi JSON phổ biến."""
    # Xử lý dấu ngoặc kép chưa escape trong chuỗi
    # Tìm: "key": "value" 
    def fix_quotes(match):
        p1, content, p2 = match.groups()
        fixed = re.sub(r'(?<!\\)"', r'\\"', content)
        return f'{p1}{fixed}{p2}'
    
    text = re.sub(r'(".*?"\s*:\s*")(.*?)("\s*[,}\s])', fix_quotes, text, flags=re.DOTALL)
    return text

def parse_json_robust(text: str, schema_class: Any = None) -> Any:
    """Hàm giải mã JSON siêu bền bỉ."""
    # 1. Clean markdown
    clean_text = text.strip().strip('`').strip()
    if clean_text.startswith('json\n'): clean_text = clean_text[5:].strip()
    if clean_text.startswith('json'): clean_text = clean_text[4:].strip()
    
    # 2. Try Standard JSON
    try:
        data = json.loads(clean_text)
        if schema_class: return schema_class.model_validate(data)
        return data
    except: pass
    
    # 3. Try Repaired JSON
    try:
        repaired = repair_json(clean_text)
        data = json.loads(repaired)
        if schema_class: return schema_class.model_validate(data)
        return data
    except: pass
    
    # 4. Regex fallback (Trích xuất các trường quan trọng nhất)
    if schema_class:
        obj = {}
        # Tìm các chuỗi "key": "value" đơn giản
        for field_name in schema_class.model_fields.keys():
            pattern = rf'"{field_name}"\s*:\s*"(.*?)"(?="\s*[,}}])'
            match = re.search(pattern, clean_text, re.DOTALL)
            if match:
                obj[field_name] = match.group(1).replace('\\"', '"').strip()
        
        if obj:
            try: return schema_class.model_validate(obj)
            except: pass
            
    # Trả về đối tượng trống của schema nếu mọi cách đều thất bại
    if schema_class: return schema_class()
    return {}

# --- 1. PYDANTIC MODELS (STRUCTURED OUTPUT) ---
# --- 1. PYDANTIC MODELS (STRUCTURED OUTPUT) ---
class CourseOverviewOut(BaseModel):
    title: str = Field(description="Tiêu đề khóa học ngắn gọn, hấp dẫn", default="")
    description: str = Field(description="Mô tả khóa học chi tiết và mục tiêu", default="")

    @model_validator(mode='before')
    @classmethod
    def unwrap(cls, data: Any) -> Any:
        # Xử lý nếu AI trả về chuỗi (có thể bị đóng khung markdown)
        if isinstance(data, str):
            data = data.strip().strip('`').strip()
            if data.startswith('json\n'): data = data[5:].strip()
            try: return json.loads(data)
            except: 
                # Thử cứu bằng repair_json
                try: return json.loads(repair_json(data))
                except: pass
        
        if isinstance(data, dict):
            # Ánh xạ ngôn ngữ (Vi -> En)
            if "tieu_de" in data: data["title"] = data["tieu_de"]
            if "mo_ta" in data: data["description"] = data["mo_ta"]
            
            if "title" in data and "description" in data: return data
            
            # Unwrap root keys
            for k in ["course", "overview", "result", "idea", "khoa_hoc", "course_idea"]:
                if k in data and isinstance(data[k], dict):
                    sub = data[k]
                    if "tieu_de" in sub: sub["title"] = sub["tieu_de"]
                    if "mo_ta" in sub: sub["description"] = sub["mo_ta"]
                    return sub
                    
            # Handle arrays
            for k in ["course_ideas", "ideas", "results", "y_tuong"]:
                if k in data and isinstance(data[k], list) and len(data[k]) > 0:
                    first = data[k][0]
                    if isinstance(first, dict):
                        if "tieu_de" in first: first["title"] = first["tieu_de"]
                        if "mo_ta" in first: first["description"] = first["mo_ta"]
                        return first
        return data

class ModuleListOut(BaseModel):
    modules: List[str] = Field(description="Danh sách các tiêu đề chương/module (chỉ tiêu đề)", default_factory=list)

    @model_validator(mode='before')
    @classmethod
    def unwrap(cls, data: Any) -> Any:
        if isinstance(data, str):
            data = data.strip().strip('`').strip()
            if data.startswith('json\n'): data = data[5:].strip()
            try: return json.loads(data)
            except:
                try: return json.loads(repair_json(data))
                except: pass

        if isinstance(data, dict):
            # Nhận diện mảng từ nhiều tên trường khác nhau (kể cả Tiếng Việt)
            potential_keys = ["modules", "curriculum", "chapters", "result", "list", "lessons", 
                              "danh_sach_chuong", "chuong", "bai_hoc", "noi_dung"]
            for k in potential_keys:
                if k in data and isinstance(data[k], list):
                    items = []
                    for item in data[k]:
                        if isinstance(item, str): items.append(item)
                        elif isinstance(item, dict):
                            t = item.get("module_title") or item.get("title") or item.get("name") or \
                                item.get("tieu_de") or item.get("tieu_de_chuong")
                            if t: items.append(t)
                    if items: return {"modules": items}
            
            if "items" in data and isinstance(data["items"], list):
                return {"modules": [str(i) for i in data["items"]]}
        return data

class LessonSkeleton(BaseModel):
    title: Any = Field(description="Tiêu đề bài học", default="")
    type: Literal["Video", "Text", "RolePlay", "Essay"] = Field(description="Loại bài học", default="Video")
    has_mini_quiz: bool = Field(description="Đánh dấu True nếu bài học này cần kiểm tra kiến thức cuối bài", default=False)

class ModuleLessonsOut(BaseModel):
    lessons: List[LessonSkeleton] = Field(description="Danh sách bài học trong chương", default_factory=list)

    @model_validator(mode='before')
    @classmethod
    def unwrap(cls, data: Any) -> Any:
        if isinstance(data, str):
            data = data.strip().strip('`').strip()
            if data.startswith('json\n'): data = data[5:].strip()
            try: return json.loads(data)
            except:
                try: return json.loads(repair_json(data))
                except: pass

        if isinstance(data, dict):
            potential_keys = ["lessons", "curriculum", "items", "result", "bai_hoc", "danh_sach_bai_hoc", "lectures", "bai_giang"]
            for k in potential_keys:
                if k in data and isinstance(data[k], list):
                    # Map nội bộ cho từng item nếu cần (Vi -> En)
                    mapped_items = []
                    for item in data[k]:
                        if isinstance(item, dict):
                            # Normalizing lesson data
                            if "tieu_de" in item: item["title"] = item["tieu_de"]
                            if "lecture_title" in item: item["title"] = item["lecture_title"]
                            if "loai" in item: item["type"] = item["loai"]
                            if "lecture_id" in item: item["id"] = item["lecture_id"]
                            mapped_items.append(item)
                    return {"lessons": mapped_items}
        return data

class VideoContentOut(BaseModel):
    video_script: Any = Field(description="Kịch bản chi tiết cho video (người nói, hình ảnh)", default="")
    content: Any = Field(description="Nội dung tóm tắt hiển thị bên dưới video định dạng HTML", default="")
    duration_seconds: int = Field(description="Thời lượng ước tính bằng giây", default=600)

    @model_validator(mode='before')
    @classmethod
    def unwrap(cls, data: Any) -> Any:
        if isinstance(data, str):
            data = data.strip().strip('`').strip()
            if data.startswith('json\n'): data = data[5:].strip()
            try: return json.loads(data)
            except:
                try: return json.loads(repair_json(data))
                except: pass
        return data

class TextContentOut(BaseModel):
    title: Any = Field(description="Tiêu đề bài học (nếu có)", default="")
    content: Any = Field(description="Nội dung bài đọc định dạng HTML sâu sắc và chi tiết", default="")
    duration_seconds: int = Field(description="Thời lượng ước tính bằng giây", default=300)

    @model_validator(mode='before')
    @classmethod
    def unwrap(cls, data: Any) -> Any:
        if isinstance(data, str):
            data = data.strip().strip('`').strip()
            if data.startswith('json\n'): data = data[5:].strip()
            try: return json.loads(data)
            except:
                try: return json.loads(repair_json(data))
                except: pass
        return data

class RolePlayContentOut(BaseModel):
    scenario: Any = Field(description="Tình huống Role Play giả lập (cụ thể ngữ cảnh, vai trò)", default="")
    pass_score: int = Field(description="Điểm đạt (0-100)", default=80)
    scoring_criteria: Any = Field(description="Tiêu chí chấm điểm", default="")
    additional_requirements: Any = Field(description="Yêu cầu hệ thống cho AI đóng vai", default="")
    duration_seconds: int = Field(description="Thời lượng ước tính bằng giây", default=600)

    @model_validator(mode='before')
    @classmethod
    def unwrap(cls, data: Any) -> Any:
        if isinstance(data, str):
            data = data.strip().strip('`').strip()
            if data.startswith('json\n'): data = data[5:].strip()
            try: return json.loads(data)
            except:
                try: return json.loads(repair_json(data))
                except: pass
        return data

class EssayContentOut(BaseModel):
    question: Any = Field(description="Câu hỏi tự luận", default="")
    min_words: int = Field(description="Số từ tối thiểu", default=200)
    max_words: int = Field(description="Số từ tối đa", default=1000)
    scoring_criteria: Any = Field(description="Tiêu chí chấm điểm", default="")

    @model_validator(mode='before')
    @classmethod
    def unwrap(cls, data: Any) -> Any:
        if isinstance(data, str):
            data = data.strip().strip('`').strip()
            if data.startswith('json\n'): data = data[5:].strip()
            try: return json.loads(data)
            except:
                try: return json.loads(repair_json(data))
                except: pass
        return data

class QuestionOut(BaseModel):
    QuestionText: Any = Field(description="Nội dung câu hỏi", default="")
    Options: Dict[str, Any] = Field(description="4 đáp án A, B, C, D", default_factory=dict)
    CorrectAnswer: str = Field(description="Đáp án đúng (A, B, C hoặc D)", default="A")

    @model_validator(mode='before')
    @classmethod
    def unwrap(cls, data: Any) -> Any:
        if isinstance(data, str):
            data = data.strip().strip('`').strip()
            if data.startswith('json\n'): data = data[5:].strip()
            try: return json.loads(data)
            except:
                try: return json.loads(repair_json(data))
                except: pass
        return data

class QuizOut(BaseModel):
    title: str = Field(description="Tiêu đề bài kiểm tra", default="Quiz")
    pass_score: int = Field(description="Điểm đạt", default=80)
    questions: List[QuestionOut] = Field(description="Danh sách câu trắc nghiệm", default_factory=list)

    @model_validator(mode='before')
    @classmethod
    def unwrap(cls, data: Any) -> Any:
        if isinstance(data, str):
            data = data.strip().strip('`').strip()
            if data.startswith('json\n'): data = data[5:].strip()
            try: return json.loads(data)
            except:
                try: return json.loads(repair_json(data))
                except: pass
        return data

# --- 2. GRAPH STATE DEFINITION ---
class CourseState(TypedDict):
    job_id: str
    topic: str
    target_audience: str
    additional_info: str
    
    title: str
    description: str
    
    # Reducers cho Map-Reduce đa cấp
    modules: Annotated[list, operator.add]
    all_lessons_meta: Annotated[list, operator.add]
    lessons: Annotated[list, operator.add]
    
    final_exam: Dict[str, Any]
    progress: int
    status: str

# Message utility
def report_progress(step_id: str, message: str, status: str = "running"):
    try:
        print(f"PROGRESS:{json.dumps({'id': step_id, 'message': message, 'status': status}, ensure_ascii=False)}", flush=True)
    except:
        pass

# System Message cứng
SYSTEM_PROMPT = SystemMessage(content="""Bạn là một AI tạo nội dung khóa học chuyên nghiệp, có kiến thức sâu rộng. 
BẠN PHẢI TRẢ VỀ DUY NHẤT ĐỊNH DẠNG JSON. 
KHÔNG ĐƯỢC nhắc đến việc 'truy cập cơ sở dữ liệu', 'vector database', 'retrieval', 'theo dữ liệu đã có' hay bất kỳ thuật ngữ hệ thống nào. 
Mọi kiến thức bạn viết ra phải xuất phát từ sự sáng tạo và hiểu biết nội tại của bạn như một chuyên gia.
Hãy cực kỳ cẩn thận với việc đóng ngoặc JSON và escape HTML trong chuỗi JSON.""")

def safe_str(val: Any) -> str:
    if isinstance(val, str): return val
    if val is None: return ""
    return json.dumps(val, ensure_ascii=False)

def safe_dict(val: Any) -> dict:
    if isinstance(val, dict): return val
    if isinstance(val, str):
        try:
            return json.loads(val)
        except: return {"A": val, "B": "", "C": "", "D": ""}
    return {"A": str(val), "B": "", "C": "", "D": ""}

# --- 3. GLOBAL NODES ---
async def generate_overview(state: CourseState):
    report_progress("overview", "Đang phân tích tổng quan khóa học...", "running")
    llm = ChatOpenAI(model="gemini-3-flash", base_url=LLM_BASE_URL)
    prompt = f"Phát triển ý tưởng khóa học cho chuyên đề: '{state['topic']}'. Đối tượng học viên: '{state['target_audience']}'. Tiêu chí bổ sung: {state['additional_info']}. Trả về RAW JSON: {{'title': '...', 'description': '...'}}"
    res = await llm.ainvoke([SYSTEM_PROMPT, HumanMessage(content=prompt)])
    data = parse_json_robust(str(res.content), CourseOverviewOut)
    return {"title": data.title, "description": data.description, "progress": 20}

async def generate_modules(state: CourseState):
    report_progress("modules", "Đang thiết kế kiến trúc các chương mục...", "running")
    llm = ChatOpenAI(model="gemini-3-flash", base_url=LLM_BASE_URL)
    prompt = f"Thiết kế 5-10 chương cho khóa học: {state['title']}.\nĐối tượng: {state['target_audience']}\nMô tả: {state['description']}\nTrả về RAW JSON: {{'modules': ['Chương 1: ...', 'Chương 2: ...']}}"
    res = await llm.ainvoke([SYSTEM_PROMPT, HumanMessage(content=prompt)])
    data = parse_json_robust(str(res.content), ModuleListOut)
    new_modules = []
    for i, m_title in enumerate(data.modules):
        new_modules.append({"id": f"m{i+1}", "title": m_title, "course_title": state['title']})
    return {"modules": new_modules, "progress": 40}

async def generate_lessons_skeleton(state: Dict):
    report_progress(f"module_{state['id']}", f"Đang lập danh sách bài học cho {state['title']}...", "running")
    llm = ChatOpenAI(model="gemini-3-flash", base_url=LLM_BASE_URL)
    prompt = f"Chương: {state['title']}\nKhóa học: {state['course_title']}\nHãy tạo 3-5 bài học. Loại bài: Video, Text, RolePlay, Essay. Trả về RAW JSON mảng 'lessons'."
    res = await llm.ainvoke([SYSTEM_PROMPT, HumanMessage(content=prompt)])
    data = parse_json_robust(str(res.content), ModuleLessonsOut)
    lessons_meta = []
    for i, l in enumerate(data.lessons):
        lessons_meta.append({
            "id": f"{state['id']}_l{i+1}",
            "module_id": state['id'],
            "module_title": state['title'],
            "course_title": state['course_title'],
            "index": i,
            "title": l.title,
            "type": l.type,
            "has_mini_quiz": l.has_mini_quiz
        })
    return {"all_lessons_meta": lessons_meta}

def route_modules(state: CourseState):
    sends = []
    for mod in state["modules"]:
        sends.append(Send("generate_lessons_skeleton", mod))
    return sends

def route_lessons(state: CourseState):
    sends = []
    for lesson in state.get("all_lessons_meta", []):
        sends.append(Send("process_lesson_coordinator", {
            "lesson": lesson, 
            "course_title": lesson["course_title"], 
            "course_audience": state["target_audience"]
        }))
    return sends

async def generate_final_exam(state: CourseState):
    report_progress("final_exam", "Đang soạn thảo bộ câu hỏi kiểm tra cuối khóa...", "running")
    llm = ChatOpenAI(model="gemini-3-flash", base_url=LLM_BASE_URL)
    prompt = (f"Khóa học: {state['title']}\nMô tả: {state['description']}\n"
              "Hãy soạn một bộ đề kiểm tra cuối khóa gồm 10-15 câu hỏi trắc nghiệm bao quát toàn bộ nội dung. "
              "YÊU CẦU: Mỗi câu hỏi phải có chính xác 4 lựa chọn A, B, C, D. "
              "Trả về RAW JSON: {'title': 'Bài kiểm tra cuối khóa', 'pass_score': 80, "
              "'questions': [{'QuestionText': '...', 'Options': {'A': '...', 'B': '...', 'C': '...', 'D': '...'}, 'CorrectAnswer': 'A'}]}")
    res = await llm.ainvoke([SYSTEM_PROMPT, HumanMessage(content=prompt)])
    data = parse_json_robust(str(res.content), QuizOut)
    
    quiz_questions = []
    for q in data.questions:
        quiz_questions.append({
            "QuestionText": q.QuestionText,
            "Options": q.Options,
            "CorrectAnswer": q.CorrectAnswer
        })
        
    final_quiz = {
        "Title": data.title or "Bài kiểm tra cuối khóa",
        "PassScore": data.pass_score or 80,
        "questions": quiz_questions
    }
    return {"final_exam": final_quiz}

def collate_lessons(state: CourseState):
    return {}

# --- 4. SUBGRAPH CHO LESSON (MAP) ---
class SubLessonState(TypedDict):
    lesson: Dict[str, Any]
    course_title: str
    course_audience: str

def route_sublesson_type(state: SubLessonState):
    return state["lesson"]["type"]

def route_to_quiz(state: SubLessonState):
    if state["lesson"].get("has_mini_quiz"):
        return "quiz_gen"
    return END

async def video_gen(state: SubLessonState):
    llm = ChatOpenAI(model="gemini-3-flash", base_url=LLM_BASE_URL)
    # Đồng bộ phong cách 'Chuyên gia biên kịch' từ C#
    system_prompt = SystemMessage(content="Bạn là một chuyên gia biên kịch và quay dựng bài giảng video chuyên nghiệp. BẠN PHẢI TRẢ VỀ DUY NHẤT ĐỊNH DẠNG JSON.")
    user_prompt = f"Khóa học: {state['course_title']}\nChương: {state['lesson']['module_title']}\nBài (Video): {state['lesson']['title']}\nHãy gợi ý khung sườn video chi tiết (Draft Script) và một nội dung tóm tắt HTML. Trả về RAW JSON: {{'video_script': '...', 'content': '...', 'duration_seconds': 600}}"
    
    report_progress(state["lesson"]["id"], f"Đang tạo kịch bản video: {state['lesson']['title']}", "running")
    res = await llm.ainvoke([system_prompt, HumanMessage(content=user_prompt)])
    data = parse_json_robust(str(res.content), VideoContentOut)
    state["lesson"]["content"] = safe_str(data.content)
    state["lesson"]["video_script"] = safe_str(data.video_script)
    state["lesson"]["duration_seconds"] = data.duration_seconds
    return {"lesson": state["lesson"]}

async def text_gen(state: SubLessonState):
    llm = ChatOpenAI(model="gemini-3-flash", base_url=LLM_BASE_URL)
    prompt = f"Khóa học: {state['course_title']}\nChương: {state['lesson']['module_title']}\nBài (Đọc hiểu): {state['lesson']['title']}\nBiên soạn nội dung bài giảng HTML sâu sắc (trên 1000 từ). Trả về RAW JSON: {{'title': '...', 'content': '...', 'duration_seconds': 300}}"
    report_progress(state["lesson"]["id"], f"Đang soạn nội dung bài đọc: {state['lesson']['title']}", "running")
    res = await llm.ainvoke([SYSTEM_PROMPT, HumanMessage(content=prompt)])
    data = parse_json_robust(str(res.content), TextContentOut)
    state["lesson"]["content"] = safe_str(data.content)
    state["lesson"]["duration_seconds"] = data.duration_seconds
    return {"lesson": state["lesson"]}

async def roleplay_gen(state: SubLessonState):
    llm = ChatOpenAI(model="gemini-3-flash", base_url=LLM_BASE_URL)
    prompt = (f"Khóa học: {state['course_title']}\nChương: {state['lesson']['module_title']}\nBài (RolePlay): {state['lesson']['title']}\n"
              "Thiết kế tình huống nhập vai chi tiết.\n"
              "YÊU CẦU CỰC KỲ QUAN TRỌNG VỀ ĐỊNH DẠNG 'scoring_criteria':\n"
              "1. Bạn PHẢI liệt kê các tiêu chí đánh giá cụ thể.\n"
              "2. Mỗi tiêu chí PHẢI đi kèm tỷ lệ % trọng số (ví dụ: 20%, 50%).\n"
              "3. TỔNG CỘNG CỦA TẤT CẢ CÁC TRỌNG SỐ PHẢI CHÍNH XÁC BẰNG 100%.\n"
              "BẮT BUỘC KIỂM TRA LẠI PHÉP CỘNG TRƯỚC KHI TRẢ VỀ.\n"
              "Ví dụ format: '1. Kỹ năng lắng nghe (30%), 2. Xử lý từ chối (40%), 3. Thuyết phục khách hàng (30%)'.\n"
              "Trả về RAW JSON: {'scenario': '...', 'pass_score': 80, 'scoring_criteria': '...', 'additional_requirements': '...'}")
    report_progress(state["lesson"]["id"], f"Đang thiết kế RolePlay: {state['lesson']['title']}", "running")
    res = await llm.ainvoke([SYSTEM_PROMPT, HumanMessage(content=prompt)])
    data = parse_json_robust(str(res.content), RolePlayContentOut)
    state["lesson"]["content"] = safe_str(data.scenario)
    state["lesson"]["role_play"] = {
        "Scenario": safe_str(data.scenario),
        "PassScore": data.pass_score,
        "ScoringCriteria": safe_str(data.scoring_criteria),
        "AdditionalRequirements": safe_str(data.additional_requirements)
    }
    state["lesson"]["duration_seconds"] = data.duration_seconds
    return {"lesson": state["lesson"]}

async def essay_gen(state: SubLessonState):
    llm = ChatOpenAI(model="gemini-3-flash", base_url=LLM_BASE_URL)
    prompt = (f"Khóa học: {state['course_title']}\nChương: {state['lesson']['module_title']}\nBài (Tự luận): {state['lesson']['title']}\n"
              "Thiết kế câu hỏi tự luận chuyên sâu.\n"
              "YÊU CẦU CỰC KỲ QUAN TRỌNG VỀ ĐỊNH DẠNG 'scoring_criteria':\n"
              "1. Liệt kê các tiêu chí chấm điểm cụ thể.\n"
              "2. Mỗi tiêu chí kèm theo tỷ lệ % trọng số.\n"
              "3. TỔNG CỘNG CÁC TRỌNG SỐ PHẢI CHÍNH XÁC BẰNG 100%.\n"
              "BẮT BUỘC KIỂM TRA LẠI PHÉP CỘNG TRƯỚC KHI TRẢ VỀ.\n"
              "Ví dụ format: '1. Cấu trúc bài viết (20%), 2. Sự mạch lạc (30%), 3. Kiến thức chuyên môn (50%)'.\n"
              "Trả về RAW JSON: {'question': '...', 'min_words': 200, 'max_words': 1000, 'scoring_criteria': '...'}")
    report_progress(state["lesson"]["id"], f"Đang soạn câu hỏi tự luận: {state['lesson']['title']}", "running")
    res = await llm.ainvoke([SYSTEM_PROMPT, HumanMessage(content=prompt)])
    data = parse_json_robust(str(res.content), EssayContentOut)
    state["lesson"]["content"] = safe_str(data.question)
    state["lesson"]["essay"] = {
        "Question": safe_str(data.question),
        "MinWords": data.min_words,
        "MaxWords": data.max_words,
        "ScoringCriteria": safe_str(data.scoring_criteria)
    }
    return {"lesson": state["lesson"]}

async def quiz_gen(state: SubLessonState):
    llm = ChatOpenAI(model="gemini-3-flash", base_url=LLM_BASE_URL)
    prompt = (f"Soạn Quiz cho bài: {state['lesson']['title']}. Khóa học: {state['course_title']}\n"
              "Tạo 3-5 câu hỏi trắc nghiệm A, B, C, D. "
              "TRẢ VỀ RAW JSON: {'title': '...', 'pass_score': 80, "
              "'questions': [{'QuestionText': '...', 'Options': {'A': '...', 'B': '...', 'C': '...', 'D': '...'}, 'CorrectAnswer': 'A'}]}")
    report_progress(state["lesson"]["id"], f"Đang tạo Quiz cho bài: {state['lesson']['title']}", "running")
    res = await llm.ainvoke([SYSTEM_PROMPT, HumanMessage(content=prompt)])
    data = parse_json_robust(str(res.content), QuizOut)
    quiz_questions = []
    for q in data.questions:
        quiz_questions.append({
            "QuestionText": q.QuestionText,
            "Options": q.Options,
            "CorrectAnswer": q.CorrectAnswer
        })
    state["lesson"]["quiz"] = {
        "Title": data.title,
        "PassScore": data.pass_score,
        "questions": quiz_questions
    }
    return {"lesson": state["lesson"]}

lesson_builder = StateGraph(SubLessonState)
retry_policy = RetryPolicy(max_attempts=3)

lesson_builder.add_node("video_gen", video_gen, retry=retry_policy)
lesson_builder.add_node("text_gen", text_gen, retry=retry_policy)
lesson_builder.add_node("roleplay_gen", roleplay_gen, retry=retry_policy)
lesson_builder.add_node("essay_gen", essay_gen, retry=retry_policy)
lesson_builder.add_node("quiz_gen", quiz_gen, retry=retry_policy)

lesson_builder.set_conditional_entry_point(
    route_sublesson_type,
    {
        "Video": "video_gen",
        "Text": "text_gen",
        "RolePlay": "roleplay_gen",
        "Essay": "essay_gen"
    }
)
# Cực kì sạch sẽ: Tự động điều tuyến Quiz
for node in ["video_gen", "text_gen", "roleplay_gen", "essay_gen"]:
    lesson_builder.add_conditional_edges(node, route_to_quiz, {"quiz_gen": "quiz_gen", END: END})
lesson_builder.add_edge("quiz_gen", END)

lesson_graph = lesson_builder.compile()

# Return to Main Graph Coordinator
async def process_lesson_coordinator(state: SubLessonState):
    l = state["lesson"]
    report_progress(l["id"], f"Đang soạn nội dung [{l['type']}]: {l['title']}", "running")
    # Subgraph chạy bên trong Node song song
    res = await lesson_graph.ainvoke(state)
    report_progress(l["id"], f"Hoàn tất soạn: {l['title']}", "completed")
    return {"lessons": [res["lesson"]]}

# --- 5. MAIN GRAPH BINDING ---
builder = StateGraph(CourseState)
builder.add_node("generate_overview", generate_overview, retry=retry_policy)
builder.add_node("generate_modules", generate_modules, retry=retry_policy)
builder.add_node("generate_lessons_skeleton", generate_lessons_skeleton, retry=retry_policy)
builder.add_node("collate_lessons", collate_lessons)
builder.add_node("process_lesson_coordinator", process_lesson_coordinator)
builder.add_node("generate_final_exam", generate_final_exam, retry=retry_policy)

builder.add_edge(START, "generate_overview")
builder.add_edge("generate_overview", "generate_modules")
builder.add_conditional_edges("generate_modules", route_modules, ["generate_lessons_skeleton"])
builder.add_edge("generate_lessons_skeleton", "collate_lessons")
builder.add_conditional_edges("collate_lessons", route_lessons, ["process_lesson_coordinator"])
builder.add_edge("process_lesson_coordinator", "generate_final_exam")
builder.add_edge("generate_final_exam", END)

# ENTRY POINT
async def main():
    if len(sys.argv) < 2:
        print("Usage: python course_gen.py <input_json>")
        return

    input_data = json.loads(sys.argv[1])
    job_id = input_data.get("job_id", str(uuid.uuid4()))
    
    initial_state = {
        "job_id": job_id,
        "topic": input_data.get("topic", ""),
        "target_audience": input_data.get("target_audience", "Người mới"),
        "additional_info": input_data.get("additional_info", ""),
        "modules": [],
        "lessons": [],
        "all_lessons_meta": [],
        "final_exam": {}
    }

    # Time-travel & Recovery Backend
    db_path = os.path.join(os.path.dirname(__file__), "checkpoints.sqlite")
    async with aiosqlite.connect(db_path) as conn:
        checkpointer = AsyncSqliteSaver(conn)
        await checkpointer.setup()
        
        # Compile với Checkpointer
        graph = builder.compile(checkpointer=checkpointer)
        config = {"configurable": {"thread_id": job_id}}
        
        # Gọi luồng
        result = await graph.ainvoke(initial_state, config)
        
    # Mapping kết quả 3 giai đoạn: Gộp lesson vào module dựa trên module_id
    results_map = {r["id"]: r for r in result["lessons"]}
    
    # Xây dựng cấu trúc module từ state['modules']
    modules_completed = []
    for m in result["modules"]:
        mod_id = m["id"]
        mod_title = m["title"]
        mod_lessons = []
        
        # Tìm các bài học thuộc module này từ results_map
        for l in result["lessons"]:
            if l.get("module_id") == mod_id:
                mod_lessons.append(l)
                
                
        modules_completed.append({
            "title": mod_title,
            "lessons": mod_lessons
        })
            
    final_output = {
        "title": result["title"],
        "description": result["description"],
        "modules": modules_completed,
        "final_exam": result.get("final_exam", {})
    }
    
    print("[RESULT_JSON_START]")
    print(json.dumps(final_output, ensure_ascii=False))
    print("[RESULT_JSON_END]")

if __name__ == "__main__":
    asyncio.run(main())
