namespace GymBase.Base.Entities;

/// <summary>
/// Cách duy nhất để ghi các trường audit. Các hàm audit bắt buộc thời gian là UTC,
/// để một giá trị giờ địa phương không lọt vào DB. Tầng DL gọi các hàm này trước khi insert/update/delete.
/// </summary>
public static class EntityAuditExtensions
{
    /// <summary>Ghi người tạo và thời điểm tạo.</summary>
    public static void MarkCreated(this BaseAuditableEntity entity, string? userName, DateTime utcNow)
    {
        EnsureUtc(utcNow);
        entity.ngay_tao = utcNow;
        entity.nguoi_tao = userName;
    }

    /// <summary>Ghi người sửa và thời điểm sửa; không đụng tới thông tin tạo.</summary>
    public static void MarkUpdated(this BaseAuditableEntity entity, string? userName, DateTime utcNow)
    {
        EnsureUtc(utcNow);
        entity.ngay_chinh_sua = utcNow;
        entity.nguoi_chinh_sua = userName;
    }

    private static void EnsureUtc(DateTime value)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException(
                $"Thời gian phải là UTC (DateTimeKind.Utc), nhận được {value.Kind}. Dùng DateTime.UtcNow hoặc TimeProvider.GetUtcNow().",
                nameof(value));
        }
    }
}
