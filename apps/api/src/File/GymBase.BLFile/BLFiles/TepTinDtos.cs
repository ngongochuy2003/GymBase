using System.Text.Json.Serialization;

namespace GymBase.BLFile.BLFiles;

public sealed record XinUploadRequestDto(
    [property: JsonPropertyName("muc_dich")] string? MucDich,
    [property: JsonPropertyName("loai_mime")] string? LoaiMime,
    [property: JsonPropertyName("kich_thuoc_byte")] long KichThuocByte,
    [property: JsonPropertyName("ten_tep")] string? TenTep);

/// <summary>
/// Nơi app gửi file: POST <c>multipart/form-data</c> tới <c>Url</c> với mọi <c>Fields</c> (đúng thứ tự, trước file), trường file tên
/// <c>FileField</c> đặt cuối cùng. Kích thước file phải đúng bằng số byte đã khai báo khi xin upload.
/// </summary>
public sealed record UploadTargetDto(
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("method")] string Method,
    [property: JsonPropertyName("fields")] IReadOnlyDictionary<string, string> Fields,
    [property: JsonPropertyName("file_field")] string FileField,
    [property: JsonPropertyName("het_han")] DateTime HetHan);

public sealed record XinUploadResultDto(
    [property: JsonPropertyName("file_id")] Guid FileId,
    [property: JsonPropertyName("upload")] UploadTargetDto Upload);

public sealed record XacNhanUploadResultDto(
    [property: JsonPropertyName("file_id")] Guid FileId,
    [property: JsonPropertyName("loai_mime")] string LoaiMime,
    [property: JsonPropertyName("kich_thuoc_byte")] long KichThuocByte,
    [property: JsonPropertyName("muc_dich")] string MucDich,
    [property: JsonPropertyName("trang_thai")] string TrangThai);

public sealed record FileUrlDto(
    [property: JsonPropertyName("file_id")] Guid FileId,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("het_han")] DateTime HetHan);

public sealed record LayUrlsRequestDto(
    [property: JsonPropertyName("file_ids")] IReadOnlyList<Guid>? FileIds);
