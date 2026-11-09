using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using GymBase.Poc.Media.Configuration;
using GymBase.Poc.Media.Contracts;
using GymBase.Poc.Media.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GymBase.Poc.Media.Tests.Integration;

public class SecurityValidationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly InMemoryMediaStorage _inMemoryStorage = new();

    public SecurityValidationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IMediaStorage>(_inMemoryStorage);
                services.PostConfigure<MediaOptions>(opt =>
                {
                    opt.UseInMemoryStorage = true;
                    opt.AllowAnonymousForPoc = true;
                });
            });
        });
    }

    [Fact]
    public async Task Confirm_WithFakeImage_TextFileDisguisedAsJpeg_ShouldReturnBadRequest()
    {
        var client = _factory.CreateClient();

        // Giả lập hacker tải lên file shell/HTML nhưng đặt Content-Type là image/jpeg
        var fakeBytes = Encoding.UTF8.GetBytes("<?php echo 'malicious backdoor code'; ?>");
        const string stagingKey = "staging/avatar/2026/11/fake-backdoor.tmp";
        _inMemoryStorage.DirectPut(stagingKey, fakeBytes, "image/jpeg");

        var confirmRequest = new ConfirmUploadRequest
        {
            UploadId = "upload-fake",
            StagingKey = stagingKey,
            Purpose = MediaPurpose.Avatar
        };

        var response = await client.PostAsJsonAsync("/api/v1/media/confirm", confirmRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("magic bytes");
    }

    [Fact]
    public async Task Confirm_WithOversizedFile_ShouldReturnConflict_AndCleanUpStaging()
    {
        var client = _factory.CreateClient();

        // Avatar chỉ cho phép tối đa 5MB. Tạo file giả lập 6MB trên staging
        var oversizedBytes = new byte[6 * 1024 * 1024];
        const string stagingKey = "staging/avatar/2026/11/oversized.tmp";
        _inMemoryStorage.DirectPut(stagingKey, oversizedBytes, "image/jpeg");

        var confirmRequest = new ConfirmUploadRequest
        {
            UploadId = "upload-oversized",
            StagingKey = stagingKey,
            Purpose = MediaPurpose.Avatar
        };

        var response = await client.PostAsJsonAsync("/api/v1/media/confirm", confirmRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("vượt quá dung lượng tối đa");

        // Staging file phải bị dọn dẹp ngay để không tốn lưu trữ
        (await _inMemoryStorage.ExistsAsync(stagingKey)).Should().BeFalse();
    }

    [Fact]
    public async Task Confirm_WithPathTraversalAttempt_ShouldReturnForbidden()
    {
        var client = _factory.CreateClient();

        var confirmRequest = new ConfirmUploadRequest
        {
            UploadId = "upload-hack",
            StagingKey = "staging/avatar/../../../etc/passwd",
            Purpose = MediaPurpose.Avatar
        };

        var response = await client.PostAsJsonAsync("/api/v1/media/confirm", confirmRequest);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
