import os
from sentence_transformers import SentenceTransformer, util

# MODELİ YÜKLE
model = SentenceTransformer('sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2')

# .txt dosyalarının bulunduğu klasör
klasor_yolu = "metinler"
dosya_metinleri = {}

# TÜM .txt DOSYALARINI OKU
for dosya_adi in os.listdir(klasor_yolu):
    if dosya_adi.endswith(".txt"):
        yol = os.path.join(klasor_yolu, dosya_adi)
        with open(yol, "r", encoding="utf-8") as f:
            icerik = f.read()
            dosya_metinleri[dosya_adi] = icerik

# METİNLERİ VEKTÖRE DÖNÜŞTÜR
dosya_adlari = list(dosya_metinleri.keys())
metinler = list(dosya_metinleri.values())
vektorler = model.encode(metinler, convert_to_tensor=True)

# TÜM ÇİFTLERİ KARŞILAŞTIR VE BENZERLİK HESAPLA
print("\n📊 Kopya Benzerlik Sonuçları:\n")
for i in range(len(dosya_adlari)):
    for j in range(i + 1, len(dosya_adlari)):
        sim = util.cos_sim(vektorler[i], vektorler[j]).item()
        print(f"{dosya_adlari[i]} ↔ {dosya_adlari[j]} : %{sim * 100:.2f} benzerlik")
