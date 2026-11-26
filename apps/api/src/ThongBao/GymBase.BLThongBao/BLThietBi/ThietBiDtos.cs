using System.Text.Json.Serialization;

namespace GymBase.BLThongBao.BLThietBi;

/// <summary>
/// App báo thiết bị của mình: gọi khi đăng nhập, khi mở app và khi token FCM đổi (ADR-2, quyết định 6, 9).
/// <c>push_token</c> có thể vắng nếu người dùng chưa cấp quyền thông báo.
/// </summary>
public sealed record DangKyThietBiDto(
    [property: JsonPropertyName("ma_thiet_bi")] string? MaThietBi,
    [property: JsonPropertyName("nen_tang")] string? NenTang,
    [property: JsonPropertyName("push_token")] string? PushToken,
    [property: JsonPropertyName("mui_gio")] string? MuiGio);
