using GymBase.Entities.Auth;
using GymBase.Entities.Biz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.Biz;

/// <summary>
/// Bảng <c>thiet_bi</c>. <c>push_token</c> unique (cho phép NULL) để một token không nằm ở hai tài khoản, và phân biệt
/// hoa thường vì FCM token phân biệt.
/// </summary>
public sealed class ThietBiConfiguration : IEntityTypeConfiguration<thiet_bi>
{
    public void Configure(EntityTypeBuilder<thiet_bi> builder)
    {
        builder.ToTable(nameof(thiet_bi), table => table.HasEnumCheck(e => e.nen_tang));

        builder.ConfigureAudit();
        builder.Property(e => e.ma_thiet_bi).HasMaxLength(128);
        builder.Property(e => e.nen_tang).AsEnumString(12);
        builder.Property(e => e.push_token).HasMaxLength(512).UseCollation(ConfigurationExtensions.BinaryCollation);

        builder.HasIndex(e => e.push_token).IsUnique();
        builder.HasIndex(e => new { e.tai_khoan_id, e.ma_thiet_bi }).IsUnique();
        builder.HasIndex(e => new { e.tai_khoan_id, e.dang_hoat_dong });

        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.tai_khoan_id);
    }
}
