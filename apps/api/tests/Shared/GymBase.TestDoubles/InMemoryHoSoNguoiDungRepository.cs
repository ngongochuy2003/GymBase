using System.Collections.Concurrent;
using GymBase.DLBiz.Contract.DLChiSoCoThe;
using GymBase.DLBiz.Contract.DLHoSoNguoiDung;
using GymBase.Entities.Biz;

namespace GymBase.TestDoubles;

/// <summary>
/// "Bảng" hồ sơ trong bộ nhớ, đăng ký singleton. Hồ sơ có khóa chính là <c>tai_khoan_id</c> nên không dùng
/// <see cref="InMemoryStore{TEntity}"/> (cần <c>Id</c>).
/// </summary>
public sealed class InMemoryHoSoNguoiDungStore
{
    private readonly ConcurrentDictionary<Guid, ho_so_nguoi_dung> _rows = new();

    public void Set(ho_so_nguoi_dung hoSo) => _rows[hoSo.tai_khoan_id] = hoSo;

    public ho_so_nguoi_dung? Find(Guid taiKhoanId) => _rows.GetValueOrDefault(taiKhoanId);
}

/// <summary>"Bảng" chỉ số cơ thể trong bộ nhớ: bảng chỉ thêm, giữ nguyên thứ tự chèn.</summary>
public sealed class InMemoryChiSoCoTheStore
{
    private readonly ConcurrentQueue<chi_so_co_the> _rows = new();

    public void Add(chi_so_co_the chiSo) => _rows.Enqueue(chiSo);

    public IReadOnlyList<chi_so_co_the> All() => _rows.ToList();
}

/// <summary>Repository hồ sơ giả cho test; lưu hồ sơ và chỉ số như một lần ghi của DL thật.</summary>
public sealed class InMemoryHoSoNguoiDungRepository(
    InMemoryHoSoNguoiDungStore hoSoStore, InMemoryChiSoCoTheStore chiSoStore) : IHoSoNguoiDungRepository
{
    public Task<ho_so_nguoi_dung?> DLFindByTaiKhoanIdAsync(Guid taiKhoanId, CancellationToken cancellationToken) =>
        Task.FromResult(hoSoStore.Find(taiKhoanId) is { } hoSo ? Copy(hoSo) : null);

    public Task DLUpsertAsync(ho_so_nguoi_dung hoSo, chi_so_co_the? chiSoMoi, CancellationToken cancellationToken)
    {
        hoSoStore.Set(Copy(hoSo));
        if (chiSoMoi is not null)
        {
            chiSoStore.Add(chiSoMoi);
        }

        return Task.CompletedTask;
    }

    /// <summary>Trả bản sao để BL sửa object mà chưa gọi lưu thì dữ liệu không đổi, giống đọc từ DB thật.</summary>
    private static ho_so_nguoi_dung Copy(ho_so_nguoi_dung hoSo) => new()
    {
        tai_khoan_id = hoSo.tai_khoan_id,
        loai_nguoi_dung = hoSo.loai_nguoi_dung,
        ten_hien_thi = hoSo.ten_hien_thi,
        gioi_thieu = hoSo.gioi_thieu,
        anh_dai_dien_tep_tin_id = hoSo.anh_dai_dien_tep_tin_id,
        muc_tieu_tap_luyen = hoSo.muc_tieu_tap_luyen,
        ngay_sinh = hoSo.ngay_sinh,
        gioi_tinh = hoSo.gioi_tinh,
        trinh_do_tap = hoSo.trinh_do_tap,
        chuyen_mon = hoSo.chuyen_mon,
        so_nam_kinh_nghiem = hoSo.so_nam_kinh_nghiem,
        khu_vuc_hoat_dong = hoSo.khu_vuc_hoat_dong,
        vi_do = hoSo.vi_do,
        kinh_do = hoSo.kinh_do,
        ban_kinh_phuc_vu_km = hoSo.ban_kinh_phuc_vu_km,
        trang_thai_xac_minh = hoSo.trang_thai_xac_minh,
        ngay_xac_minh = hoSo.ngay_xac_minh,
        nguoi_xac_minh_id = hoSo.nguoi_xac_minh_id,
        ly_do_tu_choi = hoSo.ly_do_tu_choi,
        ngay_tao = hoSo.ngay_tao,
        nguoi_tao = hoSo.nguoi_tao,
        ngay_chinh_sua = hoSo.ngay_chinh_sua,
        nguoi_chinh_sua = hoSo.nguoi_chinh_sua,
    };
}

/// <summary>Repository chỉ số cơ thể giả cho test.</summary>
public sealed class InMemoryChiSoCoTheRepository(InMemoryChiSoCoTheStore store) : IChiSoCoTheRepository
{
    public Task<chi_so_co_the?> DLFindMoiNhatAsync(Guid hocVienId, CancellationToken cancellationToken) =>
        Task.FromResult(store.All()
            .Where(c => c.hoc_vien_id == hocVienId)
            .OrderByDescending(c => c.ngay_do)
            .ThenByDescending(c => c.ngay_tao)
            .FirstOrDefault());
}
