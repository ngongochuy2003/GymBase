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

            // Dữ liệu tham chiếu: API hồ sơ kiểm tra mã có tồn tại, nên phải có ở mọi môi trường, không chỉ DB dev.
            // Đợt này danh mục phẳng: cap = 1, không có mục cha, muc_luc = ma, muc_luc_ma = '<loại>/<mã>'.
            // Sửa mã phải thêm migration mới; sửa tên/trạng thái thì sửa trực tiếp trong DB.
            migrationBuilder.Sql("""
                INSERT INTO `danh_muc` (`Id`, `ma_dinh_danh`, `ma`, `ten`, `cap`, `muc_luc`, `muc_luc_ma`, `tinh_trang`, `ngay_tao`) VALUES
                ('0193f0d1-0000-7000-8000-000000000001', 'muc_tieu_tap_luyen', 'tang_co', 'Tăng cơ', 1, 'tang_co', 'muc_tieu_tap_luyen/tang_co', 1, UTC_TIMESTAMP(6)),
                ('0193f0d1-0000-7000-8000-000000000002', 'muc_tieu_tap_luyen', 'giam_mo', 'Giảm mỡ', 1, 'giam_mo', 'muc_tieu_tap_luyen/giam_mo', 1, UTC_TIMESTAMP(6)),
                ('0193f0d1-0000-7000-8000-000000000003', 'muc_tieu_tap_luyen', 'duy_tri', 'Duy trì vóc dáng', 1, 'duy_tri', 'muc_tieu_tap_luyen/duy_tri', 1, UTC_TIMESTAMP(6)),
                ('0193f0d1-0000-7000-8000-000000000004', 'muc_tieu_tap_luyen', 'tang_suc_ben', 'Tăng sức bền', 1, 'tang_suc_ben', 'muc_tieu_tap_luyen/tang_suc_ben', 1, UTC_TIMESTAMP(6)),
                ('0193f0d1-0000-7000-8000-000000000005', 'muc_tieu_tap_luyen', 'linh_hoat', 'Linh hoạt, giảm đau mỏi', 1, 'linh_hoat', 'muc_tieu_tap_luyen/linh_hoat', 1, UTC_TIMESTAMP(6)),
                ('0193f0d2-0000-7000-8000-000000000001', 'khu_vuc', 'ha_noi', 'Hà Nội', 1, 'ha_noi', 'khu_vuc/ha_noi', 1, UTC_TIMESTAMP(6)),
                ('0193f0d2-0000-7000-8000-000000000002', 'khu_vuc', 'tp_hcm', 'TP. Hồ Chí Minh', 1, 'tp_hcm', 'khu_vuc/tp_hcm', 1, UTC_TIMESTAMP(6)),
                ('0193f0d2-0000-7000-8000-000000000003', 'khu_vuc', 'da_nang', 'Đà Nẵng', 1, 'da_nang', 'khu_vuc/da_nang', 1, UTC_TIMESTAMP(6)),
                ('0193f0d2-0000-7000-8000-000000000004', 'khu_vuc', 'hai_phong', 'Hải Phòng', 1, 'hai_phong', 'khu_vuc/hai_phong', 1, UTC_TIMESTAMP(6)),
                ('0193f0d2-0000-7000-8000-000000000005', 'khu_vuc', 'can_tho', 'Cần Thơ', 1, 'can_tho', 'khu_vuc/can_tho', 1, UTC_TIMESTAMP(6)),
                ('0193f0d3-0000-7000-8000-000000000001', 'chuyen_mon_pt', 'tang_co', 'Tăng cơ', 1, 'tang_co', 'chuyen_mon_pt/tang_co', 1, UTC_TIMESTAMP(6)),
                ('0193f0d3-0000-7000-8000-000000000002', 'chuyen_mon_pt', 'giam_mo', 'Giảm mỡ', 1, 'giam_mo', 'chuyen_mon_pt/giam_mo', 1, UTC_TIMESTAMP(6)),
                ('0193f0d3-0000-7000-8000-000000000003', 'chuyen_mon_pt', 'cai_thien_voc_dang', 'Cải thiện vóc dáng', 1, 'cai_thien_voc_dang', 'chuyen_mon_pt/cai_thien_voc_dang', 1, UTC_TIMESTAMP(6)),
                ('0193f0d3-0000-7000-8000-000000000004', 'chuyen_mon_pt', 'phuc_hoi_chuc_nang', 'Phục hồi chức năng', 1, 'phuc_hoi_chuc_nang', 'chuyen_mon_pt/phuc_hoi_chuc_nang', 1, UTC_TIMESTAMP(6)),
                ('0193f0d3-0000-7000-8000-000000000005', 'chuyen_mon_pt', 'dinh_duong', 'Dinh dưỡng', 1, 'dinh_duong', 'chuyen_mon_pt/dinh_duong', 1, UTC_TIMESTAMP(6));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "danh_muc");
        }
    }
}
