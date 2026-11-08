namespace GymBase.Base.Paging;

/// <summary>
/// Một trang dữ liệu, trả ra API dạng <c>{ "data": [...], "meta": { ... } }</c>.
/// Tên property snake_case để JSON trùng tên (quy ước Q2), không cần cấu hình serializer.
/// </summary>
public sealed class PagingResult<T>
{
    public PagingResult(IReadOnlyList<T> data, int page, int pageSize, int total)
    {
        this.data = data;
        meta = new PagingMeta(page, pageSize, total);
    }

    private PagingResult(IReadOnlyList<T> data, PagingMeta meta)
    {
        this.data = data;
        this.meta = meta;
    }

    /// <summary>Các bản ghi của trang hiện tại.</summary>
    public IReadOnlyList<T> data { get; }

    /// <summary>Thông tin phân trang.</summary>
    public PagingMeta meta { get; }

    /// <summary>Đổi kiểu phần tử (ví dụ entity → DTO), giữ nguyên thông tin phân trang.</summary>
    public PagingResult<TOut> Map<TOut>(Func<T, TOut> map) => new(data.Select(map).ToList(), meta);
}

/// <summary>Thông tin phân trang đi kèm <see cref="PagingResult{T}.data"/>.</summary>
public sealed class PagingMeta
{
    public PagingMeta(int page, int pageSize, int total)
    {
        this.page = page;
        page_size = pageSize;
        this.total = total;
        total_page = pageSize <= 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

        var from = (page - 1) * pageSize + 1;
        ranger = total == 0 || page < 1 || page > total_page
            ? new PagingRange(0, 0)
            : new PagingRange(from, Math.Min(from + pageSize - 1, total));
    }

    /// <summary>Trang hiện tại, bắt đầu từ 1.</summary>
    public int page { get; }

    /// <summary>Số bản ghi mỗi trang.</summary>
    public int page_size { get; }

    /// <summary>Tổng số bản ghi thỏa điều kiện (mọi trang).</summary>
    public int total { get; }

    /// <summary>Tổng số trang; 0 khi không có bản ghi.</summary>
    public int total_page { get; }

    /// <summary>Thứ tự bản ghi đầu và cuối của trang này (đếm từ 1); cả hai là 0 khi trang rỗng.</summary>
    public PagingRange ranger { get; }
}

/// <summary>Khoảng bản ghi của một trang, dùng để hiện "21–40 trên 57".</summary>
public sealed record PagingRange(int from, int to);
