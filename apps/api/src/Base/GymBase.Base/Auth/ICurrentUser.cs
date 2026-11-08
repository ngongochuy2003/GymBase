namespace GymBase.Base.Auth;

/// <summary>Người đang gọi request. Các giá trị là null khi chưa đăng nhập hoặc chạy ngoài request (job, seed).</summary>
public interface ICurrentUser
{
    /// <summary>Id người dùng, lấy từ token.</summary>
    Guid? UserId { get; }

    /// <summary>Tên đăng nhập, dùng để ghi nguoi_tao và nguoi_chinh_sua.</summary>
    string? UserName { get; }

    /// <summary>Id phiên đăng nhập của token hiện tại.</summary>
    Guid? SessionId { get; }
}
