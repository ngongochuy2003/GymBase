using GymBase.Base.Exceptions;
using GymBase.Base.Paging;

namespace GymBase.Base.Crud;

/// <summary>
/// Nghiệp vụ CRUD mà controller dùng chung (<c>BaseCrudController</c>) gọi; controller không biết entity.
/// Lỗi nghiệp vụ ném <see cref="BusinessException"/>, không trả null hay false.
/// </summary>
public interface ICrudService<TDto, in TCreateDto, in TUpdateDto>
{
    Task<PagingResult<TDto>> BLGetPagingAsync(PagingRequest request, CancellationToken cancellationToken);

    /// <exception cref="NotFoundException">Không tồn tại hoặc đã xóa.</exception>
    Task<TDto> BLGetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<TDto> BLCreateAsync(TCreateDto dto, CancellationToken cancellationToken);

    /// <exception cref="NotFoundException">Không tồn tại hoặc đã xóa.</exception>
    Task<TDto> BLUpdateAsync(Guid id, TUpdateDto dto, CancellationToken cancellationToken);

    /// <exception cref="NotFoundException">Không tồn tại hoặc đã xóa.</exception>
    Task BLDeleteAsync(Guid id, CancellationToken cancellationToken);
}
