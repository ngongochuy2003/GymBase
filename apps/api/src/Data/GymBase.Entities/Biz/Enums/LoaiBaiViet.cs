namespace GymBase.Entities.Biz.Enums;

/// <summary>Loại nội dung của bài viết.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum LoaiBaiViet
{
    van_ban,
    hinh_anh,
    video,
    hon_hop,
}
