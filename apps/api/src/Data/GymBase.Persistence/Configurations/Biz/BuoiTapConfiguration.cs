using GymBase.Entities.Auth;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.Biz;

/// <summary>
/// Bảng <c>buoi_tap</c>. Buổi tự tập không được gắn gói (không bao giờ bị trừ). Các cột <c>hoc_vien_id</c>, <c>pt_id</c>,
/// <c>goi_pt_hoc_vien_id</c> phải khớp <c>ket_noi_pt</c> của buổi, database không ép được nên service kiểm tra.
/// </summary>
public sealed class BuoiTapConfiguration : IEntityTypeConfiguration<buoi_tap>
{
    public void Configure(EntityTypeBuilder<buoi_tap> builder)
    {
        builder.ToTable(nameof(buoi_tap), table =>
        {
            table.HasEnumCheck(e => e.loai_buoi);
            table.HasEnumCheck(e => e.trang_thai);
            table.HasEnumCheck(e => e.trang_thai_khieu_nai);
            table.HasCheckConstraint(
                "CK_buoi_tap_tu_tap_khong_goi",
                $"`loai_buoi` <> '{nameof(LoaiBuoi.tu_tap)}' OR `goi_pt_hoc_vien_id` IS NULL");
        });

        builder.ConfigureAudit();
        builder.Property(e => e.loai_buoi).AsEnumString(16);
        builder.Property(e => e.trang_thai).AsEnumString(24);
        builder.Property(e => e.noi_dung_khieu_nai).AsText();
        builder.Property(e => e.trang_thai_khieu_nai).AsEnumString(24);

        builder.HasIndex(e => new { e.hoc_vien_id, e.bat_dau_du_kien }).IsDescending(false, true);
        builder.HasIndex(e => new { e.pt_id, e.trang_thai });
        builder.HasIndex(e => new { e.goi_pt_hoc_vien_id, e.trang_thai });

        builder.HasRestrictForeignKey(typeof(ket_noi_pt), e => e.ket_noi_pt_id);
        builder.HasRestrictForeignKey(typeof(goi_pt_hoc_vien), e => e.goi_pt_hoc_vien_id);
        builder.HasRestrictForeignKey(typeof(muc_ke_hoach), e => e.muc_ke_hoach_id);
        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.hoc_vien_id);
        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.pt_id);
        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.nguoi_xac_nhan_id);
        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.nguoi_xu_ly_khieu_nai_id);
    }
}
