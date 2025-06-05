 


1. Presentation Layer: UI Framework Using : Razor , HTML ,bootstrap ,tailwind Muharrem
Emirhan Taşcı
2. Business Layer: OOP Components: C#, kullanıcı türleri sınıf
olacak(Öğrenci,Öğretmen,Admin) Talha Emre Ünal
3. Data Layer: ORM / Migrations Using: Entity Framework Core , Migrations Muharrem
Emirhan Taşcı
4. Web Service Implementation: REST API (api/student/plagiarismcheck vb.) Talha Emre
Ünal
5. RBAC Implementation: ASP.NET’in RoleManager kütüphanesi Hacı Osman Gündoğdu
6. Authorization Implementation: ASP.NET’in RoleManager ve Authorize kütüphanesi Hacı
Osman Gündoğdu
7. Session / Cookie Management: asp.net httpcontext kütüphanesi Talha Emre Ünal

<br>

8. Extension / Third Party Library Using:   Ömer Faruk Günaydın <br>
Bu kısımda, Flask kütüphanesi kullanılarak oluşturulmuş bir REST API sunucusu yer almaktadır.<br>
Amaç, istemciden alınan klasör yolu üzerinden metin dosyalarına yönelik yapay zeka içerik analizi veya kopya tespiti işlemlerini web tabanlı olarak gerçekleştirmektir. flask kütüphanesi API oluşturmak için, os kütüphanesi ise klasör ve dosya işlemleri için kullanılmıştır. API, iki farklı uç noktaya (/kopya ve /ai) gelen POST isteklerini alır, gerekli analiz fonksiyonlarını çağırır ve sonuçları  geri döndürür. 

<br>

9. Web Security Implementation: HTTPS, ANTI-CSRF Hacı Osman Gündoğdu

<br>

10. Cloud Service (AI) Using:   Ömer Faruk Günaydın <br>
Bu kısımda iki farklı Python dosyası yer almaktadır. İlk Python dosyası olan ai_kontrol.py, klasördeki .txt uzantılı dosyaları okuyarak içeriklerin yapay zeka tarafından yazılıp yazılmadığını tespit eder. Bunun için metinler önce deep_translator kütüphanesi ile Türkçeden İngilizceye çevrilir, ardından transformers ve torch kütüphaneleri kullanılarak OpenAI’nin roberta-base-openai-detector modeli ile analiz edilir. İkinci Python dosyası olan kopya_kontrol.py ise klasördeki .txt dosyaları arasında benzerlik analizi yapar. Bu işlemde sentence-transformers kütüphanesi kullanılarak metinler vektörlere dönüştürülür ve util.cos_sim fonksiyonu ile aralarındaki benzerlik oranı hesaplanır. <br>
Ana hedef, verilen metinlerin yapay zeka ürünü olup olmadığını ve dosyalar arasında kopya içerik bulunup bulunmadığını tespit etmektir.
<br>
<br>








