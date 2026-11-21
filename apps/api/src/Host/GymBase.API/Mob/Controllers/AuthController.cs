using GymBase.API.Controllers;
using GymBase.Base.Auth;
using GymBase.BLAuth.BLXacThuc;

namespace GymBase.API.Mob.Controllers;

/// <summary>Đăng nhập cho app mobile: /api/v1/mob/auth.</summary>
public sealed class AuthController(
    DangNhapService loginService, PhienService sessionService, MatKhauService passwordService, TaiKhoanHienTaiService meService,
    IQuyenHanProvider quyenHanProvider, ICurrentUser currentUser)
    : AuthControllerBase(loginService, sessionService, passwordService, meService, quyenHanProvider, currentUser);
