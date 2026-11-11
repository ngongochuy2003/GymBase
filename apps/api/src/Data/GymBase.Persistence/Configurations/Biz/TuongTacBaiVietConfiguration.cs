using GymBase.Entities.Auth;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.Biz;

/// <summary>
/// Bảng <c>tuong_tac_bai_viet</c>: lượt thích và bình luận. Mỗi người chỉ có một lượt thích còn hiệu lực trên một bài,
/// chặn bằng cột sinh tự động kèm unique index. <c>tuong_tac_cha_id</c> phải cùng bài viết, do service kiểm tra.
/// </summary>
public sealed class TuongTacBaiVietConfiguration : IEntityTypeConfiguration<tuong_tac_bai_viet>
{
    private const string ActiveLikeColumn = "thich_hieu_luc";

    public void Configure(EntityTypeBuilder<tuong_tac_bai_viet> builder)
    {
        builder.ToTable(nameof(tuong_tac_bai_viet), table =>
        {
            table.HasEnumCheck(e => e.loai_tuong_tac);

            // Bình luận phải có nội dung. Lượt thích không có nội dung và không trả lời ai.
            table.HasCheckConstraint(
                "CK_tuong_tac_bai_viet_noi_dung",
                $"(`loai_tuong_tac` = '{nameof(LoaiTuongTac.binh_luan)}' AND `noi_dung` IS NOT NULL) " +
                $"OR (`loai_tuong_tac` = '{nameof(LoaiTuongTac.thich)}' AND `noi_dung` IS NULL AND `tuong_tac_cha_id` IS NULL)");
        });

        builder.Property(e => e.loai_tuong_tac).AsEnumString(16);
        builder.Property(e => e.noi_dung).AsText();

        builder.HasUniqueComputedColumn(
            ActiveLikeColumn,
            73,
            $"IF(`loai_tuong_tac` = '{nameof(LoaiTuongTac.thich)}' AND `ngay_xoa` IS NULL, CONCAT(`bai_viet_id`, '|', `tai_khoan_id`), NULL)");
        builder.HasIndex(e => new { e.bai_viet_id, e.loai_tuong_tac, e.ngay_tao }).IsDescending(false, false, true);

        builder.HasRestrictForeignKey(typeof(bai_viet), e => e.bai_viet_id);
        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.tai_khoan_id);
        builder.HasRestrictForeignKey(typeof(tuong_tac_bai_viet), e => e.tuong_tac_cha_id);
    }
}
