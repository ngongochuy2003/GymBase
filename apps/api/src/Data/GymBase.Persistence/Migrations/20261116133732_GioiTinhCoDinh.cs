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
            // Giới tính là danh sách cố định trong code (enum GioiTinh), không còn đọc từ danh_muc: bỏ các dòng đã seed ở
            // ThemThongTinCaNhanHoSo, và đưa giá trị lạ (nếu có) về NULL để thêm được ràng buộc CHECK.
            migrationBuilder.Sql("DELETE FROM `danh_muc` WHERE `ma_dinh_danh` = 'gioi_tinh';");
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

            migrationBuilder.Sql("""
                INSERT INTO `danh_muc` (`Id`, `ma_dinh_danh`, `ma`, `ten`, `cap`, `muc_luc`, `muc_luc_ma`, `tinh_trang`, `ngay_tao`) VALUES
                ('0193f0d4-0000-7000-8000-000000000001', 'gioi_tinh', 'nam', 'Nam', 1, 'nam', 'gioi_tinh/nam', 1, UTC_TIMESTAMP(6)),
                ('0193f0d4-0000-7000-8000-000000000002', 'gioi_tinh', 'nu', 'Nữ', 1, 'nu', 'gioi_tinh/nu', 1, UTC_TIMESTAMP(6)),
                ('0193f0d4-0000-7000-8000-000000000003', 'gioi_tinh', 'khac', 'Khác', 1, 'khac', 'gioi_tinh/khac', 1, UTC_TIMESTAMP(6)),
                ('0193f0d4-0000-7000-8000-000000000004', 'gioi_tinh', 'khong_muon_noi', 'Không muốn nói', 1, 'khong_muon_noi', 'gioi_tinh/khong_muon_noi', 1, UTC_TIMESTAMP(6));
                """);
        }
    }
}
