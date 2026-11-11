using GymBase.Entities.Auth;
using GymBase.Entities.Biz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.Biz;

/// <summary>Bảng <c>nhat_ky_hoat_dong</c>: ảnh bữa ăn gắn qua <c>tep_tin</c>, mỗi lần sửa hoặc đánh giá ghi vào lịch sử.</summary>
public sealed class NhatKyHoatDongConfiguration : IEntityTypeConfiguration<nhat_ky_hoat_dong>
{
    public void Configure(EntityTypeBuilder<nhat_ky_hoat_dong> builder)
    {
        builder.ToTable(nameof(nhat_ky_hoat_dong), table =>
        {
            table.HasEnumCheck(e => e.loai_hoat_dong);
            table.HasEnumCheck(e => e.trang_thai);
            table.HasCheckConstraint("CK_nhat_ky_hoat_dong_phien_ban", "`phien_ban_ke_hoach` IS NULL OR `phien_ban_ke_hoach` >= 1");
        });

        builder.ConfigureAudit();
        builder.Property(e => e.loai_hoat_dong).AsEnumString(16);
        builder.Property(e => e.noi_dung).AsText();
        builder.Property(e => e.chi_tiet).AsJsonText();
        builder.Property(e => e.trang_thai).AsEnumString(24);
        builder.Property(e => e.nhan_xet).AsText();

        builder.HasIndex(e => new { e.hoc_vien_id, e.ngay_hoat_dong }).IsDescending(false, true);
        builder.HasIndex(e => new { e.ket_noi_pt_id, e.trang_thai });
        builder.HasIndex(e => new { e.muc_ke_hoach_id, e.ngay_hoat_dong });

        builder.HasRestrictForeignKey(typeof(ket_noi_pt), e => e.ket_noi_pt_id);
        builder.HasRestrictForeignKey(typeof(muc_ke_hoach), e => e.muc_ke_hoach_id);
        builder.HasRestrictForeignKey(typeof(buoi_tap), e => e.buoi_tap_id);
        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.hoc_vien_id);
        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.nguoi_danh_gia_id);
    }
}
