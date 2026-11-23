# 📝 Not Defteri API

**ASP.NET Core 8**, **Entity Framework Core**, **PostgreSQL** ve **Redis** ile yazılmış, N-katmanlı mimariye sahip bir not uygulaması (REST API). Docker ile çalışır.

<!-- CI kurulunca buraya durum rozeti (badge) eklenecek -->

## Hızlı başlangıç

Gereken tek şey **Docker Desktop**.

```powershell
docker compose up -d --build
```

http://localhost:5000 adresini aç. **Swagger** sayfası açılır, tüm API'yi oradan deneyebilirsin.

```powershell
docker compose down        # durdur (veriler kalır)
docker compose down -v     # durdur ve verileri sil
```

## API

| Metot | Adres | Ne yapar |
|---|---|---|
| `GET` | `/api/notes` | Tüm notları listeler (Redis'te 1 dk önbelleklenir) |
| `GET` | `/api/notes/{id}` | Tek bir notu getirir |
| `POST` | `/api/notes` | Not ekler: `{ "title": "...", "content": "..." }` |
| `PUT` | `/api/notes/{id}` | Notu günceller |
| `DELETE` | `/api/notes/{id}` | Notu siler |
| `GET` | `/health` | `{"app":"ok","db":"ok","redis":"ok"}` |

Tüm cevaplar aynı kalıptadır:
```json
{ "data": { ... }, "success": true, "message": "Not eklendi" }
```

### Kurallar

| Alan | Kural |
|---|---|
| Başlık | Zorunlu, en fazla **100** karakter |
| İçerik | Zorunlu, en fazla **500** karakter |
| Boşluklar | Baştaki ve sondaki boşluklar silinir, aradaki çoklu boşluk teke iner |

## Mimari: N-katmanlı yapı

Bir istek, katmanlardan yukarıdan aşağıya geçer:

```
 HTTP isteği
     │
     ▼
┌─────────────┐  Controllers: isteği alır, cevabı döner (200, 400, 404...)
│   WebAPI    │  Program.cs: parçaları birbirine bağlar
└──────┬──────┘
       ▼
┌─────────────┐  NoteManager: iş kuralları, cache yönetimi
│  Business   │  NoteRules: doğrulama kuralları (test edilen yer)
└──────┬──────┘
       ▼
┌─────────────┐  EfNoteDal: veritabanı işlemleri
│ DataAccess  │  NotDefteriContext: EF Core ↔ PostgreSQL köprüsü, Migrations
└──────┬──────┘
       ▼
   PostgreSQL

┌─────────────┐  Note (tablo), NoteCreateDto (dışarıdan gelen veri)
│  Entities   │  Tüm katmanlar kullanır
└─────────────┘
┌─────────────┐  Her projede tekrar kullanılabilen altyapı:
│    Core     │  generic repository, Result sınıfları, cache arayüzü
└─────────────┘
```

**Temel kural:** Her katman sadece **altındaki** katmanı tanır. Katmanlar birbirini sınıf adıyla değil, **arayüz (interface)** üzerinden kullanır (`INoteService`, `INoteDal`, `ICacheService`). Hangi arayüze hangi sınıfın verileceği `Business/DependencyResolvers/ServiceRegistration.cs` dosyasında yazar (Dependency Injection).

## Geliştirme

Gereksinim: **.NET 8 SDK**

```powershell
dotnet build       # tüm katmanları derle
dotnet test        # birim testlerini çalıştır
```

Visual Studio'da çalıştırmak için önce veritabanı ve Redis'i Docker ile aç, sonra `WebAPI` projesini başlat (http://localhost:5001/swagger):

```powershell
docker compose up -d db redis
dotnet run --project src/WebAPI
```

### Veritabanı değişikliği (migration)

Bir entity'ye alan ekledikten sonra:

```powershell
dotnet tool install --global dotnet-ef      # bir kez
dotnet ef migrations add AlanEklendi --project src/DataAccess --startup-project src/WebAPI
```

Uygulama açılırken migration'lar otomatik uygulanır.

### Smoke test

Uygulamanın uçtan uca çalıştığını kontrol eder (Git Bash, Linux veya Mac'te):

```bash
bash scripts/smoke-test.sh http://localhost:5000
```

## Proje yapısı

```
DockerOgrenNet/
├── src/
│   ├── Core/                  Ortak altyapı (repository, Result, cache)
│   ├── Entities/              Note, NoteCreateDto
│   ├── DataAccess/            EF Core, NotDefteriContext, Migrations
│   ├── Business/              NoteManager, NoteRules, DI kayıtları
│   └── WebAPI/                Controllers, Program.cs, Dockerfile
├── tests/Business.Tests/      xUnit testleri (sahte veritabanı ile)
├── scripts/smoke-test.sh      Uçtan uca kontrol (CI ve CD kullanır)
├── docker-compose.yml         API + PostgreSQL + Redis
├── docker-compose.deploy.yml  Hazır image ile çalıştırma (CD)
├── docs/docker-ders/          Docker öğrenme materyalleri (önceki çalışma)
└── ogrenme/                   Git, GitHub ve CI/CD öğrenme şablonları
```
