using GymBase.Base.Entities;
using GymBase.Base.Paging;

namespace GymBase.Base.Crud;

/// <summary>
/// Thao tác dữ liệu dùng chung cho mọi bảng. DL của từng bảng kế thừa interface này
/// (ví dụ <c>ITaiKhoanRepository : IBaseRepository&lt;tai_khoan&gt;</c>) rồi thêm truy vấn riêng nếu cần.
/// </summary>
public interface IBaseRepository<TEntity> where TEntity : BaseEntity
{
    /// <summary>
    /// Một trang bản ghi theo <see cref="PagingRequest.search"/>, <see cref="PagingRequest.filter"/> và
    /// <see cref="PagingRequest.sort"/>; không có sort thì mới tạo trước.
    /// </summary>
    /// <exception cref="GymBase.Base.Exceptions.ValidationFailedException">sort hoặc filter không hợp lệ.</exception>
    Task<PagingResult<TEntity>> DLGetPagingAsync(PagingRequest request, CancellationToken cancellationToken);

    /// <summary>Một bản ghi theo khóa chính; null nếu không có.</summary>
    Task<TEntity?> DLGetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Thêm bản ghi. Id, ngày tạo... do BL gán sẵn.</summary>
    Task DLInsertAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>Ghi đè bản ghi theo Id; false nếu không có.</summary>
    Task<bool> DLUpdateAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>Xóa hẳn bản ghi; false nếu không có. Bản ghi đang được bảng khác tham chiếu thì ném <c>ConflictException</c>.</summary>
    Task<bool> DLDeleteAsync(Guid id, CancellationToken cancellationToken);
}
