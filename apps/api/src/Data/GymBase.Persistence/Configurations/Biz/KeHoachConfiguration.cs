using GymBase.Entities.Auth;
using GymBase.Entities.Biz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.Biz;

/// <summary>Bảng <c>ke_hoach</c>: kế hoạch ăn hoặc tập trong một kết nối PT.</summary>
public sealed class KeHoachConfiguration : IEntityTypeConfiguration<ke_hoach>
{
    public void Configure(EntityTypeBuilder<ke_hoach> builder)
    {
        builder.ToTable(nameof(ke_hoach), table =>
        {
            table.HasEnumCheck(e => e.loai_ke_hoach);
            table.HasEnumCheck(e => e.trang_thai);
            table.HasCheckConstraint("CK_ke_hoach_ngay", "`ngay_ket_thuc` IS NULL OR `ngay_ket_thuc` >= `ngay_bat_dau`");
            table.HasCheckConstraint("CK_ke_hoach_phien_ban", "`phien_ban` >= 1");
        });

        builder.ConfigureAudit();
        builder.Property(e => e.loai_ke_hoach).AsEnumString(16);
        builder.Property(e => e.ten_ke_hoach).HasMaxLength(160);
        builder.Property(e => e.mo_ta).AsText();
        builder.Property(e => e.trang_thai).AsEnumString(16);

        builder.HasIndex(e => new { e.ket_noi_pt_id, e.loai_ke_hoach, e.trang_thai });

        builder.HasRestrictForeignKey(typeof(ket_noi_pt), e => e.ket_noi_pt_id);
        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.nguoi_tao_id);
    }
}
