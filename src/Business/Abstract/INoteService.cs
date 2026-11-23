using Core.Utilities.Results;
using Entities.Concrete;
using Entities.DTOs;

namespace Business.Abstract;

/// <summary>
/// Not işlemlerinin sözleşmesi. Controller sadece bu arayüzü tanır,
/// arkada NoteManager mı başka bir sınıf mı çalışıyor bilmez.
/// </summary>
public interface INoteService
{
    Task<IDataResult<List<Note>>> GetAllAsync();

    Task<IDataResult<Note>> GetByIdAsync(int id);

    Task<IDataResult<Note>> AddAsync(NoteCreateDto dto);

    Task<IResult> UpdateAsync(int id, NoteCreateDto dto);

    Task<IResult> DeleteAsync(int id);
}
