using GymBase.Base.Entities;

namespace GymBase.Entities.Logging;

/// <summary>Audit log: ai đã gọi lệnh thay đổi dữ liệu nào, kết quả ra sao, mất bao lâu.</summary>
public sealed class system_log : BaseEntity
{
    /// <summary>Tên đăng nhập; null khi chưa đăng nhập.</summary>
    public string? nguoi_dung { get; set; }

    public string phuong_thuc { get; set; } = string.Empty;

    /// <summary>Đường dẫn thực tế, không gồm query string.</summary>
    public string duong_dan { get; set; } = string.Empty;

    public int ma_trang_thai { get; set; }

    public long thoi_gian_ms { get; set; }

    /// <summary>Mã để tìm các dòng log kỹ thuật của cùng request.</summary>
    public string trace_id { get; set; } = string.Empty;

    public string? ip { get; set; }

    /// <summary>Thời điểm bắt đầu xử lý (UTC).</summary>
    public DateTime thoi_diem { get; set; }
}
