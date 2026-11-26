using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymBase.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ThemThongBaoOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_thong_bao_trang_thai_gui_thoi_diem_du_kien",
                table: "thong_bao");

            migrationBuilder.AddColumn<string>(
                name: "kenh",
                table: "thong_bao",
                type: "varchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "push")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "so_lan_thu",
                table: "thong_bao",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "thoi_diem_nhan",
                table: "thong_bao",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "thoi_diem_thu_lai",
                table: "thong_bao",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "mui_gio",
                table: "thiet_bi",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cai_dat_thong_bao",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tai_khoan_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_thong_bao = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bat = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    nguoi_tao = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_chinh_sua = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    nguoi_chinh_sua = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cai_dat_thong_bao", x => x.Id);
                    table.CheckConstraint("CK_cai_dat_thong_bao_loai_thong_bao", "`loai_thong_bao` IN ('nhac_an', 'nhac_tap', 'tin_nhan', 'danh_gia', 'ket_noi_pt')");
                    table.ForeignKey(
                        name: "FK_cai_dat_thong_bao_tai_khoan_tai_khoan_id",
                        column: x => x.tai_khoan_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_thong_bao_kenh_trang_thai_gui_thoi_diem_du_kien",
                table: "thong_bao",
                columns: new[] { "kenh", "trang_thai_gui", "thoi_diem_du_kien" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_thong_bao_kenh",
                table: "thong_bao",
                sql: "`kenh` IN ('push', 'local', 'chi_hop_thu')");

            migrationBuilder.CreateIndex(
                name: "IX_cai_dat_thong_bao_tai_khoan_id_loai_thong_bao",
                table: "cai_dat_thong_bao",
                columns: new[] { "tai_khoan_id", "loai_thong_bao" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cai_dat_thong_bao");

            migrationBuilder.DropIndex(
                name: "IX_thong_bao_kenh_trang_thai_gui_thoi_diem_du_kien",
                table: "thong_bao");

            migrationBuilder.DropCheckConstraint(
                name: "CK_thong_bao_kenh",
                table: "thong_bao");

            migrationBuilder.DropColumn(
                name: "kenh",
                table: "thong_bao");

            migrationBuilder.DropColumn(
                name: "so_lan_thu",
                table: "thong_bao");

            migrationBuilder.DropColumn(
                name: "thoi_diem_nhan",
                table: "thong_bao");

            migrationBuilder.DropColumn(
                name: "thoi_diem_thu_lai",
                table: "thong_bao");

            migrationBuilder.DropColumn(
                name: "mui_gio",
                table: "thiet_bi");

            migrationBuilder.CreateIndex(
                name: "IX_thong_bao_trang_thai_gui_thoi_diem_du_kien",
                table: "thong_bao",
                columns: new[] { "trang_thai_gui", "thoi_diem_du_kien" });
        }
    }
}
