using GymBase.Base.Auth;
using GymBase.Base.Entities;
using GymBase.Base.Exceptions;
using GymBase.BLAuth;
using GymBase.BLAuth.BLXacThuc;
using GymBase.Entities.Auth;
using GymBase.TestDoubles;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace GymBase.UnitTests.Auth;

internal static class AuthTestHelpers
{
    public const string ValidPassword = "Mk-thu-nghiem-1";

    /// <summary>Ít vòng để test chạy nhanh; bản chạy thật dùng 600.000 vòng.</summary>
    public static IPasswordHasher<tai_khoan> FastHasher { get; } =
        new PasswordHasher<tai_khoan>(Options.Create(new PasswordHasherOptions { IterationCount = 1_000 }));
}

/// <summary>Luồng đăng nhập, xoay vòng refresh token, khóa khi sai nhiều, đổi mật khẩu.</summary>
public class XacThucServiceTests
{
    private const string UserName = "hv.dangnhap";
    private const string Password = AuthTestHelpers.ValidPassword;

    private sealed class MovableClock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 11, 20, 8, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class FakeIssuer : ITokenIssuer
    {
        public AccessTokenRequest? Last { get; private set; }

        public IssuedAccessToken CreateAccessToken(AccessTokenRequest request)
        {
            Last = request;
            return new IssuedAccessToken("access-token", 900);
        }
    }

    private readonly MovableClock _clock = new();
    private readonly FakeIssuer _issuer = new();
    private readonly InMemoryStore<tai_khoan> _userStore = new();
    private readonly InMemoryStore<phien_dang_nhap> _sessionStore = new();
    private readonly LoginThrottle _throttle;
    private readonly tai_khoan _user;

    public XacThucServiceTests()
    {
        _throttle = new LoginThrottle(_clock);
        _ = new InMemoryTaiKhoanRepository(_userStore); // nạp dữ liệu mẫu
        _user = new tai_khoan { ten_dang_nhap = UserName, ho_ten = "Học viên", vai_tro = AppRoles.User };
        _user.mat_khau_hash = AuthTestHelpers.FastHasher.HashPassword(_user, Password);
        _user.MarkCreated("seed", _clock.Now.UtcDateTime);
        _userStore.TryAdd(_user);
    }

    private XacThucService Service(FakeCurrentUser? currentUser = null) => new(
        new InMemoryTaiKhoanRepository(_userStore),
        new InMemoryPhienDangNhapRepository(_sessionStore),
        AuthTestHelpers.FastHasher,
        _issuer,
        _throttle,
        currentUser ?? new FakeCurrentUser(),
        _clock,
        new DangNhapValidator(),
        new LamMoiTokenValidator(),
        new DoiMatKhauValidator(),
        new DatLaiMatKhauValidator());

    private Task<KetQuaDangNhapDto> Login(string userName = UserName, string password = Password) =>
        Service().BLLoginAsync(new DangNhapDto { ten_dang_nhap = userName, mat_khau = password }, CancellationToken.None);

    private Task<KetQuaDangNhapDto> Refresh(string token) =>
        Service().BLRefreshAsync(new LamMoiTokenDto { refresh_token = token }, CancellationToken.None);

    private async Task AssertLoginRejected(string userName = UserName, string password = Password)
    {
        var error = await Assert.ThrowsAsync<UnauthorizedException>(() => Login(userName, password));
        Assert.Equal(AuthErrorCodes.InvalidCredentials, error.Code);
        Assert.Equal("Tên đăng nhập hoặc mật khẩu không đúng.", error.Message);
    }

    private void Advance(TimeSpan span) => _clock.Now += span;

    [Fact]
    public async Task Login_returns_tokens_and_user_and_stores_only_the_hash()
    {
        var result = await Login();

        Assert.Equal("access-token", result.access_token);
        Assert.Equal(UserName, result.user.ten_dang_nhap);
        Assert.Equal(_user.Id, _issuer.Last!.UserId);
        Assert.Equal(AppRoles.User, _issuer.Last.Role);
        var session = Assert.Single(_sessionStore.All());
        Assert.Equal(session.Id, _issuer.Last.SessionId);
        Assert.NotEqual(result.refresh_token, session.token_hash);
        Assert.Equal(64, session.token_hash.Length);
    }

    [Fact]
    public async Task Login_ignores_username_case_and_spaces()
    {
        var result = await Login("  HV.DANGNHAP ");

        Assert.Equal(UserName, result.user.ten_dang_nhap);
    }

