using GymBase.Base.Entities;
using GymBase.Base.Paging;
using GymBase.DLAuth.Contract.DLTaiKhoan;
using GymBase.Entities.Auth;

namespace GymBase.TestDoubles;

/// <summary>DL người dùng giả cho test, nạp sẵn 5 bản ghi mẫu.</summary>
public sealed class InMemoryTaiKhoanRepository : InMemoryBaseRepository<tai_khoan>, ITaiKhoanRepository
{
    public InMemoryTaiKhoanRepository(InMemoryStore<tai_khoan> store) : base(store)
    {
        store.SeedOnce(CreateMockUsers);
    }

    protected override IEnumerable<string?> GetSearchValues(tai_khoan user) =>
        [user.ten_dang_nhap, user.ho_ten, user.email, user.so_dien_thoai];

    protected override QueryColumns<tai_khoan> AllowedColumns => TaiKhoanQueryColumns.Allowed;

    public Task<bool> DLExistsByTenDangNhapAsync(string tenDangNhap, CancellationToken cancellationToken) =>
        Task.FromResult(Store.All().Any(user =>
            string.Equals(user.ten_dang_nhap, tenDangNhap, StringComparison.OrdinalIgnoreCase)));

    public Task<tai_khoan?> DLFindByTenDangNhapAsync(string tenDangNhap, CancellationToken cancellationToken) =>
        Task.FromResult(Store.All().FirstOrDefault(user =>
            string.Equals(user.ten_dang_nhap, tenDangNhap, StringComparison.OrdinalIgnoreCase)));

    private static IEnumerable<tai_khoan> CreateMockUsers() =>
    [
        CreateMock("0193f0a0-0000-7000-8000-000000000001", "admin", "Quản trị viên", "admin@gymbase.local", "0900000001", "Admin", true, 1),
        CreateMock("0193f0a0-0000-7000-8000-000000000002", "pt.minh", "Nguyễn Văn Minh", "minh@gymbase.local", "0900000002", "User", true, 2),
        CreateMock("0193f0a0-0000-7000-8000-000000000003", "pt.lan", "Trần Thị Lan", "lan@gymbase.local", "0900000003", "User", true, 3),
        CreateMock("0193f0a0-0000-7000-8000-000000000004", "hv.huy", "Ngô Ngọc Huy", "huy@gymbase.local", "0900000004", "User", true, 4),
        CreateMock("0193f0a0-0000-7000-8000-000000000005", "hv.phuong", "Lê Thu Phương", null, "0900000005", "User", false, 5),
    ];

    private static tai_khoan CreateMock(
        string id, string userName, string fullName, string? email, string? phone,
        string role, bool isActive, int createdDay)
    {
        var user = new tai_khoan
        {
            Id = Guid.Parse(id),
            ten_dang_nhap = userName,
            ho_ten = fullName,
            email = email,
            so_dien_thoai = phone,
            vai_tro = role,
            dang_hoat_dong = isActive,
            mat_khau_hash = "mock-not-a-real-hash",
        };
        user.MarkCreated("seed", new DateTime(2026, 11, createdDay, 0, 0, 0, DateTimeKind.Utc));
        return user;
    }
}
