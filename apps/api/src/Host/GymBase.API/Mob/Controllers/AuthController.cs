using GymBase.API.Controllers;
using GymBase.Base.Auth;
using GymBase.BLAuth.BLXacThuc;

namespace GymBase.API.Mob.Controllers;

/// <summary>Đăng nhập cho app mobile: /api/v1/mob/auth.</summary>
public sealed class AuthController(XacThucService authService, IQuyenHanProvider quyenHanProvider, ICurrentUser currentUser)
    : AuthControllerBase(authService, quyenHanProvider, currentUser);
