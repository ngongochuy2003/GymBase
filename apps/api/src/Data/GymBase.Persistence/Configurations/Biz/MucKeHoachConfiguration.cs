using GymBase.Entities.Biz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.Biz;

/// <summary>
/// Bảng <c>muc_ke_hoach</c>. <c>loai_muc</c> phải khớp loại của kế hoạch cha và <c>chi_tiet</c> phải đúng schema theo loại,
/// do service kiểm tra.
/// </summary>
public sealed class MucKeHoachConfiguration : IEntityTypeConfiguration<muc_ke_hoach>
{
    public void Configure(EntityTypeBuilder<muc_ke_hoach> builder)
    {
        builder.ToTable(nameof(muc_ke_hoach), table =>
        {
            table.HasEnumCheck(e => e.loai_muc);
            table.HasEnumCheck(e => e.trang_thai);
            table.HasCheckConstraint("CK_muc_ke_hoach_nhac_truoc", "`nhac_truoc_phut` >= 0");
            table.HasCheckConstraint("CK_muc_ke_hoach_cac_thu", "JSON_TYPE(`cac_thu_trong_tuan`) = 'ARRAY'");
        });

        builder.ConfigureAudit();
        builder.Property(e => e.loai_muc).AsEnumString(16);
        builder.Property(e => e.tieu_de).HasMaxLength(160);
        builder.Property(e => e.cac_thu_trong_tuan).AsJson();
        builder.Property(e => e.mui_gio).HasMaxLength(64);
        builder.Property(e => e.chi_tiet).AsJsonText();
        builder.Property(e => e.trang_thai).AsEnumString(16);

        builder.HasIndex(e => new { e.ke_hoach_id, e.trang_thai });

        builder.HasRestrictForeignKey(typeof(ke_hoach), e => e.ke_hoach_id);
    }
}
