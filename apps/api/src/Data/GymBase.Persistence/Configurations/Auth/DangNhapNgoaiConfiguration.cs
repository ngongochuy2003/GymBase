using GymBase.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.Auth;

/// <summary>
/// Bảng <c>dang_nhap_ngoai</c>. Hai unique: một tài khoản Google không gắn vào hai tài khoản GymBase, và mỗi tài khoản
/// chỉ liên kết một tài khoản cho mỗi nhà cung cấp.
/// </summary>
public sealed class DangNhapNgoaiConfiguration : IEntityTypeConfiguration<dang_nhap_ngoai>
{
    public void Configure(EntityTypeBuilder<dang_nhap_ngoai> builder)
    {
        builder.ToTable(nameof(dang_nhap_ngoai), table => table.HasEnumCheck(e => e.nha_cung_cap));

        builder.Property(e => e.nha_cung_cap).AsEnumString(20);
        builder.Property(e => e.ma_ngoai).HasMaxLength(255);

        builder.HasIndex(e => new { e.nha_cung_cap, e.ma_ngoai }).IsUnique();
        builder.HasIndex(e => new { e.tai_khoan_id, e.nha_cung_cap }).IsUnique();

        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.tai_khoan_id);
    }
}
