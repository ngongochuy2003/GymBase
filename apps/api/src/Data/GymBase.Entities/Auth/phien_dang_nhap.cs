using GymBase.Base.Entities;

namespace GymBase.Entities.Auth;

/// <summary>
/// Một phiên đăng nhập trên một thiết bị. Chỉ lưu SHA-256 của refresh token, không lưu bản gốc.
/// Id của phiên là claim <c>sid</c> trong access token.
/// </summary>
public sealed class phien_dang_nhap : BaseEntity
{
    public Guid nguoi_dung_id { get; set; }

    /// <summary>SHA-256 của refresh token hiện tại.</summary>
    public string token_hash { get; set; } = string.Empty;

    /// <summary>SHA-256 của refresh token vừa bị thay thế, để phát hiện token cũ bị dùng lại.</summary>
    public string? token_hash_truoc { get; set; }

    /// <summary>Thời điểm đổi token gần nhất (UTC).</summary>
    public DateTime? ngay_xoay_vong { get; set; }

    /// <summary>Hết hạn nếu không dùng tới thời điểm này (UTC).</summary>
    public DateTime ngay_het_han { get; set; }

    /// <summary>Hết hạn tuyệt đối, dù vẫn đang dùng (UTC).</summary>
    public DateTime ngay_het_han_toi_da { get; set; }

    /// <summary>Khác null là phiên đã bị thu hồi (UTC).</summary>
    public DateTime? ngay_thu_hoi { get; set; }

    public string? ten_thiet_bi { get; set; }

    public DateTime ngay_tao { get; set; }
}
