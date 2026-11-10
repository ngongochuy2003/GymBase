namespace GymBase.Base.Auth;

/// <summary>Các giá trị hợp lệ của cột vai_tro, cũng là claim role trong token.</summary>
public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Trainer = "Trainer";
    public const string Trainee = "Trainee";

    public static IReadOnlySet<string> All { get; } = new HashSet<string>(StringComparer.Ordinal) { Admin, Trainer, Trainee };
}
