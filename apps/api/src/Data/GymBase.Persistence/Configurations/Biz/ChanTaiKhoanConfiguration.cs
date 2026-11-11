using GymBase.Entities.Auth;
using GymBase.Entities.Biz;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.Biz;

/// <summary>Bảng <c>chan_tai_khoan</c>: khóa chính kép, không được tự chặn.</summary>
public sealed class ChanTaiKhoanConfiguration : IEntityTypeConfiguration<chan_tai_khoan>
{
    public void Configure(EntityTypeBuilder<chan_tai_khoan> builder)
    {
        builder.ToTable(nameof(chan_tai_khoan), table =>
            table.HasCheckConstraint("CK_chan_tai_khoan_khong_tu_chan", "`nguoi_chan_id` <> `nguoi_bi_chan_id`"));

        builder.HasKey(e => new { e.nguoi_chan_id, e.nguoi_bi_chan_id });

        builder.HasIndex(e => e.nguoi_bi_chan_id);

        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.nguoi_chan_id);
        builder.HasRestrictForeignKey(typeof(tai_khoan), e => e.nguoi_bi_chan_id);
    }
}
