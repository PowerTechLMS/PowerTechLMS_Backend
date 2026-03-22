import os
import sys
import subprocess

def install_dependencies():
    print("Checking dependencies...")
    try:
        import optimum
        from optimum.onnxruntime import ORTModelForSeq2SeqLM
    except ImportError:
        print("Installing missing dependencies (optimum[onnxruntime], transformers, torch)...")
        subprocess.check_call([sys.executable, "-m", "pip", "install", "optimum[onnxruntime]", "transformers", "torch"])

def export_to_onnx(model_id, output_dir):
    from optimum.onnxruntime import ORTModelForSeq2SeqLM
    from transformers import AutoTokenizer

    print(f"Exporting model {model_id} to ONNX using Optimum...")
    
    # This will download and export the model to multiple ONNX files (encoder, decoder, decoder_with_past)
    # Optimum handles the T5 complexity automatically.
    model = ORTModelForSeq2SeqLM.from_pretrained(model_id, export=True)
    tokenizer = AutoTokenizer.from_pretrained(model_id)
    
    model.save_pretrained(output_dir)
    tokenizer.save_pretrained(output_dir)
    
    print(f"Export complete! Files saved to {output_dir}")

if __name__ == "__main__":
    model_id = "protonx-models/protonx-legal-tc"
    output_dir = os.path.join(os.getcwd(), "models", "protonx-legal-tc")
    if not os.path.exists(output_dir):
        os.makedirs(output_dir)
    
    try:
        install_dependencies()
        export_to_onnx(model_id, output_dir)
    except Exception as e:
        print(f"CRITICAL ERROR during ONNX conversion: {e}")
        sys.exit(1)

if __name__ == "__main__":
    model_id = "protonx-models/protonx-legal-tc"
    output_dir = os.path.join(os.getcwd(), "models")
    if not os.path.exists(output_dir):
        os.makedirs(output_dir)
    
    output_path = os.path.join(output_dir, "protonx-legal-tc.onnx")
    
    try:
        install_dependencies()
        export_to_onnx(model_id, output_path)
    except Exception as e:
        print(f"CRITICAL ERROR during ONNX conversion: {e}")
        sys.exit(1)
