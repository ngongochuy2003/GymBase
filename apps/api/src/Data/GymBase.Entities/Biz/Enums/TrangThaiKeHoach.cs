namespace GymBase.Entities.Biz.Enums;

/// <summary>Vòng đời kế hoạch.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum TrangThaiKeHoach
{
    nhap,
    dang_ap_dung,
    tam_dung,
    ket_thuc,
}
