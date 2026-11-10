using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymBase.Persistence.Migrations
{
    /// <summary>
    /// Đổi mọi cột Guid từ binary(16) sang varchar(36) và chuyển dữ liệu đang có. Viết tay vì EF chỉ sinh lệnh đổi
    /// kiểu cột, không đổi 16 byte thành chuỗi UUID. Điều kiện LENGTH giúp chạy lại được nếu bị ngắt giữa chừng,
    /// vì MySQL không rollback được lệnh ALTER.
    /// </summary>
    public partial class ChangeGuidToVarchar36 : Migration
    {
        private const string ForeignKey = "FK_phien_dang_nhap_nguoi_dung_nguoi_dung_id";

        private static readonly (string Table, string Column)[] GuidColumns =
        [
            ("nguoi_dung", "Id"),
            ("phien_dang_nhap", "Id"),
            ("phien_dang_nhap", "nguoi_dung_id"),
            ("system_log", "Id"),
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            DropForeignKey(migrationBuilder);

            foreach (var (table, column) in GuidColumns)
            {
                migrationBuilder.Sql($"ALTER TABLE `{table}` MODIFY `{column}` varbinary(36) NOT NULL;");
                migrationBuilder.Sql($"UPDATE `{table}` SET `{column}` = BIN_TO_UUID(`{column}`) WHERE LENGTH(`{column}`) = 16;");
                migrationBuilder.Sql($"ALTER TABLE `{table}` MODIFY `{column}` varchar(36) CHARACTER SET utf8mb4 NOT NULL;");
            }

            AddForeignKey(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            DropForeignKey(migrationBuilder);

            foreach (var (table, column) in GuidColumns)
            {
                migrationBuilder.Sql($"ALTER TABLE `{table}` MODIFY `{column}` varbinary(36) NOT NULL;");
                migrationBuilder.Sql($"UPDATE `{table}` SET `{column}` = UUID_TO_BIN(`{column}`) WHERE LENGTH(`{column}`) = 36;");
                migrationBuilder.Sql($"ALTER TABLE `{table}` MODIFY `{column}` binary(16) NOT NULL;");
            }

            AddForeignKey(migrationBuilder);
        }

        private static void DropForeignKey(MigrationBuilder migrationBuilder) =>
            migrationBuilder.DropForeignKey(name: ForeignKey, table: "phien_dang_nhap");

        private static void AddForeignKey(MigrationBuilder migrationBuilder) =>
            migrationBuilder.AddForeignKey(
                name: ForeignKey,
                table: "phien_dang_nhap",
                column: "nguoi_dung_id",
                principalTable: "nguoi_dung",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
    }
}
