namespace GymBase.Entities.Biz.Enums;

/// <summary>Loại bản ghi có lịch sử chỉnh sửa. Danh sách đề xuất, tài liệu thiết kế để mở.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum LoaiDoiTuongLichSu
{
    nhat_ky_hoat_dong,
    ke_hoach,
    muc_ke_hoach,
    buoi_tap,
    goi_pt_hoc_vien,
    bao_cao_vi_pham,
}
