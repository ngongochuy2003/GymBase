using GymBase.DLAuth.Contract.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.DLAuth.MySql.Configurations;

/// <summary>Bảng <c>nguoi_dung</c>. Độ dài cột khớp DataAnnotations trong DTO của BLAuth.</summary>
public sealed class NguoiDungConfiguration : IEntityTypeConfiguration<nguoi_dung>
{
    /// <summary>Cột sinh tự động: bằng tai_khoan khi còn hiệu lực, NULL khi đã xóa mềm.</summary>
    private const string ActiveUsernameColumn = "tai_khoan_hieu_luc";

    public void Configure(EntityTypeBuilder<nguoi_dung> builder)
    {
        builder.ToTable(nameof(nguoi_dung));

        builder.Property(e => e.tai_khoan).HasMaxLength(50);
        builder.Property(e => e.ho_ten).HasMaxLength(100);
        builder.Property(e => e.email).HasMaxLength(100);
        builder.Property(e => e.so_dien_thoai).HasMaxLength(20);
        builder.Property(e => e.vai_tro).HasMaxLength(20);
        builder.Property(e => e.mat_khau_hash).HasMaxLength(255);
        builder.Property(e => e.nguoi_tao).HasMaxLength(100);
        builder.Property(e => e.nguoi_chinh_sua).HasMaxLength(100);

        // MySQL không có partial index. NULL được phép trùng trong unique index, nên cột này chặn trùng
        // giữa các tài khoản còn hiệu lực mà vẫn cho dùng lại tên của tài khoản đã xóa mềm.
        builder.Property<string?>(ActiveUsernameColumn)
            .HasMaxLength(50)
            .HasComputedColumnSql("IF(`is_deleted` = 0, `tai_khoan`, NULL)", stored: false);
        builder.HasIndex(ActiveUsernameColumn).IsUnique();
    }
}
