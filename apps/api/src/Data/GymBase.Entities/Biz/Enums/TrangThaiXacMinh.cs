namespace GymBase.Entities.Biz.Enums;

/// <summary>Trạng thái xác minh PT do Admin duyệt.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum TrangThaiXacMinh
{
    chua_xac_minh,
    dang_xem_xet,
    da_xac_minh,
    tu_choi,
}
