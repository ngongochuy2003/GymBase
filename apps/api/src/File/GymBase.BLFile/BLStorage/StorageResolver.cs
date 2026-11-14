using Microsoft.Extensions.Options;

namespace GymBase.BLFile.BLStorage;

public sealed class StorageResolver : IStorageResolver
{
    private readonly LocalFileStorage _localStorage;
    private readonly S3FileStorage _s3Storage;
    private readonly StorageOptions _options;

    public StorageResolver(
        LocalFileStorage localStorage,
        S3FileStorage s3Storage,
        IOptions<StorageOptions> options)
    {
        _localStorage = localStorage;
        _s3Storage = s3Storage;
        _options = options.Value;
    }

    public IFileStorage Resolve(string providerName)
    {
        if (string.Equals(providerName, "LOCAL", StringComparison.OrdinalIgnoreCase))
        {
            return _localStorage;
        }

        if (string.Equals(providerName, "S3", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(providerName, "MINIO", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(providerName, _options.BucketName, StringComparison.OrdinalIgnoreCase))
        {
            return _s3Storage;
        }

        return GetDefault();
    }

    public IFileStorage GetDefault()
    {
        return Resolve(_options.DefaultProvider);
    }
}
