namespace GymBase.AMISBase.DL.Contract.Paging;

// TODO(PAGING): thêm sort và filter; chỉ nhận cột nằm trong danh sách cho phép để tránh SQL injection. Mục 6.
/// <summary>
/// Tham số lấy danh sách có phân trang, nhận từ query string (?page=1&amp;pageSize=20&amp;search=...).
/// Đặt ở DL.Contract để cả BL và DL dùng chung.
/// </summary>
public sealed class PagingRequest
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    /// <summary>Trang cần lấy, bắt đầu từ 1.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Số bản ghi mỗi trang, tối đa <see cref="MaxPageSize"/>.</summary>
    public int PageSize { get; set; } = DefaultPageSize;

    /// <summary>Từ khóa tìm kiếm; mỗi màn hình tự quyết định tìm theo cột nào.</summary>
    public string? Search { get; set; }

    /// <summary>Số bản ghi bỏ qua, dùng cho OFFSET trong SQL.</summary>
    public int Skip => (Page - 1) * PageSize;

    /// <summary>Trả về bản đã đưa về giá trị hợp lệ: page ≥ 1, 1 ≤ pageSize ≤ 100, search đã trim.</summary>
    public PagingRequest Normalize() => new()
    {
        Page = Math.Max(Page, 1),
        PageSize = PageSize < 1 ? DefaultPageSize : Math.Min(PageSize, MaxPageSize),
        Search = string.IsNullOrWhiteSpace(Search) ? null : Search.Trim(),
    };
}
