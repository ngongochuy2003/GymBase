using GymBase.API.Controllers;
using GymBase.BLAuth.XacThuc;

namespace GymBase.API.Mob.Controllers;

/// <summary>Đăng nhập cho app mobile: /api/v1/mob/auth.</summary>
public sealed class AuthController(AuthService authService) : AuthControllerBase(authService);
