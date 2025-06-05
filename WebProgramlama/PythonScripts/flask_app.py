import os
from flask import Flask, request, jsonify
from flask_cors import CORS  # ADD THIS LINE
from sentence_transformers import SentenceTransformer, util
from deep_translator import GoogleTranslator
from transformers import AutoTokenizer, AutoModelForSequenceClassification
import torch

app = Flask(__name__)
CORS(app)  # ADD THIS LINE

# Load models at startup
print("Loading models...")
similarity_model = SentenceTransformer('sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2')

ai_model_name = "roberta-base-openai-detector"
tokenizer = AutoTokenizer.from_pretrained(ai_model_name)
ai_model = AutoModelForSequenceClassification.from_pretrained(ai_model_name)
print("Models loaded successfully!")

def kopya_kontrol(klasor_yolu):
    """Check for plagiarism between text files"""
    dosya_metinleri = {}
    
    for dosya_adi in os.listdir(klasor_yolu):
        if dosya_adi.endswith(".txt"):
            yol = os.path.join(klasor_yolu, dosya_adi)
            with open(yol, "r", encoding="utf-8") as f:
                dosya_metinleri[dosya_adi] = f.read()
    
    if len(dosya_metinleri) < 2:
        return {"hata": "En az 2 txt dosyası gerekli"}
    
    dosya_adlari = list(dosya_metinleri.keys())
    metinler = list(dosya_metinleri.values())
    vektorler = similarity_model.encode(metinler, convert_to_tensor=True)
    
    benzerlik_sonuclari = []
    for i in range(len(dosya_adlari)):
        for j in range(i + 1, len(dosya_adlari)):
            sim = util.cos_sim(vektorler[i], vektorler[j]).item()
            benzerlik_sonuclari.append({
                "dosya1": dosya_adlari[i],
                "dosya2": dosya_adlari[j],
                "benzerlik": round(sim * 100, 2)
            })
    
    return benzerlik_sonuclari

def ai_kontrol(klasor_yolu):
    """Check if text files contain AI-generated content"""
    sonuclar = []
    
    for filename in os.listdir(klasor_yolu):
        if filename.endswith(".txt"):
            file_path = os.path.join(klasor_yolu, filename)
            
            try:
                with open(file_path, "r", encoding="utf-8") as f:
                    turkish_text = f.read().strip()
                
                if not turkish_text:
                    sonuclar.append({
                        "dosya": filename,
                        "hata": "Dosya boş"
                    })
                    continue
                
                english_text = GoogleTranslator(source='tr', target='en').translate(turkish_text)
                inputs = tokenizer(english_text, return_tensors="pt", truncation=True, max_length=512)
                
                with torch.no_grad():
                    outputs = ai_model(**inputs)
                
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

@app.route('/health', methods=['GET'])
def health_check():
    """Health check endpoint"""
    return jsonify({"status": "healthy", "message": "API is running"})

@app.route('/kopya', methods=['POST'])
def kopya_api():
    """Plagiarism check endpoint"""
    try:
        data = request.get_json()
        klasor_yolu = data.get("klasor_yolu")
        
        # ADD THESE LINES FOR DOCKER PATH MAPPING
        if klasor_yolu and os.getenv('DOCKER_ENV'):  # Only in Docker
            if '\\' in klasor_yolu:
                klasor_yolu = klasor_yolu.replace('\\', '/')
            if 'Assignments' in klasor_yolu and not klasor_yolu.startswith('/app'):
                parts = klasor_yolu.split('Assignments')
                if len(parts) > 1:
                    assignment_part = parts[1].strip('/')
                    klasor_yolu = f'/app/Assignments/{assignment_part}'
        
        if not klasor_yolu or not os.path.isdir(klasor_yolu):
            return jsonify({"hata": "Geçerli klasör yolu girilmedi."}), 400
        
        sonuc = kopya_kontrol(klasor_yolu)
        
        if isinstance(sonuc, dict) and "hata" in sonuc:
            return jsonify(sonuc), 400
        
        return jsonify({
            "durum": "basarili",
            "sonuclar": sonuc
        })
        
    except Exception as e:
        return jsonify({"hata": f"Bir hata oluştu: {str(e)}"}), 500

@app.route('/ai', methods=['POST'])
def ai_api():
    """AI detection endpoint"""
    try:
        data = request.get_json()
        klasor_yolu = data.get("klasor_yolu")
        
        # ADD THESE LINES FOR DOCKER PATH MAPPING
        if klasor_yolu and os.getenv('DOCKER_ENV'):  # Only in Docker
            if '\\' in klasor_yolu:
                klasor_yolu = klasor_yolu.replace('\\', '/')
            if 'Assignments' in klasor_yolu and not klasor_yolu.startswith('/app'):
                parts = klasor_yolu.split('Assignments')
                if len(parts) > 1:
                    assignment_part = parts[1].strip('/')
                    klasor_yolu = f'/app/Assignments/{assignment_part}'
        
        if not klasor_yolu or not os.path.isdir(klasor_yolu):
            return jsonify({"hata": "Geçerli klasör yolu girilmedi."}), 400
        
        sonuc = ai_kontrol(klasor_yolu)
        
        return jsonify({
            "durum": "basarili",
            "sonuclar": sonuc
        })
        
    except Exception as e:
        return jsonify({"hata": f"Bir hata oluştu: {str(e)}"}), 500

@app.errorhandler(404)
def not_found(e):
    return jsonify({"hata": "Endpoint bulunamadı"}), 404

@app.errorhandler(500)
def internal_error(e):
    return jsonify({"hata": "Sunucu hatası"}), 500

if __name__ == '__main__':
    print("Starting Flask API...")
    print("Available endpoints:")
    print("- GET  /health - Health check")
    print("- POST /kopya - Plagiarism check")
    print("- POST /ai - AI detection")
    # CHANGE THIS LINE FOR DOCKER
    app.run(debug=True, host='0.0.0.0', port=5000)  # Changed to 0.0.0.0 for Docker