    [Fact]
    public async Task Wrong_password_and_unknown_user_give_the_same_error()
    {
        await AssertLoginRejected(password: "sai-mat-khau-nay");
        await AssertLoginRejected(userName: "khong.ton.tai");
    }

    [Fact]
    public async Task Disabled_user_gets_the_same_error()
    {
        _user.dang_hoat_dong = false;
        _userStore.TryReplace(_user);

        await AssertLoginRejected();
    }

    [Fact]
    public async Task User_with_malformed_or_empty_hash_cannot_log_in()
    {
        await AssertLoginRejected("admin", "mock-not-a-real-hash");

        _user.mat_khau_hash = string.Empty;
        _userStore.TryReplace(_user);
        await AssertLoginRejected();
    }

    [Fact]
    public async Task Fifth_failure_locks_even_the_correct_password()
    {
        for (var i = 0; i < 5; i++)
        {
            await AssertLoginRejected(password: "sai-mat-khau-nay");
        }

        await AssertLoginRejected();

        Advance(TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(1));
        Assert.Equal(UserName, (await Login()).user.ten_dang_nhap);
    }

    [Fact]
    public async Task Unknown_username_is_throttled_the_same_way()
    {
        for (var i = 0; i < 5; i++)
        {
            await AssertLoginRejected(userName: "khong.ton.tai", password: "sai-mat-khau-nay");
        }

        Assert.True(_throttle.IsLocked("khong.ton.tai"));
    }

