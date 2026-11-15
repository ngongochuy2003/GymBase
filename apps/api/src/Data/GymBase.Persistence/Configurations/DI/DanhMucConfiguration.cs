using GymBase.Entities.DI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.DI;

/// <summary>
/// Bảng <c>danh_muc</c>: dữ liệu tham chiếu cho dropdown (mục tiêu tập luyện, khu vực, chuyên môn PT).
/// Khóa theo cặp (<c>ma_dinh_danh</c>, <c>ma</c>); <c>danh_muc_cap_tren_id</c> tự tham chiếu để sau này dựng cây.
/// </summary>
public sealed class DanhMucConfiguration : IEntityTypeConfiguration<danh_muc>
{
    public void Configure(EntityTypeBuilder<danh_muc> builder)
    {
        builder.ToTable(nameof(danh_muc));

        builder.Property(e => e.ma_dinh_danh).HasMaxLength(255);
        builder.Property(e => e.ma).HasMaxLength(255);
        builder.Property(e => e.ten).HasMaxLength(255);
        builder.Property(e => e.mo_ta).HasColumnType("longtext");
        builder.Property(e => e.muc_luc).HasMaxLength(512);
        builder.Property(e => e.muc_luc_ma).HasMaxLength(512);
        builder.Property(e => e.ghi_chu).HasColumnType("longtext");

        builder.ConfigureAudit();

        // Cùng một mã được dùng ở nhiều loại, nên duy nhất theo cặp loại + mã.
        builder.HasIndex(e => new { e.ma_dinh_danh, e.ma }).IsUnique();
        builder.HasIndex(e => e.muc_luc_ma);
        builder.HasIndex(e => e.danh_muc_cap_tren_id);

        builder.HasRestrictForeignKey(typeof(danh_muc), e => e.danh_muc_cap_tren_id);
    }
}
