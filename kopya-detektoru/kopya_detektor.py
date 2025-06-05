import os
import json
from flask import Flask, request, jsonify
from sentence_transformers import SentenceTransformer, util
from deep_translator import GoogleTranslator
from transformers import AutoTokenizer, AutoModelForSequenceClassification
import torch
from werkzeug.utils import secure_filename
import tempfile
import shutil

app = Flask(__name__)
app.config['MAX_CONTENT_LENGTH'] = 16 * 1024 * 1024  # 16MB max file size

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
        return {"error": "At least 2 text files are required for comparison"}
    
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
                        "hata": "File is empty"
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

@app.route('/plagiarism-check', methods=['POST'])
def plagiarism_check():
    """
    Check for plagiarism between uploaded text files
    Accepts multiple .txt files
    """
    try:
        if 'files' not in request.files:
            return jsonify({"error": "No files provided"}), 400
        
        files = request.files.getlist('files')
        
        if len(files) < 2:
            return jsonify({"error": "At least 2 files are required for plagiarism check"}), 400
        
        # Create temporary directory
        with tempfile.TemporaryDirectory() as temp_dir:
            # Save uploaded files
            for file in files:
                if file.filename == '':
                    continue
                if file and file.filename.endswith('.txt'):
                    filename = secure_filename(file.filename)
                    file.save(os.path.join(temp_dir, filename))
            
            # Check if we have enough files
            txt_files = [f for f in os.listdir(temp_dir) if f.endswith('.txt')]
            if len(txt_files) < 2:
                return jsonify({"error": "At least 2 valid .txt files are required"}), 400
            
            # Run plagiarism check
            result = kopya_kontrol(temp_dir)
            
            if isinstance(result, dict) and "error" in result:
                return jsonify(result), 400
            
            return jsonify({
                "status": "success",
                "files_processed": len(txt_files),
                "comparisons": result
            })
    
    except Exception as e:
        return jsonify({"error": f"An error occurred: {str(e)}"}), 500

@app.route('/ai-detection', methods=['POST'])
def ai_detection():
    """
    Check if uploaded text files contain AI-generated content
    Accepts multiple .txt files
    """
    try:
        if 'files' not in request.files:
            return jsonify({"error": "No files provided"}), 400
        
        files = request.files.getlist('files')
        
        if not files:
            return jsonify({"error": "No files provided"}), 400
        
        # Create temporary directory
        with tempfile.TemporaryDirectory() as temp_dir:
            # Save uploaded files
            saved_files = 0
            for file in files:
                if file.filename == '':
                    continue
                if file and file.filename.endswith('.txt'):
                    filename = secure_filename(file.filename)
                    file.save(os.path.join(temp_dir, filename))
                    saved_files += 1
            
            if saved_files == 0:
                return jsonify({"error": "No valid .txt files provided"}), 400
            
            # Run AI detection
            result = ai_kontrol(temp_dir)
            
            return jsonify({
                "status": "success",
                "files_processed": saved_files,
                "results": result
            })
    
    except Exception as e:
        return jsonify({"error": f"An error occurred: {str(e)}"}), 500

@app.route('/analyze-text', methods=['POST'])
def analyze_text():
    """
    Analyze a single text directly (without file upload)
    Accepts JSON with 'text' field
    """
    try:
        data = request.get_json()
        
        if not data or 'text' not in data:
            return jsonify({"error": "No text provided. Send JSON with 'text' field"}), 400
        
        text = data['text'].strip()
        
        if not text:
            return jsonify({"error": "Text cannot be empty"}), 400
        
        # Translate to English for AI detection
        english_text = GoogleTranslator(source='tr', target='en').translate(text)
        inputs = tokenizer(english_text, return_tensors="pt", truncation=True, max_length=512)
        
        with torch.no_grad():
            outputs = ai_model(**inputs)
        
        probs = torch.softmax(outputs.logits, dim=1)
        human_prob = probs[0][0].item()
        ai_prob = probs[0][1].item()
        
        return jsonify({
            "status": "success",
            "original_text_length": len(text),
            "translated_text_length": len(english_text),
            "insan_olasilik": round(human_prob, 4),
            "ai_olasilik": round(ai_prob, 4),
            "prediction": "AI-generated" if ai_prob > human_prob else "Human-written"
        })
    
    except Exception as e:
        return jsonify({"error": f"An error occurred: {str(e)}"}), 500

@app.route('/compare-texts', methods=['POST'])
def compare_texts():
    """
    Compare similarity between two texts directly
    Accepts JSON with 'text1' and 'text2' fields
    """
    try:
        data = request.get_json()
        
        if not data or 'text1' not in data or 'text2' not in data:
            return jsonify({"error": "Both 'text1' and 'text2' fields are required"}), 400
        
        text1 = data['text1'].strip()
        text2 = data['text2'].strip()
        
        if not text1 or not text2:
            return jsonify({"error": "Both texts must be non-empty"}), 400
        
        # Encode texts and calculate similarity
        vectors = similarity_model.encode([text1, text2], convert_to_tensor=True)
        similarity = util.cos_sim(vectors[0], vectors[1]).item()
        
        return jsonify({
            "status": "success",
            "text1_length": len(text1),
            "text2_length": len(text2),
            "benzerlik": round(similarity * 100, 2),
            "similarity_level": "High" if similarity > 0.8 else "Medium" if similarity > 0.5 else "Low"
        })
    
    except Exception as e:
        return jsonify({"error": f"An error occurred: {str(e)}"}), 500

@app.errorhandler(413)
def too_large(e):
    return jsonify({"error": "File too large. Maximum size is 16MB"}), 413

@app.errorhandler(404)
def not_found(e):
    return jsonify({"error": "Endpoint not found"}), 404

@app.errorhandler(500)
def internal_error(e):
    return jsonify({"error": "Internal server error"}), 500

if __name__ == "__main__":
    print("Starting Text Analysis API...")
    print("Available endpoints:")
    print("- GET  /health - Health check")
    print("- POST /plagiarism-check - Upload multiple .txt files for plagiarism check")
    print("- POST /ai-detection - Upload multiple .txt files for AI detection")
    print("- POST /analyze-text - Analyze single text via JSON")
    print("- POST /compare-texts - Compare two texts via JSON")
    
    app.run(debug=True, host='0.0.0.0', port=5000)


 
