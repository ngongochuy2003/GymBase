namespace GymBase.Entities.Biz.Enums;

/// <summary>Kế hoạch dinh dưỡng hay tập luyện.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum LoaiKeHoach
{
    dinh_duong,
    tap_luyen,
}
