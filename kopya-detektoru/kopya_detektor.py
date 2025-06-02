import os
from sentence_transformers import SentenceTransformer, util

model = SentenceTransformer('sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2')

def kopya_kontrol(klasor_yolu):
    dosya_metinleri = {}

    for dosya_adi in os.listdir(klasor_yolu):
        if dosya_adi.endswith(".txt"):
            yol = os.path.join(klasor_yolu, dosya_adi)
            with open(yol, "r", encoding="utf-8") as f:
                dosya_metinleri[dosya_adi] = f.read()

    dosya_adlari = list(dosya_metinleri.keys())
    metinler = list(dosya_metinleri.values())
    vektorler = model.encode(metinler, convert_to_tensor=True)

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


 
