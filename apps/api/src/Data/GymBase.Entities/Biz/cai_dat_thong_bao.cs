using GymBase.Base.Entities;
using GymBase.Entities.Biz.Enums;

namespace GymBase.Entities.Biz;

/// <summary>
/// Người dùng bật hoặc tắt từng loại thông báo. Chưa có dòng cho một loại nghĩa là đang bật. Lưu ở server theo tài khoản
/// để đổi máy vẫn giữ.
/// </summary>
public sealed class cai_dat_thong_bao : BaseAuditableEntity
{
    public Guid tai_khoan_id { get; set; }

    public LoaiThongBao loai_thong_bao { get; set; }

    public bool bat { get; set; } = true;
}
