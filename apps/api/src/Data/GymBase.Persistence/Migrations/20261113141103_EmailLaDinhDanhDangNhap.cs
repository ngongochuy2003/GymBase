using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymBase.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EmailLaDinhDanhDangNhap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Email trở thành định danh đăng nhập (ADR-0001): chuẩn hóa chữ thường trước khi đặt unique, và cấp email
            // cho tài khoản chưa có (Admin, dữ liệu mẫu) từ tên đăng nhập để vẫn đăng nhập được.
            // Tài khoản có tên đăng nhập chứa ký tự lạ thì giữ email NULL (không đăng nhập được cho tới khi Admin đặt email).
            // Hai email khác nhau chỉ ở hoa thường làm migration lỗi ở bước tạo index: sửa tay rồi chạy lại.
            migrationBuilder.Sql("UPDATE `tai_khoan` SET `email` = LOWER(TRIM(`email`)) WHERE `email` IS NOT NULL;");
            migrationBuilder.Sql(
                "UPDATE `tai_khoan` SET `email` = LOWER(CONCAT(TRIM(`ten_dang_nhap`), '@gymbase.local')) " +
                "WHERE `email` IS NULL AND `ten_dang_nhap` REGEXP '^[A-Za-z0-9._-]+$';");

            migrationBuilder.CreateIndex(
                name: "IX_tai_khoan_email",
                table: "tai_khoan",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tai_khoan_email",
                table: "tai_khoan");
        }
    }
}
