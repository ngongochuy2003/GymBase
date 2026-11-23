using System.Text.Json.Serialization;

namespace GymBase.ModuleContracts.TepTin;

/// <summary>URL đọc đã ký của một file. <c>LoaiMime</c> để app biết ảnh hay video khi URL không nói rõ.</summary>
public sealed record FileUrlDto(
    [property: JsonPropertyName("file_id")] Guid FileId,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("het_han")] DateTime HetHan,
    [property: JsonPropertyName("loai_mime")] string LoaiMime,
    [property: JsonPropertyName("blurhash")] string? Blurhash = null);

/// <summary>Cấp URL đọc (presigned GET) cho nhóm Biz mà không phải tham chiếu BLFile. Quyền xem do BLFile kiểm.</summary>
public interface IFileUrlProvider
{
    /// <summary>
    /// URL đọc nhiều file theo lô cho <paramref name="nguoiXemId"/>. File không tồn tại, chưa upload xong hoặc người xem không được xem
    /// thì vắng mặt trong kết quả (không phân biệt, để khỏi lộ sự tồn tại).
    /// </summary>
    Task<IReadOnlyDictionary<Guid, FileUrlDto>> BLLayUrlsAsync(
        Guid nguoiXemId, IReadOnlyCollection<Guid> fileIds, CancellationToken ct = default);
}
