using System.Text.Json.Serialization;

namespace GymBase.Poc.Media.Contracts;

public sealed record UploadUrlRequest
{
    [JsonPropertyName("purpose")]
    public string Purpose { get; init; } = string.Empty;

    [JsonPropertyName("content_type")]
    public string ContentType { get; init; } = string.Empty;

    [JsonPropertyName("size_bytes")]
    public long SizeBytes { get; init; }
}

public sealed record UploadUrlResponse
{
    [JsonPropertyName("upload_id")]
    public string UploadId { get; init; } = string.Empty;

    [JsonPropertyName("presigned_put_url")]
    public string PresignedPutUrl { get; init; } = string.Empty;

    [JsonPropertyName("staging_key")]
    public string StagingKey { get; init; } = string.Empty;

    [JsonPropertyName("expires_in_seconds")]
    public int ExpiresInSeconds { get; init; }
}
