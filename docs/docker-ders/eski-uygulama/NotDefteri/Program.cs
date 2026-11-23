// ---------------------------------------------------------------------------
// Docker Not Defteri (.NET 8 Minimal API)
//
// Bu kod hazır, dokunmana gerek yok. Senin işin onu Docker'a koymak.
//
// Uygulama 3 parçadan oluşur:
//   1. Bu ASP.NET Core uygulaması   -> senin build edeceğin image
//   2. PostgreSQL                   -> notları saklar (kalıcı veri, VOLUME lazım)
//   3. Redis                        -> ziyaret sayacı (hızlı, geçici veri)
//
// Ayarlar appsettings.json'dan okunur. ORTAM DEĞİŞKENİ verirsen appsettings'i EZER:
//   appsettings.json:  "ConnectionStrings": { "Postgres": "..." }
//   ortam değişkeni :  ConnectionStrings__Postgres=...      (iki alt çizgi = ":")
// ---------------------------------------------------------------------------

using System.Net;
using Npgsql;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var appName = app.Configuration["AppName"] ?? "Docker Not Defteri (.NET)";

var pgBuilder = new NpgsqlConnectionStringBuilder(
    app.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("ConnectionStrings:Postgres ayarı bulunamadı"))
{
    Timeout = 3 // bağlanamazsa sayfa 15 sn donmasın
};
var pgConnStr = pgBuilder.ConnectionString;
var pgTarget = $"{pgBuilder.Host}:{pgBuilder.Port}/{pgBuilder.Database}";

var redisConnStr = app.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("ConnectionStrings:Redis ayarı bulunamadı");

app.Logger.LogInformation("[{App}] ortam: {Env}", appName, app.Environment.EnvironmentName);
app.Logger.LogInformation("  Postgres -> {Target}", pgTarget);
app.Logger.LogInformation("  Redis    -> {Target}", redisConnStr);

// ---------------------------------------------------------------------------
// Redis: bağlantıyı sakla, koparsa yeniden bağlan
// ---------------------------------------------------------------------------
ConnectionMultiplexer? redisMux = null;
var redisLock = new object();

IDatabase GetRedis()
{
    lock (redisLock)
    {
        if (redisMux is { IsConnected: true })
            return redisMux.GetDatabase();

        redisMux?.Dispose();
        redisMux = null;

        var opts = ConfigurationOptions.Parse(redisConnStr);
        opts.ConnectTimeout = 2000;
        opts.SyncTimeout = 2000;
        opts.ConnectRetry = 1;
        opts.AbortOnConnectFail = true;

        redisMux = ConnectionMultiplexer.Connect(opts);
        return redisMux.GetDatabase();
    }
}

// ---------------------------------------------------------------------------
// PostgreSQL: bağlantı aç, tablo yoksa oluştur
// ---------------------------------------------------------------------------
var tableReady = false;

async Task<NpgsqlConnection> OpenDbAsync()
{
    var conn = new NpgsqlConnection(pgConnStr);
    await conn.OpenAsync();
    if (!tableReady)
    {
        await using var cmd = new NpgsqlCommand("""
            CREATE TABLE IF NOT EXISTS notes (
                id         SERIAL PRIMARY KEY,
                text       TEXT NOT NULL,
                created_at TIMESTAMPTZ NOT NULL DEFAULT now()
            )
            """, conn);
        await cmd.ExecuteNonQueryAsync();
        tableReady = true;
    }
    return conn;
}

static string Err(Exception e) => $"{e.GetType().Name}: {e.Message}";

// ---------------------------------------------------------------------------
// Sayfalar
// ---------------------------------------------------------------------------
app.MapGet("/", async () =>
{
    // Redis: ziyaret sayacı
    long? visits = null;
    string? redisError = null;
    try { visits = await GetRedis().StringIncrementAsync("ziyaret_sayisi"); }
    catch (Exception e) { redisError = Err(e); }

    // Postgres: notlar
    var notes = new List<Note>();
    string? dbError = null;
    try
    {
        await using var conn = await OpenDbAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT id, text, created_at FROM notes ORDER BY id DESC", conn);
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
            notes.Add(new Note(r.GetInt32(0), r.GetString(1), r.GetDateTime(2)));
    }
    catch (Exception e) { dbError = Err(e); }

    var html = Page.Render(
        appName,
        hostname: Environment.MachineName, // container içinde = container ID'nin başı
        visits, redisError, redisConnStr,
        notes, dbError, pgTarget);

    return Results.Content(html, "text/html; charset=utf-8");
});

app.MapPost("/notlar", async (HttpRequest req) =>
{
    var form = await req.ReadFormAsync();
    var text = form["text"].ToString().Trim();
    if (text.Length > 0)
    {
        await using var conn = await OpenDbAsync();
        await using var cmd = new NpgsqlCommand("INSERT INTO notes (text) VALUES (@t)", conn);
        cmd.Parameters.AddWithValue("t", text);
        await cmd.ExecuteNonQueryAsync();
    }
    return Results.Redirect("/");
});

app.MapPost("/notlar/{id:int}/sil", async (int id) =>
{
    await using var conn = await OpenDbAsync();
    await using var cmd = new NpgsqlCommand("DELETE FROM notes WHERE id = @id", conn);
    cmd.Parameters.AddWithValue("id", id);
    await cmd.ExecuteNonQueryAsync();
    return Results.Redirect("/");
});

