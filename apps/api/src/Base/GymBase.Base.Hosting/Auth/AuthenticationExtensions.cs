using System.Threading.RateLimiting;
using GymBase.Base.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace GymBase.Base.Hosting.Auth;

public static class AuthenticationExtensions
{
    /// <summary>Tên chính sách giới hạn số lần gọi cho các endpoint nhạy cảm như đăng nhập.</summary>
    public const string AuthRateLimitPolicy = "auth";

    private const int AuthRequestsPerMinute = 20;

    /// <summary>
    /// Kiểm tra JWT, đặt mặc định mọi endpoint phải đăng nhập, đăng ký cấp token và giới hạn số lần gọi.
    /// Endpoint công khai phải ghi rõ <c>[AllowAnonymous]</c>.
    /// </summary>
    public static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = JwtSettings.From(configuration);

        services.AddSingleton(settings);
        services.AddSingleton<ITokenIssuer, JwtTokenIssuer>();
        services.AddSingleton<IGoogleIdTokenVerifier, GoogleIdTokenVerifier>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = settings.Issuer,
                ValidateAudience = true,
                ValidAudience = settings.Audience,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(settings.SigningKey),
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                ValidTypes = [JwtSettings.TokenType],
                ClockSkew = TimeSpan.FromSeconds(30),
                NameClaimType = JwtClaimNames.UserName,
                RoleClaimType = JwtClaimNames.Role,
            };
        });

        // Policy theo hồ sơ cần IQuyenHanProvider (BLBiz đăng ký); chỉ được gọi khi endpoint dùng policy đó.
        services.AddScoped<IAuthorizationHandler, QuyenHanHandler>();
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
            .AddPolicy(AppPolicies.RequireAdmin, policy => policy.RequireRole(AppRoles.Admin))
            .AddPolicy(AppPolicies.RequireProfile, policy => policy.RequireAuthenticatedUser()
                .AddRequirements(new QuyenHanRequirement(AppPolicies.RequireProfile)))
            .AddPolicy(AppPolicies.RequirePtProfile, policy => policy.RequireAuthenticatedUser()
                .AddRequirements(new QuyenHanRequirement(AppPolicies.RequirePtProfile)))
            .AddPolicy(AppPolicies.RequireVerifiedPt, policy => policy.RequireAuthenticatedUser()
                .AddRequirements(new QuyenHanRequirement(AppPolicies.RequireVerifiedPt)));

        // TODO(HOST): sau Nginx phải bật ForwardedHeaders, nếu không mọi client chung một IP của proxy. Mục 14.
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(AuthRateLimitPolicy, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = AuthRequestsPerMinute,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                }));
        });

        return services;
    }
}
