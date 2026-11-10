using GymBase.Entities.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.Logging;

/// <summary>Bảng <c>system_log</c>. Độ dài các cột khớp giới hạn trong <see cref="SystemLogLimits"/>.</summary>
public sealed class SystemLogConfiguration : IEntityTypeConfiguration<system_log>
{
    public void Configure(EntityTypeBuilder<system_log> builder)
    {
        builder.ToTable(nameof(system_log));

        builder.Property(e => e.nguoi_dung).HasMaxLength(SystemLogLimits.UserName);
        builder.Property(e => e.phuong_thuc).HasMaxLength(SystemLogLimits.Method);
        builder.Property(e => e.duong_dan).HasMaxLength(SystemLogLimits.Path);
        builder.Property(e => e.trace_id).HasMaxLength(SystemLogLimits.TraceId);
        builder.Property(e => e.ip).HasMaxLength(SystemLogLimits.IpAddress);

        builder.HasIndex(e => e.thoi_diem);
        builder.HasIndex(e => new { e.nguoi_dung, e.thoi_diem });
    }
}

/// <summary>Độ dài tối đa các cột của <c>system_log</c>.</summary>
public static class SystemLogLimits
{
    public const int UserName = 50;
    public const int Method = 10;
    public const int Path = 500;
    public const int TraceId = 64;

    /// <summary>Độ dài lớn nhất của địa chỉ IPv6 dạng chữ.</summary>
    public const int IpAddress = 45;
}