// Healthcheck için (Bonus görev). Uygulama ayaktaysa 200 döner.
app.MapGet("/health", async () =>
{
    var db = "ok";
    var redis = "ok";
    try { await using var c = await OpenDbAsync(); } catch { db = "down"; }
    try { await GetRedis().PingAsync(); } catch { redis = "down"; }
    return Results.Json(new { app = "ok", db, redis });
});

app.Run();

// ---------------------------------------------------------------------------
record Note(int Id, string Text, DateTime CreatedAt);

static class Page
{
    static string E(string s) => WebUtility.HtmlEncode(s);

    public static string Render(
        string appName, string hostname,
        long? visits, string? redisError, string redisTarget,
        List<Note> notes, string? dbError, string dbTarget)
    {
        string StatusCard(string label, string? error, string target, string bigValue) =>
            error is not null
                ? $"""
                  <div class="card">
                    <div class="label">{E(label)}</div>
                    <div class="bad">Bağlanamadı</div>
                    <div class="target">hedef: {E(target)}</div>
                    <div class="err">{E(error)}</div>
                  </div>
                  """
                : $"""
                  <div class="card">
                    <div class="label">{E(label)}</div>
                    <div class="big">{E(bigValue)}</div>
                    <div class="ok">Bağlı</div>
                    <div class="target">hedef: {E(target)}</div>
                  </div>
                  """;

        string notesHtml;
        if (dbError is not null)
        {
            notesHtml = """<div class="empty">Veritabanı bağlı olmadığı için not eklenemiyor.</div>""";
        }
        else
        {
            var items = notes.Count == 0
                ? """<div class="empty">Henüz not yok.</div>"""
                : "<ul>" + string.Concat(notes.Select(n => $"""
                    <li>
                      <span>{E(n.Text)}<span class="date">{n.CreatedAt.ToLocalTime():dd.MM.yyyy HH:mm}</span></span>
                      <form method="post" action="/notlar/{n.Id}/sil"><button class="del" type="submit">sil</button></form>
                    </li>
                    """)) + "</ul>";

            notesHtml = $"""
                <form class="add" method="post" action="/notlar">
                  <input type="text" name="text" placeholder="Bir not yaz..." autocomplete="off" required>
                  <button type="submit">Ekle</button>
                </form>
                {items}
                """;
        }

        var redisCard = StatusCard("Redis · ziyaret sayacı", redisError, redisTarget, visits?.ToString() ?? "");
        var dbCard = StatusCard("PostgreSQL · notlar", dbError, dbTarget, notes.Count.ToString());

        return $$"""
            <!doctype html>
            <html lang="tr">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <title>{{E(appName)}}</title>
              <style>
                * { box-sizing: border-box; }
                body { font-family: system-ui, -apple-system, "Segoe UI", sans-serif; background: #f4f6f9; color: #1d2330; margin: 0; padding: 32px 16px; }
                .wrap { max-width: 720px; margin: 0 auto; }
                h1 { margin: 0 0 4px; font-size: 28px; }
                .sub { color: #66708a; margin-bottom: 24px; font-size: 14px; }
                .sub code { background: #e6eaf2; padding: 2px 6px; border-radius: 4px; }
                .grid { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; margin-bottom: 20px; }
                .card { background: #fff; border-radius: 10px; padding: 16px; box-shadow: 0 1px 3px rgba(0,0,0,.06); }
                .label { font-size: 12px; text-transform: uppercase; letter-spacing: .05em; color: #66708a; }
                .ok { color: #13834b; font-weight: 600; }
                .bad { color: #c0392b; font-weight: 600; }
                .big { font-size: 32px; font-weight: 700; margin-top: 6px; }
                .err { font-family: ui-monospace, Consolas, monospace; font-size: 12px; color: #8a2a20; background: #fdecea; padding: 8px; border-radius: 6px; margin-top: 8px; word-break: break-word; }
                .target { font-family: ui-monospace, Consolas, monospace; font-size: 12px; color: #66708a; margin-top: 4px; }
                form.add { display: flex; gap: 8px; margin-bottom: 12px; }
                input[type=text] { flex: 1; padding: 10px 12px; border: 1px solid #ccd3e0; border-radius: 8px; font-size: 15px; }
                button { padding: 10px 16px; border: 0; border-radius: 8px; background: #512bd4; color: #fff; font-size: 15px; cursor: pointer; }
                button.del { background: transparent; color: #c0392b; padding: 4px 8px; font-size: 13px; }
                ul { list-style: none; padding: 0; margin: 0; }
                li { display: flex; justify-content: space-between; align-items: center; padding: 10px 0; border-bottom: 1px solid #eef1f6; }
                li:last-child { border-bottom: 0; }
                .date { color: #99a1b3; font-size: 12px; margin-left: 8px; }
                .empty { color: #99a1b3; font-style: italic; }
                @media (max-width: 560px) { .grid { grid-template-columns: 1fr; } }
              </style>
            </head>
            <body>
            <div class="wrap">
              <h1>🐳 {{E(appName)}}</h1>
              <div class="sub">Bu sayfayı sunan makine / container: <code>{{E(hostname)}}</code></div>
              <div class="grid">
                {{redisCard}}
                {{dbCard}}
              </div>
              <div class="card">
                <div class="label" style="margin-bottom:10px">Notlar</div>
                {{notesHtml}}
              </div>
            </div>
            </body>
            </html>
            """;
    }
}
