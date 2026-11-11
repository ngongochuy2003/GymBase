using GymBase.Base.Entities;
using GymBase.Entities.Biz.Enums;

namespace GymBase.Entities.Biz;

// TODO(DB): chưa có migration (AddBizTables); sơ đồ ở database/diagrams/08-thong-bao.md. Mục 3.
/// <summary>
/// Điện thoại của người dùng, giữ push token FCM theo từng thiết bị. Một tài khoản có thể có nhiều thiết bị.
/// </summary>
public sealed class thiet_bi : BaseAuditableEntity
{
    public Guid tai_khoan_id { get; set; }

    /// <summary>Định danh cài đặt do app cấp, không dùng định danh quảng cáo. Unique cùng tai_khoan_id.</summary>
    public string ma_thiet_bi { get; set; } = string.Empty;

    public NenTang nen_tang { get; set; }

    /// <summary>FCM token mới nhất. Unique, cho phép null.</summary>
    public string? push_token { get; set; }

    /// <summary>Có cho phép gửi push hay không.</summary>
    public bool dang_hoat_dong { get; set; }

    /// <summary>Lần cuối thiết bị liên hệ (UTC).</summary>
    public DateTime ngay_hoat_dong_cuoi { get; set; }
}
