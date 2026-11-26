using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymBase.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GioiTinhCoDinh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Giới tính là danh sách cố định trong code (enum GioiTinh), không đọc từ danh_muc. Đưa giá trị lạ (nếu có) về NULL
            // để thêm được ràng buộc CHECK.
            migrationBuilder.Sql(
                "UPDATE `ho_so_nguoi_dung` SET `gioi_tinh` = NULL WHERE `gioi_tinh` NOT IN ('nam', 'nu', 'khac', 'khong_muon_noi');");

            migrationBuilder.AlterColumn<string>(
                name: "gioi_tinh",
                table: "ho_so_nguoi_dung",
                type: "varchar(14)",
                maxLength: 14,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ho_so_nguoi_dung_gioi_tinh",
                table: "ho_so_nguoi_dung",
                sql: "`gioi_tinh` IN ('nam', 'nu', 'khac', 'khong_muon_noi')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ho_so_nguoi_dung_gioi_tinh",
                table: "ho_so_nguoi_dung");

            migrationBuilder.AlterColumn<string>(
                name: "gioi_tinh",
                table: "ho_so_nguoi_dung",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(14)",
                oldMaxLength: 14,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
