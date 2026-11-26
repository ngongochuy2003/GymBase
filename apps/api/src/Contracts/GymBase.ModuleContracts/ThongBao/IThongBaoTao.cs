namespace GymBase.ModuleContracts.ThongBao;

/// <summary>
/// Cách các nhóm khác (tin nhắn, kết nối PT, đánh giá, lịch tập) tạo một thông báo mà không phải tham chiếu BLThongBao.
/// Hiện thực nằm ở BLThongBao và được host đăng ký. Chỉ ghi dòng hộp thư (<c>thong_bao</c>); việc đẩy push do worker nền lo.
/// </summary>
public interface IThongBaoTao
{
    /// <summary>
    /// Tạo thông báo cho một tài khoản. Gọi lại với cùng <see cref="TaoThongBaoRequest.MaChongTrung"/> thì không tạo thêm dòng
    /// và trả về Id của dòng đã có, nên retry của nghiệp vụ gọi vào đây là an toàn.
    /// </summary>
    /// <exception cref="GymBase.Base.Exceptions.ValidationFailedException">Loại thông báo không hợp lệ, hoặc thiếu tiêu đề hay mã chống trùng.</exception>
    Task<Guid> BLTaoAsync(TaoThongBaoRequest request, CancellationToken ct = default);
}

/// <param name="TaiKhoanId">Người nhận.</param>
/// <param name="LoaiThongBao">Tên <c>LoaiThongBao</c> (snake_case): nhac_an, nhac_tap, tin_nhan, danh_gia, ket_noi_pt.</param>
/// <param name="TieuDe">Tiêu đề ngắn, hiện cả trên màn hình khóa nên giữ trung tính (không ghi dữ liệu sức khỏe).</param>
/// <param name="NoiDung">Chi tiết cho hộp thư trong app; không đi qua push.</param>
/// <param name="LoaiDoiTuong">Nguồn phát sinh, ví dụ <c>tin_nhan</c>; cùng <paramref name="DoiTuongId"/> để app mở đúng màn hình.</param>
/// <param name="MaChongTrung">Khóa chống lập, unique cùng tài khoản. Ví dụ <c>tin_nhan:{id}</c>.</param>
/// <param name="ThoiDiemDuKien">Lúc cần gửi (UTC); null là ngay bây giờ.</param>
public sealed record TaoThongBaoRequest(
    Guid TaiKhoanId,
    string LoaiThongBao,
    string TieuDe,
    string? NoiDung,
    string? LoaiDoiTuong,
    Guid? DoiTuongId,
    string MaChongTrung,
    DateTime? ThoiDiemDuKien = null);
