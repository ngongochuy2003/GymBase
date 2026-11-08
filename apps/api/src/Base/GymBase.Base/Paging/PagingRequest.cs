namespace GymBase.Base.Paging;

// TODO(PAGING): thêm sort (dạng sort=-ngay_tao,ho_ten) và filter; chỉ nhận cột nằm trong danh sách cho phép,
//   cột ngoài danh sách trả 400. Mục 6.
/// <summary>
/// Tham số lấy danh sách có phân trang, nhận từ query string (?page=1&amp;page_size=20&amp;search=...).
/// Tên property snake_case để trùng tên tham số (quy ước Q2). Đặt ở DL.Contract để cả BL và DL dùng chung.
/// </summary>
public sealed class PagingRequest
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    /// <summary>Trang cần lấy, bắt đầu từ 1.</summary>
    public int page { get; set; } = 1;

    /// <summary>Số bản ghi mỗi trang, tối đa <see cref="MaxPageSize"/>. Không có giá trị "lấy tất cả".</summary>
    public int page_size { get; set; } = DefaultPageSize;

    /// <summary>Từ khóa tìm kiếm; mỗi màn hình tự quyết định tìm theo cột nào.</summary>
    public string? search { get; set; }

    /// <summary>Số bản ghi bỏ qua trước trang này.</summary>
    public int Skip => (page - 1) * page_size;

    /// <summary>Trả về bản đã đưa về giá trị hợp lệ: page ≥ 1, 1 ≤ page_size ≤ 100, search đã trim.</summary>
    public PagingRequest Normalize() => new()
    {
        page = Math.Max(page, 1),
        page_size = page_size < 1 ? DefaultPageSize : Math.Min(page_size, MaxPageSize),
        search = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
    };
}
