namespace GymBase.Poc.Media.Abstractions;

/// <summary>
/// Bộ định tuyến provider lưu trữ.
/// Quyết định đọc/xóa file từ LOCAL hay S3 dựa trên cột nha_cung_cap của từng bản ghi tep_tin.
/// </summary>
public interface IStorageResolver
{
    /// <summary>
    /// Lấy provider theo tên: "LOCAL" hoặc "S3"
    /// </summary>
    IFileStorage Resolve(string providerName);

    /// <summary>
    /// Lấy provider mặc định dùng cho các upload mới (cấu hình trong Storage:DefaultProvider).
    /// </summary>
    IFileStorage GetDefault();
}
