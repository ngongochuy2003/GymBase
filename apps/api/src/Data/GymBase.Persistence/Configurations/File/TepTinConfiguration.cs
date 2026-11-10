using GymBase.Entities.File;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.File;

/// <summary>Bảng <c>tep_tin</c>.</summary>
public sealed class TepTinConfiguration : IEntityTypeConfiguration<tep_tin>
{
    public void Configure(EntityTypeBuilder<tep_tin> builder)
    {
        builder.ToTable(nameof(tep_tin));

        builder.Property(e => e.ten_tep_goc).HasMaxLength(255).IsRequired();
        builder.Property(e => e.khoa_luu_tru).HasMaxLength(500).IsRequired();
        builder.Property(e => e.nha_cung_cap).HasMaxLength(20).IsRequired().HasDefaultValue("LOCAL");
        builder.Property(e => e.loai_mime).HasMaxLength(100).IsRequired();
        builder.Property(e => e.muc_dich).HasMaxLength(50).IsRequired();
        builder.Property(e => e.trang_thai).HasMaxLength(20).IsRequired().HasDefaultValue("UPLOADING");
        builder.Property(e => e.che_do_truy_cap).HasMaxLength(20).IsRequired().HasDefaultValue("cong_khai");
        builder.Property(e => e.blurhash).HasMaxLength(100);

        builder.HasIndex(e => e.khoa_luu_tru).IsUnique();
        builder.HasIndex(e => e.nguoi_tai_len_id);
        builder.HasIndex(e => new { e.nha_cung_cap, e.trang_thai });
    }
}
