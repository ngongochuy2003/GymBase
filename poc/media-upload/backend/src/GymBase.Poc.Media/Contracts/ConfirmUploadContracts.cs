using System.Text.Json.Serialization;

namespace GymBase.Poc.Media.Contracts;

public sealed record ConfirmUploadRequest
{
    [JsonPropertyName("upload_id")]
    public string UploadId { get; init; } = string.Empty;

    [JsonPropertyName("staging_key")]
    public string StagingKey { get; init; } = string.Empty;

    [JsonPropertyName("purpose")]
    public string Purpose { get; init; } = string.Empty;
}

public sealed record ImageVariantUrls
{
    [JsonPropertyName("thumb")]
    public string Thumb { get; init; } = string.Empty;

    [JsonPropertyName("medium")]
    public string Medium { get; init; } = string.Empty;

    [JsonPropertyName("large")]
    public string Large { get; init; } = string.Empty;
}

public sealed record ConfirmUploadResponse
{
    [JsonPropertyName("media_id")]
    public string MediaId { get; init; } = string.Empty;

    [JsonPropertyName("content_hash")]
    public string ContentHash { get; init; } = string.Empty;

    [JsonPropertyName("urls")]
    public ImageVariantUrls Urls { get; init; } = new();

    [JsonPropertyName("width")]
    public int Width { get; init; }

    [JsonPropertyName("height")]
    public int Height { get; init; }

    [JsonPropertyName("blurhash")]
    public string Blurhash { get; init; } = string.Empty;

    [JsonPropertyName("total_size_bytes")]
    public long TotalSizeBytes { get; init; }

    [JsonPropertyName("is_deduplicated")]
    public bool IsDeduplicated { get; init; }
}
