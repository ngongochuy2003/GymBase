using GymBase.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.Auth;

/// <summary>Bảng <c>tai_khoan</c>. Độ dài cột khớp DataAnnotations trong DTO của BLAuth.</summary>
public sealed class TaiKhoanConfiguration : IEntityTypeConfiguration<tai_khoan>
{
    public void Configure(EntityTypeBuilder<tai_khoan> builder)
    {
        builder.ToTable(nameof(tai_khoan));

        builder.Property(e => e.ten_dang_nhap).HasMaxLength(50);
        builder.Property(e => e.ho_ten).HasMaxLength(100);
        builder.Property(e => e.email).HasMaxLength(100);
        builder.Property(e => e.so_dien_thoai).HasMaxLength(20);
        builder.Property(e => e.vai_tro).HasMaxLength(20);
        builder.Property(e => e.mat_khau_hash).HasMaxLength(255);
        builder.Property(e => e.nguoi_tao).HasMaxLength(100);
        builder.Property(e => e.nguoi_chinh_sua).HasMaxLength(100);

        // Tên đăng nhập duy nhất, không phân biệt hoa thường (collation mặc định). Tài khoản chỉ dùng Google có
        // ten_dang_nhap NULL; MySQL cho phép nhiều NULL trong unique index.
        builder.HasIndex(e => e.ten_dang_nhap).IsUnique();
    }
}
