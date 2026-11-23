using Business.Abstract;
using Business.Constants;
using Business.Rules;
using Core.CrossCuttingConcerns.Caching;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;

namespace Business.Concrete;

/// <summary>
/// Not iş mantığı: kuralları kontrol et, veritabanına yaz/oku, cache'i yönet.
///
/// Bağımlılıklar constructor'dan gelir (Dependency Injection).
/// NoteManager, EfNoteDal'ı veya Redis'i "new" ile oluşturmaz; sadece arayüzleri bilir.
/// Testlerde bu sayede sahte (fake) bir veritabanı verebiliriz.
/// </summary>
public class NoteManager : INoteService
{
    private const string AllNotesCacheKey = "notes:all";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);

    private readonly INoteDal _noteDal;
    private readonly ICacheService _cache;

    public NoteManager(INoteDal noteDal, ICacheService cache)
    {
        _noteDal = noteDal;
        _cache = cache;
    }

    public async Task<IDataResult<List<Note>>> GetAllAsync()
    {
        // 1) Önce cache'e bak. Varsa veritabanına hiç gitme.
        var cached = await _cache.GetAsync<List<Note>>(AllNotesCacheKey);
        if (cached is not null)
            return new SuccessDataResult<List<Note>>(cached, Messages.NotesListedFromCache);

        // 2) Yoksa veritabanından oku, sonra cache'e koy.
        var notes = (await _noteDal.GetAllAsync())
            .OrderByDescending(n => n.CreatedAt)
            .ToList();

        await _cache.SetAsync(AllNotesCacheKey, notes, CacheDuration);
        return new SuccessDataResult<List<Note>>(notes, Messages.NotesListed);
    }

    public async Task<IDataResult<Note>> GetByIdAsync(int id)
    {
        var note = await _noteDal.GetAsync(n => n.Id == id);
        return note is null
            ? new ErrorDataResult<Note>(Messages.NoteNotFound)
            : new SuccessDataResult<Note>(note, Messages.NoteListed);
    }

    public async Task<IDataResult<Note>> AddAsync(NoteCreateDto dto)
    {
        var validation = NoteRules.Validate(dto);
        if (!validation.Success)
            return new ErrorDataResult<Note>(validation.Message);

        var note = new Note
        {
            Title = NoteRules.Clean(dto.Title),
            Content = NoteRules.Clean(dto.Content),
            CreatedAt = DateTime.UtcNow
        };

        await _noteDal.AddAsync(note);
        await _cache.RemoveAsync(AllNotesCacheKey);   // liste değişti, eski cache geçersiz

        return new SuccessDataResult<Note>(note, Messages.NoteAdded);
    }

    public async Task<IResult> UpdateAsync(int id, NoteCreateDto dto)
    {
        var validation = NoteRules.Validate(dto);
        if (!validation.Success)
            return validation;

        var note = await _noteDal.GetAsync(n => n.Id == id);
        if (note is null)
            return new ErrorResult(Messages.NoteNotFound);

        note.Title = NoteRules.Clean(dto.Title);
        note.Content = NoteRules.Clean(dto.Content);
        note.UpdatedAt = DateTime.UtcNow;

        await _noteDal.UpdateAsync(note);
        await _cache.RemoveAsync(AllNotesCacheKey);

        return new SuccessResult(Messages.NoteUpdated);
    }

    public async Task<IResult> DeleteAsync(int id)
    {
        var note = await _noteDal.GetAsync(n => n.Id == id);
        if (note is null)
            return new ErrorResult(Messages.NoteNotFound);

        await _noteDal.DeleteAsync(note);
        await _cache.RemoveAsync(AllNotesCacheKey);

        return new SuccessResult(Messages.NoteDeleted);
    }
}
