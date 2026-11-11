using GymBase.Entities.Auth;
using GymBase.Entities.Biz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.Biz;

/// <summary>Bảng <c>thanh_vien_cuoc_tro_chuyen</c>: khóa chính kép (hội thoại, tài khoản).</summary>
public sealed class ThanhVienCuocTroChuyenConfiguration : IEntityTypeConfiguration<thanh_vien_cuoc_tro_chuyen>
{
    public void Configure(EntityTypeBuilder<thanh_vien_cuoc_tro_chuyen> builder)
    {
        builder.ToTable(nameof(thanh_vien_cuoc_tro_chuyen));

        builder.HasKey(e => new { e.cuoc_tro_chuyen_id, e.tai_khoan_id });

        builder.HasIndex(e => new { e.tai_khoan_id, e.ngay_tham_gia }).IsDescending(false, true);

        builder.HasRestrictForeignKey(typeof(cuoc_tro_chuyen), e => e.cuoc_tro_chuyen_id);
        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.tai_khoan_id);
    }
}
