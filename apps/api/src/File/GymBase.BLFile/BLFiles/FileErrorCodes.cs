namespace GymBase.BLFile.BLFiles;

/// <summary>Mã lỗi của upload file, trả ở trường <c>code</c> của ProblemDetails. Đã đưa ra thì không đổi tên, không đổi nghĩa.</summary>
public static class FileErrorCodes
{
    /// <summary>400: <c>muc_dich</c> không có trong danh sách cho phép.</summary>
    public const string PurposeInvalid = "FILE_PURPOSE_INVALID";

    /// <summary>400: loại MIME không được phép cho mục đích này.</summary>
    public const string TypeNotAllowed = "FILE_TYPE_NOT_ALLOWED";

    /// <summary>400: dung lượng vượt giới hạn của loại file, hoặc không dương.</summary>
    public const string TooLarge = "FILE_TOO_LARGE";

    /// <summary>422: hết hạn mức file chờ hoặc hạn mức trong ngày.</summary>
    public const string QuotaExceeded = "FILE_QUOTA_EXCEEDED";

    /// <summary>400: confirm khi chưa có object trên storage.</summary>
    public const string NotUploaded = "FILE_NOT_UPLOADED";

    /// <summary>400: object trên storage không khớp khai báo (kích thước, MIME, magic bytes); file bị đánh dấu lỗi.</summary>
    public const string ContentMismatch = "FILE_CONTENT_MISMATCH";

    /// <summary>422: file không ở trạng thái cho phép thao tác (đã lỗi, đã xóa).</summary>
    public const string InvalidState = "FILE_INVALID_STATE";
}
