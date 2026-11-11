namespace GymBase.Entities.Biz.Enums;

/// <summary>Loại đối tượng bị báo cáo. Liên kết đa hình, không có khóa ngoại.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum LoaiDoiTuongBaoCao
{
    bai_viet,
    tuong_tac_bai_viet,
    tai_khoan,
    tin_nhan,
}
