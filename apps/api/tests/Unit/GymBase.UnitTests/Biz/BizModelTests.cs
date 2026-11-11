using GymBase.Entities.Auth;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using GymBase.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GymBase.UnitTests.Biz;

/// <summary>Kiểm tra model EF của 21 bảng nghiệp vụ và dang_nhap_ngoai đúng thiết kế. Không cần kết nối MySQL.</summary>
public class BizModelTests
{
    private static readonly string[] NewTables =
    [
        nameof(dang_nhap_ngoai), nameof(ho_so_nguoi_dung), nameof(tep_tin), nameof(bai_viet), nameof(tuong_tac_bai_viet),
        nameof(theo_doi), nameof(ket_noi_pt), nameof(cuoc_tro_chuyen), nameof(thanh_vien_cuoc_tro_chuyen), nameof(tin_nhan),
        nameof(ke_hoach), nameof(muc_ke_hoach), nameof(nhat_ky_hoat_dong), nameof(lich_su_chinh_sua), nameof(chi_so_co_the),
        nameof(goi_pt_hoc_vien), nameof(buoi_tap), nameof(bien_dong_so_buoi), nameof(thiet_bi), nameof(thong_bao),
        nameof(bao_cao_vi_pham), nameof(chan_tai_khoan),
    ];

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>();
        PersistenceServiceCollectionExtensions.ConfigureMySql(options, "Server=localhost;Database=model_only");
        return new AppDbContext(options.Options);
    }

    /// <summary>Model đầy đủ lúc thiết kế: model chạy thật đã bị lược collation, chiều index, check constraint, converter.</summary>
    private static IModel DesignModel(AppDbContext db) => db.GetService<IDesignTimeModel>().Model;

    [Fact]
    public void Every_new_table_is_in_the_model()
    {
        using var db = CreateContext();

        var tables = DesignModel(db).GetEntityTypes().Select(t => t.GetTableName()).ToHashSet();

        Assert.All(NewTables, table => Assert.Contains(table, tables));
    }

    [Fact]
    public void Create_script_builds_for_every_table()
    {
        using var db = CreateContext();

        var script = db.Database.GenerateCreateScript();

        Assert.All(NewTables, table => Assert.Contains($"CREATE TABLE `{table}`", script));
    }

    [Fact]
    public void Foreign_keys_restrict_delete_except_sessions()
    {
        using var db = CreateContext();

        var foreignKeys = DesignModel(db).GetEntityTypes().SelectMany(t => t.GetForeignKeys()).ToList();

        Assert.NotEmpty(foreignKeys);
        foreach (var fk in foreignKeys)
        {
            var table = fk.DeclaringEntityType.GetTableName();
            var expected = (table, fk.Properties[0].Name) switch
            {
                (nameof(phien_dang_nhap), nameof(phien_dang_nhap.tai_khoan_id)) => DeleteBehavior.Cascade,
                (nameof(phien_dang_nhap), nameof(phien_dang_nhap.thiet_bi_id)) => DeleteBehavior.SetNull,
                _ => DeleteBehavior.Restrict,
            };
            Assert.True(fk.DeleteBehavior == expected, $"{table}.{fk.Properties[0].Name} là {fk.DeleteBehavior}, cần {expected}");
        }
    }

    [Fact]
    public void Only_sessions_cascade_in_create_script()
    {
        using var db = CreateContext();

        var script = db.Database.GenerateCreateScript();

        Assert.Equal(1, CountOf(script, "ON DELETE CASCADE"));
        Assert.Equal(1, CountOf(script, "ON DELETE SET NULL"));
    }

    [Fact]
    public void Enum_columns_are_stored_as_text_and_limited_by_check()
    {
        using var db = CreateContext();

        var property = DesignModel(db).FindEntityType(typeof(ket_noi_pt))!.FindProperty(nameof(ket_noi_pt.trang_thai))!;
        var script = db.Database.GenerateCreateScript();

        Assert.Equal("varchar(20)", property.GetColumnType());
        Assert.Equal(typeof(string), property.GetProviderClrType());
        Assert.Contains(
            "`trang_thai` IN ('cho_phan_hoi', 'dang_hoat_dong', 'tu_choi', 'da_huy', 'da_ket_thuc')",
            script);
    }

    [Fact]
    public void Json_lists_are_stored_as_json_with_enum_names()
    {
        using var db = CreateContext();

        var property = DesignModel(db).FindEntityType(typeof(ket_noi_pt))!.FindProperty(nameof(ket_noi_pt.pham_vi_phu_trach))!;
        var converter = property.GetValueConverter()!;
        var stored = (string)converter.ConvertToProvider(new List<PhamViPhuTrach> { PhamViPhuTrach.tap_luyen, PhamViPhuTrach.dinh_duong })!;
        var back = (List<PhamViPhuTrach>)converter.ConvertFromProvider(stored)!;

        Assert.Equal("json", property.GetColumnType());
        Assert.Equal("[\"tap_luyen\",\"dinh_duong\"]", stored);
        Assert.Equal([PhamViPhuTrach.tap_luyen, PhamViPhuTrach.dinh_duong], back);
    }

    [Theory]
    [InlineData("ket_noi_pt", "cap_dang_mo")]
    [InlineData("tuong_tac_bai_viet", "thich_hieu_luc")]
    [InlineData("bien_dong_so_buoi", "tru_buoi_hieu_luc")]
    [InlineData("bien_dong_so_buoi", "hoan_buoi_hieu_luc")]
    public void Partial_unique_rules_use_computed_column_with_unique_index(string table, string column)
    {
        using var db = CreateContext();

        var entity = DesignModel(db).GetEntityTypes().Single(t => t.GetTableName() == table);
        var property = entity.FindProperty(column)!;
        var script = db.Database.GenerateCreateScript();

        Assert.NotNull(property.GetComputedColumnSql());
        Assert.Contains(entity.GetIndexes(), index => index.IsUnique && index.Properties.Single().Name == column);
        Assert.Contains($"`{column}`", script);
    }

    [Fact]
    public void Open_connection_rule_only_counts_pending_and_active()
    {
        using var db = CreateContext();

        var sql = DesignModel(db).FindEntityType(typeof(ket_noi_pt))!.FindProperty("cap_dang_mo")!.GetComputedColumnSql();

        Assert.Equal(
            "IF(`trang_thai` IN ('cho_phan_hoi', 'dang_hoat_dong'), CONCAT(`pt_id`, '|', `hoc_vien_id`), NULL)",
            sql);
    }

    [Theory]
    [InlineData("tep_tin", "s3_key")]
    [InlineData("thiet_bi", "push_token")]
    [InlineData("bien_dong_so_buoi", "ma_chong_trung")]
    [InlineData("cuoc_tro_chuyen", "ma_hoi_thoai_doi")]
    public void Single_column_unique_indexes_exist(string table, string column)
    {
        using var db = CreateContext();

        var entity = DesignModel(db).GetEntityTypes().Single(t => t.GetTableName() == table);

        Assert.Contains(entity.GetIndexes(), index => index.IsUnique && index.Properties.Count == 1 && index.Properties[0].Name == column);
    }

    [Theory]
    [InlineData("tin_nhan", "nguoi_gui_id", "ma_tin_nhan_client")]
    [InlineData("thiet_bi", "tai_khoan_id", "ma_thiet_bi")]
    [InlineData("thong_bao", "tai_khoan_id", "ma_chong_trung")]
    [InlineData("dang_nhap_ngoai", "nha_cung_cap", "ma_ngoai")]
    [InlineData("dang_nhap_ngoai", "tai_khoan_id", "nha_cung_cap")]
    public void Composite_unique_indexes_exist(string table, string first, string second)
    {
        using var db = CreateContext();

        var entity = DesignModel(db).GetEntityTypes().Single(t => t.GetTableName() == table);

        Assert.Contains(
            entity.GetIndexes(),
            index => index.IsUnique && index.Properties.Select(p => p.Name).SequenceEqual([first, second]));
    }

    [Theory]
    [InlineData("theo_doi", "CK_theo_doi_khong_tu_theo_doi")]
    [InlineData("chan_tai_khoan", "CK_chan_tai_khoan_khong_tu_chan")]
    [InlineData("ket_noi_pt", "CK_ket_noi_pt_khac_tai_khoan")]
    [InlineData("bien_dong_so_buoi", "CK_bien_dong_so_buoi_dau")]
    [InlineData("buoi_tap", "CK_buoi_tap_tu_tap_khong_goi")]
    [InlineData("goi_pt_hoc_vien", "CK_goi_pt_hoc_vien_so_buoi_mua")]
    [InlineData("tuong_tac_bai_viet", "CK_tuong_tac_bai_viet_noi_dung")]
    [InlineData("cuoc_tro_chuyen", "CK_cuoc_tro_chuyen_ma_hoi_thoai_doi")]
    public void Business_rules_are_check_constraints(string table, string constraint)
    {
        using var db = CreateContext();

        var entity = DesignModel(db).GetEntityTypes().Single(t => t.GetTableName() == table);

        Assert.Contains(entity.GetCheckConstraints(), check => check.Name == constraint);
    }

    [Fact]
    public void Storage_key_and_push_token_are_case_sensitive()
    {
        using var db = CreateContext();

        var s3Key = DesignModel(db).FindEntityType(typeof(tep_tin))!.FindProperty(nameof(tep_tin.s3_key))!;
        var pushToken = DesignModel(db).FindEntityType(typeof(thiet_bi))!.FindProperty(nameof(thiet_bi.push_token))!;

        Assert.Equal("utf8mb4_bin", s3Key.GetCollation());
        Assert.Equal("utf8mb4_bin", pushToken.GetCollation());
    }

    [Fact]
    public void Profile_key_is_account_id_and_audit_columns_are_limited()
    {
        using var db = CreateContext();

        var profile = DesignModel(db).FindEntityType(typeof(ho_so_nguoi_dung))!;
        var post = DesignModel(db).FindEntityType(typeof(bai_viet))!;

        Assert.Equal([nameof(ho_so_nguoi_dung.tai_khoan_id)], profile.FindPrimaryKey()!.Properties.Select(p => p.Name));
        Assert.Equal(100, post.FindProperty(nameof(bai_viet.nguoi_tao))!.GetMaxLength());
        Assert.Equal(100, profile.FindProperty(nameof(ho_so_nguoi_dung.nguoi_chinh_sua))!.GetMaxLength());
    }

    [Fact]
    public void Newest_first_indexes_are_descending()
    {
        using var db = CreateContext();

        var index = DesignModel(db).FindEntityType(typeof(bai_viet))!.GetIndexes()
            .Single(i => i.Properties.Select(p => p.Name).SequenceEqual([nameof(bai_viet.tac_gia_id), nameof(bai_viet.ngay_dang)]));

        Assert.Equal([false, true], index.IsDescending!);
    }

    private static int CountOf(string text, string value) =>
        (text.Length - text.Replace(value, string.Empty, StringComparison.Ordinal).Length) / value.Length;
}
