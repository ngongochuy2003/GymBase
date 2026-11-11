namespace GymBase.Entities.Biz.Enums;

/// <summary>Mục lịch là bữa ăn hay buổi tập. Phải khớp loại của kế hoạch cha.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum LoaiMuc
{
    bua_an,
    buoi_tap,
}
