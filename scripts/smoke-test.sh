#!/usr/bin/env bash
# ---------------------------------------------------------------------------
# Smoke test ("duman testi"): Uygulama gerçekten çalışıyor mu?
#
#   1) /health  -> uygulama, PostgreSQL ve Redis "ok" olana kadar bekle (en fazla 60 sn)
#   2) POST /api/notes -> bir not ekle
#   3) GET  /api/notes -> eklenen not listede mi?
#
# Kullanım:  bash scripts/smoke-test.sh [adres]     (varsayılan: http://localhost:5000)
# Bir adım başarısız olursa çıkış kodu 1 olur, böylece CI adımı KIRMIZI yanar.
# ---------------------------------------------------------------------------
set -euo pipefail

BASE_URL="${1:-http://localhost:5000}"
DENEME=30
BEKLEME=2

echo "== 1) Sağlık kontrolü: $BASE_URL/health"
SAGLIKLI=false
for i in $(seq 1 "$DENEME"); do
  CEVAP="$(curl -fsS "$BASE_URL/health" 2>/dev/null || true)"
  echo "[$i/$DENEME] ${CEVAP:-cevap yok}"
  if echo "$CEVAP" | grep -q '"db":"ok"' && echo "$CEVAP" | grep -q '"redis":"ok"'; then
    SAGLIKLI=true
    break
  fi
  sleep "$BEKLEME"
done

if [ "$SAGLIKLI" != true ]; then
  echo "❌ Uygulama $((DENEME * BEKLEME)) saniyede sağlıklı hale gelmedi."
  exit 1
fi

BASLIK="smoke-test-$(date +%s)"

echo "== 2) Not ekleniyor: $BASLIK"
curl -fsS -X POST "$BASE_URL/api/notes" \
  -H "Content-Type: application/json" \
  -d "{\"title\":\"$BASLIK\",\"content\":\"CI tarafından oluşturuldu\"}"
echo

echo "== 3) Not listede mi?"
if curl -fsS "$BASE_URL/api/notes" | grep -q "$BASLIK"; then
  echo "✅ Smoke test başarılı: API, PostgreSQL ve Redis çalışıyor."
else
  echo "❌ Eklenen not listede bulunamadı."
  exit 1
fi
