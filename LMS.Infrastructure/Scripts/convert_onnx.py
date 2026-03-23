import os
import torch
from transformers import AutoTokenizer, AutoModelForSeq2SeqLM

def convert_to_onnx(model_id, save_dir):
    print(f"Loading model {model_id}...")
    tokenizer = AutoTokenizer.from_pretrained(model_id)
    model = AutoModelForSeq2SeqLM.from_pretrained(model_id)
    if not os.path.exists(save_dir):
        os.makedirs(save_dir)
    tokenizer.save_pretrained(save_dir)
    try:
        from optimum.onnxruntime import ORTModelForSeq2SeqLM
        print("Using Optimum for export...")
        ort_model = ORTModelForSeq2SeqLM.from_pretrained(model_id, export=True)
        ort_model.save_pretrained(save_dir)
        print(f"Exported successfully to {save_dir}")
    except ImportError:
        print("Optimum not found. Please install: pip install optimum[onnxruntime]")

if __name__ == "__main__":
    model_name = "protonx-models/protonx-legal-tc"
    target_path = os.path.join("..", "LMS.API", "models", "protonx-legal-tc")
    convert_to_onnx(model_name, target_path)
