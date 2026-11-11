using GymBase.Entities.Auth;
using GymBase.Entities.Biz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.Biz;

/// <summary>Bảng <c>goi_pt_hoc_vien</c>. Số buổi còn lại không lưu, tính từ sổ cái <c>bien_dong_so_buoi</c>.</summary>
public sealed class GoiPtHocVienConfiguration : IEntityTypeConfiguration<goi_pt_hoc_vien>
{
    public void Configure(EntityTypeBuilder<goi_pt_hoc_vien> builder)
    {
        builder.ToTable(nameof(goi_pt_hoc_vien), table =>
        {
            table.HasEnumCheck(e => e.trang_thai);
            table.HasCheckConstraint("CK_goi_pt_hoc_vien_so_buoi_mua", "`so_buoi_mua` > 0");
            table.HasCheckConstraint("CK_goi_pt_hoc_vien_gia", "`gia_thoa_thuan` IS NULL OR `gia_thoa_thuan` >= 0");
            table.HasCheckConstraint("CK_goi_pt_hoc_vien_ngay", "`ngay_het_han` IS NULL OR `ngay_het_han` >= `ngay_bat_dau`");
        });

        builder.ConfigureAudit();
        builder.Property(e => e.ten_goi).HasMaxLength(160);
        builder.Property(e => e.gia_thoa_thuan).HasPrecision(18, 2);
        builder.Property(e => e.trang_thai).AsEnumString(20);

        builder.HasIndex(e => new { e.ket_noi_pt_id, e.trang_thai });

        builder.HasRestrictForeignKey(typeof(ket_noi_pt), e => e.ket_noi_pt_id);
        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.nguoi_tao_id);
    }
}
