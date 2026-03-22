import sys
import torch
import os
from transformers import AutoTokenizer, AutoModelForSeq2SeqLM

# Force UTF-8 for Windows console
if sys.platform == "win32":
    import io
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8')

# Disable logging to keep stdout clean for the C# result
import logging
logging.disable(logging.CRITICAL)
os.environ["TRANSFORMERS_VERBOSITY"] = "error"

def refine_batch(texts):
    model_path = "protonx-models/protonx-legal-tc"
    
    tokenizer = AutoTokenizer.from_pretrained(model_path)
    model = AutoModelForSeq2SeqLM.from_pretrained(model_path)
    
    device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
    model.to(device)
    model.eval()

    refined_results = []
    for text in texts:
        if not text.strip():
            refined_results.append("")
            continue
            
        inputs = tokenizer(
            text,
            return_tensors="pt",
            truncation=True,
            max_length=256
        ).to(device)

        with torch.no_grad():
            outputs = model.generate(
                **inputs,
                num_beams=3, # Fast enough
                max_new_tokens=256,
                early_stopping=True
            )
        refined_results.append(tokenizer.decode(outputs[0], skip_special_tokens=True))
    
    return refined_results

if __name__ == "__main__":
    if len(sys.argv) < 2:
        sys.exit(1)
    
    if sys.argv[1] == "--batch" and len(sys.argv) == 3:
        # Read from file
        with open(sys.argv[2], "r", encoding="utf-8") as f:
            lines = f.readlines()
        
        results = refine_batch(lines)
        for r in results:
            print(r.replace("\n", " ")) # Ensure one line per result
    else:
        input_text = sys.argv[1]
        try:
            tokenizer = AutoTokenizer.from_pretrained("protonx-models/protonx-legal-tc")
            model = AutoModelForSeq2SeqLM.from_pretrained("protonx-models/protonx-legal-tc")
            device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
            model.to(device)
            
            inputs = tokenizer(input_text, return_tensors="pt", truncation=True, max_length=256).to(device)
            with torch.no_grad():
                outputs = model.generate(**inputs, num_beams=3, max_new_tokens=256)
            print(tokenizer.decode(outputs[0], skip_special_tokens=True))
        except Exception as e:
            sys.stderr.write(str(e))
            sys.exit(1)
