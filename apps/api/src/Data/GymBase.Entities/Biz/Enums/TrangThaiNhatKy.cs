namespace GymBase.Entities.Biz.Enums;

/// <summary>Vòng đời nhật ký: nộp, PT đánh giá, sửa lại.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum TrangThaiNhatKy
{
    nhap,
    cho_kiem_tra,
    dat,
    can_dieu_chinh,
    hoan_thanh,
}
