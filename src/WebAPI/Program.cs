// ===========================================================================
// Program.cs: Uygulamanın BAŞLANGIÇ NOKTASI.
//
// İki bölümden oluşur:
//   1) builder.Services...  -> Uygulamanın İHTİYAÇLARINI kaydet (servisler)
//   2) app.Use... / Map...  -> Gelen HTTP isteğinin GEÇECEĞİ YOLU kur (pipeline)
//
// İş kuralları burada YOK. Onlar Business katmanında.
// Burası sadece parçaları birbirine bağlayan "kontrol paneli".
// ===========================================================================

using Business.DependencyResolvers;
using WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// 1) SERVİSLER
// ---------------------------------------------------------------------------
builder.Services.AddControllers();               // Controllers klasöründeki sınıfları bul
builder.Services.AddEndpointsApiExplorer();      // Swagger için endpoint listesini çıkar
builder.Services.AddSwaggerGen();                // Swagger dokümanını oluştur

// Business, DataAccess, PostgreSQL ve Redis kayıtlarının hepsi bu tek satırda.
// İçini görmek için: Business/DependencyResolvers/ServiceRegistration.cs
builder.Services.AddBusinessServices(builder.Configuration);

var app = builder.Build();

// ---------------------------------------------------------------------------
// 2) BAŞLANGIÇ İŞLERİ
// ---------------------------------------------------------------------------
// Veritabanı tablolarını oluştur veya güncelle (EF Core migration).
// İçini görmek için: WebAPI/Extensions/DatabaseExtensions.cs
await app.MigrateDatabaseAsync();

// ---------------------------------------------------------------------------
// 3) PIPELINE: Her HTTP isteği yukarıdan aşağıya bu adımlardan geçer
// ---------------------------------------------------------------------------
// Swagger normalde sadece Development ortamında açılır. Öğrenme projesi
// olduğu için Docker'da (Production) da açık bırakıyoruz.
app.UseSwagger();                                // /swagger -> API dokümanı ve test sayfası
app.UseSwaggerUI();

app.UseDefaultFiles();                           // "/" istenince wwwroot/index.html'i ver
app.UseStaticFiles();                            // wwwroot klasöründeki dosyaları (HTML, CSS, JS) sun

app.MapControllers();                            // /api/notes gibi adresleri controller'lara yönlendir

app.Run();
