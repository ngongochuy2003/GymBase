using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GymBase.Base.Hosting;
using GymBase.Base.Hosting.Errors;
using GymBase.Base.Exceptions;
using GymBase.TestDoubles;
using GymBase.BLAuth;
using GymBase.BLAuth.BLTaiKhoan;
using GymBase.Entities.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GymBase.IntegrationTests;

/// <summary>
/// Gọi HTTP thật qua TestServer với cấu hình xử lý lỗi giống GymBase.API (AddAppErrorHandling,
/// UseAppErrorHandling) để kiểm tra dạng ProblemDetails theo docs/api/quy-uoc-response.md.
/// </summary>
public sealed class ErrorHandlingTests : IAsyncLifetime
{
    private const string Users = "/api/v1/web/testusers";
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _app = await TestApp.StartAsync();
        _client = _app.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    [Fact]
    public async Task Not_found_returns_problem_details_with_code_and_trace_id()
    {
        var response = await _client.GetAsync($"{Users}/{Guid.NewGuid()}");

        var problem = await ReadProblemAsync(response, HttpStatusCode.NotFound);
        Assert.Equal(ErrorCodes.NotFound, problem.GetProperty("code").GetString());
        Assert.Equal("Không tìm thấy", problem.GetProperty("title").GetString());
        Assert.Equal("Không tìm thấy người dùng hoặc người dùng đã bị xóa.", problem.GetProperty("detail").GetString());
        Assert.StartsWith(Users, problem.GetProperty("instance").GetString());
        Assert.False(string.IsNullOrEmpty(problem.GetProperty("trace_id").GetString()));
        Assert.False(problem.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task Duplicate_username_returns_409_with_detail_code_and_field_error()
    {
        var response = await _client.PostAsJsonAsync(Users, new
        {
            ten_dang_nhap = "pt.minh",
            email = "trung.ten@gymbase.test",
            mat_khau = "Mk-thu-nghiem-1",
            ho_ten = "Trùng tên",
            vai_tro = "User",
        });

        var problem = await ReadProblemAsync(response, HttpStatusCode.Conflict);
        Assert.Equal(AuthErrorCodes.UserUsernameTaken, problem.GetProperty("code").GetString());
        Assert.Equal("Tên đăng nhập 'pt.minh' đã tồn tại.", problem.GetProperty("detail").GetString());
        Assert.Equal(1, problem.GetProperty("errors").GetProperty("ten_dang_nhap").GetArrayLength());
    }

    [Fact]
    public async Task Invalid_body_from_framework_gets_same_shape()
    {
        var response = await _client.PostAsJsonAsync(Users, new { ho_ten = "Thiếu tài khoản", vai_tro = "User" });

        var problem = await ReadProblemAsync(response, HttpStatusCode.BadRequest);
        Assert.Equal(ErrorCodes.ValidationFailed, problem.GetProperty("code").GetString());
        Assert.Equal("Dữ liệu không hợp lệ", problem.GetProperty("title").GetString());
        Assert.Equal(ValidationFailedException.DefaultDetail, problem.GetProperty("detail").GetString());
        Assert.True(problem.GetProperty("errors").TryGetProperty("ten_dang_nhap", out _));
        Assert.False(string.IsNullOrEmpty(problem.GetProperty("trace_id").GetString()));
        Assert.False(problem.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task Business_rule_returns_422_with_custom_code()
    {
        var response = await _client.GetAsync("/api/v1/web/errortest/rule");

        var problem = await ReadProblemAsync(response, HttpStatusCode.UnprocessableEntity);
        Assert.Equal("MEMBERSHIP_EXPIRED", problem.GetProperty("code").GetString());
        Assert.Equal("Gói tập đã hết hạn.", problem.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Unexpected_exception_returns_500_without_internal_details()
    {
        var response = await _client.GetAsync("/api/v1/web/errortest/boom");

        var body = await response.Content.ReadAsStringAsync();
        var problem = await ReadProblemAsync(response, HttpStatusCode.InternalServerError);
        Assert.Equal(ErrorCodes.InternalError, problem.GetProperty("code").GetString());
        Assert.Equal(ProblemDetailsCustomizer.InternalErrorDetail, problem.GetProperty("detail").GetString());
        Assert.DoesNotContain("bí mật", body);
        Assert.DoesNotContain("InvalidOperationException", body);
    }

    [Fact]
    public async Task Unknown_route_returns_problem_details()
    {
        using var client = TestApp.ClientWithRole(_app, "User");

        var response = await client.GetAsync("/api/v1/web/khong-co");

        var problem = await ReadProblemAsync(response, HttpStatusCode.NotFound);
        Assert.Equal(ErrorCodes.NotFound, problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Success_still_returns_plain_dto()
    {
        var response = await _client.GetAsync($"{Users}/0193f0a0-0000-7000-8000-000000000001");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("admin", user.GetProperty("ten_dang_nhap").GetString());
    }

    [Theory]
    [InlineData(BusinessErrorKind.Validation, 400)]
    [InlineData(BusinessErrorKind.Unauthorized, 401)]
    [InlineData(BusinessErrorKind.Forbidden, 403)]
    [InlineData(BusinessErrorKind.NotFound, 404)]
    [InlineData(BusinessErrorKind.Conflict, 409)]
    [InlineData(BusinessErrorKind.BusinessRule, 422)]
    public void Each_error_kind_maps_to_its_status_code(BusinessErrorKind kind, int status) =>
        Assert.Equal(status, AppExceptionHandler.ToStatusCode(kind));

    private static async Task<JsonElement> ReadProblemAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        Assert.Equal(expected, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal((int)expected, problem.GetProperty("status").GetInt32());
        return problem;
    }
}
