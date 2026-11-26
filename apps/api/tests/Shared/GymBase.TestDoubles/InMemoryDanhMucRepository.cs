using GymBase.Base.Entities;
using GymBase.DLDI.Contract.DLDanhMuc;
using GymBase.Entities.DI;

namespace GymBase.TestDoubles;

/// <summary>
/// DL danh mục giả cho test, nạp sẵn dữ liệu tham chiếu giống migration <c>SeedDanhMuc</c> để test tích hợp
/// thấy đúng các loại mà app thật có. Khu vực chỉ lấy 5 tỉnh/thành đại diện.
/// </summary>
public sealed class InMemoryDanhMucRepository : InMemoryBaseRepository<danh_muc>, IDanhMucRepository
{
    public InMemoryDanhMucRepository(InMemoryStore<danh_muc> store) : base(store)
    {
        store.SeedOnce(CreateMockCategories);
    }

    protected override IEnumerable<string?>? GetSearchValues(danh_muc entity) => [entity.ten, entity.ma];

    public Task<IReadOnlyList<danh_muc>> DLFindDangDungAsync(
        IReadOnlyCollection<string>? maDinhDanhs, string? maCapTren, CancellationToken cancellationToken)
    {
        var tatCa = Store.All().ToList();
        IReadOnlyList<danh_muc> rows = tatCa
            .Where(e => e.tinh_trang)
            .Where(e => maDinhDanhs is not { Count: > 0 } || maDinhDanhs.Contains(e.ma_dinh_danh, StringComparer.Ordinal))
            .Where(e => maCapTren == null
                ? e.danh_muc_cap_tren_id == null
                : tatCa.Any(cha => cha.Id == e.danh_muc_cap_tren_id && cha.ma_dinh_danh == e.ma_dinh_danh && cha.ma == maCapTren))
            .OrderBy(e => e.ma_dinh_danh, StringComparer.Ordinal)
            .ThenBy(e => e.ten, StringComparer.Ordinal)
            .ToList();

        return Task.FromResult(rows);
    }

    public Task<bool> DLExistsByMaAsync(string maDinhDanh, string ma, CancellationToken cancellationToken) =>
        Task.FromResult(Store.All().Any(e => e.ma_dinh_danh == maDinhDanh && e.ma == ma && e.tinh_trang));

    private static IEnumerable<danh_muc> CreateMockCategories() =>
    [
        CreateMock(1, "muc_tieu_tap_luyen", "tang_co", "Tăng cơ"),
        CreateMock(2, "muc_tieu_tap_luyen", "giam_mo", "Giảm mỡ"),
        CreateMock(3, "muc_tieu_tap_luyen", "duy_tri", "Duy trì vóc dáng"),
        CreateMock(4, "muc_tieu_tap_luyen", "tang_suc_ben", "Tăng sức bền"),
        CreateMock(5, "muc_tieu_tap_luyen", "linh_hoat", "Linh hoạt, giảm đau mỏi"),
        CreateMock(6, "khu_vuc", "tinh_01", "Hà Nội"),
        CreateMock(7, "khu_vuc", "tinh_29", "TP. Hồ Chí Minh"),
        CreateMock(8, "khu_vuc", "tinh_21", "Đà Nẵng"),
        CreateMock(9, "khu_vuc", "tinh_04", "Hải Phòng"),
        CreateMock(10, "khu_vuc", "tinh_33", "Cần Thơ"),
        CreateMock(11, "chuyen_mon_pt", "tang_co", "Tăng cơ"),
        CreateMock(12, "chuyen_mon_pt", "giam_mo", "Giảm mỡ"),
        CreateMock(13, "chuyen_mon_pt", "cai_thien_voc_dang", "Cải thiện vóc dáng"),
        CreateMock(14, "chuyen_mon_pt", "phuc_hoi_chuc_nang", "Phục hồi chức năng"),
        CreateMock(15, "chuyen_mon_pt", "dinh_duong", "Dinh dưỡng"),
        CreateMock(16, "trinh_do_tap", "moi_bat_dau", "Mới bắt đầu"),
        CreateMock(17, "trinh_do_tap", "co_nen_tang", "Có nền tảng"),
        CreateMock(18, "trinh_do_tap", "lau_nam", "Tập lâu năm"),
    ];

    private static danh_muc CreateMock(int index, string maDinhDanh, string ma, string ten)
    {
        var item = new danh_muc
        {
            Id = new Guid($"0193f0d{(index - 1) / 5 + 1}-0000-7000-8000-{index:D12}"),
            ma_dinh_danh = maDinhDanh,
            ma = ma,
            ten = ten,
            muc_luc = ma,
            muc_luc_ma = $"{maDinhDanh}/{ma}",
            cap = 1,
            tinh_trang = true,
        };
        item.MarkCreated(null, new DateTime(2026, 11, 15, 0, 0, 0, DateTimeKind.Utc));
        return item;
    }
}
