using System.Net;
using System.Text;
using System.Text.Json;
using GymBase.BLThongBao.BLDayThongBao;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace GymBase.UnitTests.ThongBao;

public sealed class FcmPushSenderTests
{
    private static readonly Guid ThongBaoId = Guid.Parse("0193f0a0-0000-7000-8000-000000000001");
    private static readonly DateTime Now = new(2026, 11, 24, 12, 0, 0, DateTimeKind.Utc);

    private sealed class StubHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> tra) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Request = request;
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(ct);
            return await tra(request);
        }
    }

    private sealed class FakeTokens(Func<Task<string>>? lay = null) : IFcmAccessTokenProvider
    {
        public Task<string> GetAccessTokenAsync(CancellationToken ct) => lay is null ? Task.FromResult("access-token-1") : lay();
    }

    private sealed class FixedTime : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(Now);
    }

    private static HttpResponseMessage Json(HttpStatusCode code, string json) =>
        new(code) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private static HttpResponseMessage Loi(HttpStatusCode code, string status, string? errorCode = null, string message = "loi") =>
        Json(code, JsonSerializer.Serialize(new
        {
            error = new
            {
                code = (int)code, message, status,
                details = errorCode is null
                    ? Array.Empty<object>()
                    : [new Dictionary<string, string> { ["@type"] = "type.googleapis.com/google.firebase.fcm.v1.FcmError", ["errorCode"] = errorCode }],
            },
        }));

    private static (FcmPushSender Sender, StubHandler Handler) Tao(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> tra, IFcmAccessTokenProvider? tokens = null)
    {
        var handler = new StubHandler(tra);
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://fcm.googleapis.com/") };
        var options = Options.Create(new ThongBaoOptions { Fcm = { ProjectId = "gymbase-dev" } });
        return (new FcmPushSender(http, tokens ?? new FakeTokens(), options, new FixedTime(), NullLogger<FcmPushSender>.Instance), handler);
    }

    private static TinPush Tin(
        string token = "token-a", string loai = "tin_nhan", UuTienPush uuTien = UuTienPush.Cao, TimeSpan? ttl = null) =>
        new(token, ThongBaoId, loai, "tin_nhan", Guid.Parse("0193f0a0-0000-7000-8000-000000000002"), "Bạn có tin nhắn mới", uuTien, ttl ?? TimeSpan.FromHours(1));

    private static JsonElement Message(StubHandler h) => JsonDocument.Parse(h.Body!).RootElement.GetProperty("message");

    [Fact]
    public async Task Success_PostsToTheProjectEndpoint_WithBearerToken_AndReturnsTheMessageName()
    {
        var (sender, handler) = Tao(_ => Task.FromResult(Json(HttpStatusCode.OK, """{"name":"projects/gymbase-dev/messages/0:123"}""")));

        var ketQua = await sender.GuiAsync(Tin());

        Assert.Equal(KetQuaDay.ThanhCong, ketQua.Ket);
        Assert.Equal("projects/gymbase-dev/messages/0:123", ketQua.MaTin);
        Assert.Equal(HttpMethod.Post, handler.Request!.Method);
        Assert.Equal("https://fcm.googleapis.com/v1/projects/gymbase-dev/messages:send", handler.Request.RequestUri!.ToString());
        Assert.Equal("Bearer", handler.Request.Headers.Authorization!.Scheme);
        Assert.Equal("access-token-1", handler.Request.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task Payload_IsNeutral_TitleOnly_AndDataValuesAreStrings()
    {
        var (sender, handler) = Tao(_ => Task.FromResult(Json(HttpStatusCode.OK, """{"name":"n"}""")));

        await sender.GuiAsync(Tin());

        var message = Message(handler);
        Assert.Equal("token-a", message.GetProperty("token").GetString());
        var notification = message.GetProperty("notification");
        Assert.Equal("Bạn có tin nhắn mới", notification.GetProperty("title").GetString());
        Assert.False(notification.TryGetProperty("body", out _));

        var data = message.GetProperty("data");
        Assert.Equal(ThongBaoId.ToString(), data.GetProperty("thong_bao_id").GetString());
        Assert.Equal("tin_nhan", data.GetProperty("loai_thong_bao").GetString());
        Assert.Equal("tin_nhan", data.GetProperty("loai_doi_tuong").GetString());
        Assert.Equal("0193f0a0-0000-7000-8000-000000000002", data.GetProperty("doi_tuong_id").GetString());
        Assert.All(data.EnumerateObject(), p => Assert.Equal(JsonValueKind.String, p.Value.ValueKind));
    }

    [Fact]
    public async Task Android_UsesTheTypeAsChannel_AndHighPriorityWithTtl()
    {
        var (sender, handler) = Tao(_ => Task.FromResult(Json(HttpStatusCode.OK, """{"name":"n"}""")));

        await sender.GuiAsync(Tin(loai: "nhac_tap", uuTien: UuTienPush.Cao, ttl: TimeSpan.FromMinutes(10)));

        var android = Message(handler).GetProperty("android");
        Assert.Equal("HIGH", android.GetProperty("priority").GetString());
        Assert.Equal("600s", android.GetProperty("ttl").GetString());
        var notification = android.GetProperty("notification");
        Assert.Equal("nhac_tap", notification.GetProperty("channel_id").GetString());
        Assert.Equal("PUBLIC", notification.GetProperty("visibility").GetString());
    }

    [Fact]
    public async Task NormalPriority_MapsToNormal_OnAndroidAndApns()
    {
        var (sender, handler) = Tao(_ => Task.FromResult(Json(HttpStatusCode.OK, """{"name":"n"}""")));

        await sender.GuiAsync(Tin(loai: "danh_gia", uuTien: UuTienPush.Thuong, ttl: TimeSpan.FromDays(1)));

        var message = Message(handler);
        Assert.Equal("NORMAL", message.GetProperty("android").GetProperty("priority").GetString());
        Assert.Equal("86400s", message.GetProperty("android").GetProperty("ttl").GetString());
        var apns = message.GetProperty("apns").GetProperty("headers");
        Assert.Equal("5", apns.GetProperty("apns-priority").GetString());
        Assert.Equal("alert", apns.GetProperty("apns-push-type").GetString());
        Assert.Equal(new DateTimeOffset(Now.AddDays(1)).ToUnixTimeSeconds().ToString(), apns.GetProperty("apns-expiration").GetString());
    }

    [Fact]
    public async Task Apns_HighPriority_Is10()
    {
        var (sender, handler) = Tao(_ => Task.FromResult(Json(HttpStatusCode.OK, """{"name":"n"}""")));

        await sender.GuiAsync(Tin(uuTien: UuTienPush.Cao));

        Assert.Equal("10", Message(handler).GetProperty("apns").GetProperty("headers").GetProperty("apns-priority").GetString());
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, "NOT_FOUND", "UNREGISTERED", "Requested entity was not found.", KetQuaDay.TokenChet)]
    [InlineData(HttpStatusCode.NotFound, "NOT_FOUND", null, "Requested entity was not found.", KetQuaDay.TokenChet)]
    [InlineData(HttpStatusCode.BadRequest, "INVALID_ARGUMENT", "INVALID_ARGUMENT", "The registration token is not a valid FCM registration token", KetQuaDay.TokenChet)]
    [InlineData(HttpStatusCode.BadRequest, "INVALID_ARGUMENT", "INVALID_ARGUMENT", "Invalid JSON payload received.", KetQuaDay.LoiCauHinh)]
    [InlineData(HttpStatusCode.Forbidden, "PERMISSION_DENIED", "SENDER_ID_MISMATCH", "SenderId mismatch", KetQuaDay.LoiCauHinh)]
    [InlineData(HttpStatusCode.Forbidden, "PERMISSION_DENIED", null, "The caller does not have permission", KetQuaDay.LoiCauHinh)]
    [InlineData(HttpStatusCode.Unauthorized, "UNAUTHENTICATED", "THIRD_PARTY_AUTH_ERROR", "auth", KetQuaDay.LoiCauHinh)]
    [InlineData(HttpStatusCode.TooManyRequests, "RESOURCE_EXHAUSTED", "QUOTA_EXCEEDED", "quota", KetQuaDay.LoiTam)]
    [InlineData(HttpStatusCode.ServiceUnavailable, "UNAVAILABLE", "UNAVAILABLE", "unavailable", KetQuaDay.LoiTam)]
    [InlineData(HttpStatusCode.InternalServerError, "INTERNAL", "INTERNAL", "internal", KetQuaDay.LoiTam)]
    public async Task FcmErrors_AreMappedToTheFourOutcomes(
        HttpStatusCode code, string status, string? errorCode, string message, KetQuaDay mongDoi)
    {
        var (sender, _) = Tao(_ => Task.FromResult(Loi(code, status, errorCode, message)));

        var ketQua = await sender.GuiAsync(Tin());

        Assert.Equal(mongDoi, ketQua.Ket);
        Assert.False(string.IsNullOrEmpty(ketQua.Loi));
    }

    [Fact]
    public async Task NonJsonErrorBody_FromAProxyOr5xx_IsTransient()
    {
        var (sender, _) = Tao(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadGateway) { Content = new StringContent("<html>Bad gateway</html>") }));

        Assert.Equal(KetQuaDay.LoiTam, (await sender.GuiAsync(Tin())).Ket);
    }

    [Fact]
    public async Task NetworkFailure_AndTimeout_AreTransient()
    {
        var (mang, _) = Tao(_ => throw new HttpRequestException("mạng đứt"));
        var (cham, _) = Tao(_ => throw new TaskCanceledException("timeout"));

        Assert.Equal(KetQuaDay.LoiTam, (await mang.GuiAsync(Tin())).Ket);
        Assert.Equal(KetQuaDay.LoiTam, (await cham.GuiAsync(Tin())).Ket);
    }

    [Fact]
    public async Task CancellationByTheCaller_IsNotSwallowed()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var (sender, _) = Tao(_ => throw new OperationCanceledException(cts.Token));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sender.GuiAsync(Tin(), cts.Token));
    }

    [Fact]
    public async Task AccessTokenFailure_IsAConfigError_ButNetworkFailureThereIsTransient()
    {
        var (hong, handlerHong) = Tao(_ => Task.FromResult(Json(HttpStatusCode.OK, "{}")), new FakeTokens(() => throw new InvalidOperationException("invalid_grant")));
        var (mang, _) = Tao(_ => Task.FromResult(Json(HttpStatusCode.OK, "{}")), new FakeTokens(() => throw new HttpRequestException("oauth2 không tới được")));

        Assert.Equal(KetQuaDay.LoiCauHinh, (await hong.GuiAsync(Tin())).Ket);
        Assert.Null(handlerHong.Request);
        Assert.Equal(KetQuaDay.LoiTam, (await mang.GuiAsync(Tin())).Ket);
    }

    [Fact]
    public async Task ErrorText_NeverContainsThePushToken()
    {
        var (sender, _) = Tao(_ => Task.FromResult(Loi(HttpStatusCode.NotFound, "NOT_FOUND", "UNREGISTERED", "token-bi-mat not found")));

        var ketQua = await sender.GuiAsync(Tin(token: "token-bi-mat"));

        Assert.DoesNotContain("token-bi-mat", ketQua.Loi);
    }
}
