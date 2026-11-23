# ===========================================================================
# Multi-stage Dockerfile (N-katmanlı proje)
# Build context: proje kökü (DockerOgrenNet/)
#   docker build -f src/WebAPI/Dockerfile -t notdefteri .
# ===========================================================================

# ---------- 1. AŞAMA: derleme (SDK image'ı büyük, ama sadece derlerken lazım) ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Önce SADECE .csproj dosyalarını kopyala ve restore et.
# Kod değişse bile .csproj'lar değişmediyse Docker bu katmanı önbellekten kullanır,
# NuGet paketleri her build'de yeniden inmez.
COPY src/Core/Core.csproj             src/Core/
COPY src/Entities/Entities.csproj     src/Entities/
COPY src/DataAccess/DataAccess.csproj src/DataAccess/
COPY src/Business/Business.csproj     src/Business/
COPY src/WebAPI/WebAPI.csproj         src/WebAPI/
RUN dotnet restore src/WebAPI/WebAPI.csproj

# Şimdi kodun geri kalanını kopyala ve yayınla (publish).
COPY src/ src/
RUN dotnet publish src/WebAPI/WebAPI.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- 2. AŞAMA: çalıştırma (sadece runtime, küçük image) ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
USER $APP_UID
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WebAPI.dll"]
