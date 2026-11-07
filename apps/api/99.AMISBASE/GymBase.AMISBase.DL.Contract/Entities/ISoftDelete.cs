namespace GymBase.AMISBase.DL.Contract.Entities;

/// <summary>
/// Entity xóa mềm: khi xóa chỉ bật cờ <see cref="is_deleted"/>, bản ghi vẫn còn trong DB
/// (ví dụ danh mục gói tập vẫn được các buổi tập cũ tham chiếu).
/// Độc lập với audit: entity có thể xóa mềm mà không cần <see cref="BaseAuditableEntity"/>.
/// </summary>
public interface ISoftDelete
{
    /// <summary>true nếu bản ghi đã bị xóa mềm.</summary>
    bool is_deleted { get; set; }
}
