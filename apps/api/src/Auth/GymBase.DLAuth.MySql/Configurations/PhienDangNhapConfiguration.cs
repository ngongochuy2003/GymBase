using GymBase.DLAuth.Contract.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.DLAuth.MySql.Configurations;

/// <summary>Bảng <c>phien_dang_nhap</c>. SHA-256 dạng hex dài đúng 64 ký tự.</summary>
public sealed class PhienDangNhapConfiguration : IEntityTypeConfiguration<phien_dang_nhap>
{
    private const int HashLength = 64;

    public void Configure(EntityTypeBuilder<phien_dang_nhap> builder)
    {
        builder.ToTable(nameof(phien_dang_nhap));

        builder.Property(e => e.token_hash).HasMaxLength(HashLength).IsFixedLength();
        builder.Property(e => e.token_hash_truoc).HasMaxLength(HashLength).IsFixedLength();
        builder.Property(e => e.ten_thiet_bi).HasMaxLength(100);

        builder.HasIndex(e => e.token_hash).IsUnique();
        builder.HasIndex(e => e.token_hash_truoc);
        builder.HasIndex(e => e.nguoi_dung_id);

        builder.HasOne<nguoi_dung>()
            .WithMany()
            .HasForeignKey(e => e.nguoi_dung_id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
