namespace GymBase.Entities.Biz.Enums;

/// <summary>Loại hội thoại. V1 chỉ dùng truc_tiep.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum LoaiCuocTroChuyen
{
    truc_tiep,
    nhom,
}
