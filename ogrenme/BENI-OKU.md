# 🎓 Git · GitHub · CI/CD öğrenme klasörü

Bu klasördeki dosyalar **şablondur**. Her biri satır satır açıklamalıdır.
Derslerde sırası gelince onları **sen** projede doğru yere kopyalayacak, inceleyecek ve gerekirse değiştireceksin.

> ⚠️ Sırası gelmeden kopyalama. Örneğin `.github/workflows/` klasörüne konan bir dosya,
> GitHub'a gönderdiğin anda çalışmaya başlar.

| Dosya | Nereye kopyalanacak | Ne zaman |
|---|---|---|
| `git/ornek.gitattributes` | `.gitattributes` (proje kökü) | Git bölümü: satır sonları dersi |
| `ci/1-ci-basit.yml` | `.github/workflows/ci.yml` | CI bölümü: ilk workflow |
| `ci/2-ci-tam.yml` | `.github/workflows/ci.yml` (üstüne yaz) | CI bölümü: Docker testi |
| `cd/cd.yml` | `.github/workflows/cd.yml` | CD bölümü: image yayınlama ve deploy |
| `cd/release.yml` | `.github/workflows/release.yml` | CD bölümü: sürüm (v1.0.0) |
| `github/dependabot.yml` | `.github/dependabot.yml` | Profesyonel ayarlar |
| `github/CODEOWNERS` | `.github/CODEOWNERS` | Profesyonel ayarlar |
| `github/pull_request_template.md` | `.github/pull_request_template.md` | Profesyonel ayarlar |
| `github/ISSUE_TEMPLATE/` | `.github/ISSUE_TEMPLATE/` | Profesyonel ayarlar |

**Not:** `.gitignore` dosyası bilerek eksik bırakıldı. Onu derste birlikte düzelteceğiz. 😉
