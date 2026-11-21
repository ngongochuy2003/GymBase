using GymBase.Entities.Auth;
using Microsoft.AspNetCore.Identity;

namespace GymBase.BLAuth.BLXacThuc;

/// <summary>
/// Kiểm tra mật khẩu. Luôn băm, kể cả khi không có tài khoản hoặc tài khoản không có mật khẩu (chỉ dùng Google), để thời
/// gian phản hồi không lộ tài khoản có tồn tại hay không.
/// </summary>
public sealed class MatKhauVerifier(IPasswordHasher<tai_khoan> passwordHasher)
{
    private static readonly tai_khoan PlaceholderUser = new();

    private static string? s_dummyHash;

    public PasswordVerificationResult Verify(tai_khoan? user, string password)
    {
        s_dummyHash ??= passwordHasher.HashPassword(PlaceholderUser, Guid.NewGuid().ToString());

        var hasHash = !string.IsNullOrEmpty(user?.mat_khau_hash);

        try
        {
            var result = passwordHasher.VerifyHashedPassword(
                user ?? PlaceholderUser, hasHash ? user!.mat_khau_hash! : s_dummyHash, password);
            return hasHash ? result : PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            return PasswordVerificationResult.Failed;
        }
    }
}
