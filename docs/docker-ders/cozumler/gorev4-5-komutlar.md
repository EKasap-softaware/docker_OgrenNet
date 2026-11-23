# Görev 4 ve 5 — Elle network ve volume (PowerShell)

> PowerShell'de satırı bölmek için satır sonuna **ters tırnak** (`` ` ``) koyulur.

## Görev 4

```powershell
# 1) Ortak ağ
docker network create notnet

# 2) Redis  (-p YOK: sadece ağdaki diğer container'lar erişecek)
docker run -d --name redis --network notnet redis:7-alpine

# 3) Postgres
docker run -d --name db --network notnet `
  -e POSTGRES_USER=postgres `
  -e POSTGRES_PASSWORD=postgres `
  -e POSTGRES_DB=notlar `
  postgres:16

# 4) Uygulama
docker run -d --name not-app --network notnet -p 5000:8080 `
  -e "ConnectionStrings__Postgres=Host=db;Port=5432;Username=postgres;Password=postgres;Database=notlar" `
  -e "ConnectionStrings__Redis=redis:6379" `
  not-defteri:2.0
```

**Neden `localhost` değil de `db`?**
Container içinde `localhost` **o container'ın kendisi** demektir. Uygulama container'ının içinde Postgres çalışmıyor.
Aynı network'teki container'lar birbirini **container adıyla** bulur, çünkü Docker'ın kendi DNS'i bu isimleri çözer.

**`-p` kimin için gerekli?**
Sadece **senin bilgisayarından** container'a erişmek için. Aynı ağdaki container'lar birbirine `-p` olmadan da bağlanabilir.

**Visual Studio'dan bu Postgres'e bağlanmak için:**
`db`'yi `-p 5432:5432` ile başlatırsın. Bu durumda VS'de çalışan uygulama `Host=localhost` ile bağlanır.
Yani `appsettings.json` içindeki `localhost` değerleri **Docker dışında** çalışırken geçerli. Container'dayken ortam değişkenleri bu değerleri ezer.
(Bilgisayarında zaten Postgres kuruluysa 5432 portu dolu olabilir. Bu durumda `-p 5433:5432` kullan ve bağlantı cümlesinde `Port=5433` yaz.)

## Görev 5

```powershell
docker rm -f db

# Named volume ile:
docker run -d --name db --network notnet `
  -e POSTGRES_USER=postgres `
  -e POSTGRES_PASSWORD=postgres `
  -e POSTGRES_DB=notlar `
  -v pgdata:/var/lib/postgresql/data `
  postgres:16

# Test: not ekle -> sil -> aynı komutla tekrar başlat -> notlar duruyor
docker volume ls
docker volume inspect pgdata
```

**Redis sayacı:** Volume vermediğimiz için Redis container'ı silinince sayaç sıfırlanır. Ziyaret sayacı için bu sorun değil. Önemli bir veri tutsaydık `-v redisdata:/data` eklememiz gerekirdi.

**Named volume ve bind mount:**
- Named volume (`pgdata:/...`): Verinin nerede tutulacağına Docker karar verir. Veritabanları için uygundur, Windows'ta da hızlı çalışır ve dosya izinleri sorun çıkarmaz.
- Bind mount (`./src:/src`): Bilgisayarındaki belirli bir klasörü container'a bağlar. Kod geliştirme ve config dosyaları için uygundur.
