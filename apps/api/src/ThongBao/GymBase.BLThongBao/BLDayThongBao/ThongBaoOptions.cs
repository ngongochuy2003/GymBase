namespace GymBase.BLThongBao.BLDayThongBao;

/// <summary>Cấu hình mục <c>ThongBao</c> của appsettings.</summary>
public sealed class ThongBaoOptions
{
    public const string SectionName = "ThongBao";

    /// <summary><c>LOG</c> (chỉ ghi log) hoặc <c>FCM</c>.</summary>
    public string PushProvider { get; set; } = "LOG";

    /// <summary>Giây giữa hai lượt quét của worker.</summary>
    public int IntervalSeconds { get; set; } = 5;

    public int BatchSize { get; set; } = 50;

    /// <summary>Số lần tối đa gặp lỗi tạm trước khi đánh dấu <c>loi</c>.</summary>
    public int MaxAttempts { get; set; } = 5;

    /// <summary>Thời gian chờ sau lần lỗi đầu; mỗi lần sau gấp đôi, tối đa <see cref="MaxBackoffSeconds"/>.</summary>
    public int BackoffSeconds { get; set; } = 30;

    public int MaxBackoffSeconds { get; set; } = 600;

    /// <summary>Dòng <c>dang_gui</c> quá số phút này được coi là worker đã chết và giành lại.</summary>
    public int ReclaimMinutes { get; set; } = 5;
}
