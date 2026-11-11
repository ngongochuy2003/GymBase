namespace GymBase.Base.Paging;

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

    /// <summary>Sắp xếp, ví dụ <c>-ngay_tao,ho_ten</c>; trống thì mới tạo trước. Xem <see cref="PagingQuery"/>.</summary>
    public string? sort { get; set; }

    /// <summary>Lọc, ví dụ <c>vai_tro:eq:User,ngay_tao:gte:2026-11-01</c>. Xem <see cref="PagingQuery"/>.</summary>
    public string? filter { get; set; }

    /// <summary>Số bản ghi bỏ qua trước trang này.</summary>
    public int Skip => (page - 1) * page_size;

    /// <summary>Trả về bản đã đưa về giá trị hợp lệ: page ≥ 1, 1 ≤ page_size ≤ 100, search, sort, filter đã trim.</summary>
    public PagingRequest Normalize() => new()
    {
        page = Math.Max(page, 1),
        page_size = page_size < 1 ? DefaultPageSize : Math.Min(page_size, MaxPageSize),
        search = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
        sort = string.IsNullOrWhiteSpace(sort) ? null : sort.Trim(),
        filter = string.IsNullOrWhiteSpace(filter) ? null : filter.Trim(),
    };
}
