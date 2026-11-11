namespace GymBase.Entities.Biz.Enums;

/// <summary>Loại thông báo. Danh sách chưa đầy đủ, thêm khi có nghiệp vụ mới.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum LoaiThongBao
{
    nhac_an,
    nhac_tap,
    tin_nhan,
    danh_gia,
    ket_noi_pt,
}
