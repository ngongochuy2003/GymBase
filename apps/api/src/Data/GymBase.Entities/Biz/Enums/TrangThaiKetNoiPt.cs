namespace GymBase.Entities.Biz.Enums;

/// <summary>Vòng đời kết nối PT với học viên.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum TrangThaiKetNoiPt
{
    cho_phan_hoi,
    dang_hoat_dong,
    tu_choi,
    da_huy,
    da_ket_thuc,
}
