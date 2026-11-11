using GymBase.Entities.Auth;
using GymBase.Entities.Biz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.Biz;

/// <summary>Bảng <c>theo_doi</c>: khóa chính kép, không được tự theo dõi.</summary>
public sealed class TheoDoiConfiguration : IEntityTypeConfiguration<theo_doi>
{
    public void Configure(EntityTypeBuilder<theo_doi> builder)
    {
        builder.ToTable(nameof(theo_doi), table =>
            table.HasCheckConstraint("CK_theo_doi_khong_tu_theo_doi", "`nguoi_theo_doi_id` <> `nguoi_duoc_theo_doi_id`"));

        builder.HasKey(e => new { e.nguoi_theo_doi_id, e.nguoi_duoc_theo_doi_id });

        builder.HasIndex(e => new { e.nguoi_duoc_theo_doi_id, e.ngay_theo_doi }).IsDescending(false, true);

        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.nguoi_theo_doi_id);
        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.nguoi_duoc_theo_doi_id);
    }
}
