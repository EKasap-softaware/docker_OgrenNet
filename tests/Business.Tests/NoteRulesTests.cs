// ---------------------------------------------------------------------------
// NoteRules için birim testleri (xUnit).
//
// Bir test 3 parçadan oluşur (AAA kalıbı):
//   Arrange -> hazırlık (girdiyi oluştur)
//   Act     -> çalıştır (test edilen metodu çağır)
//   Assert  -> doğrula (sonuç beklediğimiz gibi mi?)
//
// [Fact]   = tek bir senaryo
// [Theory] = aynı testi farklı girdilerle birden çok kez çalıştırır
// ---------------------------------------------------------------------------

using Business.Constants;
using Business.Rules;
using Entities.DTOs;

namespace Business.Tests;

public class NoteRulesTests
{
    [Theory]
    [InlineData("  merhaba  ", "merhaba")]
    [InlineData("alışveriş    listesi", "alışveriş listesi")]
    [InlineData("  a \t b \n c  ", "a b c")]
    [InlineData(null, "")]
    [InlineData("   ", "")]
    public void Clean_bosluklari_duzenler(string? girdi, string beklenen)
    {
        Assert.Equal(beklenen, NoteRules.Clean(girdi));
    }

    [Fact]
    public void Gecerli_not_kabul_edilir()
    {
        // Arrange
        var dto = new NoteCreateDto { Title = "Market", Content = "Süt, ekmek" };

        // Act
        var sonuc = NoteRules.Validate(dto);

        // Assert
        Assert.True(sonuc.Success);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public void Basligi_bos_olan_not_reddedilir(string? baslik)
    {
        var sonuc = NoteRules.Validate(new NoteCreateDto { Title = baslik, Content = "içerik" });

        Assert.False(sonuc.Success);
        Assert.Equal(Messages.TitleRequired, sonuc.Message);
    }

    [Fact]
    public void Icerigi_bos_olan_not_reddedilir()
    {
        var sonuc = NoteRules.Validate(new NoteCreateDto { Title = "Başlık", Content = "  " });

        Assert.False(sonuc.Success);
        Assert.Equal(Messages.ContentRequired, sonuc.Message);
    }

    [Fact]
    public void Baslik_tam_sinirda_ise_kabul_edilir()
    {
        var dto = new NoteCreateDto { Title = new string('a', 100), Content = "içerik" };

        Assert.True(NoteRules.Validate(dto).Success);
    }

    [Fact]
    public void Baslik_siniri_asarsa_reddedilir()
    {
        var dto = new NoteCreateDto { Title = new string('a', 101), Content = "içerik" };

        Assert.False(NoteRules.Validate(dto).Success);
    }

    [Fact]
    public void Icerik_tam_sinirda_ise_kabul_edilir()
    {
        var dto = new NoteCreateDto { Title = "Başlık", Content = new string('b', 500) };

        Assert.True(NoteRules.Validate(dto).Success);
    }

    [Fact]
    public void Icerik_siniri_asarsa_reddedilir()
    {
        var dto = new NoteCreateDto { Title = "Başlık", Content = new string('b', 501) };

        var sonuc = NoteRules.Validate(dto);

        Assert.False(sonuc.Success);
        Assert.Equal(Messages.ContentTooLong(500), sonuc.Message);
    }
}
