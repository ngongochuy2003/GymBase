namespace GymBase.Entities.Biz.Enums;

/// <summary>Trạng thái đẩy thông báo qua FCM. da_gui không chứng minh thiết bị đã nhận hay người dùng đã đọc.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum TrangThaiGui
{
    cho_gui,
    dang_gui,
    da_gui,
    loi,
    huy,
}
