namespace GymBase.BLThongBao.BLDayThongBao;

/// <summary>Cấu hình mục <c>ThongBao</c> của appsettings.</summary>
public sealed class ThongBaoOptions
{
    public const string SectionName = "ThongBao";

    /// <summary><c>LOG</c> (chỉ ghi log) hoặc <c>FCM</c>.</summary>
    public string PushProvider { get; set; } = "LOG";

    /// <summary>Cấu hình FCM, chỉ dùng khi <see cref="PushProvider"/> là <c>FCM</c>.</summary>
    public FcmOptions Fcm { get; set; } = new();

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

/// <summary>Mục <c>ThongBao:Fcm</c>. Khóa service account là bí mật: để file ngoài repo, nạp đường dẫn qua user-secrets hoặc biến môi trường.</summary>
public sealed class FcmOptions
{
    /// <summary>Id project Firebase. Bỏ trống thì lấy <c>project_id</c> trong file service account.</summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>Đường dẫn tới file JSON của service account (Firebase Admin SDK).</summary>
    public string CredentialsPath { get; set; } = string.Empty;
}
