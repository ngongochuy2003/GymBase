using GymBase.Entities.Auth;
using GymBase.Entities.Biz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.Biz;

/// <summary>
/// Bảng <c>ho_so_nguoi_dung</c>. Khóa chính là <c>tai_khoan_id</c> kiêm khóa ngoại, nên mỗi tài khoản đúng một hồ sơ.
/// </summary>
public sealed class HoSoNguoiDungConfiguration : IEntityTypeConfiguration<ho_so_nguoi_dung>
{
    public void Configure(EntityTypeBuilder<ho_so_nguoi_dung> builder)
    {
        builder.ToTable(nameof(ho_so_nguoi_dung), table =>
        {
            table.HasEnumCheck(e => e.loai_nguoi_dung);
            table.HasEnumCheck(e => e.trang_thai_xac_minh);
            table.HasCheckConstraint("CK_ho_so_nguoi_dung_kinh_nghiem", "`so_nam_kinh_nghiem` IS NULL OR `so_nam_kinh_nghiem` >= 0");
            table.HasCheckConstraint("CK_ho_so_nguoi_dung_ban_kinh", "`ban_kinh_phuc_vu_km` IS NULL OR `ban_kinh_phuc_vu_km` > 0");
            table.HasCheckConstraint("CK_ho_so_nguoi_dung_vi_do", "`vi_do` IS NULL OR `vi_do` BETWEEN -90 AND 90");
            table.HasCheckConstraint("CK_ho_so_nguoi_dung_kinh_do", "`kinh_do` IS NULL OR `kinh_do` BETWEEN -180 AND 180");
        });

        builder.HasKey(e => e.tai_khoan_id);

        builder.Property(e => e.loai_nguoi_dung).AsEnumString(12);
        builder.Property(e => e.ten_hien_thi).HasMaxLength(100);
        builder.Property(e => e.gioi_thieu).AsText();
        builder.Property(e => e.muc_tieu_tap_luyen).AsText();
        builder.Property(e => e.chuyen_mon).AsJson();
        builder.Property(e => e.khu_vuc_hoat_dong).HasMaxLength(255);
        builder.Property(e => e.vi_do).HasPrecision(9, 6);
        builder.Property(e => e.kinh_do).HasPrecision(9, 6);
        builder.Property(e => e.trang_thai_xac_minh).AsEnumString(20);
        builder.Property(e => e.ly_do_tu_choi).AsText();
        builder.Property(e => e.nguoi_tao).HasMaxLength(100);
        builder.Property(e => e.nguoi_chinh_sua).HasMaxLength(100);

        builder.HasIndex(e => new { e.loai_nguoi_dung, e.trang_thai_xac_minh });

        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.tai_khoan_id);
        builder.HasRestrictForeignKey(typeof(tep_tin), e => e.anh_dai_dien_tep_tin_id);
        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.nguoi_xac_minh_id);
    }
}
