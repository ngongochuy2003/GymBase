namespace GymBase.Entities.Biz.Enums;

/// <summary>Vòng đời buổi tập. Trạng thái sau khi xử lý khiếu nại chưa chốt.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum TrangThaiBuoiTap
{
    da_len_lich,
    da_hoan_thanh,
    da_xac_nhan,
    dang_khieu_nai,
    da_huy,
}
