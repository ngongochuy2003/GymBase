using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymBase.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ThemXuLyAnhTepTin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "blurhash",
                table: "tep_tin",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ngay_bat_dau_xu_ly",
                table: "tep_tin",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "trang_thai_xu_ly",
                table: "tep_tin",
                type: "varchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "khong_can")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_tep_tin_trang_thai_xu_ly_ngay_tao",
                table: "tep_tin",
                columns: new[] { "trang_thai_xu_ly", "ngay_tao" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_tep_tin_trang_thai_xu_ly",
                table: "tep_tin",
                sql: "`trang_thai_xu_ly` IN ('khong_can', 'cho_xu_ly', 'dang_xu_ly', 'xong', 'loi')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tep_tin_trang_thai_xu_ly_ngay_tao",
                table: "tep_tin");

            migrationBuilder.DropCheckConstraint(
                name: "CK_tep_tin_trang_thai_xu_ly",
                table: "tep_tin");

            migrationBuilder.DropColumn(
                name: "blurhash",
                table: "tep_tin");

            migrationBuilder.DropColumn(
                name: "ngay_bat_dau_xu_ly",
                table: "tep_tin");

            migrationBuilder.DropColumn(
                name: "trang_thai_xu_ly",
                table: "tep_tin");
        }
    }
}
