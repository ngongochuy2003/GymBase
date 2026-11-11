namespace GymBase.Entities.Biz.Enums;

/// <summary>Loại nội dung tin nhắn.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum LoaiTinNhan
{
    van_ban,
    tep_tin,
    hon_hop,
}
