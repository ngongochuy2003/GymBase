using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymBase.Persistence.Migrations
{
    /// <summary>
    /// Dữ liệu tham chiếu của <c>danh_muc</c>. Nằm trong migration vì API hồ sơ kiểm tra mã, nên môi trường nào cũng phải có.
    /// </summary>
    public partial class SeedDanhMuc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(DocSql("SeedDanhMuc.sql"));
            // Sinh từ Excel bằng database/scripts/tao-seed-khu-vuc.py.
            migrationBuilder.Sql(DocSql("SeedKhuVuc.sql"));
        }

        /// <summary>Xóa toàn bộ danh mục, mục con trước vì khóa ngoại tới mục cha là RESTRICT.</summary>
        protected override void Down(MigrationBuilder migrationBuilder) =>
            migrationBuilder.Sql("DELETE FROM `danh_muc` ORDER BY `cap` DESC;");

        private static string DocSql(string tenFile)
        {
            using var stream = typeof(SeedDanhMuc).Assembly
                .GetManifestResourceStream("GymBase.Persistence.Migrations.Sql." + tenFile)!;
            using var reader = new System.IO.StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
