using GymBase.AMISBase.DL.Contract.Entities;
using GymBase.AMISBase.DL.Contract.Paging;
using GymBase.DLSystem.Contract;
using GymBase.DLSystem.Contract.Entities;

namespace GymBase.DLSystem.Mysql;

/// <summary>
/// MOCK: trả dữ liệu cố định trong bộ nhớ vì chưa có DB (DB-01).
/// Khi có DB, thay thân các hàm bằng SQL raw; chữ ký và hành vi (lọc xóa mềm, tìm kiếm,
/// sắp xếp mới tạo trước, phân trang) phải giữ nguyên để BL và API không phải sửa.
/// </summary>
public sealed class UserDL : IUserDL
{
    private static readonly IReadOnlyList<User> MockUsers =
    [
        CreateMock("0193f0a0-0000-7000-8000-000000000001", "admin", "Quản trị viên", "admin@gymbase.local", "0900000001", "Admin", true, false, 1),
        CreateMock("0193f0a0-0000-7000-8000-000000000002", "pt.minh", "Nguyễn Văn Minh", "minh@gymbase.local", "0900000002", "Trainer", true, false, 2),
        CreateMock("0193f0a0-0000-7000-8000-000000000003", "pt.lan", "Trần Thị Lan", "lan@gymbase.local", "0900000003", "Trainer", true, false, 3),
        CreateMock("0193f0a0-0000-7000-8000-000000000004", "hv.huy", "Ngô Ngọc Huy", "huy@gymbase.local", "0900000004", "Trainee", true, false, 4),
        CreateMock("0193f0a0-0000-7000-8000-000000000005", "hv.phuong", "Lê Thu Phương", null, "0900000005", "Trainee", false, false, 5),
        CreateMock("0193f0a0-0000-7000-8000-000000000006", "hv.daxoa", "Tài khoản đã xóa", null, null, "Trainee", true, true, 6),
    ];

    public Task<PagingResult<User>> GetPagingAsync(PagingRequest request, CancellationToken cancellationToken)
    {
        var query = MockUsers.Where(u => !u.is_deleted);

        if (request.Search is { } search)
        {
            query = query.Where(u =>
                Contains(u.tai_khoan, search)
                || Contains(u.ho_ten, search)
                || Contains(u.email, search)
                || Contains(u.so_dien_thoai, search));
        }

        var filtered = query.OrderByDescending(u => u.ngay_tao).ToList();
        var page = filtered.Skip(request.Skip).Take(request.PageSize).ToList();

        return Task.FromResult(new PagingResult<User>(page, request.Page, request.PageSize, filtered.Count));
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(MockUsers.FirstOrDefault(u => u.Id == id && !u.is_deleted));

    private static bool Contains(string? value, string search) =>
        value is not null && value.Contains(search, StringComparison.OrdinalIgnoreCase);

    private static User CreateMock(
        string id, string userName, string fullName, string? email, string? phone,
        string role, bool isActive, bool isDeleted, int createdDay)
    {
        var user = new User
        {
            Id = Guid.Parse(id),
            tai_khoan = userName,
            ho_ten = fullName,
            email = email,
            so_dien_thoai = phone,
            vai_tro = role,
            dang_hoat_dong = isActive,
            mat_khau_hash = "mock-not-a-real-hash",
            is_deleted = isDeleted,
        };
        user.MarkCreated("seed", new DateTime(2026, 11, createdDay, 0, 0, 0, DateTimeKind.Utc));
        return user;
    }
}
