from flask import Flask, request, jsonify
from kopya_detektor import kopya_kontrol
from ai_kontrolu import ai_kontrol
import os

app = Flask(__name__)

@app.route('/kopya', methods=['POST'])
def kopya_api():
    data = request.get_json()
    klasor_yolu = data.get("klasor_yolu")

    if not klasor_yolu or not os.path.isdir(klasor_yolu):
        return jsonify({"hata": "Geçerli klasör yolu girilmedi."}), 400

    sonuc = kopya_kontrol(klasor_yolu)
    return jsonify(sonuc)

@app.route('/ai', methods=['POST'])
def ai_api():
    data = request.get_json()
    klasor_yolu = data.get("klasor_yolu")

    if not klasor_yolu or not os.path.isdir(klasor_yolu):
        return jsonify({"hata": "Geçerli klasör yolu girilmedi."}), 400

    sonuc = ai_kontrol(klasor_yolu)
    return jsonify(sonuc)

if __name__ == '__main__':
    app.run(debug=True)
