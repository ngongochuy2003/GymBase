namespace GymBase.Entities.Biz.Enums;

/// <summary>Chế độ truy cập file trên S3.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum CheDoTruyCap
{
    cong_khai,
    rieng_tu,
}