    [Fact]
    public async Task Lock_grows_with_repeated_failures_up_to_fifteen_minutes()
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            _throttle.RecordFailure(UserName);
        }

        Advance(TimeSpan.FromMinutes(14));
        Assert.True(_throttle.IsLocked(UserName));

        Advance(TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(1));
        Assert.False(_throttle.IsLocked(UserName));
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Successful_login_clears_failures()
    {
        for (var i = 0; i < 4; i++)
        {
            await AssertLoginRejected(password: "sai-mat-khau-nay");
        }

        await Login();
        await AssertLoginRejected(password: "sai-mat-khau-nay");

        Assert.False(_throttle.IsLocked(UserName));
    }

    [Fact]
    public async Task Refresh_rotates_the_token()
    {
        var login = await Login();

        var refreshed = await Refresh(login.refresh_token);

        Assert.NotEqual(login.refresh_token, refreshed.refresh_token);
        Assert.Single(_sessionStore.All());
    }

    [Fact]
    public async Task Previous_token_within_grace_is_a_retry_and_does_not_revoke()
    {
        var login = await Login();
        var first = await Refresh(login.refresh_token);

        Advance(TimeSpan.FromSeconds(10));
        var retried = await Refresh(login.refresh_token);

        Assert.NotEqual(first.refresh_token, retried.refresh_token);
        Assert.Null(_sessionStore.All().Single().ngay_thu_hoi);
        await Refresh(retried.refresh_token);
    }

    [Fact]
    public async Task Grace_does_not_extend_with_repeated_retries()
    {
        var login = await Login();
        await Refresh(login.refresh_token);

        Advance(TimeSpan.FromSeconds(20));
        await Refresh(login.refresh_token);

        Advance(TimeSpan.FromSeconds(20));
        await Assert.ThrowsAsync<UnauthorizedException>(() => Refresh(login.refresh_token));
    }

    [Fact]
    public async Task Previous_token_after_grace_revokes_the_session()
    {
        var login = await Login();
        var current = await Refresh(login.refresh_token);

        Advance(TimeSpan.FromMinutes(1));
        await Assert.ThrowsAsync<UnauthorizedException>(() => Refresh(login.refresh_token));

        Assert.NotNull(_sessionStore.All().Single().ngay_thu_hoi);
        await Assert.ThrowsAsync<UnauthorizedException>(() => Refresh(current.refresh_token));
    }

    [Fact]
    public async Task Unknown_refresh_token_is_rejected()
    {
        await Assert.ThrowsAsync<UnauthorizedException>(() => Refresh("khong-ton-tai"));
    }

    [Fact]
    public async Task Session_expires_after_thirty_days_of_inactivity()
    {
        var login = await Login();

        Advance(TimeSpan.FromDays(30));

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refresh(login.refresh_token));
    }

    [Fact]
    public async Task Using_the_session_extends_it_but_never_past_ninety_days()
    {
        var token = (await Login()).refresh_token;

        for (var i = 0; i < 3; i++)
        {
            Advance(TimeSpan.FromDays(29));
            token = (await Refresh(token)).refresh_token;
        }

        Advance(TimeSpan.FromDays(3));

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refresh(token));
    }

    [Fact]
    public async Task Disabled_user_cannot_refresh()
    {
        var login = await Login();
        var user = _userStore.Find(_user.Id)!;
        user.dang_hoat_dong = false;
        _userStore.TryReplace(user);

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refresh(login.refresh_token));
    }

    [Fact]
    public async Task Logout_revokes_the_current_session()
    {
        var login = await Login();
        var sessionId = _issuer.Last!.SessionId;

        await Service(new FakeCurrentUser(UserName, _user.Id, sessionId)).BLLogoutAsync(CancellationToken.None);

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refresh(login.refresh_token));
    }

    [Fact]
    public async Task Change_password_keeps_this_session_and_revokes_others()
    {
        var other = await Login();
        var current = await Login();
        var service = Service(new FakeCurrentUser(UserName, _user.Id, _issuer.Last!.SessionId));

        await service.BLChangePasswordAsync(new DoiMatKhauDto { mat_khau_cu = Password, mat_khau_moi = "Mat-khau-moi-2" }, CancellationToken.None);

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refresh(other.refresh_token));
        await Refresh(current.refresh_token);
        await AssertLoginRejected();
        await Login(password: "Mat-khau-moi-2");
    }

    [Fact]
    public async Task Change_password_requires_the_current_password()
    {
        var service = Service(new FakeCurrentUser(UserName, _user.Id));

        var error = await Assert.ThrowsAsync<ValidationFailedException>(() => service.BLChangePasswordAsync(
            new DoiMatKhauDto { mat_khau_cu = "khong-dung-roi", mat_khau_moi = "Mat-khau-moi-2" }, CancellationToken.None));

        Assert.True(error.Errors!.ContainsKey("mat_khau_cu"));
    }

    [Theory]
    [InlineData("ngan")]              // dưới 8 ký tự
    [InlineData("12345678")]          // phổ biến
    [InlineData("PASSWORD1")]         // phổ biến, khác hoa thường
    [InlineData("xx-hv.dangnhap-xx")] // chứa tên đăng nhập
    public async Task Weak_new_password_is_rejected(string newPassword)
    {
        var service = Service(new FakeCurrentUser(UserName, _user.Id));

        var error = await Assert.ThrowsAsync<ValidationFailedException>(() => service.BLChangePasswordAsync(
            new DoiMatKhauDto { mat_khau_cu = Password, mat_khau_moi = newPassword }, CancellationToken.None));

        Assert.True(error.Errors!.ContainsKey("mat_khau_moi"));
    }

    [Fact]
    public async Task Password_of_eight_characters_is_accepted()
    {
        var service = Service(new FakeCurrentUser(UserName, _user.Id));

        await service.BLChangePasswordAsync(new DoiMatKhauDto { mat_khau_cu = Password, mat_khau_moi = "kH7#mQ2z" }, CancellationToken.None);

        await Login(password: "kH7#mQ2z");
    }

    [Fact]
    public async Task Admin_reset_revokes_every_session()
    {
        var login = await Login();

        await Service().BLResetPasswordAsync(_user.Id, new DatLaiMatKhauDto { mat_khau_moi = "Mat-khau-moi-2" }, CancellationToken.None);

        await Assert.ThrowsAsync<UnauthorizedException>(() => Refresh(login.refresh_token));
    }

    [Fact]
    public async Task Create_admin_creates_a_new_admin_that_can_log_in()
    {
        await Service().BLCreateOrResetAdminAsync("quan.tri", "Mat-khau-moi-2", CancellationToken.None);

        var result = await Login("quan.tri", "Mat-khau-moi-2");

        Assert.Equal(AppRoles.Admin, result.user.vai_tro);
    }

    [Fact]
    public async Task Hash_with_fewer_iterations_is_upgraded_on_login()
    {
        var stronger = new PasswordHasher<tai_khoan>(Options.Create(new PasswordHasherOptions { IterationCount = 2_000 }));
        var service = new XacThucService(
            new InMemoryTaiKhoanRepository(_userStore), new InMemoryPhienDangNhapRepository(_sessionStore), stronger, _issuer, _throttle,
            new FakeCurrentUser(), _clock, new DangNhapValidator(), new LamMoiTokenValidator(), new DoiMatKhauValidator(), new DatLaiMatKhauValidator());
        var before = _userStore.Find(_user.Id)!.mat_khau_hash;

        await service.BLLoginAsync(new DangNhapDto { ten_dang_nhap = UserName, mat_khau = Password }, CancellationToken.None);

        Assert.NotEqual(before, _userStore.Find(_user.Id)!.mat_khau_hash);
    }
}
