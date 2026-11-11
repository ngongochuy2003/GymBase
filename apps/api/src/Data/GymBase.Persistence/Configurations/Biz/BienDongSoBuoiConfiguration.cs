using GymBase.Entities.Auth;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.Biz;

/// <summary>
/// Bảng <c>bien_dong_so_buoi</c>: sổ cái bất biến. Mỗi buổi chỉ bị trừ một lần và hoàn một lần, chặn bằng cột sinh tự động
/// kèm unique index. Việc cấm UPDATE/DELETE bằng trigger nằm trong migration, không khai báo được ở đây.
/// </summary>
public sealed class BienDongSoBuoiConfiguration : IEntityTypeConfiguration<bien_dong_so_buoi>
{
    private const string DeductColumn = "tru_buoi_hieu_luc";
    private const string RefundColumn = "hoan_buoi_hieu_luc";

    public void Configure(EntityTypeBuilder<bien_dong_so_buoi> builder)
    {
        builder.ToTable(nameof(bien_dong_so_buoi), table =>
        {
            table.HasEnumCheck(e => e.loai_bien_dong);

            // Dấu của số buổi phải khớp loại giao dịch; điều chỉnh có thể cộng hoặc trừ nhưng không được bằng 0.
            table.HasCheckConstraint(
                "CK_bien_dong_so_buoi_dau",
                $"(`loai_bien_dong` = '{nameof(LoaiBienDong.cap_buoi)}' AND `so_buoi_thay_doi` > 0) " +
                $"OR (`loai_bien_dong` = '{nameof(LoaiBienDong.tru_buoi)}' AND `so_buoi_thay_doi` < 0) " +
                $"OR (`loai_bien_dong` = '{nameof(LoaiBienDong.hoan_buoi)}' AND `so_buoi_thay_doi` > 0) " +
                $"OR (`loai_bien_dong` = '{nameof(LoaiBienDong.dieu_chinh)}' AND `so_buoi_thay_doi` <> 0)");
        });

        builder.Property(e => e.loai_bien_dong).AsEnumString(20);
        builder.Property(e => e.ma_chong_trung).HasMaxLength(120);
        builder.Property(e => e.ly_do).AsText();

        builder.HasIndex(e => e.ma_chong_trung).IsUnique();
        builder.HasUniqueComputedColumn(
            DeductColumn,
            36,
            $"IF(`loai_bien_dong` = '{nameof(LoaiBienDong.tru_buoi)}', `buoi_tap_id`, NULL)");
        builder.HasUniqueComputedColumn(
            RefundColumn,
            36,
            $"IF(`loai_bien_dong` = '{nameof(LoaiBienDong.hoan_buoi)}', `buoi_tap_id`, NULL)");

        builder.HasRestrictForeignKey(typeof(goi_pt_hoc_vien), e => e.goi_pt_hoc_vien_id);
        builder.HasRestrictForeignKey(typeof(buoi_tap), e => e.buoi_tap_id);
        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.nguoi_tao_id);
    }
}
