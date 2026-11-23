using DataAccess.Concrete.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Extensions;

public static class DatabaseExtensions
{
    /// <summary>
    /// Bekleyen migration'ları veritabanına uygular (tablo yoksa oluşturur).
    ///
    /// Neden tekrar deniyoruz? Docker'da PostgreSQL bizden birkaç saniye geç
    /// hazır olabilir. Hemen pes etmek yerine 10 kere, 3'er saniye arayla deneriz.
    /// </summary>
    public static async Task MigrateDatabaseAsync(this WebApplication app)
    {
        const int maxAttempts = 10;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var scope = app.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<NotDefteriContext>();
                await context.Database.MigrateAsync();

                app.Logger.LogInformation("Veritabanı hazır.");
                return;
            }
            catch (Exception e) when (attempt < maxAttempts)
            {
                app.Logger.LogWarning("Veritabanına bağlanılamadı ({Deneme}/{Max}): {Hata}",
                    attempt, maxAttempts, e.Message);
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }
    }
}
