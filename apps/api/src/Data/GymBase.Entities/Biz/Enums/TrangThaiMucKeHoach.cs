namespace GymBase.Entities.Biz.Enums;

/// <summary>Trạng thái mục lịch trong kế hoạch.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum TrangThaiMucKeHoach
{
    dang_ap_dung,
    tam_dung,
    ngung,
}
