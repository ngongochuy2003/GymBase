using System.Net;
using System.Net.Http.Json;
using GymBase.Base.Auth;
using GymBase.TestDoubles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace GymBase.IntegrationTests;

/// <summary>Audit log của các request thay đổi dữ liệu, qua pipeline thật của AddAppApi / UseAppApi.</summary>
public sealed class AuditLogTests : IAsyncLifetime
{
    private const string Users = "/api/v1/web/testusers";

    private static readonly object ValidUser =
        new { ten_dang_nhap = "hv.audit", email = "hv.audit@gymbase.test", mat_khau = "Mk-thu-nghiem-1", ho_ten = "Thử", vai_tro = "User" };

    private WebApplication _app = null!;
    private HttpClient _client = null!;
    private RecordingAuditLogWriter _audit = null!;

    public async Task InitializeAsync()
    {
        _app = await TestApp.StartAsync();
        _client = _app.GetTestClient();
        _audit = _app.Services.GetRequiredService<RecordingAuditLogWriter>();
    }

    public async Task DisposeAsync() => await _app.DisposeAsync();

    [Fact]
    public async Task Successful_write_records_method_path_status_and_utc_time()
    {
        var response = await _client.PostAsJsonAsync(Users + "?bi_mat=abc", ValidUser);

        var entry = await _audit.NextAsync();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("POST", entry.Method);
        Assert.Equal(Users, entry.Path);
        Assert.Equal(201, entry.StatusCode);
        Assert.True(entry.DurationMs >= 0);
        Assert.NotEmpty(entry.TraceId);
        Assert.Equal(DateTimeKind.Utc, entry.OccurredAtUtc.Kind);
        Assert.Null(entry.UserName);
    }

    [Fact]
    public async Task Business_error_records_final_status()
    {
        var response = await _client.DeleteAsync($"{Users}/{Guid.CreateVersion7()}");

        var entry = await _audit.NextAsync();
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(404, entry.StatusCode);
    }

    [Fact]
    public async Task Signed_in_user_is_recorded()
    {
        var client = TestApp.ClientWithRole(_app, AppRoles.Admin);

        await client.PostAsJsonAsync(Users, ValidUser);

        Assert.Equal("test.user", (await _audit.NextAsync()).UserName);
    }

    [Fact]
    public async Task Read_request_is_not_recorded()
    {
        var read = await _client.GetAsync(Users);
        Assert.Equal(HttpStatusCode.OK, read.StatusCode);

        // Ghi thêm một lệnh: bản ghi đầu tiên phải là lệnh này, không phải GET ở trên.
        await _client.PostAsJsonAsync(Users, ValidUser);

        Assert.Equal("POST", (await _audit.NextAsync()).Method);
        Assert.False(_audit.HasPending);
    }

    [Fact]
    public async Task Writer_failure_does_not_affect_response()
    {
        _audit.FailWith = new InvalidOperationException("DB lỗi");

        var response = await _client.PostAsJsonAsync(Users, ValidUser);

        await _audit.NextAsync();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
