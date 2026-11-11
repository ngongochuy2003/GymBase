namespace GymBase.Entities.Biz.Enums;

/// <summary>Hành động được ghi vào lịch sử chỉnh sửa. Danh sách đề xuất, tài liệu thiết kế để mở.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum HanhDongLichSu
{
    tao,
    cap_nhat,
    danh_gia,
    xac_nhan,
    xu_ly_khieu_nai,
    xu_ly_bao_cao,
    huy,
    ket_thuc,
}
