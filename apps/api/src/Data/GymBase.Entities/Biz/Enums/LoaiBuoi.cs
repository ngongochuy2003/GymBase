namespace GymBase.Entities.Biz.Enums;

/// <summary>Buổi có PT trực tiếp (được trừ gói) hay tự tập (không bao giờ trừ).</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum LoaiBuoi
{
    pt_truc_tiep,
    tu_tap,
}
