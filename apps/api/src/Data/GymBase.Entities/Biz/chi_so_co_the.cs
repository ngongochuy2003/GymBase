using GymBase.Base.Entities;

namespace GymBase.Entities.Biz;

/// <summary>
/// Cân nặng, chiều cao và chỉ số cơ thể. Không chia sẻ mặc định cho mọi PT, chỉ chia sẻ khi gắn ket_noi_pt_id.
/// </summary>
public sealed class chi_so_co_the : BaseEntity
{
    /// <summary>Chủ sở hữu chỉ số.</summary>
    public Guid hoc_vien_id { get; set; }

    /// <summary>Kết nối PT được chia sẻ.</summary>
    public Guid? ket_noi_pt_id { get; set; }

    public DateTime ngay_do { get; set; }

    public decimal? can_nang_kg { get; set; }

    public decimal? chieu_cao_cm { get; set; }

    /// <summary>Từ 0 đến 100.</summary>
    public decimal? ty_le_mo_pct { get; set; }

    public string? ghi_chu { get; set; }

    /// <summary>Ai tạo bản ghi.</summary>
    public Guid nguoi_nhap_id { get; set; }

    public DateTime ngay_tao { get; set; }
}
