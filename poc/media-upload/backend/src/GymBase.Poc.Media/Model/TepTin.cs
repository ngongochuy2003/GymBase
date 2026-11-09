namespace GymBase.Poc.Media.Model;

/// <summary>
/// Thực thể ánh xạ trực tiếp bảng tep_tin trong MySQL (tên tiếng Việt không dấu, snake_case).
/// Nguồn chân lý metadata tệp tin của toàn bộ hệ thống GymBase V1.
/// </summary>
public sealed class TepTin
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid NguoiTaiLenId { get; set; }

    public string TenGoc { get; set; } = string.Empty;

    public string KhoaLuuTru { get; set; } = string.Empty;

    /// <summary>
    /// Nhà cung cấp lưu trữ cho bản ghi này: "LOCAL" hoặc "S3"
    /// </summary>
    public string NhaCungCap { get; set; } = "LOCAL";

    public string LoaiMime { get; set; } = string.Empty;

    public long DungLuong { get; set; }

    /// <summary>
    /// Mục đích nghiệp vụ: avatar, meal, post, workout, progress
    /// </summary>
    public string MucDich { get; set; } = string.Empty;

    /// <summary>
    /// Trạng thái vòng đời: UPLOADING, READY, DELETING, DELETED, FAILED
    /// </summary>
    public string TrangThai { get; set; } = "UPLOADING";

    /// <summary>
    /// Chuỗi Blurhash placeholder phục vụ mobile hiển thị ảnh mượt mà
    /// </summary>
    public string? Blurhash { get; set; }

    public DateTime NgayTao { get; set; } = DateTime.UtcNow;

    public DateTime NgayCapNhat { get; set; } = DateTime.UtcNow;

    public DateTime? NgayXoa { get; set; }
}
