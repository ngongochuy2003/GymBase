using GymBase.AMISBase.DL.Contract.Paging;

namespace GymBase.AMISBase.BL;

/// <summary>Nghiệp vụ CRUD mà controller dùng chung (<c>BaseCrudController</c>) gọi; controller không biết entity.</summary>
public interface ICrudBL<TDto, in TCreateDto, in TUpdateDto>
{
    Task<PagingResult<TDto>> GetPagingAsync(PagingRequest request, CancellationToken cancellationToken);

    /// <summary>null nếu không tồn tại hoặc đã xóa.</summary>
    Task<TDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<TDto> CreateAsync(TCreateDto dto, CancellationToken cancellationToken);

    /// <summary>null nếu không tồn tại hoặc đã xóa.</summary>
    Task<TDto?> UpdateAsync(Guid id, TUpdateDto dto, CancellationToken cancellationToken);

    /// <summary>false nếu không tồn tại hoặc đã xóa.</summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
