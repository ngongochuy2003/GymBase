using GymBase.Entities.Auth;
using GymBase.Entities.Biz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.Biz;

/// <summary>
/// Bảng <c>thong_bao</c>: hộp thông báo và hàng đợi nhắc giờ. <c>(tai_khoan_id, ma_chong_trung)</c> unique để worker
/// và retry không tạo thông báo trùng. <c>(loai_doi_tuong, doi_tuong_id)</c> là liên kết đa hình.
/// </summary>
public sealed class ThongBaoConfiguration : IEntityTypeConfiguration<thong_bao>
{
    public void Configure(EntityTypeBuilder<thong_bao> builder)
    {
        builder.ToTable(nameof(thong_bao), table =>
        {
            table.HasEnumCheck(e => e.loai_thong_bao);
            table.HasEnumCheck(e => e.trang_thai_gui);
        });

        builder.Property(e => e.loai_thong_bao).AsEnumString(32);
        builder.Property(e => e.tieu_de).HasMaxLength(160);
        builder.Property(e => e.noi_dung).AsText();
        builder.Property(e => e.loai_doi_tuong).HasMaxLength(32);
        builder.Property(e => e.trang_thai_gui).AsEnumString(16);
        builder.Property(e => e.ma_chong_trung).HasMaxLength(160);

        builder.HasIndex(e => new { e.tai_khoan_id, e.ma_chong_trung }).IsUnique();
        builder.HasIndex(e => new { e.trang_thai_gui, e.thoi_diem_du_kien });
        builder.HasIndex(e => new { e.tai_khoan_id, e.ngay_tao }).IsDescending(false, true);

        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.tai_khoan_id);
    }
}
