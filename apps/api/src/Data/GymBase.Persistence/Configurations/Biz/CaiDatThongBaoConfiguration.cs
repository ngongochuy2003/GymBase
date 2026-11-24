using GymBase.Entities.Auth;
using GymBase.Entities.Biz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.Biz;

/// <summary>Bảng <c>cai_dat_thong_bao</c>: <c>(tai_khoan_id, loai_thong_bao)</c> unique, mỗi loại một dòng.</summary>
public sealed class CaiDatThongBaoConfiguration : IEntityTypeConfiguration<cai_dat_thong_bao>
{
    public void Configure(EntityTypeBuilder<cai_dat_thong_bao> builder)
    {
        builder.ToTable(nameof(cai_dat_thong_bao), table => table.HasEnumCheck(e => e.loai_thong_bao));

        builder.ConfigureAudit();
        builder.Property(e => e.loai_thong_bao).AsEnumString(32);

        builder.HasIndex(e => new { e.tai_khoan_id, e.loai_thong_bao }).IsUnique();

        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.tai_khoan_id);
    }
}
