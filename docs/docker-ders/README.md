# 🐳 Docker Öğren — .NET Not Defteri

Bu projede **uygulama kodu hazır** (.NET 8 Minimal API), ama **Docker ile ilgili tek bir dosya bile yok.**
`Dockerfile`, `.dockerignore`, multi-stage build, network, volume ve `docker-compose.yml` dosyalarının hepsini sen yazacaksın.

## Uygulama ne yapıyor?

- **Ziyaret sayacı** → **Redis**'te tutuluyor (StackExchange.Redis)
- **Not ekleme / silme** → **PostgreSQL**'de saklanıyor (Npgsql)
- Sayfanın üstünde **hangi container'dan sunulduğu** yazıyor

Sayfada Redis ve PostgreSQL için birer kart var. Bağlantı yoksa kart kırmızı olup hatayı gösteriyor, bağlantı varsa yeşil oluyor. Her adımda neyin eksik olduğunu bu kartlardan anlayacaksın.

```
DockerOgrenNet/
├── README.md                  ← şu an buradasın
├── DockerOgrenNet.sln         ← Visual Studio / Rider ile aç
├── src/
│   └── NotDefteri/
│       ├── NotDefteri.csproj
│       ├── Program.cs         ← uygulama (hazır, dokunma)
│       ├── appsettings.json   ← bağlantı ayarları
│       ├── Properties/launchSettings.json
│       ├── sifreler.txt       ← "gizli" bilgiler (image'a GİRMEMELİ!)
│       └── debug.log          ← çöp dosya (image'a GİRMEMELİ!)
└── cozumler/                  ← takılırsan bak, ama önce kendin dene 🙂
```

## .NET'te ayarlar ortam değişkeniyle nasıl ezilir?

`appsettings.json` içinde şöyle bir yapı var:
```json
"ConnectionStrings": {
  "Postgres": "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=notlar",
  "Redis": "localhost:6379"
}
```
Docker'da bu değerleri değiştirmek için `appsettings.json` dosyasına dokunmazsın, **ortam değişkeni** verirsin.
JSON'daki `:` (iç içe yapı) ortam değişkeninde **iki alt çizgi** `__` olur:

| appsettings.json yolu | Ortam değişkeni |
|---|---|
| `ConnectionStrings:Postgres` | `ConnectionStrings__Postgres` |
| `ConnectionStrings:Redis` | `ConnectionStrings__Redis` |
| `AppName` | `AppName` |

Ortam değişkeni her zaman `appsettings.json` değerini **ezer**. Bu bütün .NET projelerinde geçerli bir kural, Docker'a özel değil.

> 💡 Komutlar **PowerShell** için yazıldı. Hepsini `DockerOgrenNet` klasörünün içinde çalıştır.
> Docker Desktop açık olmalı: `docker run hello-world`

---

## 🔹 Görev 0 — Uygulamayı Docker'sız tanı (isteğe bağlı)

`DockerOgrenNet.sln` dosyasını Visual Studio'da açıp çalıştır (ya da `dotnet run --project src/NotDefteri` komutunu kullan).
http://localhost:5000 adresinde sayfa açılacak, iki kart da **kırmızı** olacak, çünkü bilgisayarında Redis ve Postgres çalışmıyor.
Amaç bu ikisini bilgisayarına hiç kurmadan, Docker ile ayağa kaldırmak.

---

## ✅ Görev 1 — İlk Dockerfile (tek aşamalı)

**Amaç:** Uygulamayı bir image haline getirip container olarak çalıştırmak.

**Yapacakların:**
1. `src/NotDefteri/` klasörüne `Dockerfile` adında bir dosya oluştur (uzantısı olmayacak).
2. Dosyanın içinde sırasıyla şunlar olmalı:
   - Temel image olarak `mcr.microsoft.com/dotnet/sdk:8.0` kullan (içinde .NET SDK var)
   - Çalışma klasörünü `/src` yap
   - Önce **sadece** `NotDefteri.csproj` dosyasını kopyala ve `dotnet restore` çalıştır
   - Ardından kodun geri kalanını kopyala
   - `dotnet publish -c Release -o /app` ile derle
   - Container açıldığında `dotnet /app/NotDefteri.dll` çalışsın (`ENTRYPOINT` kullan)
3. Build et:
   ```powershell
   docker build -t not-defteri:1.0 ./src/NotDefteri
   ```
4. Çalıştır:
   ```powershell
   docker run --rm -p 5000:5000 --name not-app not-defteri:1.0
   ```
5. Tarayıcıda http://localhost:5000 adresini aç.

