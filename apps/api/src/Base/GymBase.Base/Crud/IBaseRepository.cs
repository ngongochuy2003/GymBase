using GymBase.Base.Entities;
using GymBase.Base.Paging;

namespace GymBase.Base.Crud;

/// <summary>
/// Thao tác dữ liệu dùng chung cho mọi bảng. DL của từng bảng kế thừa interface này
/// (ví dụ <c>IUserRepository : IBaseRepository&lt;tai_khoan&gt;</c>) rồi thêm truy vấn riêng nếu cần.
/// Bản ghi đã xóa mềm (<see cref="ISoftDelete"/>) không bao giờ được trả về.
/// </summary>
public interface IBaseRepository<TEntity> where TEntity : BaseEntity
{
    /// <summary>
    /// Một trang bản ghi theo <see cref="PagingRequest.search"/>, <see cref="PagingRequest.filter"/> và
    /// <see cref="PagingRequest.sort"/>; không có sort thì mới tạo trước.
    /// </summary>
    /// <exception cref="GymBase.Base.Exceptions.ValidationFailedException">sort hoặc filter không hợp lệ.</exception>
    Task<PagingResult<TEntity>> GetPagingAsync(PagingRequest request, CancellationToken cancellationToken);

    /// <summary>Một bản ghi theo khóa chính; null nếu không có hoặc đã xóa mềm.</summary>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Thêm bản ghi. Id, ngày tạo... do BL gán sẵn.</summary>
    Task InsertAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>Ghi đè bản ghi theo Id; false nếu không có hoặc đã xóa mềm.</summary>
    Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>Xóa hẳn bản ghi. Xóa mềm thì BL gọi <see cref="UpdateAsync"/> với is_deleted = true.</summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
