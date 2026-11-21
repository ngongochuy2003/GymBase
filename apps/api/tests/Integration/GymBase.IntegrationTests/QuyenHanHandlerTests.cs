using System.Security.Claims;
using GymBase.Base.Auth;
using GymBase.Base.Hosting.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace GymBase.IntegrationTests;

/// <summary>Handler policy theo hồ sơ phải dừng truy vấn quyền khi client ngắt kết nối.</summary>
public sealed class QuyenHanHandlerTests
{
    private sealed class RecordingProvider : IQuyenHanProvider
    {
        public CancellationToken Seen { get; private set; }

        public Task<QuyenHanDto> BLLayQuyenHanAsync(Guid taiKhoanId, CancellationToken cancellationToken)
        {
            Seen = cancellationToken;
            return Task.FromResult(new QuyenHanDto(true, null, null, false, false, []));
        }
    }

    private static AuthorizationHandlerContext ContextFor(object? resource)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(JwtClaimNames.Subject, Guid.CreateVersion7().ToString())], "test"));
        var requirement = new QuyenHanRequirement(AppPolicies.RequireProfile);
        return new AuthorizationHandlerContext([requirement], user, resource);
    }

    [Fact]
    public async Task Permission_lookup_follows_the_request_abort_token()
    {
        using var aborted = new CancellationTokenSource();
        var provider = new RecordingProvider();

        await new QuyenHanHandler(provider).HandleAsync(ContextFor(new DefaultHttpContext { RequestAborted = aborted.Token }));
        await aborted.CancelAsync();

        Assert.True(provider.Seen.IsCancellationRequested);
    }

    [Fact]
    public async Task Permission_lookup_works_without_an_http_context()
    {
        var context = ContextFor(null);

        await new QuyenHanHandler(new RecordingProvider()).HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }
}
