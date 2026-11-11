using GymBase.Entities.Auth;
using GymBase.Entities.Biz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.Biz;

/// <summary>
/// Bảng <c>tin_nhan</c>. <c>(nguoi_gui_id, ma_tin_nhan_client)</c> unique để mạng di động gửi lại không tạo tin trùng.
/// </summary>
public sealed class TinNhanConfiguration : IEntityTypeConfiguration<tin_nhan>
{
    public void Configure(EntityTypeBuilder<tin_nhan> builder)
    {
        builder.ToTable(nameof(tin_nhan), table => table.HasEnumCheck(e => e.loai_tin_nhan));

        builder.Property(e => e.loai_tin_nhan).AsEnumString(16);
        builder.Property(e => e.noi_dung).AsText();

        builder.HasIndex(e => new { e.cuoc_tro_chuyen_id, e.ngay_gui }).IsDescending(false, true);
        builder.HasIndex(e => new { e.nguoi_gui_id, e.ma_tin_nhan_client }).IsUnique();

        builder.HasRestrictForeignKey(typeof(cuoc_tro_chuyen), e => e.cuoc_tro_chuyen_id);
        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.nguoi_gui_id);
        builder.HasRestrictForeignKey(typeof(tin_nhan), e => e.tra_loi_tin_nhan_id);
    }
}
