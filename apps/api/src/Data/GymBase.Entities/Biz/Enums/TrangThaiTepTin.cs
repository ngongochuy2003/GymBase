namespace GymBase.Entities.Biz.Enums;

/// <summary>Vòng đời file: xin upload, xác nhận trên S3, gắn vào bản ghi, xóa.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum TrangThaiTepTin
{
    cho_tai_len,
    da_tai_len,
    da_lien_ket,
    loi,
    da_xoa,
}
