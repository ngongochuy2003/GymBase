namespace GymBase.BLFile.BLStorage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string DefaultProvider { get; set; } = "LOCAL";
    public string LocalRoot { get; set; } = "uploads";
    public string BucketName { get; set; } = "gymbase-files";
    public string Region { get; set; } = "ap-southeast-1";
    public string? ServiceUrl { get; set; }
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
    public bool ForcePathStyle { get; set; } = true;
    public string? CdnBaseUrl { get; set; }

    /// <summary>
    /// Địa chỉ storage mà thiết bị với tới được, dùng để ký presigned URL khi khác <see cref="ServiceUrl"/> (ví dụ MinIO dev:
    /// <c>ServiceUrl</c> là localhost, điện thoại cần IP máy dev). Trống thì dùng <see cref="ServiceUrl"/>.
    /// </summary>
    public string? PublicServiceUrl { get; set; }

    /// <summary>Hạn presigned PUT (phút). Đủ để gửi video 100MB trên mạng yếu.</summary>
    public int PutExpiryMinutes { get; set; } = 15;

    /// <summary>Hạn presigned GET cho file riêng tư (phút).</summary>
    public int GetPrivateExpiryMinutes { get; set; } = 5;

    /// <summary>Hạn presigned GET cho file công khai (phút).</summary>
    public int GetPublicExpiryMinutes { get; set; } = 15;

    public long MaxImageBytes { get; set; } = 10 * 1024 * 1024;
    public long MaxDocumentBytes { get; set; } = 10 * 1024 * 1024;
    public long MaxVideoBytes { get; set; } = 100 * 1024 * 1024;

    /// <summary>Số file đang chờ tải lên (<c>cho_tai_len</c>) tối đa của một tài khoản.</summary>
    public int MaxPendingUploads { get; set; } = 20;

    /// <summary>Hạn mức mỗi tài khoản mỗi ngày (tính theo giờ UTC+7, đếm cả file lỗi).</summary>
    public int MaxImagesPerDay { get; set; } = 50;
    public int MaxVideosPerDay { get; set; } = 10;
    public int MaxDocumentsPerDay { get; set; } = 5;
    public long MaxBytesPerDay { get; set; } = 500L * 1024 * 1024;

    /// <summary>Worker xử lý ảnh nền quét việc mới mỗi bao nhiêu giây.</summary>
    public int ProcessingIntervalSeconds { get; set; } = 10;

    /// <summary>Số tệp xử lý tối đa mỗi lượt.</summary>
    public int ProcessingBatchSize { get; set; } = 10;

    /// <summary>Việc `dang_xu_ly` quá số phút này thì coi như worker đã chết và được nhận lại.</summary>
    public int ProcessingReclaimMinutes { get; set; } = 10;

    /// <summary>Job dọn file chạy mỗi bao nhiêu phút.</summary>
    public int CleanupIntervalMinutes { get; set; } = 10;

    /// <summary>File <c>cho_tai_len</c> hoặc <c>loi</c> quá số phút này thì bị xóa (cả object lẫn dòng).</summary>
    public int PendingMaxAgeMinutes { get; set; } = 60;

    /// <summary>File <c>da_tai_len</c> mà chưa gắn vào đâu quá số giờ này thì bị xóa.</summary>
    public int UnlinkedMaxAgeHours { get; set; } = 24;

    /// <summary>Số file xử lý tối đa mỗi lượt dọn, để một lượt không chạy quá lâu.</summary>
    public int CleanupBatchSize { get; set; } = 200;
}
