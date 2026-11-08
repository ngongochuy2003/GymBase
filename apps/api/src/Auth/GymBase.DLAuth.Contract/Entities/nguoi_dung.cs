using GymBase.Base.Entities;

namespace GymBase.DLAuth.Contract.Entities;

/// <summary>
/// Người dùng hệ thống. Tên class trùng tên bảng <c>nguoi_dung</c>, property trùng tên cột.
/// Thư mục <c>System</c> chỉ để sắp xếp theo phân hệ; namespace chung là <c>GymBase.DLAuth.Contract.Entities</c>.
/// </summary>
public sealed class nguoi_dung : BaseAuditableEntity, ISoftDelete
{
    /// <summary>Tên đăng nhập, duy nhất.</summary>
    public string tai_khoan { get; set; } = string.Empty;

    public string ho_ten { get; set; } = string.Empty;

    public string? email { get; set; }

    public string? so_dien_thoai { get; set; }

    // TODO(AUTH): chuẩn hóa vai trò (enum hoặc bảng vai_tro) khi làm phân quyền. Mục 12.
    /// <summary>Vai trò: Admin, Trainer, Trainee. Sẽ chuẩn hóa khi làm phân quyền.</summary>
    public string vai_tro { get; set; } = string.Empty;

    /// <summary>false khi tài khoản bị khóa.</summary>
    public bool dang_hoat_dong { get; set; } = true;

    /// <summary>Mật khẩu đã băm. Không bao giờ trả ra API.</summary>
    public string mat_khau_hash { get; set; } = string.Empty;

    public bool is_deleted { get; set; }
}
