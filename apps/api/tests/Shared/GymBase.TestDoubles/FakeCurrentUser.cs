using GymBase.Base.Auth;

namespace GymBase.TestDoubles;

/// <summary>Người dùng cố định cho test; không truyền tên thì là chưa đăng nhập.</summary>
public sealed class FakeCurrentUser(string? userName = null, Guid? userId = null, Guid? sessionId = null) : ICurrentUser
{
    public Guid? UserId { get; } = userId;

    public string? UserName { get; } = userName;

    public Guid? SessionId { get; } = sessionId;
}
