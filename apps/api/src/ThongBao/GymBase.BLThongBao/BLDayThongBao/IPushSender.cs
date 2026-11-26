namespace GymBase.BLThongBao.BLDayThongBao;

public enum UuTienPush
{
    Thuong,
    Cao,
}

/// <summary>Kết quả gửi một tin tới một thiết bị, đã quy về bốn nhóm để worker quyết định thử lại hay không.</summary>
public enum KetQuaDay
{
    ThanhCong,

    /// <summary>Lỗi tạm (429, 5xx, UNAVAILABLE, mất mạng): thử lại sau.</summary>
    LoiTam,

    /// <summary>Token đã chết (UNREGISTERED, INVALID_ARGUMENT do token): xóa token, không thử lại.</summary>
    TokenChet,

    /// <summary>Lỗi cấu hình (SENDER_ID_MISMATCH, thiếu quyền service account): không thử lại, cần người xử lý.</summary>
    LoiCauHinh,
}

public sealed record KetQuaPush(KetQuaDay Ket, string? MaTin = null, string? Loi = null);

/// <summary>
/// Tin push gửi tới một thiết bị. Cố ý chỉ có mã và tiêu đề ngắn, không có nội dung: dữ liệu sức khỏe nhạy cảm, tiêu đề hiện
/// trên màn hình khóa và đi qua máy chủ Google, Apple. Chi tiết app tự lấy qua API hộp thư.
/// </summary>
public sealed record TinPush(
    string PushToken,
    Guid ThongBaoId,
    string LoaiThongBao,
    string? LoaiDoiTuong,
    Guid? DoiTuongId,
    string TieuDe,
    UuTienPush UuTien,
    TimeSpan Ttl);

/// <summary>
/// Cổng ra tới dịch vụ push (seam, quyết định 11 của ADR-2). Bản thật gọi FCM; bản log dùng khi dev chưa có Firebase; test dùng bản giả.
/// Hiện thực không ném lỗi nghiệp vụ: lỗi của FCM được quy về <see cref="KetQuaDay"/>.
/// </summary>
public interface IPushSender
{
    Task<KetQuaPush> GuiAsync(TinPush tin, CancellationToken ct = default);
}
