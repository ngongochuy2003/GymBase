using GymBase.Entities.Auth;
using GymBase.Entities.Biz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.Biz;

/// <summary>Bảng <c>bai_viet</c>. File đính kèm gắn qua <c>tep_tin</c>, không có bảng riêng.</summary>
public sealed class BaiVietConfiguration : IEntityTypeConfiguration<bai_viet>
{
    public void Configure(EntityTypeBuilder<bai_viet> builder)
    {
        builder.ToTable(nameof(bai_viet), table =>
        {
            table.HasEnumCheck(e => e.loai_bai_viet);
            table.HasEnumCheck(e => e.che_do_hien_thi);
            table.HasEnumCheck(e => e.trang_thai);
        });

        builder.ConfigureAudit();
        builder.Property(e => e.noi_dung).AsText();
        builder.Property(e => e.loai_bai_viet).AsEnumString(16);
        builder.Property(e => e.che_do_hien_thi).AsEnumString(16);
        builder.Property(e => e.trang_thai).AsEnumString(16);

        builder.HasIndex(e => new { e.trang_thai, e.ngay_dang }).IsDescending(false, true);
        builder.HasIndex(e => new { e.tac_gia_id, e.ngay_dang }).IsDescending(false, true);

        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.tac_gia_id);
    }
}
