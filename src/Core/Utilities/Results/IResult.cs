namespace Core.Utilities.Results;

/// <summary>
/// Bir işlemin sonucu: başarılı mı, kullanıcıya ne mesaj gösterilecek?
/// Business katmanı hata fırlatmak (exception) yerine bu nesneyi döner.
/// </summary>
public interface IResult
{
    bool Success { get; }
    string Message { get; }
}

/// <summary>Sonuçla birlikte veri de dönen işlemler için (örn. not listesi).</summary>
public interface IDataResult<out T> : IResult
{
    T? Data { get; }
}
