namespace GymBase.Entities.Biz;

/// <summary>
/// Chặn tương tác giữa hai tài khoản. Khóa chính kép (nguoi_chan_id, nguoi_bi_chan_id), không được tự chặn.
/// </summary>
public sealed class chan_tai_khoan
{
    public Guid nguoi_chan_id { get; set; }

    public Guid nguoi_bi_chan_id { get; set; }

    public DateTime ngay_chan { get; set; }
}
