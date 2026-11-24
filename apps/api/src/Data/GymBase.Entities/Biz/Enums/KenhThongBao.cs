namespace GymBase.Entities.Biz.Enums;

/// <summary>Đường một thông báo tới người dùng ngoài hộp thư trong app.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum KenhThongBao
{
    /// <summary>Server đẩy qua FCM (worker quét <c>cho_gui</c>).</summary>
    push,

    /// <summary>App tự đặt lịch trên máy (nhắc ăn, nhắc tập). Server không push, chỉ ghi dòng cho hộp thư.</summary>
    local,

    /// <summary>Người dùng đã tắt loại thông báo này: dòng vẫn hiện trong hộp thư, không có chuông.</summary>
    chi_hop_thu,
}
