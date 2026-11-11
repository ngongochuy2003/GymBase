using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymBase.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "system_log",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_dung = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phuong_thuc = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    duong_dan = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ma_trang_thai = table.Column<int>(type: "int", nullable: false),
                    thoi_gian_ms = table.Column<long>(type: "bigint", nullable: false),
                    trace_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ip = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    thoi_diem = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_log", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tai_khoan",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_dang_nhap = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ho_ten = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    so_dien_thoai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    vai_tro = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dang_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    mat_khau_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ten_dang_nhap_hieu_luc = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, computedColumnSql: "IF(`is_deleted` = 0, `ten_dang_nhap`, NULL)", stored: false)
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
                    table.PrimaryKey("PK_tai_khoan", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "dang_nhap_ngoai",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tai_khoan_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nha_cung_cap = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ma_ngoai = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dang_nhap_ngoai", x => x.Id);
                    table.CheckConstraint("CK_dang_nhap_ngoai_nha_cung_cap", "`nha_cung_cap` IN ('google')");
                    table.ForeignKey(
                        name: "FK_dang_nhap_ngoai_tai_khoan_tai_khoan_id",
                        column: x => x.tai_khoan_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "phien_dang_nhap",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tai_khoan_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    token_hash = table.Column<string>(type: "char(64)", fixedLength: true, maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    token_hash_truoc = table.Column<string>(type: "char(64)", fixedLength: true, maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_xoay_vong = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ngay_het_han = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ngay_het_han_toi_da = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ngay_thu_hoi = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ten_thiet_bi = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_phien_dang_nhap", x => x.Id);
                    table.ForeignKey(
                        name: "FK_phien_dang_nhap_tai_khoan_tai_khoan_id",
                        column: x => x.tai_khoan_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_dang_nhap_ngoai_nha_cung_cap_ma_ngoai",
                table: "dang_nhap_ngoai",
                columns: new[] { "nha_cung_cap", "ma_ngoai" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dang_nhap_ngoai_tai_khoan_id_nha_cung_cap",
                table: "dang_nhap_ngoai",
                columns: new[] { "tai_khoan_id", "nha_cung_cap" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_phien_dang_nhap_tai_khoan_id",
                table: "phien_dang_nhap",
                column: "tai_khoan_id");

            migrationBuilder.CreateIndex(
                name: "IX_phien_dang_nhap_token_hash",
                table: "phien_dang_nhap",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_phien_dang_nhap_token_hash_truoc",
                table: "phien_dang_nhap",
                column: "token_hash_truoc");

            migrationBuilder.CreateIndex(
                name: "IX_system_log_nguoi_dung_thoi_diem",
                table: "system_log",
                columns: new[] { "nguoi_dung", "thoi_diem" });

            migrationBuilder.CreateIndex(
                name: "IX_system_log_thoi_diem",
                table: "system_log",
                column: "thoi_diem");

            migrationBuilder.CreateIndex(
                name: "IX_tai_khoan_ten_dang_nhap_hieu_luc",
                table: "tai_khoan",
                column: "ten_dang_nhap_hieu_luc",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dang_nhap_ngoai");

            migrationBuilder.DropTable(
                name: "phien_dang_nhap");

            migrationBuilder.DropTable(
                name: "system_log");

            migrationBuilder.DropTable(
                name: "tai_khoan");
        }
    }
}
