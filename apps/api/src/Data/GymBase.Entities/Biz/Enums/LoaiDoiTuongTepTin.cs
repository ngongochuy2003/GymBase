namespace GymBase.Entities.Biz.Enums;

/// <summary>Loại bản ghi sở hữu file. Liên kết đa hình, không có khóa ngoại.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum LoaiDoiTuongTepTin
{
    bai_viet,
    tin_nhan,
    nhat_ky_hoat_dong,
    ho_so_nguoi_dung,
}
