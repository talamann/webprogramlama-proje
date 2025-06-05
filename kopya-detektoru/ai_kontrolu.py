import os
from deep_translator import GoogleTranslator
from transformers import AutoTokenizer, AutoModelForSequenceClassification
import torch

model_name = "roberta-base-openai-detector"
tokenizer = AutoTokenizer.from_pretrained(model_name)
model = AutoModelForSequenceClassification.from_pretrained(model_name)

def ai_kontrol(klasor_yolu):
    sonuclar = []

    for filename in os.listdir(klasor_yolu):
        if filename.endswith(".txt"):
            file_path = os.path.join(klasor_yolu, filename)

            try:
                with open(file_path, "r", encoding="utf-8") as f:
                    turkish_text = f.read().strip()

                english_text = GoogleTranslator(source='tr', target='en').translate(turkish_text)
                inputs = tokenizer(english_text, return_tensors="pt", truncation=True)

                with torch.no_grad():
                    outputs = model(**inputs)

                probs = torch.softmax(outputs.logits, dim=1)
                human_prob = probs[0][0].item()
                ai_prob = probs[0][1].item()

                sonuclar.append({
                    "dosya": filename,
                    "insan_olasilik": round(human_prob, 4),
                    "ai_olasilik": round(ai_prob, 4)
                })

            except Exception as e:
                sonuclar.append({
                    "dosya": filename,
                    "hata": str(e)
                })
 
    return sonuclar


 