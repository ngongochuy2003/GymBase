namespace GymBase.Entities.Biz.Enums;

/// <summary>Vòng đời bài viết.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum TrangThaiBaiViet
{
    nhap,
    da_dang,
    an,
    go_bo,
}
