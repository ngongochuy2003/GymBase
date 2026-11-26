using GymBase.Entities.Biz.Enums;

namespace GymBase.BLThongBao.BLDayThongBao;

/// <summary>Quy tắc theo loại thông báo: ưu tiên và thời gian sống của push (quyết định 8 của ADR-2), kênh mặc định (quyết định 12, 17).</summary>
public static class ThongBaoPolicy
{
    /// <summary>Thông báo có giờ giấc và tin nhắn dùng ưu tiên cao; loại ít gấp dùng thường.</summary>
    public static UuTienPush UuTien(LoaiThongBao loai) => loai switch
    {
        LoaiThongBao.nhac_tap or LoaiThongBao.nhac_an or LoaiThongBao.tin_nhan => UuTienPush.Cao,
        _ => UuTienPush.Thuong,
    };

    /// <summary>Quá thời gian này mà chưa giao được thì thôi, không giao muộn.</summary>
    public static TimeSpan Ttl(LoaiThongBao loai) => loai switch
    {
        LoaiThongBao.nhac_tap or LoaiThongBao.nhac_an => TimeSpan.FromMinutes(10),
        LoaiThongBao.tin_nhan => TimeSpan.FromHours(1),
        _ => TimeSpan.FromDays(1),
    };

    /// <summary>Nhắc ăn, nhắc tập do app đặt lịch trên máy nên server không push; loại khác push.</summary>
    public static KenhThongBao KenhMacDinh(LoaiThongBao loai) =>
        loai is LoaiThongBao.nhac_an or LoaiThongBao.nhac_tap ? KenhThongBao.local : KenhThongBao.push;
}
