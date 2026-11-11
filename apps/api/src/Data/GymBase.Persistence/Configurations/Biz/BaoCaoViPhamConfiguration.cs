using GymBase.Entities.Auth;
using GymBase.Entities.Biz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.Biz;

/// <summary>Bảng <c>bao_cao_vi_pham</c>. <c>(loai_doi_tuong, doi_tuong_id)</c> là liên kết đa hình, không có khóa ngoại.</summary>
public sealed class BaoCaoViPhamConfiguration : IEntityTypeConfiguration<bao_cao_vi_pham>
{
    public void Configure(EntityTypeBuilder<bao_cao_vi_pham> builder)
    {
        builder.ToTable(nameof(bao_cao_vi_pham), table =>
        {
            table.HasEnumCheck(e => e.loai_doi_tuong);
            table.HasEnumCheck(e => e.ly_do);
            table.HasEnumCheck(e => e.trang_thai);
        });

        builder.Property(e => e.loai_doi_tuong).AsEnumString(32);
        builder.Property(e => e.ly_do).AsEnumString(64);
        builder.Property(e => e.mo_ta).AsText();
        builder.Property(e => e.trang_thai).AsEnumString(20);

        builder.HasIndex(e => new { e.trang_thai, e.ngay_tao });
        builder.HasIndex(e => new { e.loai_doi_tuong, e.doi_tuong_id });

        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.nguoi_bao_cao_id);
        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.nguoi_xu_ly_id);
    }
}
