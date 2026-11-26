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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
