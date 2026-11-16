namespace GymBase.Entities.Biz.Enums;

/// <summary>Giới tính trên hồ sơ. Danh sách cố định trong code, không đọc từ bảng danh_muc.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum GioiTinh
{
    nam,
    nu,
    khac,
    khong_muon_noi,
}
