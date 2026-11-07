using GymBase.AMISBase.DL.Contract.Paging;
using GymBase.DLSystem.Contract;

namespace GymBase.BLSystem.Users;

/// <summary>
/// Nghiệp vụ người dùng, dùng chung cho SystemAPI (web) và MobAPI (mobile).
/// Không truy cập DB trực tiếp; chỉ gọi <see cref="IUserDL"/>.
/// </summary>
public sealed class UserBL(IUserDL userDL)
{
    /// <summary>Lấy danh sách người dùng có phân trang và tìm kiếm.</summary>
    public async Task<PagingResult<UserDto>> GetPagingAsync(PagingRequest request, CancellationToken cancellationToken)
    {
        var users = await userDL.GetPagingAsync(request.Normalize(), cancellationToken);
        return users.Map(UserDto.FromEntity);
    }

    /// <summary>Lấy một người dùng; null nếu không tồn tại.</summary>
    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await userDL.GetByIdAsync(id, cancellationToken);
        return user is null ? null : UserDto.FromEntity(user);
    }
}
