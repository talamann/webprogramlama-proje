import os
from deep_translator import GoogleTranslator
from transformers import AutoTokenizer, AutoModelForSequenceClassification
import torch

# Model ve tokenizer'ı bir kez yükle
model_name = "roberta-base-openai-detector"
tokenizer = AutoTokenizer.from_pretrained(model_name)
model = AutoModelForSequenceClassification.from_pretrained(model_name)

# Metin dosyalarının bulunduğu klasör
folder_path = r"metinler"

# Tüm .txt dosyalarını sırayla işle
for filename in os.listdir(folder_path):
    if filename.endswith(".txt"):
        file_path = os.path.join(folder_path, filename)
        
        with open(file_path, "r", encoding="utf-8") as f:
            turkish_text = f.read().strip()

        print(f"File: {filename}")
        print(f"Original (TR): {turkish_text}")

        try:
            # Türkçeyi İngilizceye çevir
            english_text = GoogleTranslator(source='tr', target='en').translate(turkish_text)
            print(f"ingilizceye çevrildi (EN): {english_text}")

            # Tokenize et
            inputs = tokenizer(english_text, return_tensors="pt", truncation=True)

            # Tahmin yap
            with torch.no_grad():
                outputs = model(**inputs)

            # Olasılıkları hesapla
            probs = torch.softmax(outputs.logits, dim=1)
            human_prob = probs[0][0].item()
            ai_prob = probs[0][1].item()

            print(f"🤖 AI ile yazilmis olma olasligi: {ai_prob:.4f}")
            print(f"👤 insan tarafından yazilmis olma olasiligi: {human_prob:.4f}")

        except Exception as e:
            print(f"⚠️ Error processing {filename}: {e}")
