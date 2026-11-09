using System.Text.Json.Serialization;

namespace GymBase.Poc.Media.BL;

public sealed record FileUploadResultDto(
    [property: JsonPropertyName("file_id")] Guid FileId,
    [property: JsonPropertyName("original_name")] string OriginalName,
    [property: JsonPropertyName("content_type")] string ContentType,
    [property: JsonPropertyName("size")] long Size,
    [property: JsonPropertyName("purpose")] string Purpose,
    [property: JsonPropertyName("provider")] string Provider,
    [property: JsonPropertyName("blurhash")] string? Blurhash,
    [property: JsonPropertyName("content_url")] string ContentUrl
);

public sealed record FileContentStreamDto(
    Stream Stream,
    string ContentType,
    string OriginalName
);

public sealed record TepTinMetadataDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("original_name")] string OriginalName,
    [property: JsonPropertyName("content_type")] string ContentType,
    [property: JsonPropertyName("size")] long Size,
    [property: JsonPropertyName("purpose")] string Purpose,
    [property: JsonPropertyName("provider")] string Provider,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("blurhash")] string? Blurhash,
    [property: JsonPropertyName("created_at")] DateTime CreatedAt
);

public sealed record MigrationResultDto(
    [property: JsonPropertyName("total_scanned")] int TotalScanned,
    [property: JsonPropertyName("success_count")] int SuccessCount,
    [property: JsonPropertyName("failure_count")] int FailureCount,
    [property: JsonPropertyName("errors")] IReadOnlyList<string> Errors
);
