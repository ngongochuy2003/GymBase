namespace GymBase.Entities.Biz.Enums;

/// <summary>Lý do báo cáo vi phạm.</summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum LyDoBaoCao
{
    spam,
    gia_mao,
    quay_roi,
    noi_dung_xau,
    khac,
}
