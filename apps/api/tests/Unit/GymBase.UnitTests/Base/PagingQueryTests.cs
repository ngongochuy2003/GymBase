using GymBase.Base.Entities;
using GymBase.Base.Exceptions;
using GymBase.Base.Paging;

namespace GymBase.UnitTests.Base;

/// <summary>Cú pháp sort, filter và việc chặn cột ngoài danh sách cho phép.</summary>
public class PagingQueryTests
{
    private sealed class Row : BaseEntity
    {
        public string ten { get; set; } = string.Empty;
        public string? ghi_chu { get; set; }
        public int so_buoi { get; set; }
        public bool hoat_dong { get; set; }
        public DateTime ngay { get; set; }
        public DateTime? ngay_het_han { get; set; }
        public string bi_mat { get; set; } = string.Empty;
    }

    private static readonly QueryColumns<Row> Columns = new QueryColumns<Row>()
        .Add(r => r.ten).Add(r => r.ghi_chu).Add(r => r.so_buoi).Add(r => r.hoat_dong).Add(r => r.ngay).Add(r => r.ngay_het_han);

    private static readonly IQueryable<Row> Rows = new List<Row>
    {
        new() { ten = "b", ghi_chu = "gói vip", so_buoi = 10, hoat_dong = true, ngay = new DateTime(2026, 11, 1, 8, 0, 0, DateTimeKind.Utc) },
        new() { ten = "a", ghi_chu = null, so_buoi = 20, hoat_dong = false, ngay = new DateTime(2026, 11, 2, 8, 0, 0, DateTimeKind.Utc), ngay_het_han = new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc) },
        new() { ten = "c", ghi_chu = "thường", so_buoi = 20, hoat_dong = true, ngay = new DateTime(2026, 11, 3, 8, 0, 0, DateTimeKind.Utc) },
    }.AsQueryable();

    private static string Sorted(string sort) =>
        string.Concat(PagingQuery.ApplySort(Rows, sort, Columns).Select(r => r.ten));

    private static string Filtered(string filter) =>
        string.Concat(PagingQuery.ApplyFilter(Rows, filter, Columns).OrderBy(r => r.ten).Select(r => r.ten));

    [Theory]
    [InlineData("ten", "abc")]
    [InlineData("-ten", "cba")]
    [InlineData("-so_buoi, ten", "acb")]
    public void Sorts_by_allowed_columns(string sort, string expected) => Assert.Equal(expected, Sorted(sort));

    [Theory]
    [InlineData("ten:eq:a", "a")]
    [InlineData("ten:ne:a", "bc")]
    [InlineData("so_buoi:gte:20", "ac")]
    [InlineData("so_buoi:lt:20", "b")]
    [InlineData("hoat_dong:eq:true", "bc")]
    [InlineData("ghi_chu:contains:vip", "b")]
    [InlineData("hoat_dong:eq:true,so_buoi:eq:20", "c")]
    [InlineData("ngay:gte:2026-11-02T08:00:00Z", "ac")]
    [InlineData("ngay_het_han:lt:2026-12-02", "a")]
    public void Filters_by_allowed_columns(string filter, string expected) => Assert.Equal(expected, Filtered(filter));

    [Theory]
    [InlineData("bi_mat")]
    [InlineData("-bi_mat")]
    [InlineData("-")]
    [InlineData("ten,")]
    public void Rejects_sort_outside_allowed_columns(string sort)
    {
        var error = Assert.Throws<ValidationFailedException>(() => Sorted(sort));

        Assert.Equal(ErrorCodes.ValidationFailed, error.Code);
        Assert.True(error.Errors!.ContainsKey("sort"));
    }

    [Theory]
    [InlineData("bi_mat:eq:x")]    // cột ngoài danh sách
    [InlineData("ten")]            // thiếu toán tử
    [InlineData("ten:eq")]         // thiếu giá trị
    [InlineData("ten:like:a")]     // toán tử lạ
    [InlineData("ten:gt:a")]       // chuỗi không so lớn hơn
    [InlineData("hoat_dong:gt:true")]
    [InlineData("so_buoi:contains:1")]
    [InlineData("so_buoi:eq:abc")] // sai kiểu
    [InlineData("ngay:gte:khong-phai-ngay")]
    public void Rejects_invalid_filter(string filter)
    {
        var error = Assert.Throws<ValidationFailedException>(() => Filtered(filter));

        Assert.True(error.Errors!.ContainsKey("filter"));
    }

    [Fact]
    public void Normalize_trims_sort_and_filter()
    {
        var request = new PagingRequest { sort = " -ten ", filter = "   " }.Normalize();

        Assert.Equal("-ten", request.sort);
        Assert.Null(request.filter);
    }
}
