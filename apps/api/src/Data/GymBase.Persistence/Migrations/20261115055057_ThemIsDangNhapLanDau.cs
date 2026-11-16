using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymBase.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ThemIsDangNhapLanDau : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_dang_nhap_lan_dau",
                table: "tai_khoan",
                type: "tinyint(1)",
                nullable: false,
                // Tài khoản có trước migration đã dùng app, không hiện onboarding; tài khoản mới do entity đặt true.
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_dang_nhap_lan_dau",
                table: "tai_khoan");
        }
    }
}
