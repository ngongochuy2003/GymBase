using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymBase.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ThemThongTinCaNhanHoSo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "gioi_tinh",
                table: "ho_so_nguoi_dung",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateOnly>(
                name: "ngay_sinh",
                table: "ho_so_nguoi_dung",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "trinh_do_tap",
                table: "ho_so_nguoi_dung",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            // Dữ liệu tham chiếu cho dropdown giới tính và trình độ tập; API hồ sơ kiểm tra mã nên phải có ở mọi môi trường.
            // Danh mục phẳng như ThemBangDanhMuc: cap = 1, muc_luc = ma, muc_luc_ma = '<loại>/<mã>'.
            migrationBuilder.Sql("""
                INSERT INTO `danh_muc` (`Id`, `ma_dinh_danh`, `ma`, `ten`, `cap`, `muc_luc`, `muc_luc_ma`, `tinh_trang`, `ngay_tao`) VALUES
                ('0193f0d4-0000-7000-8000-000000000001', 'gioi_tinh', 'nam', 'Nam', 1, 'nam', 'gioi_tinh/nam', 1, UTC_TIMESTAMP(6)),
                ('0193f0d4-0000-7000-8000-000000000002', 'gioi_tinh', 'nu', 'Nữ', 1, 'nu', 'gioi_tinh/nu', 1, UTC_TIMESTAMP(6)),
                ('0193f0d4-0000-7000-8000-000000000003', 'gioi_tinh', 'khac', 'Khác', 1, 'khac', 'gioi_tinh/khac', 1, UTC_TIMESTAMP(6)),
                ('0193f0d4-0000-7000-8000-000000000004', 'gioi_tinh', 'khong_muon_noi', 'Không muốn nói', 1, 'khong_muon_noi', 'gioi_tinh/khong_muon_noi', 1, UTC_TIMESTAMP(6)),
                ('0193f0d5-0000-7000-8000-000000000001', 'trinh_do_tap', 'moi_bat_dau', 'Mới bắt đầu', 1, 'moi_bat_dau', 'trinh_do_tap/moi_bat_dau', 1, UTC_TIMESTAMP(6)),
                ('0193f0d5-0000-7000-8000-000000000002', 'trinh_do_tap', 'co_nen_tang', 'Có nền tảng', 1, 'co_nen_tang', 'trinh_do_tap/co_nen_tang', 1, UTC_TIMESTAMP(6)),
                ('0193f0d5-0000-7000-8000-000000000003', 'trinh_do_tap', 'lau_nam', 'Tập lâu năm', 1, 'lau_nam', 'trinh_do_tap/lau_nam', 1, UTC_TIMESTAMP(6));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DELETE FROM `danh_muc` WHERE `ma_dinh_danh` IN ('gioi_tinh', 'trinh_do_tap');");

            migrationBuilder.DropColumn(
                name: "gioi_tinh",
                table: "ho_so_nguoi_dung");

            migrationBuilder.DropColumn(
                name: "ngay_sinh",
                table: "ho_so_nguoi_dung");

            migrationBuilder.DropColumn(
                name: "trinh_do_tap",
                table: "ho_so_nguoi_dung");
        }
    }
}
