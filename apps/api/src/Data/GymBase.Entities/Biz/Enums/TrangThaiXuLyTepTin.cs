namespace GymBase.Entities.Biz.Enums;

/// <summary>
/// Việc xử lý nền sau khi tệp được xác nhận (bỏ EXIF, xoay đúng chiều, tính blurhash). Cột này đồng thời là hàng đợi của worker:
/// <c>cho_xu_ly</c> là việc chưa làm, <c>dang_xu_ly</c> là việc một worker đã nhận.
/// </summary>
/// <remarks>Tên thành viên trùng giá trị lưu trong DB (snake_case), lưu dạng chuỗi bằng <c>HasConversion&lt;string&gt;()</c>.</remarks>
public enum TrangThaiXuLyTepTin
{
    /// <summary>Không cần xử lý (PDF, video, ảnh HEIC chưa giải mã được).</summary>
    khong_can,
    cho_xu_ly,
    dang_xu_ly,
    xong,
    loi,
}
