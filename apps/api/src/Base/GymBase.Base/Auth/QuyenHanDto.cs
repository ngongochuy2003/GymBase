namespace GymBase.Base.Auth;

/// <summary>
/// Quyền theo hồ sơ của một tài khoản ở thời điểm hiện tại, tính từ database (không đọc từ token). Vai trò hệ thống (`vai_tro`)
/// không nằm ở đây, client lấy ở <c>GET /auth/me</c>.
/// Client dùng để hiện hoặc ẩn chức năng; backend vẫn kiểm tra lại ở từng request. Thiết kế: docs/api/phan-quyen.md.
/// </summary>
/// <param name="da_thiet_lap_ho_so">false nếu tài khoản chưa chọn nhóm (chưa có <c>ho_so_nguoi_dung</c>).</param>
/// <param name="loai_nguoi_dung">hoc_vien, pt hoặc ca_hai; null nếu chưa có hồ sơ.</param>
/// <param name="trang_thai_xac_minh">chua_xac_minh, dang_xem_xet, da_xac_minh hoặc tu_choi; null nếu chưa có hồ sơ.</param>
/// <param name="la_pt">Có hồ sơ PT (pt hoặc ca_hai), kể cả chưa xác minh.</param>
/// <param name="la_pt_da_xac_minh">PT đã được duyệt và tài khoản không bị khóa: mới được nhận và quản lý học viên.</param>
/// <param name="nang_luc">Danh sách năng lực, giá trị ở <see cref="NangLuc"/>.</param>
public sealed record QuyenHanDto(
    bool da_thiet_lap_ho_so,
    string? loai_nguoi_dung,
    string? trang_thai_xac_minh,
    bool la_pt,
    bool la_pt_da_xac_minh,
    IReadOnlyList<string> nang_luc);

/// <summary>Các giá trị của <see cref="QuyenHanDto.nang_luc"/>.</summary>
public static class NangLuc
{
    /// <summary>PT đã xác minh: nhận học viên và lập kế hoạch qua kết nối PT.</summary>
    public const string QuanLyHocVien = "quan_ly_hoc_vien";
}
