using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.Biz;

/// <summary>
/// Bảng <c>cuoc_tro_chuyen</c>. <c>ma_hoi_thoai_doi</c> unique để một cặp tài khoản chỉ có một chat trực tiếp;
/// MySQL cho phép nhiều NULL nên chat nhóm để trống.
/// </summary>
public sealed class CuocTroChuyenConfiguration : IEntityTypeConfiguration<cuoc_tro_chuyen>
{
    public void Configure(EntityTypeBuilder<cuoc_tro_chuyen> builder)
    {
        builder.ToTable(nameof(cuoc_tro_chuyen), table =>
        {
            table.HasEnumCheck(e => e.loai_cuoc_tro_chuyen);
            table.HasCheckConstraint(
                "CK_cuoc_tro_chuyen_ma_hoi_thoai_doi",
                $"`loai_cuoc_tro_chuyen` <> '{nameof(LoaiCuocTroChuyen.truc_tiep)}' OR `ma_hoi_thoai_doi` IS NOT NULL");
        });

        builder.Property(e => e.loai_cuoc_tro_chuyen).AsEnumString(12);
        builder.Property(e => e.ma_hoi_thoai_doi).HasMaxLength(80);
        builder.Property(e => e.ten_nhom).HasMaxLength(120);

        builder.HasIndex(e => e.ma_hoi_thoai_doi).IsUnique();
    }
}
