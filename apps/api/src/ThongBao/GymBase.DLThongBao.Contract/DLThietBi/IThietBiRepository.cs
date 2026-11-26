using GymBase.Base.Crud;
using GymBase.Entities.Biz;

namespace GymBase.DLThongBao.Contract;

public interface IThietBiRepository : IBaseRepository<thiet_bi>
{
    Task<thiet_bi?> DLFindByMaThietBiAsync(Guid taiKhoanId, string maThietBi, CancellationToken ct = default);

    /// <summary>Thiết bị đang hoạt động và có push token của một tài khoản.</summary>
    Task<IReadOnlyList<thiet_bi>> DLFindActiveWithTokenAsync(Guid taiKhoanId, CancellationToken ct = default);

    /// <summary>Token FCM đã chết: xóa token và tắt <c>dang_hoat_dong</c> để không gửi nữa.</summary>
    Task DLClearPushTokenAsync(Guid thietBiId, CancellationToken ct = default);

    /// <summary>
    /// Gỡ <paramref name="pushToken"/> khỏi mọi thiết bị khác <paramref name="tru"/> (xóa token, tắt <c>dang_hoat_dong</c>), vì
    /// <c>push_token</c> unique: token chuyển sang tài khoản khác thì dòng cũ phải nhả ra trước.
    /// </summary>
    Task DLReleasePushTokenAsync(string pushToken, Guid tru, CancellationToken ct = default);
}
