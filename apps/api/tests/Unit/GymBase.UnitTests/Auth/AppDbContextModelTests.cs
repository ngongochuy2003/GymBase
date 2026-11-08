using GymBase.DLAuth.Contract.Entities;
using GymBase.DLAuth.MySql;
using GymBase.DLBiz.MySql.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymBase.UnitTests.Auth;

/// <summary>Kiểm tra model EF dựng ra đúng quyết định về DB. Không cần kết nối MySQL.</summary>
public class AppDbContextModelTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>();
        PersistenceServiceCollectionExtensions.ConfigureMySql(options, "Server=localhost;Database=model_only");
        return new AppDbContext(options.Options, new EntityConfigurationAssemblies([typeof(UserRepository).Assembly]));
    }

    [Fact]
    public void Id_is_stored_as_binary16()
    {
        using var db = CreateContext();

        var id = db.Model.FindEntityType(typeof(nguoi_dung))!.FindProperty(nameof(nguoi_dung.Id))!;

        Assert.Equal("binary(16)", id.GetColumnType());
    }

    [Fact]
    public void Soft_deleted_rows_are_filtered_out()
    {
        using var db = CreateContext();

        var sql = db.Set<nguoi_dung>().ToQueryString();

        Assert.Contains("NOT (`n`.`is_deleted`)", sql);
    }

    [Fact]
    public void Username_is_unique_only_among_active_users()
    {
        using var db = CreateContext();

        var script = db.Database.GenerateCreateScript();

        Assert.Contains("`tai_khoan_hieu_luc` varchar(50)", script);
        Assert.Contains("IF(`is_deleted` = 0, `tai_khoan`, NULL)", script);
        Assert.Contains("CREATE UNIQUE INDEX", script);
    }
}
