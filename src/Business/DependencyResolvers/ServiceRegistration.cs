using Business.Abstract;
using Business.Concrete;
using Core.CrossCuttingConcerns.Caching;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Business.DependencyResolvers;

/// <summary>
/// Dependency Injection (DI) kayıtları: "Biri X isterse ona Y ver" listesi.
///
///   INoteService isteyen  -> NoteManager alır
///   INoteDal isteyen      -> EfNoteDal alır
///   ICacheService isteyen -> DistributedCacheService (Redis) alır
///
/// Program.cs'te tek satırla çağrılır:  builder.Services.AddBusinessServices(builder.Configuration);
/// </summary>
public static class ServiceRegistration
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
    {
        // --- PostgreSQL + Entity Framework ---
        var postgres = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres ayarı bulunamadı");

        services.AddDbContext<NotDefteriContext>(options => options.UseNpgsql(postgres));

        // --- Redis ---
        var redis = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("ConnectionStrings:Redis ayarı bulunamadı");

        var redisOptions = ConfigurationOptions.Parse(redis);
        redisOptions.AbortOnConnectFail = false;   // Redis kapalıysa uygulama yine de açılsın
        redisOptions.ConnectTimeout = 2000;
        redisOptions.SyncTimeout = 2000;

        // Bağlantı ilk ihtiyaç duyulduğunda kurulur (Lazy) ve tüm uygulama tek bağlantıyı paylaşır.
        var connection = new Lazy<IConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(redisOptions));
        services.AddSingleton(_ => connection.Value);
        services.AddStackExchangeRedisCache(options =>
        {
            options.ConnectionMultiplexerFactory = () => Task.FromResult(connection.Value);
            options.InstanceName = "notdefteri:";
        });

        // --- Katmanlar ---
        // Scoped    = her HTTP isteği için bir tane (DbContext ile aynı ömür)
        // Singleton = uygulama boyunca tek bir tane
        services.AddScoped<INoteDal, EfNoteDal>();
        services.AddScoped<INoteService, NoteManager>();
        services.AddSingleton<ICacheService, DistributedCacheService>();

        return services;
    }
}
