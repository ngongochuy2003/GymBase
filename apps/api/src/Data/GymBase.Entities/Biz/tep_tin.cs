using GymBase.Base.Entities;
using GymBase.Entities.Biz.Enums;

namespace GymBase.Entities.Biz;

// TODO(DB): chưa có migration (AddBizTables); sơ đồ ở database/diagrams/02-tep-tin.md. Mục 3.
/// <summary>
/// Metadata của file trên Amazon S3. Database không chứa binary.
/// </summary>
public sealed class tep_tin : BaseEntity
{
    public Guid nguoi_tai_len_id { get; set; }

    /// <summary>Null lúc mới xin upload.</summary>
    public LoaiDoiTuongTepTin? loai_doi_tuong { get; set; }

    /// <summary>Id bản ghi sở hữu, không có khóa ngoại; service kiểm tra.</summary>
    public Guid? doi_tuong_id { get; set; }

    /// <summary>Phân biệt file cùng đối tượng, ví dụ anh_dai_dien, chung_chi_pt. Danh sách chưa chốt.</summary>
    public string? muc_dich { get; set; }

    public string s3_bucket { get; set; } = string.Empty;

    /// <summary>Object key trên S3, không phải URL. Unique, phân biệt hoa thường.</summary>
    public string s3_key { get; set; } = string.Empty;

    public string? ten_tep_goc { get; set; }

    public string loai_mime { get; set; } = string.Empty;

    /// <summary>Kích thước đã xác minh từ S3.</summary>
    public long kich_thuoc_byte { get; set; }

    public CheDoTruyCap che_do_truy_cap { get; set; }

    public TrangThaiTepTin trang_thai { get; set; }

    /// <summary>Thứ tự ảnh hoặc video trong đối tượng.</summary>
    public int? thu_tu_hien_thi { get; set; }

    /// <summary>Lúc đăng ký upload (UTC).</summary>
    public DateTime ngay_tao { get; set; }

    /// <summary>Lúc backend kiểm tra object trên S3 xong (UTC).</summary>
    public DateTime? ngay_xac_nhan { get; set; }
}
