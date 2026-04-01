import sys
import json
import argparse
import torch
from faster_whisper import WhisperModel

# Force UTF-8 encoding for stdout and stderr on Windows
if hasattr(sys.stdout, 'reconfigure'):
    sys.stdout.reconfigure(encoding='utf-8')
if hasattr(sys.stderr, 'reconfigure'):
    sys.stderr.reconfigure(encoding='utf-8')

def generate_subtitles(video_path, model_size="small", device="auto", compute_type="default"):
    sys.stderr.write(f"Đang khởi tạo model '{model_size}' trên '{device}'...\n")
    if device == "auto":
        device = "cuda" if torch.cuda.is_available() else "cpu"
    
    if compute_type == "default":
        compute_type = "float16" if device == "cuda" else "int8"
    
    model = WhisperModel(model_size, device=device, compute_type=compute_type)
    
    sys.stderr.write("Đang bắt đầu quá trình gỡ băng (transcribe)...\n")
    segments, _ = model.transcribe(
        video_path, 
        beam_size=5, 
        vad_filter=True,
        vad_parameters=dict(min_silence_duration_ms=500)
    )

    results = []
    for segment in segments:
        results.append({
            "start": segment.start,
            "end": segment.end,
            "text": segment.text.strip()
        })
    
    sys.stderr.write("Hoàn tất gỡ băng. Đang xuất kết quả JSON...\n")
    sys.stdout.write(json.dumps(results, ensure_ascii=False))

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--input", required=True)
    parser.add_argument("--model", default="small")
    parser.add_argument("--device", default="auto")
    parser.add_argument("--compute_type", default="default")
    args = parser.parse_args()
    
    try:
        generate_subtitles(args.input, args.model, args.device, args.compute_type)
    except Exception as e:
        import traceback
        sys.stderr.write(f"Lỗi Python: {str(e)}\n")
        traceback.print_exc(file=sys.stderr)
        sys.exit(1)
