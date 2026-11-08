using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymBase.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPhienDangNhap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "phien_dang_nhap",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    nguoi_dung_id = table.Column<Guid>(type: "binary(16)", nullable: false),
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
                        name: "FK_phien_dang_nhap_nguoi_dung_nguoi_dung_id",
                        column: x => x.nguoi_dung_id,
                        principalTable: "nguoi_dung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_phien_dang_nhap_nguoi_dung_id",
                table: "phien_dang_nhap",
                column: "nguoi_dung_id");

            migrationBuilder.CreateIndex(
                name: "IX_phien_dang_nhap_token_hash",
                table: "phien_dang_nhap",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_phien_dang_nhap_token_hash_truoc",
                table: "phien_dang_nhap",
                column: "token_hash_truoc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "phien_dang_nhap");
        }
    }
}
