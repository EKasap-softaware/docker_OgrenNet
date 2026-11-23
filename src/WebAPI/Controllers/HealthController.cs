using DataAccess.Concrete.EntityFramework;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace WebAPI.Controllers;

/// <summary>
/// GET /health → Uygulama, veritabanı ve Redis ayakta mı?
/// Docker, CI ve CD bu adrese bakarak "uygulama sağlıklı mı" diye karar verir.
/// Örnek cevap: {"app":"ok","db":"ok","redis":"ok"}
/// </summary>
[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    private readonly NotDefteriContext _context;
    private readonly IConnectionMultiplexer _redis;

    public HealthController(NotDefteriContext context, IConnectionMultiplexer redis)
    {
        _context = context;
        _redis = redis;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var db = await _context.Database.CanConnectAsync() ? "ok" : "down";

        var redis = "ok";
        try { await _redis.GetDatabase().PingAsync(); }
        catch { redis = "down"; }

        return Ok(new { app = "ok", db, redis });
    }
}