**🪤 Tuzak:** Büyük ihtimalle sayfa **açılmayacak**. Bu beklenen bir durum.
Terminaldeki loglarda `Now listening on: ...` satırını bul. Uygulama hangi portu dinliyor?
(Bilgi: Microsoft'un .NET 8 image'ları `ASPNETCORE_HTTP_PORTS=8080` ayarıyla geliyor. Yani `launchSettings.json` container içinde **kullanılmıyor**.)
Bunu iki farklı şekilde çözebilirsin. İkisini de bul:
- a) `docker run` komutunu değiştirerek
- b) Dockerfile'a bir `ENV` satırı ekleyerek

**Başardığını nasıl anlarsın:** Sayfa açılıyor, iki kart da **kırmızı**. Bu normal, çünkü Redis ve Postgres'i henüz çalıştırmadık.

**Düşün:**
- `.csproj` dosyasını neden kodun geri kalanından **ayrı ve önce** kopyalayıp `restore` ettik?
- `RUN dotnet publish` ile `ENTRYPOINT ["dotnet", ...]` arasındaki fark ne?

---

## ✅ Görev 2 — .dockerignore (.NET'te neden şart?)

**Amaç:** Image'a gizli ve gereksiz dosyaların girmesini engellemek. .NET'te bin/obj klasörlerinin neden sorun çıkardığını anlamak.

**Yapacakların:**
1. Image'ın içinde neler olduğuna bak. Bunun için ENTRYPOINT'i geçici olarak `ls` ile değiştiriyoruz:
   ```powershell
   docker run --rm --entrypoint ls not-defteri:1.0 -la /src
   ```
   `sifreler.txt` ve `debug.log` dosyalarını görüyor musun? 😬 Görev 0'ı yaptıysan `bin` ve `obj` klasörleri de orada olacak.
2. `src/NotDefteri/` klasörüne bir `.dockerignore` dosyası oluştur. Şunlar image'a **girmemeli**: `bin/`, `obj/`, `sifreler.txt`, `.env`, `*.log`. (`.env` henüz projede yok, ama gerçek projelerde hep olur, şimdiden ekle.)
3. Tekrar build et ve 1. adımdaki komutla kontrol et.
4. **Cache deneyi:** `Program.cs` içindeki herhangi bir yorum satırını değiştirip build et.
   Çıktıda `dotnet restore` adımının yanında **CACHED** yazıyor mu?
5. Şimdi `.csproj` dosyasına boş bir satır ekleyip build et. Bu sefer ne oldu?

**Düşün — .NET'e özel bir konu:**
`obj/project.assets.json` dosyası, NuGet paketlerinin **senin bilgisayarındaki** yollarını tutuyor (örneğin `C:\Users\...\.nuget\packages`).
Bu dosya Linux container'ının içine kopyalanırsa ne olur? `.dockerignore` olmadan `bin/obj` klasörlerini kopyalamak neden tehlikeli?

---

## ✅ Görev 3 — Multi-stage build (image'ı küçült)

**Amaç:** Derlemek için gereken SDK'yı (~800 MB) son image'a taşımamak.

**Önce bak:**
```powershell
docker images not-defteri
```
Image'ın boyutu ne kadar? Bu image'ın içinde derleyici, NuGet ve SDK'nın tamamı var. Uygulamayı **çalıştırmak** için bunların hiçbirine ihtiyaç yok.

**Yapacakların:** Dockerfile'ı **iki aşamaya** böl:
- **1. aşama (`AS build`):** `sdk:8.0` image'ını kullan. restore ve publish burada yapılsın, çıktı `/app/publish` klasörüne gitsin.
- **2. aşama:** `mcr.microsoft.com/dotnet/aspnet:8.0` image'ını kullan (sadece runtime, SDK yok).
  1. aşamadaki `/app/publish` klasörünü buraya kopyala (`COPY --from=build ...`) ve `ENTRYPOINT` tanımla.

Bu sefer `not-defteri:2.0` tag'iyle build et ve iki image'ı karşılaştır:
```powershell
docker build -t not-defteri:2.0 ./src/NotDefteri
docker images not-defteri
```
Ne kadar küçüldü? 🎉

**Düşün:**
- Son image'ın içinde `Program.cs` dosyası var mı? Kontrol et: `docker run --rm --entrypoint ls not-defteri:2.0 -la /app`
- 2.0 image'ında `sifreler.txt` dosyası var mı? Var ya da yok, neden? Bu durumda `.dockerignore` gereksiz mi oldu? (İpucu: Build aşaması da bir image katmanı.)

> Bundan sonraki görevlerde `not-defteri:2.0` image'ını kullan.

