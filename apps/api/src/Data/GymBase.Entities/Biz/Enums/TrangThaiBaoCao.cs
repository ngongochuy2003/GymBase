namespace GymBase.Entities.Biz.Enums;

/// <summary>Vòng đời báo cáo vi phạm.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum TrangThaiBaoCao
{
    moi,
    dang_xu_ly,
    da_giai_quyet,
    bo_qua,
}
