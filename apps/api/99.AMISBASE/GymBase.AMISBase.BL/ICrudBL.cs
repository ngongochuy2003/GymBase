using GymBase.AMISBase.BL.Exceptions;
using GymBase.AMISBase.DL.Contract.Paging;

namespace GymBase.AMISBase.BL;

/// <summary>
/// Nghiệp vụ CRUD mà controller dùng chung (<c>BaseCrudController</c>) gọi; controller không biết entity.
/// Lỗi nghiệp vụ ném <see cref="BusinessException"/>, không trả null hay false.
/// </summary>
public interface ICrudBL<TDto, in TCreateDto, in TUpdateDto>
{
    Task<PagingResult<TDto>> GetPagingAsync(PagingRequest request, CancellationToken cancellationToken);

    /// <exception cref="NotFoundException">Không tồn tại hoặc đã xóa.</exception>
    Task<TDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<TDto> CreateAsync(TCreateDto dto, CancellationToken cancellationToken);

    /// <exception cref="NotFoundException">Không tồn tại hoặc đã xóa.</exception>
    Task<TDto> UpdateAsync(Guid id, TUpdateDto dto, CancellationToken cancellationToken);

    /// <exception cref="NotFoundException">Không tồn tại hoặc đã xóa.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
