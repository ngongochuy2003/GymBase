using GymBase.BLFile.Storage;
using Microsoft.Extensions.Options;
using Xunit;

namespace GymBase.UnitTests.File;

public class LocalFileStorageTests : IDisposable
{
    private readonly string _testTempDir;
    private readonly LocalFileStorage _storage;

    public LocalFileStorageTests()
    {
        _testTempDir = Path.Combine(Path.GetTempPath(), "gymbase_local_storage_tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testTempDir);

        var options = new StorageOptions
        {
            DefaultProvider = "LOCAL",
            LocalRoot = _testTempDir
        };

        _storage = new LocalFileStorage(Options.Create(options));
    }

    [Fact]
    public async Task SaveAsync_And_OpenReadAsync_ShouldWorkCorrectly()
    {
        var key = "avatar/user1/test.webp";
        var content = new MemoryStream("Fake image bytes"u8.ToArray());

        await _storage.SaveAsync(key, content, "image/webp");

        var readStream = await _storage.OpenReadAsync(key);
        using var ms = new MemoryStream();
        await readStream.CopyToAsync(ms);
        var readText = System.Text.Encoding.UTF8.GetString(ms.ToArray());

        Assert.Equal("Fake image bytes", readText);
    }

    [Fact]
    public async Task DeleteAsync_ExistingFile_ShouldRemoveFromDisk()
    {
        var key = "avatar/user1/to_delete.webp";
        var content = new MemoryStream("Delete me"u8.ToArray());

        await _storage.SaveAsync(key, content, "image/webp");
        await _storage.DeleteAsync(key);

        await Assert.ThrowsAsync<FileNotFoundException>(() => _storage.OpenReadAsync(key));
    }

    [Theory]
    [InlineData("../secret.txt")]
    [InlineData("avatar/../../etc/passwd")]
    [InlineData("..\\windows\\system32")]
    [InlineData("/absolute/path")]
    [InlineData("avatar/user/../../../config.json")]
    [InlineData("C:\\boot.ini")]
    public void ResolveAndValidatePath_PathTraversal_ShouldThrowUnauthorizedAccessException(string maliciousKey)
    {
        Assert.Throws<UnauthorizedAccessException>(() => _storage.ResolveAndValidatePath(maliciousKey));
    }

    public void Dispose()
    {
        if (Directory.Exists(_testTempDir))
        {
            try { Directory.Delete(_testTempDir, true); } catch { }
        }
    }
}
