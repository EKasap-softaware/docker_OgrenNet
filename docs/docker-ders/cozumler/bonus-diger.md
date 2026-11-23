# Bonus B3, B4, B6, B7, B8

## B3 — restart
```powershell
docker compose ps                        # app container'ının adını bul
docker kill dockerogrennet-app-1
docker compose ps                        # birkaç saniye sonra tekrar "Up" olmalı
```
`docker stop` / `docker compose stop` ile durdurursan `unless-stopped` onu **tekrar başlatmaz**, çünkü bilerek durdurmuş oluyorsun.

## B4 — Root olmayan kullanıcı
`gorev3-Dockerfile` dosyasında `ENTRYPOINT` satırından önce:
```dockerfile
USER $APP_UID
```
```powershell
docker exec dockerogrennet-app-1 whoami     # -> app
```
Uygulama 8080 portunu dinliyor. 1024'ün üstündeki portlar root yetkisi gerektirmediği için bu sorunsuz çalışır.
(.NET 8 image'larının varsayılan portu 80 yerine 8080 olmasının sebebi de bu.)

## B6 — Saat dilimi
`environment` bölümüne `TZ: Europe/Istanbul` ekle. Uygulama `ToLocalTime()` kullanıyor, bu yüzden container'ın saat dilimini değiştirmek yeterli.

## B7 — Docker Hub
```powershell
docker login
docker tag not-defteri:2.0 KULLANICIADIN/not-defteri:2.0
docker push KULLANICIADIN/not-defteri:2.0
```

## B8 — Chiseled image
Son aşamada `FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled` kullan.
- Boyut belirgin şekilde küçülür (`docker images` ile karşılaştır).
- İçinde **shell yok**, `ls` yok, paket yöneticisi yok. Bu yüzden `--entrypoint ls` çalışmaz.
  Saldırganın içeride kullanabileceği araç olmadığı için daha güvenlidir.
- Varsayılan olarak root olmayan kullanıcıyla çalışır.
