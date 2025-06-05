 


1. Presentation Layer: UI Framework Using : Razor , HTML ,bootstrap ,tailwind Muharrem
Emirhan Taşcı 
Bu katman, kullanıcının sistemle etkileşim kurduğu arayüzü içerir. Razor syntax'ı kullanılarak dinamik sayfalar oluşturulmuştur. HTML temel yapıyı sağlarken, görsel tasarım ve responsive yapı için Bootstrap ve Tailwind CSS birlikte kullanılmıştır. Kullanıcı dostu bir arayüz oluşturulması hedeflenmiş, tüm cihazlarla uyumlu tasarımlar yapılmıştır.

2. Business Layer: OOP Components: C#, kullanıcı türleri sınıf
olacak(Öğrenci,Öğretmen,Admin) Talha Emre Ünal
İş katmanı, uygulamanın iş kurallarını ve mantığını içerir. Nesne yönelimli programlama (OOP) prensiplerine uygun olarak geliştirilen bu katmanda, Kullanici adlı bir temel sınıf oluşturulmuş ve bu sınıftan türeyen Ogrenci, Ogretmen, ve Admin sınıfları ile kullanıcı türleri modellenmiştir. 

3. Data Layer: ORM / Migrations Using: Entity Framework Core , Migrations Muharrem
Emirhan Taşcı 
Kullanılan sınıflara uygun bir migration dosyası hazırlanmıştır. Daha sonra repositoryler kullanılarak business layer kısmının ORM'e bulaşmadan veritabanı ile etkileşimi sağlanmıştır.
4. Web Service Implementation: REST API (api/student/plagiarismcheck vb.) Talha Emre
Ünal 
Bu katmanda, sistem bileşenleri arasında veri alışverişini sağlayan RESTful API servisleri geliştirilmiştir. API, istemci tarafının (örneğin kullanıcı arayüzünün) ihtiyaç duyduğu verilere erişmesini sağlarken aynı zamanda dış sistemlerle entegrasyon imkanı da sunar.
Projemizde Controller ve Service vasıtası ile ai tarafı ve data tarafı ile iletişim kurulmuştur.

5. RBAC Implementation: ASP.NET’in RoleManager kütüphanesi Hacı Osman Gündoğdu
RBAC sistemi, kullanıcıların rollerine göre sistem kaynaklarına erişimini kontrol etmek amacıyla geliştirilmiştir. ASP.NET’in RoleManager kütüphanesi kullanılarak, kullanıcı rolleri (Öğrenci, Öğretmen, Admin) tanımlanmış ve yönetilmiştir. Her rolün sadece kendi yetki alanı dahilindeki işlemleri gerçekleştirebilmesi sağlanarak güvenli ve kontrollü bir erişim altyapısı oluşturulmuştur.

6. Authorization Implementation: ASP.NET’in RoleManager ve Authorize kütüphanesi Hacı
Osman Gündoğdu
Bu katmanda kullanıcıların yetkileri doğrultusunda sistem kaynaklarına erişimini sınırlayan yetkilendirme mekanizmaları geliştirilmiştir. ASP.NET’in RoleManager ve [Authorize] attribute'u birlikte kullanılarak belirli sayfalara veya API uç noktalarına sadece ilgili role sahip kullanıcıların erişmesine izin verilmiştir. Bu sayede uygulama güvenliği artırılmıştır.

7. Session / Cookie Management: asp.net httpcontext kütüphanesi Talha Emre Ünal
Kullanıcı oturum yönetimi ve veri sürekliliği için ASP.NET’in HttpContext yapısı kullanılmıştır. Giriş yapan kullanıcıların bilgileri Session ve Cookie'ler aracılığıyla saklanmış; bu bilgiler, kullanıcının oturumu süresince kimliğinin doğrulanması ve yetkilendirme işlemleri için kullanılmıştır. Ayrıca kullanıcı deneyimini iyileştirmek amacıyla çerezler ile oturum bilgisi yönetimi sağlanmıştır.

<br>

8. Extension / Third Party Library Using:   Ömer Faruk Günaydın <br>
Bu kısımda, Flask kütüphanesi kullanılarak oluşturulmuş bir REST API sunucusu yer almaktadır.<br>
Amaç, istemciden alınan klasör yolu üzerinden metin dosyalarına yönelik yapay zeka içerik analizi veya kopya tespiti işlemlerini web tabanlı olarak gerçekleştirmektir. flask kütüphanesi API oluşturmak için, os kütüphanesi ise klasör ve dosya işlemleri için kullanılmıştır. API, iki farklı uç noktaya (/kopya ve /ai) gelen POST isteklerini alır, gerekli analiz fonksiyonlarını çağırır ve sonuçları  geri döndürür. 

<br>

9. Web Security Implementation: HTTPS, ANTI-CSRF Hacı Osman Gündoğdu
Uygulamanın güvenliği, hem veri iletiminde hem de kullanıcı işlemlerinde sağlanmıştır.

HTTPS kullanılarak tüm veri iletimi şifrelenmiş ve kullanıcı ile sunucu arasındaki iletişim güvenli hale getirilmiştir.

Anti-CSRF (Cross-Site Request Forgery) önlemleri ASP.NET’in yerleşik @Html.AntiForgeryToken() ve [ValidateAntiForgeryToken] yapılarıyla uygulanmıştır. Bu sayede kötü niyetli sitelerin kullanıcı adına işlem gerçekleştirmesi engellenmiş ve form tabanlı işlemler güvence altına alınmıştır.

<br>

10. Cloud Service (AI) Using:   Ömer Faruk Günaydın <br>
Bu kısımda iki farklı Python dosyası yer almaktadır. İlk Python dosyası olan ai_kontrol.py, klasördeki .txt uzantılı dosyaları okuyarak içeriklerin yapay zeka tarafından yazılıp yazılmadığını tespit eder. Bunun için metinler önce deep_translator kütüphanesi ile Türkçeden İngilizceye çevrilir, ardından transformers ve torch kütüphaneleri kullanılarak OpenAI’nin roberta-base-openai-detector modeli ile analiz edilir. İkinci Python dosyası olan kopya_kontrol.py ise klasördeki .txt dosyaları arasında benzerlik analizi yapar. Bu işlemde sentence-transformers kütüphanesi kullanılarak metinler vektörlere dönüştürülür ve util.cos_sim fonksiyonu ile aralarındaki benzerlik oranı hesaplanır. <br>
Ana hedef, verilen metinlerin yapay zeka ürünü olup olmadığını ve dosyalar arasında kopya içerik bulunup bulunmadığını tespit etmektir.
<br>
<br>








