using GymBase.Entities.Auth;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.Biz;

/// <summary>
/// Bảng <c>ket_noi_pt</c>. Mỗi cặp PT với học viên chỉ có một kết nối đang mở (<c>cho_phan_hoi</c> hoặc
/// <c>dang_hoat_dong</c>), chặn bằng cột sinh tự động kèm unique index.
/// </summary>
public sealed class KetNoiPtConfiguration : IEntityTypeConfiguration<ket_noi_pt>
{
    private const string OpenPairColumn = "cap_dang_mo";

    public void Configure(EntityTypeBuilder<ket_noi_pt> builder)
    {
        builder.ToTable(nameof(ket_noi_pt), table =>
        {
            table.HasEnumCheck(e => e.trang_thai);
            table.HasCheckConstraint("CK_ket_noi_pt_khac_tai_khoan", "`pt_id` <> `hoc_vien_id`");
            table.HasCheckConstraint("CK_ket_noi_pt_pham_vi", "JSON_TYPE(`pham_vi_phu_trach`) = 'ARRAY'");
        });

        builder.Property(e => e.pham_vi_phu_trach).AsJson();
        builder.Property(e => e.loi_nhan).AsText();
        builder.Property(e => e.trang_thai).AsEnumString(20);
        builder.Property(e => e.ly_do_ket_thuc).AsText();

        builder.HasUniqueComputedColumn(
            OpenPairColumn,
            73,
            $"IF(`trang_thai` IN ('{nameof(TrangThaiKetNoiPt.cho_phan_hoi)}', '{nameof(TrangThaiKetNoiPt.dang_hoat_dong)}'), CONCAT(`pt_id`, '|', `hoc_vien_id`), NULL)");
        builder.HasIndex(e => new { e.pt_id, e.trang_thai });
        builder.HasIndex(e => new { e.hoc_vien_id, e.trang_thai });

        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.pt_id);
        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.hoc_vien_id);
        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.nguoi_khoi_tao_id);
    }
}
