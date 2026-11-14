namespace GymBase.BLFile.BLStorage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string DefaultProvider { get; set; } = "LOCAL";
    public string LocalRoot { get; set; } = "uploads";
    public string BucketName { get; set; } = "gymbase-files";
    public string Region { get; set; } = "ap-southeast-1";
    public string? ServiceUrl { get; set; }
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
    public bool ForcePathStyle { get; set; } = true;
    public string? CdnBaseUrl { get; set; }
    public long MaxUploadBytes { get; set; } = 10 * 1024 * 1024;
    public int PresignExpiryMinutes { get; set; } = 15;
    public bool AllowAnonymousForDev { get; set; } = false;
}
