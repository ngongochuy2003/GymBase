using System.Text.Json;
using GymBase.Base.Paging;

namespace GymBase.UnitTests.Base;

/// <summary>Thông tin phân trang (<see cref="PagingMeta"/>) và dạng JSON của một trang dữ liệu.</summary>
public class PagingTests
{
    [Theory]
    [InlineData(1, 20, 57, 3, 1, 20)]
    [InlineData(3, 20, 57, 3, 41, 57)] // trang cuối chưa đủ
    [InlineData(4, 20, 57, 3, 0, 0)]   // vượt quá số trang
    [InlineData(1, 20, 0, 0, 0, 0)]    // không có bản ghi
    [InlineData(2, 10, 20, 2, 11, 20)] // trang cuối vừa đủ
    public void Meta_computes_total_page_and_ranger(
        int page, int pageSize, int total, int expectedTotalPage, int expectedFrom, int expectedTo)
    {
        var meta = new PagingMeta(page, pageSize, total);

        Assert.Equal(expectedTotalPage, meta.total_page);
        Assert.Equal(new PagingRange(expectedFrom, expectedTo), meta.ranger);
    }

    [Fact]
    public void Map_keeps_meta()
    {
        var result = new PagingResult<int>([1, 2], page: 2, pageSize: 2, total: 5).Map(x => x.ToString());

        Assert.Equal(["1", "2"], result.data);
        Assert.Equal(2, result.meta.page);
        Assert.Equal(5, result.meta.total);
    }

    [Fact]
    public void Serializes_as_data_and_meta_with_snake_case_names()
    {
        var result = new PagingResult<int>([7], page: 2, pageSize: 1, total: 3);

        // Cùng cấu hình JSON mặc định của ASP.NET Core (camelCase): tên snake_case giữ nguyên.
        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Equal(
            """{"data":[7],"meta":{"page":2,"page_size":1,"total":3,"total_page":3,"ranger":{"from":2,"to":2}}}""",
            json);
    }
}
