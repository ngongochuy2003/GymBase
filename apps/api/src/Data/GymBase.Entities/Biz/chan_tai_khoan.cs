namespace GymBase.Entities.Biz;

// TODO(DB): chưa có migration (AddBizTables); sơ đồ ở database/diagrams/09-an-toan.md. Mục 3.
/// <summary>
/// Chặn tương tác giữa hai tài khoản. Khóa chính kép (nguoi_chan_id, nguoi_bi_chan_id), không được tự chặn.
/// </summary>
public sealed class chan_tai_khoan
{
    public Guid nguoi_chan_id { get; set; }

    public Guid nguoi_bi_chan_id { get; set; }

    public DateTime ngay_chan { get; set; }
}
