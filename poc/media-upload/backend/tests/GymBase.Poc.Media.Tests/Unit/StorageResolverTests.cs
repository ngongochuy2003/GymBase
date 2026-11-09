using Amazon.S3;
using FluentAssertions;
using GymBase.Poc.Media.Configuration;
using GymBase.Poc.Media.Providers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace GymBase.Poc.Media.Tests.Unit;

public class StorageResolverTests
{
    private readonly StorageResolver _resolver;
    private readonly LocalFileStorage _localStorage;
    private readonly S3FileStorage _s3Storage;

    public StorageResolverTests()
    {
        var options = new StorageOptions
        {
            DefaultProvider = "LOCAL",
            LocalRoot = Path.Combine(Path.GetTempPath(), "test_root")
        };

        _localStorage = new LocalFileStorage(Options.Create(options));
        _s3Storage = new S3FileStorage(Mock.Of<IAmazonS3>(), Options.Create(options), NullLogger<S3FileStorage>.Instance);

        _resolver = new StorageResolver(_localStorage, _s3Storage, Options.Create(options));
    }

    [Fact]
    public void Resolve_Local_ShouldReturnLocalStorage()
    {
        var provider = _resolver.Resolve("LOCAL");
        provider.Should().BeSameAs(_localStorage);
        provider.ProviderName.Should().Be("LOCAL");
    }

    [Fact]
    public void Resolve_S3_ShouldReturnS3Storage()
    {
        var provider = _resolver.Resolve("S3");
        provider.Should().BeSameAs(_s3Storage);
        provider.ProviderName.Should().Be("S3");
    }

    [Fact]
    public void Resolve_UnknownProvider_ShouldThrowNotSupportedException()
    {
        var act = () => _resolver.Resolve("AZURE_BLOB");
        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void GetDefault_ShouldReturnConfiguredDefaultProvider()
    {
        var provider = _resolver.GetDefault();
        provider.ProviderName.Should().Be("LOCAL");
    }
}
