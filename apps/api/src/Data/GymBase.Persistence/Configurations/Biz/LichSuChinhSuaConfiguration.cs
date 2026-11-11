using GymBase.Entities.Auth;
using GymBase.Entities.Biz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.Biz;

/// <summary>Bảng <c>lich_su_chinh_sua</c>. <c>(loai_doi_tuong, doi_tuong_id)</c> là liên kết đa hình, không có khóa ngoại.</summary>
public sealed class LichSuChinhSuaConfiguration : IEntityTypeConfiguration<lich_su_chinh_sua>
{
    public void Configure(EntityTypeBuilder<lich_su_chinh_sua> builder)
    {
        builder.ToTable(nameof(lich_su_chinh_sua), table =>
        {
            table.HasEnumCheck(e => e.loai_doi_tuong);
            table.HasEnumCheck(e => e.hanh_dong);
        });

        builder.Property(e => e.loai_doi_tuong).AsEnumString(32);
        builder.Property(e => e.hanh_dong).AsEnumString(32);
        builder.Property(e => e.du_lieu_truoc).AsJsonText();
        builder.Property(e => e.du_lieu_sau).AsJsonText();

        builder.HasIndex(e => new { e.loai_doi_tuong, e.doi_tuong_id, e.ngay_thuc_hien }).IsDescending(false, false, true);

        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.nguoi_thuc_hien_id);
    }
}
