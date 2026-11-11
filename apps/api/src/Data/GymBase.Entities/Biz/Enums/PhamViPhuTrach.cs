namespace GymBase.Entities.Biz.Enums;

/// <summary>Lĩnh vực PT được phụ trách trong một kết nối.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum PhamViPhuTrach
{
    tap_luyen,
    dinh_duong,
}
