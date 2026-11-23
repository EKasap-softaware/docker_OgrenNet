using Business.Concrete;
using Business.Constants;
using Business.Tests.Fakes;
using Entities.Concrete;
using Entities.DTOs;

namespace Business.Tests;

/// <summary>
/// NoteManager testleri: Gerçek veritabanı ve Redis YOK, sahteleri (Fakes) kullanıyoruz.
/// </summary>
public class NoteManagerTests
{
    private readonly FakeNoteDal _dal = new();
    private readonly FakeCacheService _cache = new();
    private readonly NoteManager _manager;

    public NoteManagerTests()
    {
        // Her test için temiz bir NoteManager (xUnit her testte constructor'ı yeniden çalıştırır)
        _manager = new NoteManager(_dal, _cache);
    }

    [Fact]
    public async Task Gecerli_not_temizlenerek_eklenir()
    {
        var sonuc = await _manager.AddAsync(new NoteCreateDto { Title = "  Market  ", Content = "süt    ekmek" });

        Assert.True(sonuc.Success);
        Assert.Equal(Messages.NoteAdded, sonuc.Message);
        var not = Assert.Single(_dal.Notes);
        Assert.Equal("Market", not.Title);
        Assert.Equal("süt ekmek", not.Content);
    }

    [Fact]
    public async Task Gecersiz_not_veritabanina_yazilmaz()
    {
        var sonuc = await _manager.AddAsync(new NoteCreateDto { Title = "", Content = "içerik" });

        Assert.False(sonuc.Success);
        Assert.Empty(_dal.Notes);
    }

    [Fact]
    public async Task Liste_ikinci_seferde_cacheten_gelir()
    {
        await _manager.AddAsync(new NoteCreateDto { Title = "A", Content = "a" });

        var ilk = await _manager.GetAllAsync();
        var ikinci = await _manager.GetAllAsync();

        Assert.Equal(Messages.NotesListed, ilk.Message);
        Assert.Equal(Messages.NotesListedFromCache, ikinci.Message);
    }

    [Fact]
    public async Task Not_eklenince_cache_temizlenir()
    {
        await _manager.GetAllAsync();                     // cache doldu
        Assert.NotEmpty(_cache.Items);

        await _manager.AddAsync(new NoteCreateDto { Title = "Yeni", Content = "not" });

        Assert.Empty(_cache.Items);                       // eski liste cache'ten silindi
    }

    [Fact]
    public async Task Notlar_yeniden_eskiye_siralanir()
    {
        _dal.Notes.Add(new Note { Id = 1, Title = "Eski", Content = "x", CreatedAt = DateTime.UtcNow.AddDays(-1) });
        _dal.Notes.Add(new Note { Id = 2, Title = "Yeni", Content = "y", CreatedAt = DateTime.UtcNow });

        var sonuc = await _manager.GetAllAsync();

        Assert.Equal("Yeni", sonuc.Data![0].Title);
    }

    [Fact]
    public async Task Olmayan_not_silinemez()
    {
        var sonuc = await _manager.DeleteAsync(42);

        Assert.False(sonuc.Success);
        Assert.Equal(Messages.NoteNotFound, sonuc.Message);
    }

    [Fact]
    public async Task Not_guncellenir_ve_tarihi_yazilir()
    {
        var eklenen = await _manager.AddAsync(new NoteCreateDto { Title = "Eski başlık", Content = "x" });

        var sonuc = await _manager.UpdateAsync(eklenen.Data!.Id, new NoteCreateDto { Title = "Yeni başlık", Content = "y" });

        Assert.True(sonuc.Success);
        Assert.Equal("Yeni başlık", _dal.Notes[0].Title);
        Assert.NotNull(_dal.Notes[0].UpdatedAt);
    }
}
