using GymBase.Base.Logging;
using GymBase.Base.Paging;
using GymBase.DLAuth.Contract.DLTaiKhoan;
using GymBase.Entities.Auth;
using GymBase.Persistence;
using GymBase.Persistence.Logging;
using Microsoft.EntityFrameworkCore;

namespace GymBase.UnitTests.Auth;

/// <summary>Kiểm tra model EF dựng ra đúng quyết định về DB. Không cần kết nối MySQL.</summary>
public class AppDbContextModelTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>();
        PersistenceServiceCollectionExtensions.ConfigureMySql(options, "Server=localhost;Database=model_only");
        return new AppDbContext(options.Options);
    }

    [Fact]
    public void Id_is_stored_as_varchar36()
    {
        using var db = CreateContext();

        var id = db.Model.FindEntityType(typeof(tai_khoan))!.FindProperty(nameof(tai_khoan.Id))!;

        Assert.Equal("varchar(36)", id.GetColumnType());
    }

    [Fact]
    public void Foreign_key_uses_same_type_as_primary_key()
    {
        using var db = CreateContext();

        var script = db.Database.GenerateCreateScript();

        Assert.Contains("`tai_khoan_id` varchar(36)", script);
        Assert.DoesNotContain("binary(16)", script);
    }

    [Fact]
    public void Guid_is_compared_as_lowercase_text()
    {
        using var db = CreateContext();
        var id = Guid.Parse("0193F0A0-0000-7000-8000-000000000001");

        var sql = db.Set<tai_khoan>().Where(u => u.Id == id).ToQueryString();

        Assert.Contains("'0193f0a0-0000-7000-8000-000000000001'", sql);
    }

    [Fact]
    public void Sort_and_filter_translate_to_sql_with_parameters()
    {
        using var db = CreateContext();
        var columns = TaiKhoanQueryColumns.Allowed;

        var filtered = PagingQuery.ApplyFilter(db.Set<tai_khoan>(), "vai_tro:eq:User,ngay_tao:gte:2026-11-01", columns);
        var sql = PagingQuery.ApplySort(filtered, "-ho_ten", columns).ToQueryString();

        Assert.Contains("ORDER BY `t`.`ho_ten` DESC, `t`.`Id` DESC", sql);
        Assert.Contains("`t`.`vai_tro` = ", sql);
        Assert.Contains("`t`.`ngay_tao` >= @", sql);
    }

    [Fact]
    public void Quote_in_filter_value_is_escaped()
    {
        using var db = CreateContext();

        var sql = PagingQuery.ApplyFilter(db.Set<tai_khoan>(), "vai_tro:eq:x' OR '1'='1", TaiKhoanQueryColumns.Allowed).ToQueryString();

        Assert.DoesNotContain("'x' OR '1'='1'", sql);
    }

    [Fact]
    public void System_log_has_expected_table_and_time_index()
    {
        using var db = CreateContext();

        var script = db.Database.GenerateCreateScript();

        Assert.Contains("CREATE TABLE `system_log`", script);
        Assert.Contains("`duong_dan` varchar(500)", script);
        Assert.Contains("ON `system_log` (`thoi_diem`)", script);
    }

    [Fact]
    public void Audit_entry_longer_than_columns_is_truncated()
    {
        var entry = new AuditLogEntry(new string('u', 60), "POST", new string('p', 600), 201, 5, "trace", null, DateTime.UtcNow);

        var row = SystemLogWriter.ToRow(entry);

        Assert.Equal(50, row.nguoi_dung!.Length);
        Assert.Equal(500, row.duong_dan.Length);
        Assert.Null(row.ip);
    }

    [Fact]
    public void Username_is_unique()
    {
        using var db = CreateContext();

        var script = db.Database.GenerateCreateScript();

        Assert.Contains("CREATE UNIQUE INDEX `IX_tai_khoan_ten_dang_nhap` ON `tai_khoan` (`ten_dang_nhap`)", script);
        Assert.DoesNotContain("is_deleted", script);
    }
}