---

## ✅ Görev 4 — Network: container'ları elle bağlamak

**Amaç:** Üç container'ı Compose kullanmadan çalıştırıp birbirine bağlamak. Compose'un arka planda ne yaptığını görmek için önce bunu elle yapacaksın.

**Yapacakların:**
1. `notnet` adında bir network oluştur.
2. Redis'i bu network'te `redis` adıyla çalıştır (`redis:7-alpine` image'ı, arka planda).
3. PostgreSQL'i bu network'te `db` adıyla çalıştır (`postgres:16` image'ı, arka planda).
   Postgres image'ı şu ortam değişkenlerini ister: `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`.
   Değerler `appsettings.json` içindeki bağlantı cümlesiyle aynı olsun.
4. Uygulamayı da aynı network'te çalıştır ve iki bağlantı cümlesini **ortam değişkeniyle** ver
   (yukarıdaki `__` tablosuna bak). Bağlantı cümlesinde `;` olduğu için değeri çift tırnak içinde yaz:
   ```powershell
   -e "ConnectionStrings__Postgres=Host=???;Port=5432;Username=postgres;Password=postgres;Database=notlar"
   ```
   **Soru:** `???` yerine ne yazmalısın? `localhost` mu, yoksa başka bir şey mi?
5. http://localhost:5000 → iki kart da **yeşil** olmalı. Birkaç not ekle.

**Faydalı komutlar:**
```powershell
docker ps
docker logs db
docker network inspect notnet
docker exec -it db psql -U postgres -d notlar -c "SELECT * FROM notes;"
docker exec -it redis redis-cli GET ziyaret_sayisi
```

**Düşün:**
- Postgres için `-p 5432:5432` vermedin. Uygulama yine de nasıl bağlanabildi?
- O zaman `-p` tam olarak **kimin** için gerekli?
- Visual Studio'dan (Docker dışından) bu Postgres'e bağlanmak isteseydin ne yapman gerekirdi?

---

## ✅ Görev 5 — Volume: verileri kaybetmemek

**Amaç:** Container silindiğinde verilerin neden kaybolduğunu görmek ve volume ile bunu çözmek.

**Yapacakların:**
1. Görev 4'te not eklemiştin. Şimdi **db** container'ını sil: `docker rm -f db`
2. `db`'yi Görev 4'teki komutla tekrar başlat ve sayfayı yenile. Notların nerede? 😱
3. `db`'yi yine sil. Bu sefer `pgdata` adında bir **named volume** ile başlat.
   Postgres verilerini `/var/lib/postgresql/data` klasöründe tutuyor.
4. Not ekle → `db`'yi sil → aynı volume ile tekrar başlat → notlar **hâlâ yerinde** olmalı. 🎉
5. İncele: `docker volume ls` ve `docker volume inspect pgdata`

**Düşün:**
- Redis container'ı silinince ziyaret sayacı sıfırlanıyor mu? Bu bir sorun mu?
- Named volume ile bind mount arasındaki fark ne? Postgres için hangisi daha uygun?

**Temizlik (Görev 6'dan önce yap):**
```powershell
docker rm -f not-app db redis
docker network rm notnet
```

---

## ✅ Görev 6 — Hepsini Compose'a taşı

**Amaç:** Görev 4 ve 5'te yazdığın uzun komutların hepsini tek bir dosyaya taşımak.

**Yapacakların:**
1. **Proje kök klasörüne** (`.sln` dosyasının yanına) `docker-compose.yml` dosyası oluştur.
2. İçinde üç servis olmalı: `app`, `db`, `redis`.
   - `app`: `./src/NotDefteri` klasöründen **build** edilsin, port eşlemesi yapılsın (Görev 1'deki tuzağı unutma!), bağlantı cümleleri ortam değişkeniyle verilsin, `depends_on` ile `db` ve `redis`'e bağlansın
   - `db`: `postgres:16`, ortam değişkenleri ve `pgdata` volume'u
   - `redis`: `redis:7-alpine`
3. En altta `pgdata` volume'unu tanımla.
4. Çalıştır:
   ```powershell
   docker compose up -d --build
   docker compose ps
   docker compose logs -f app
   ```

**Dikkat et:** Network oluşturmadın, ama container'lar birbirini buluyor. Neden? `docker network ls` ile kontrol et.

**Deneyler:**
```powershell
docker compose down          # sonra tekrar up → notlar duruyor mu?
docker compose down -v       # sonra tekrar up → şimdi ne oldu? Neden?
docker compose up -d --scale app=3   # hata verecek, neden?
```

> 💡 Visual Studio'da projeye sağ tıklayıp **Add → Docker Support** dediğinde VS de aslında bu dosyaları üretiyor. Bunları elle yazmayı öğrendikten sonra VS'nin ürettiklerini rahatça okuyabileceksin.

---

## ✅ Görev 7 — Geliştirme modu: bind mount + `dotnet watch`

**Amaç:** Kodu her değiştirdiğinde image'ı yeniden build etmek zorunda kalmamak.

**Durum:** `Program.cs` içinde sayfa başlığındaki 🐳 emojisini değiştirip sayfayı yenile. Değişiklik görünmüyor, çünkü image'ın içinde **derlenmiş DLL** var.
Python gibi dillerde kod klasörünü bağlamak yeterli olur, ama .NET'te kodun **derlenmesi** gerekiyor. Bu yüzden geliştirme sırasında container'da SDK ve `dotnet watch` kullanacağız.

**Yapacakların:** `docker-compose.yml` dosyasındaki `app` servisini geliştirme için şöyle değiştir:
1. `build` yerine doğrudan `image: mcr.microsoft.com/dotnet/sdk:8.0` kullan
2. `./src/NotDefteri` klasörünü container'da `/src` olarak bağla (**bind mount**) ve `working_dir: /src` yap
3. `command: dotnet watch run --no-launch-profile`
4. Ortam değişkenleri ekle:
   - `ASPNETCORE_ENVIRONMENT: Development`
   - `DOTNET_USE_POLLING_FILE_WATCHER: "1"` → Windows'tan bağlanan klasörlerde dosya değişikliklerini yakalamak için gerekli
   - `DOTNET_WATCH_RESTART_ON_RUDE_EDIT: "1"` → hot reload yapılamayan değişikliklerde soru sormadan yeniden başlatır
5. **.NET tuzağı:** Bilgisayarındaki `bin/obj` klasörleri (Windows'a göre derlenmiş) container'daki klasörlerin üzerine gelmemeli.
   Bunun için `/src/bin` ve `/src/obj` yollarına **anonim volume** ekle (sadece container içi yolu yazılır, sol taraf olmaz).
6. `docker compose up -d` ile başlat, `docker compose logs -f app` ile `dotnet watch`'ı izle ve `Program.cs`'te bir değişiklik yap.

**Düşün:**
- `--no-launch-profile` neden gerekli? (İpucu: `launchSettings.json` hangi adresi dinliyor?)
- Container her başladığında `dotnet restore` paketleri yeniden indiriyor. Bunu bir **named volume** ile nasıl hızlandırırsın? (İpucu: `/root/.nuget/packages`)

---

## ⭐ Bonus görevler

- **B1 — `.env` dosyası:** Şifreleri compose dosyasından çıkar, proje kökündeki bir `.env` dosyasına taşı ve `${DB_PASSWORD}` şeklinde kullan.
- **B2 — Healthcheck:** `db` servisine `pg_isready` komutuyla bir healthcheck ekle. `app` servisi, `depends_on` içinde `condition: service_healthy` kullanarak veritabanı hazır olunca başlasın.
- **B3 — restart:** `app` servisine `restart: unless-stopped` ekle, `docker kill` ile container'ı öldür ve ne olduğunu izle.
- **B4 — Root olmayan kullanıcı:** .NET 8 image'larında `app` adında bir kullanıcı hazır geliyor. Dockerfile'ın son aşamasına `USER $APP_UID` ekle. `docker exec not-app whoami` ile kontrol et.
- **B5 — Ölçekleme + nginx:** `app` servisinden port eşlemesini kaldır, önüne `nginx` koy ve `--scale app=3` ile çalıştır. Sayfayı yeniledikçe üstteki container adı değişiyor mu?
- **B6 — Saat dilimi:** Not tarihleri 3 saat geride görünüyor (container UTC'de çalışıyor). `TZ` ortam değişkeniyle düzelt.
- **B7 — Docker Hub:** Image'ı `kullaniciadin/not-defteri:2.0` olarak tag'le ve push et.
- **B8 — Chiseled image:** Son aşamada `mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled` kullan. Boyut ne kadar oldu? `--entrypoint ls` neden artık çalışmıyor?

---

## 🧹 Her şeyi temizlemek
```powershell
docker compose down -v
docker image rm not-defteri:1.0 not-defteri:2.0
docker volume rm pgdata
docker system prune
```

---

**Takıldığında:** önce `docker logs <container>` veya `docker compose logs` çıktısına bak.
Yine de çözemezsen hatayı bana yapıştır, birlikte bakalım. 🙂
