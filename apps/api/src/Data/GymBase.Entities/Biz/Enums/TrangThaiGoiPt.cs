namespace GymBase.Entities.Biz.Enums;

/// <summary>Vòng đời gói buổi PT.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum TrangThaiGoiPt
{
    cho_kich_hoat,
    dang_su_dung,
    tam_dung,
    hoan_tat,
    huy,
}
