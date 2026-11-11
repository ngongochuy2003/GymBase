using GymBase.Entities.Auth;
using GymBase.Entities.Biz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.Biz;

/// <summary>
/// Bảng <c>tep_tin</c>. <c>(loai_doi_tuong, doi_tuong_id)</c> là liên kết đa hình nên không có khóa ngoại.
/// <c>s3_key</c> dùng collation phân biệt hoa thường vì S3 phân biệt.
/// </summary>
public sealed class TepTinConfiguration : IEntityTypeConfiguration<tep_tin>
{
    public void Configure(EntityTypeBuilder<tep_tin> builder)
    {
        builder.ToTable(nameof(tep_tin), table =>
        {
            table.HasEnumCheck(e => e.loai_doi_tuong);
            table.HasEnumCheck(e => e.che_do_truy_cap);
            table.HasEnumCheck(e => e.trang_thai);
            table.HasCheckConstraint("CK_tep_tin_kich_thuoc", "`kich_thuoc_byte` >= 0");
        });

        builder.Property(e => e.loai_doi_tuong).AsEnumString(32);
        builder.Property(e => e.muc_dich).HasMaxLength(32);
        builder.Property(e => e.s3_bucket).HasMaxLength(128);
        builder.Property(e => e.s3_key).HasMaxLength(512).UseCollation(ConfigurationExtensions.BinaryCollation);
        builder.Property(e => e.ten_tep_goc).HasMaxLength(255);
        builder.Property(e => e.loai_mime).HasMaxLength(100);
        builder.Property(e => e.che_do_truy_cap).AsEnumString(16);
        builder.Property(e => e.trang_thai).AsEnumString(20);

        builder.HasIndex(e => e.s3_key).IsUnique();
        builder.HasIndex(e => new { e.loai_doi_tuong, e.doi_tuong_id });
        builder.HasIndex(e => new { e.nguoi_tai_len_id, e.ngay_tao }).IsDescending(false, true);

        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.nguoi_tai_len_id);
    }
}
