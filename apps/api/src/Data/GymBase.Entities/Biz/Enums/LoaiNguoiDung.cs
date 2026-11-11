namespace GymBase.Entities.Biz.Enums;

/// <summary>Nhóm người dùng trên hồ sơ. Điều hướng sản phẩm, không thay thế phân quyền.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum LoaiNguoiDung
{
    hoc_vien,
    pt,
    ca_hai,
}
