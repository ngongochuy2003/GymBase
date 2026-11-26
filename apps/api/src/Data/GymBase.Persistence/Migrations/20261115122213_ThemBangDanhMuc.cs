using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymBase.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ThemBangDanhMuc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "danh_muc",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ma_dinh_danh = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ma = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mo_ta = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    muc_luc = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    muc_luc_ma = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cap = table.Column<int>(type: "int", nullable: true),
                    danh_muc_cap_tren_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tinh_trang = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ghi_chu = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    nguoi_tao = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_chinh_sua = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    nguoi_chinh_sua = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_danh_muc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_danh_muc_danh_muc_danh_muc_cap_tren_id",
                        column: x => x.danh_muc_cap_tren_id,
                        principalTable: "danh_muc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_danh_muc_danh_muc_cap_tren_id",
                table: "danh_muc",
                column: "danh_muc_cap_tren_id");

            migrationBuilder.CreateIndex(
                name: "IX_danh_muc_ma_dinh_danh_ma",
                table: "danh_muc",
                columns: new[] { "ma_dinh_danh", "ma" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_danh_muc_muc_luc_ma",
                table: "danh_muc",
                column: "muc_luc_ma");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "danh_muc");
        }
    }
}
