import sys
import json
import argparse
import os
from sentence_transformers import SentenceTransformer

# Thử import qdrant-client (có thể chưa cài đặt ngay lập tức trong venv)
try:
    from qdrant_client import QdrantClient
    from qdrant_client.http.models import PointStruct
    HAS_QDRANT = True
except ImportError:
    HAS_QDRANT = False

# Đảm bảo output luôn là utf-8
if hasattr(sys.stdout, 'reconfigure'):
    sys.stdout.reconfigure(encoding='utf-8')
if hasattr(sys.stderr, 'reconfigure'):
    sys.stderr.reconfigure(encoding='utf-8')

def process_batch(input_data, model_name="all-MiniLM-L6-v2", qdrant_url=None, collection_name=None):
    """
    Xử lý danh sách các đoạn văn bản: nhúng vector và tùy chọn đẩy vào Qdrant.
    input_data: list các dict { "id": str, "text": str, "payload": dict }
    """
    try:
        if not input_data:
            sys.stderr.write("Dữ liệu đầu vào trống.\n")
            return

        sys.stderr.write(f"Đang tải model {model_name}...\n")
        model = SentenceTransformer(model_name)
        
        # Trích xuất danh sách text để nhúng vector hàng loạt
        texts = [item["text"] for item in input_data]
        sys.stderr.write(f"Đang nhúng vector cho {len(texts)} đoạn văn bản...\n")
        embeddings = model.encode(texts)
        
        # Nếu có thông tin Qdrant, thực hiện Upsert
        if qdrant_url and collection_name:
            if not HAS_QDRANT:
                raise ImportError("Thư viện qdrant-client chưa được cài đặt trong môi trường Python.")
            
            sys.stderr.write(f"Đang kết nối tới Qdrant tại {qdrant_url}...\n")
            client = QdrantClient(url=qdrant_url)
            
            points = []
            for i, item in enumerate(input_data):
                points.append(PointStruct(
                    id=item["id"],
                    vector=embeddings[i].tolist(),
                    payload={
                        "content": item["text"],
                        **(item.get("payload") or {})
                    }
                ))
            
            sys.stderr.write(f"Đang đẩy {len(points)} điểm vào collection '{collection_name}'...\n")
            client.upsert(collection_name=collection_name, points=points)
            sys.stderr.write("Hoàn tất đẩy dữ liệu vào Qdrant.\n")
            
            # Trả về kết quả thành công
            sys.stdout.write(json.dumps({"status": "success", "count": len(points)}))
        else:
            # Nếu không có Qdrant, trả về danh sách các vector
            results = []
            for i, item in enumerate(input_data):
                results.append({
                    "id": item["id"],
                    "vector": embeddings[i].tolist()
                })
            
            # Trả về kết quả: nếu là text đơn lẻ (backward compatibility) thì trả về mảng float trực tiếp
            if len(input_data) == 1 and not qdrant_url:
                sys.stdout.write(json.dumps(embeddings[0].tolist()))
            else:
                sys.stdout.write(json.dumps(results))
            
    except Exception as e:
        import traceback
        sys.stderr.write(f"Lỗi Python: {str(e)}\n")
        traceback.print_exc(file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    # Hỗ trợ truyền 1 text đơn lẻ (tương thích ngược) hoặc file JSON/stdin cho batch
    parser.add_argument("--text", help="Văn bản đơn lẻ cần tạo embedding")
    parser.add_argument("--input_json", help="Đường dẫn file JSON chứa mảng data batch")
    parser.add_argument("--model", default="all-MiniLM-L6-v2", help="Tên model sentence-transformer")
    parser.add_argument("--qdrant_url", help="URL máy chủ Qdrant")
    parser.add_argument("--collection", help="Tên collection trong Qdrant")
    args = parser.parse_args()
    
    input_data = []
    
    if args.input_json:
        # Đọc từ file JSON
        with open(args.input_json, 'r', encoding='utf-8') as f:
            input_data = json.load(f)
    elif args.text:
        # Chuyển đổi single text thành list 1 phần tử
        input_data = [{"id": "00000000-0000-0000-0000-000000000000", "text": args.text, "payload": {}}]
    else:
        # Thử đọc từ stdin nếu không có tham số
        try:
            stdin_data = sys.stdin.read()
            if stdin_data:
                input_data = json.loads(stdin_data)
        except:
            pass

    if not input_data:
        parser.print_help()
        sys.exit(1)
        
    process_batch(input_data, args.model, args.qdrant_url, args.collection)
