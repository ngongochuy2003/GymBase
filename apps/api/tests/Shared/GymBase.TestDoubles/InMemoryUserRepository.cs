using GymBase.Base.Entities;
using GymBase.DLAuth.Contract;
using GymBase.DLAuth.Contract.Entities;

namespace GymBase.TestDoubles;

/// <summary>DL người dùng giả cho test, nạp sẵn 6 bản ghi mẫu (một bản ghi đã xóa mềm).</summary>
public sealed class InMemoryUserRepository : InMemoryBaseRepository<nguoi_dung>, IUserRepository
{
    public InMemoryUserRepository(InMemoryStore<nguoi_dung> store) : base(store)
    {
        store.SeedOnce(CreateMockUsers);
    }

    protected override IEnumerable<string?> GetSearchValues(nguoi_dung user) =>
        [user.tai_khoan, user.ho_ten, user.email, user.so_dien_thoai];

    public Task<bool> ExistsByTaiKhoanAsync(string taiKhoan, CancellationToken cancellationToken) =>
        Task.FromResult(Store.All().Any(user =>
            !user.is_deleted && string.Equals(user.tai_khoan, taiKhoan, StringComparison.OrdinalIgnoreCase)));

    private static IEnumerable<nguoi_dung> CreateMockUsers() =>
    [
        CreateMock("0193f0a0-0000-7000-8000-000000000001", "admin", "Quản trị viên", "admin@gymbase.local", "0900000001", "Admin", true, false, 1),
        CreateMock("0193f0a0-0000-7000-8000-000000000002", "pt.minh", "Nguyễn Văn Minh", "minh@gymbase.local", "0900000002", "Trainer", true, false, 2),
        CreateMock("0193f0a0-0000-7000-8000-000000000003", "pt.lan", "Trần Thị Lan", "lan@gymbase.local", "0900000003", "Trainer", true, false, 3),
        CreateMock("0193f0a0-0000-7000-8000-000000000004", "hv.huy", "Ngô Ngọc Huy", "huy@gymbase.local", "0900000004", "Trainee", true, false, 4),
        CreateMock("0193f0a0-0000-7000-8000-000000000005", "hv.phuong", "Lê Thu Phương", null, "0900000005", "Trainee", false, false, 5),
        CreateMock("0193f0a0-0000-7000-8000-000000000006", "hv.daxoa", "Tài khoản đã xóa", null, null, "Trainee", true, true, 6),
    ];

    private static nguoi_dung CreateMock(
        string id, string userName, string fullName, string? email, string? phone,
        string role, bool isActive, bool isDeleted, int createdDay)
    {
        var user = new nguoi_dung
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
