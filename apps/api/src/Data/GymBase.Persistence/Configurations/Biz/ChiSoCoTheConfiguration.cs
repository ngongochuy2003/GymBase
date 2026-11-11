using GymBase.Entities.Auth;
using GymBase.Entities.Biz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.Biz;

/// <summary>Bảng <c>chi_so_co_the</c>. Chỉ chia sẻ cho PT khi gắn <c>ket_noi_pt_id</c>.</summary>
public sealed class ChiSoCoTheConfiguration : IEntityTypeConfiguration<chi_so_co_the>
{
    public void Configure(EntityTypeBuilder<chi_so_co_the> builder)
    {
        builder.ToTable(nameof(chi_so_co_the), table =>
        {
            table.HasCheckConstraint("CK_chi_so_co_the_can_nang", "`can_nang_kg` IS NULL OR `can_nang_kg` > 0");
            table.HasCheckConstraint("CK_chi_so_co_the_chieu_cao", "`chieu_cao_cm` IS NULL OR `chieu_cao_cm` > 0");
            table.HasCheckConstraint("CK_chi_so_co_the_ty_le_mo", "`ty_le_mo_pct` IS NULL OR `ty_le_mo_pct` BETWEEN 0 AND 100");
        });

        builder.Property(e => e.can_nang_kg).HasPrecision(6, 2);
        builder.Property(e => e.chieu_cao_cm).HasPrecision(5, 1);
        builder.Property(e => e.ty_le_mo_pct).HasPrecision(5, 2);
        builder.Property(e => e.ghi_chu).AsText();

        builder.HasIndex(e => new { e.hoc_vien_id, e.ngay_do }).IsDescending(false, true);
        builder.HasIndex(e => new { e.ket_noi_pt_id, e.ngay_do }).IsDescending(false, true);

        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.hoc_vien_id);
        builder.HasRestrictForeignKey(typeof(ket_noi_pt), e => e.ket_noi_pt_id);
        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.nguoi_nhap_id);
    }
}
