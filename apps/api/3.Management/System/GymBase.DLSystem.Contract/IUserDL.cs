using GymBase.AMISBase.DL.Contract.Paging;
using GymBase.DLSystem.Contract.Entities;

namespace GymBase.DLSystem.Contract;

/// <summary>Truy cập dữ liệu người dùng. Mọi hàm bỏ qua bản ghi đã xóa mềm.</summary>
public interface IUserDL
{
    /// <summary>
    /// Lấy một trang người dùng, mới tạo trước. <see cref="PagingRequest.Search"/> tìm trong
    /// tên đăng nhập, họ tên, email, số điện thoại (không phân biệt hoa thường).
    /// </summary>
    Task<PagingResult<User>> GetPagingAsync(PagingRequest request, CancellationToken cancellationToken);

    /// <summary>Lấy người dùng theo Id; null nếu không có hoặc đã xóa.</summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
