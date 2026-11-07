namespace GymBase.AMISBase.DL.Contract.Entities;

/// <summary>
/// Gốc của mọi entity ánh xạ tới một bảng. Chỉ chứa khóa chính.
/// Bảng không cần audit (bảng nối, log, lịch sử) kế thừa trực tiếp lớp này.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Khóa chính dạng UUIDv7: tăng dần theo thời gian nên index MySQL ít phân mảnh hơn UUID ngẫu nhiên.
    /// Tự sinh khi tạo object mới; khi đọc từ DB, giá trị trong DB sẽ ghi đè.
    /// </summary>
    public Guid Id { get; set; } = Guid.CreateVersion7();
}
