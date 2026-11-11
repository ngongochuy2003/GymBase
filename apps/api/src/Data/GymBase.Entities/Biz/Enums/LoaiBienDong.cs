namespace GymBase.Entities.Biz.Enums;

/// <summary>Loại giao dịch trên sổ cái số buổi.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum LoaiBienDong
{
    cap_buoi,
    tru_buoi,
    hoan_buoi,
    dieu_chinh,
}
