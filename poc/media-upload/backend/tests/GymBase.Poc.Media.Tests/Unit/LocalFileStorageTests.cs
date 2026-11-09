using System.Text;
using FluentAssertions;
using GymBase.Poc.Media.Configuration;
using GymBase.Poc.Media.Providers;
using Microsoft.Extensions.Options;
using Xunit;

namespace GymBase.Poc.Media.Tests.Unit;

public class LocalFileStorageTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly LocalFileStorage _storage;

    public LocalFileStorageTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "gymbase_test_" + Guid.NewGuid().ToString("N"));
        var options = new StorageOptions { LocalRoot = _tempRoot };
        _storage = new LocalFileStorage(Options.Create(options));
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempRoot))
            {
                Directory.Delete(_tempRoot, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup errors in temp
        }
    }

    [Fact]
    public async Task SaveAsync_And_OpenReadAsync_ShouldPersistFileLocally()
    {
        const string key = "meals/user123/meal-photo.webp";
        var contentBytes = Encoding.UTF8.GetBytes("Fake image content");
        using var saveStream = new MemoryStream(contentBytes);

        await _storage.SaveAsync(key, saveStream, "image/webp");

        var info = await _storage.GetInfoAsync(key);
        info.Should().NotBeNull();
        info!.Length.Should().Be(contentBytes.Length);

        await using var readStream = await _storage.OpenReadAsync(key);
        using var ms = new MemoryStream();
        await readStream.CopyToAsync(ms);

        ms.ToArray().Should().BeEquivalentTo(contentBytes);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveFile()
    {
        const string key = "avatars/user456/avatar.webp";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Avatar content"));

        await _storage.SaveAsync(key, stream, "image/webp");
        (await _storage.GetInfoAsync(key)).Should().NotBeNull();

        await _storage.DeleteAsync(key);
        (await _storage.GetInfoAsync(key)).Should().BeNull();
    }

    [Theory]
    [InlineData("../etc/passwd")]
    [InlineData("..\\windows\\win.ini")]
    [InlineData("/absolute/path")]
    [InlineData("meals/../../secret.txt")]
    [InlineData("meals//double-slash.txt")]
    [InlineData("meals/./current.txt")]
    [InlineData("")]
    public void ResolvePath_WithPathTraversal_ShouldThrowArgumentException(string illegalKey)
    {
        var act = () => _storage.ResolvePath(illegalKey);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid storage key*");
    }
}
