namespace GymBase.Entities.Biz;

// TODO(DB): chưa có migration (AddBizTables); sơ đồ ở database/diagrams/05-nhan-tin.md. Mục 3.
/// <summary>
/// Thành viên của hội thoại và mốc đã đọc. Khóa chính kép (cuoc_tro_chuyen_id, tai_khoan_id).
/// </summary>
public sealed class thanh_vien_cuoc_tro_chuyen
{
    public Guid cuoc_tro_chuyen_id { get; set; }

    public Guid tai_khoan_id { get; set; }

    public DateTime ngay_tham_gia { get; set; }

    /// <summary>Mốc đã xem, dùng tính số tin chưa đọc.</summary>
    public DateTime? ngay_doc_den { get; set; }

    /// <summary>Dùng khi có chat nhóm.</summary>
    public DateTime? ngay_roi { get; set; }
}
