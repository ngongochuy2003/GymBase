namespace GymBase.Entities.Biz;

// TODO(DB): chưa có migration (AddBizTables); sơ đồ ở database/diagrams/03-mang-xa-hoi.md. Mục 3.
/// <summary>
/// Quan hệ theo dõi giữa hai tài khoản. Khóa chính kép (nguoi_theo_doi_id, nguoi_duoc_theo_doi_id), không được
/// tự theo dõi.
/// </summary>
public sealed class theo_doi
{
    public Guid nguoi_theo_doi_id { get; set; }

    public Guid nguoi_duoc_theo_doi_id { get; set; }

    public DateTime ngay_theo_doi { get; set; }
}
