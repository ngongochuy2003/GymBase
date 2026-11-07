namespace GymBase.AMISBase.DL.Contract.Paging;

/// <summary>Một trang dữ liệu kèm thông tin phân trang.</summary>
public sealed class PagingResult<T>
{
    public PagingResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public IReadOnlyList<T> Items { get; }

    public int Page { get; }

    public int PageSize { get; }

    /// <summary>Tổng số bản ghi thỏa điều kiện (mọi trang).</summary>
    public int TotalCount { get; }

    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>Đổi kiểu phần tử (ví dụ entity → DTO), giữ nguyên thông tin phân trang.</summary>
    public PagingResult<TOut> Map<TOut>(Func<T, TOut> map) =>
        new(Items.Select(map).ToList(), Page, PageSize, TotalCount);
}
