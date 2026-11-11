using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymBase.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBizTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "thiet_bi_id",
                table: "phien_dang_nhap",
                type: "varchar(36)",
                maxLength: 36,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bai_viet",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tac_gia_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    noi_dung = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_bai_viet = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    che_do_hien_thi = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_dang = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    nguoi_tao = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_chinh_sua = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    nguoi_chinh_sua = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bai_viet", x => x.Id);
                    table.CheckConstraint("CK_bai_viet_che_do_hien_thi", "`che_do_hien_thi` IN ('cong_khai', 'nguoi_theo_doi', 'rieng_tu')");
                    table.CheckConstraint("CK_bai_viet_loai_bai_viet", "`loai_bai_viet` IN ('van_ban', 'hinh_anh', 'video', 'hon_hop')");
                    table.CheckConstraint("CK_bai_viet_trang_thai", "`trang_thai` IN ('nhap', 'da_dang', 'an', 'go_bo')");
                    table.ForeignKey(
                        name: "FK_bai_viet_tai_khoan_tac_gia_id",
                        column: x => x.tac_gia_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bao_cao_vi_pham",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_bao_cao_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_doi_tuong = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    doi_tuong_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ly_do = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mo_ta = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_xu_ly_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ngay_xu_ly = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bao_cao_vi_pham", x => x.Id);
                    table.CheckConstraint("CK_bao_cao_vi_pham_loai_doi_tuong", "`loai_doi_tuong` IN ('bai_viet', 'tuong_tac_bai_viet', 'tai_khoan', 'tin_nhan')");
                    table.CheckConstraint("CK_bao_cao_vi_pham_ly_do", "`ly_do` IN ('spam', 'gia_mao', 'quay_roi', 'noi_dung_xau', 'khac')");
                    table.CheckConstraint("CK_bao_cao_vi_pham_trang_thai", "`trang_thai` IN ('moi', 'dang_xu_ly', 'da_giai_quyet', 'bo_qua')");
                    table.ForeignKey(
                        name: "FK_bao_cao_vi_pham_tai_khoan_nguoi_bao_cao_id",
                        column: x => x.nguoi_bao_cao_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_bao_cao_vi_pham_tai_khoan_nguoi_xu_ly_id",
                        column: x => x.nguoi_xu_ly_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "chan_tai_khoan",
                columns: table => new
                {
                    nguoi_chan_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_bi_chan_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_chan = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chan_tai_khoan", x => new { x.nguoi_chan_id, x.nguoi_bi_chan_id });
                    table.CheckConstraint("CK_chan_tai_khoan_khong_tu_chan", "`nguoi_chan_id` <> `nguoi_bi_chan_id`");
                    table.ForeignKey(
                        name: "FK_chan_tai_khoan_tai_khoan_nguoi_bi_chan_id",
                        column: x => x.nguoi_bi_chan_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_chan_tai_khoan_tai_khoan_nguoi_chan_id",
                        column: x => x.nguoi_chan_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cuoc_tro_chuyen",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_cuoc_tro_chuyen = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ma_hoi_thoai_doi = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_nhom = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ngay_cap_nhat = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuoc_tro_chuyen", x => x.Id);
                    table.CheckConstraint("CK_cuoc_tro_chuyen_loai_cuoc_tro_chuyen", "`loai_cuoc_tro_chuyen` IN ('truc_tiep', 'nhom')");
                    table.CheckConstraint("CK_cuoc_tro_chuyen_ma_hoi_thoai_doi", "`loai_cuoc_tro_chuyen` <> 'truc_tiep' OR `ma_hoi_thoai_doi` IS NOT NULL");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ket_noi_pt",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pt_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hoc_vien_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_khoi_tao_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pham_vi_phu_trach = table.Column<string>(type: "json", nullable: false),
                    loi_nhan = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_de_nghi = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ngay_chap_nhan = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ngay_ket_thuc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ly_do_ket_thuc = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cap_dang_mo = table.Column<string>(type: "varchar(73)", maxLength: 73, nullable: true, computedColumnSql: "IF(`trang_thai` IN ('cho_phan_hoi', 'dang_hoat_dong'), CONCAT(`pt_id`, '|', `hoc_vien_id`), NULL)", stored: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ket_noi_pt", x => x.Id);
                    table.CheckConstraint("CK_ket_noi_pt_khac_tai_khoan", "`pt_id` <> `hoc_vien_id`");
                    table.CheckConstraint("CK_ket_noi_pt_pham_vi", "JSON_TYPE(`pham_vi_phu_trach`) = 'ARRAY'");
                    table.CheckConstraint("CK_ket_noi_pt_trang_thai", "`trang_thai` IN ('cho_phan_hoi', 'dang_hoat_dong', 'tu_choi', 'da_huy', 'da_ket_thuc')");
                    table.ForeignKey(
                        name: "FK_ket_noi_pt_tai_khoan_hoc_vien_id",
                        column: x => x.hoc_vien_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ket_noi_pt_tai_khoan_nguoi_khoi_tao_id",
                        column: x => x.nguoi_khoi_tao_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ket_noi_pt_tai_khoan_pt_id",
                        column: x => x.pt_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "lich_su_chinh_sua",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_doi_tuong = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    doi_tuong_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hanh_dong = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_thuc_hien_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    du_lieu_truoc = table.Column<string>(type: "json", nullable: true),
                    du_lieu_sau = table.Column<string>(type: "json", nullable: true),
                    ngay_thuc_hien = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lich_su_chinh_sua", x => x.Id);
                    table.CheckConstraint("CK_lich_su_chinh_sua_hanh_dong", "`hanh_dong` IN ('tao', 'cap_nhat', 'danh_gia', 'xac_nhan', 'xu_ly_khieu_nai', 'xu_ly_bao_cao', 'huy', 'ket_thuc')");
                    table.CheckConstraint("CK_lich_su_chinh_sua_loai_doi_tuong", "`loai_doi_tuong` IN ('nhat_ky_hoat_dong', 'ke_hoach', 'muc_ke_hoach', 'buoi_tap', 'goi_pt_hoc_vien', 'bao_cao_vi_pham')");
                    table.ForeignKey(
                        name: "FK_lich_su_chinh_sua_tai_khoan_nguoi_thuc_hien_id",
                        column: x => x.nguoi_thuc_hien_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tep_tin",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_tai_len_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_doi_tuong = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    doi_tuong_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    muc_dich = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    s3_bucket = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    s3_key = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false, collation: "utf8mb4_bin")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_tep_goc = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_mime = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    kich_thuoc_byte = table.Column<long>(type: "bigint", nullable: false),
                    che_do_truy_cap = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    thu_tu_hien_thi = table.Column<int>(type: "int", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ngay_xac_nhan = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tep_tin", x => x.Id);
                    table.CheckConstraint("CK_tep_tin_che_do_truy_cap", "`che_do_truy_cap` IN ('cong_khai', 'rieng_tu')");
                    table.CheckConstraint("CK_tep_tin_kich_thuoc", "`kich_thuoc_byte` >= 0");
                    table.CheckConstraint("CK_tep_tin_loai_doi_tuong", "`loai_doi_tuong` IN ('bai_viet', 'tin_nhan', 'nhat_ky_hoat_dong', 'ho_so_nguoi_dung')");
                    table.CheckConstraint("CK_tep_tin_trang_thai", "`trang_thai` IN ('cho_tai_len', 'da_tai_len', 'da_lien_ket', 'loi', 'da_xoa')");
                    table.ForeignKey(
                        name: "FK_tep_tin_tai_khoan_nguoi_tai_len_id",
                        column: x => x.nguoi_tai_len_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "theo_doi",
                columns: table => new
                {
                    nguoi_theo_doi_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_duoc_theo_doi_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_theo_doi = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_theo_doi", x => new { x.nguoi_theo_doi_id, x.nguoi_duoc_theo_doi_id });
                    table.CheckConstraint("CK_theo_doi_khong_tu_theo_doi", "`nguoi_theo_doi_id` <> `nguoi_duoc_theo_doi_id`");
                    table.ForeignKey(
                        name: "FK_theo_doi_tai_khoan_nguoi_duoc_theo_doi_id",
                        column: x => x.nguoi_duoc_theo_doi_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_theo_doi_tai_khoan_nguoi_theo_doi_id",
                        column: x => x.nguoi_theo_doi_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "thiet_bi",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tai_khoan_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ma_thiet_bi = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nen_tang = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    push_token = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true, collation: "utf8mb4_bin")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dang_hoat_dong = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ngay_hoat_dong_cuoi = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ngay_tao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    nguoi_tao = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_chinh_sua = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    nguoi_chinh_sua = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_thiet_bi", x => x.Id);
                    table.CheckConstraint("CK_thiet_bi_nen_tang", "`nen_tang` IN ('android', 'ios')");
                    table.ForeignKey(
                        name: "FK_thiet_bi_tai_khoan_tai_khoan_id",
                        column: x => x.tai_khoan_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "thong_bao",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tai_khoan_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_thong_bao = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tieu_de = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    noi_dung = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_doi_tuong = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    doi_tuong_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    thoi_diem_du_kien = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    thoi_diem_da_gui = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    trang_thai_gui = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_da_doc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ma_chong_trung = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_thong_bao", x => x.Id);
                    table.CheckConstraint("CK_thong_bao_loai_thong_bao", "`loai_thong_bao` IN ('nhac_an', 'nhac_tap', 'tin_nhan', 'danh_gia', 'ket_noi_pt')");
                    table.CheckConstraint("CK_thong_bao_trang_thai_gui", "`trang_thai_gui` IN ('cho_gui', 'dang_gui', 'da_gui', 'loi', 'huy')");
                    table.ForeignKey(
                        name: "FK_thong_bao_tai_khoan_tai_khoan_id",
                        column: x => x.tai_khoan_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tuong_tac_bai_viet",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bai_viet_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tai_khoan_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_tuong_tac = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    noi_dung = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tuong_tac_cha_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ngay_xoa = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    thich_hieu_luc = table.Column<string>(type: "varchar(73)", maxLength: 73, nullable: true, computedColumnSql: "IF(`loai_tuong_tac` = 'thich' AND `ngay_xoa` IS NULL, CONCAT(`bai_viet_id`, '|', `tai_khoan_id`), NULL)", stored: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tuong_tac_bai_viet", x => x.Id);
                    table.CheckConstraint("CK_tuong_tac_bai_viet_loai_tuong_tac", "`loai_tuong_tac` IN ('thich', 'binh_luan')");
                    table.CheckConstraint("CK_tuong_tac_bai_viet_noi_dung", "(`loai_tuong_tac` = 'binh_luan' AND `noi_dung` IS NOT NULL) OR (`loai_tuong_tac` = 'thich' AND `noi_dung` IS NULL AND `tuong_tac_cha_id` IS NULL)");
                    table.ForeignKey(
                        name: "FK_tuong_tac_bai_viet_bai_viet_bai_viet_id",
                        column: x => x.bai_viet_id,
                        principalTable: "bai_viet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tuong_tac_bai_viet_tai_khoan_tai_khoan_id",
                        column: x => x.tai_khoan_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tuong_tac_bai_viet_tuong_tac_bai_viet_tuong_tac_cha_id",
                        column: x => x.tuong_tac_cha_id,
                        principalTable: "tuong_tac_bai_viet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "thanh_vien_cuoc_tro_chuyen",
                columns: table => new
                {
                    cuoc_tro_chuyen_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tai_khoan_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tham_gia = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ngay_doc_den = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ngay_roi = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_thanh_vien_cuoc_tro_chuyen", x => new { x.cuoc_tro_chuyen_id, x.tai_khoan_id });
                    table.ForeignKey(
                        name: "FK_thanh_vien_cuoc_tro_chuyen_cuoc_tro_chuyen_cuoc_tro_chuyen_id",
                        column: x => x.cuoc_tro_chuyen_id,
                        principalTable: "cuoc_tro_chuyen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_thanh_vien_cuoc_tro_chuyen_tai_khoan_tai_khoan_id",
                        column: x => x.tai_khoan_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tin_nhan",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cuoc_tro_chuyen_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_gui_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ma_tin_nhan_client = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_tin_nhan = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    noi_dung = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tra_loi_tin_nhan_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_gui = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ngay_xoa = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tin_nhan", x => x.Id);
                    table.CheckConstraint("CK_tin_nhan_loai_tin_nhan", "`loai_tin_nhan` IN ('van_ban', 'tep_tin', 'hon_hop')");
                    table.ForeignKey(
                        name: "FK_tin_nhan_cuoc_tro_chuyen_cuoc_tro_chuyen_id",
                        column: x => x.cuoc_tro_chuyen_id,
                        principalTable: "cuoc_tro_chuyen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tin_nhan_tai_khoan_nguoi_gui_id",
                        column: x => x.nguoi_gui_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tin_nhan_tin_nhan_tra_loi_tin_nhan_id",
                        column: x => x.tra_loi_tin_nhan_id,
                        principalTable: "tin_nhan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "chi_so_co_the",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hoc_vien_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ket_noi_pt_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_do = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    can_nang_kg = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    chieu_cao_cm = table.Column<decimal>(type: "decimal(5,1)", precision: 5, scale: 1, nullable: true),
                    ty_le_mo_pct = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    ghi_chu = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_nhap_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chi_so_co_the", x => x.Id);
                    table.CheckConstraint("CK_chi_so_co_the_can_nang", "`can_nang_kg` IS NULL OR `can_nang_kg` > 0");
                    table.CheckConstraint("CK_chi_so_co_the_chieu_cao", "`chieu_cao_cm` IS NULL OR `chieu_cao_cm` > 0");
                    table.CheckConstraint("CK_chi_so_co_the_ty_le_mo", "`ty_le_mo_pct` IS NULL OR `ty_le_mo_pct` BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_chi_so_co_the_ket_noi_pt_ket_noi_pt_id",
                        column: x => x.ket_noi_pt_id,
                        principalTable: "ket_noi_pt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_chi_so_co_the_tai_khoan_hoc_vien_id",
                        column: x => x.hoc_vien_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_chi_so_co_the_tai_khoan_nguoi_nhap_id",
                        column: x => x.nguoi_nhap_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "goi_pt_hoc_vien",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ket_noi_pt_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_goi = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    so_buoi_mua = table.Column<int>(type: "int", nullable: false),
                    gia_thoa_thuan = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ngay_bat_dau = table.Column<DateOnly>(type: "date", nullable: false),
                    ngay_het_han = table.Column<DateOnly>(type: "date", nullable: true),
                    trang_thai = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_tao_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
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
                    table.PrimaryKey("PK_goi_pt_hoc_vien", x => x.Id);
                    table.CheckConstraint("CK_goi_pt_hoc_vien_gia", "`gia_thoa_thuan` IS NULL OR `gia_thoa_thuan` >= 0");
                    table.CheckConstraint("CK_goi_pt_hoc_vien_ngay", "`ngay_het_han` IS NULL OR `ngay_het_han` >= `ngay_bat_dau`");
                    table.CheckConstraint("CK_goi_pt_hoc_vien_so_buoi_mua", "`so_buoi_mua` > 0");
                    table.CheckConstraint("CK_goi_pt_hoc_vien_trang_thai", "`trang_thai` IN ('cho_kich_hoat', 'dang_su_dung', 'tam_dung', 'hoan_tat', 'huy')");
                    table.ForeignKey(
                        name: "FK_goi_pt_hoc_vien_ket_noi_pt_ket_noi_pt_id",
                        column: x => x.ket_noi_pt_id,
                        principalTable: "ket_noi_pt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_goi_pt_hoc_vien_tai_khoan_nguoi_tao_id",
                        column: x => x.nguoi_tao_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ke_hoach",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ket_noi_pt_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_ke_hoach = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_ke_hoach = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mo_ta = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_bat_dau = table.Column<DateOnly>(type: "date", nullable: false),
                    ngay_ket_thuc = table.Column<DateOnly>(type: "date", nullable: true),
                    phien_ban = table.Column<int>(type: "int", nullable: false),
                    trang_thai = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_tao_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
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
                    table.PrimaryKey("PK_ke_hoach", x => x.Id);
                    table.CheckConstraint("CK_ke_hoach_loai_ke_hoach", "`loai_ke_hoach` IN ('dinh_duong', 'tap_luyen')");
                    table.CheckConstraint("CK_ke_hoach_ngay", "`ngay_ket_thuc` IS NULL OR `ngay_ket_thuc` >= `ngay_bat_dau`");
                    table.CheckConstraint("CK_ke_hoach_phien_ban", "`phien_ban` >= 1");
                    table.CheckConstraint("CK_ke_hoach_trang_thai", "`trang_thai` IN ('nhap', 'dang_ap_dung', 'tam_dung', 'ket_thuc')");
                    table.ForeignKey(
                        name: "FK_ke_hoach_ket_noi_pt_ket_noi_pt_id",
                        column: x => x.ket_noi_pt_id,
                        principalTable: "ket_noi_pt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ke_hoach_tai_khoan_nguoi_tao_id",
                        column: x => x.nguoi_tao_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ho_so_nguoi_dung",
                columns: table => new
                {
                    tai_khoan_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_nguoi_dung = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ten_hien_thi = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gioi_thieu = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    anh_dai_dien_tep_tin_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    muc_tieu_tap_luyen = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    chuyen_mon = table.Column<string>(type: "json", nullable: true),
                    so_nam_kinh_nghiem = table.Column<short>(type: "smallint", nullable: true),
                    khu_vuc_hoat_dong = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    vi_do = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    kinh_do = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: true),
                    ban_kinh_phuc_vu_km = table.Column<short>(type: "smallint", nullable: true),
                    trang_thai_xac_minh = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_xac_minh = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    nguoi_xac_minh_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ly_do_tu_choi = table.Column<string>(type: "text", nullable: true)
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
                    table.PrimaryKey("PK_ho_so_nguoi_dung", x => x.tai_khoan_id);
                    table.CheckConstraint("CK_ho_so_nguoi_dung_ban_kinh", "`ban_kinh_phuc_vu_km` IS NULL OR `ban_kinh_phuc_vu_km` > 0");
                    table.CheckConstraint("CK_ho_so_nguoi_dung_kinh_do", "`kinh_do` IS NULL OR `kinh_do` BETWEEN -180 AND 180");
                    table.CheckConstraint("CK_ho_so_nguoi_dung_kinh_nghiem", "`so_nam_kinh_nghiem` IS NULL OR `so_nam_kinh_nghiem` >= 0");
                    table.CheckConstraint("CK_ho_so_nguoi_dung_loai_nguoi_dung", "`loai_nguoi_dung` IN ('hoc_vien', 'pt', 'ca_hai')");
                    table.CheckConstraint("CK_ho_so_nguoi_dung_trang_thai_xac_minh", "`trang_thai_xac_minh` IN ('chua_xac_minh', 'dang_xem_xet', 'da_xac_minh', 'tu_choi')");
                    table.CheckConstraint("CK_ho_so_nguoi_dung_vi_do", "`vi_do` IS NULL OR `vi_do` BETWEEN -90 AND 90");
                    table.ForeignKey(
                        name: "FK_ho_so_nguoi_dung_tai_khoan_nguoi_xac_minh_id",
                        column: x => x.nguoi_xac_minh_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ho_so_nguoi_dung_tai_khoan_tai_khoan_id",
                        column: x => x.tai_khoan_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ho_so_nguoi_dung_tep_tin_anh_dai_dien_tep_tin_id",
                        column: x => x.anh_dai_dien_tep_tin_id,
                        principalTable: "tep_tin",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "muc_ke_hoach",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ke_hoach_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_muc = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tieu_de = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gio_du_kien = table.Column<TimeOnly>(type: "time(6)", nullable: false),
                    cac_thu_trong_tuan = table.Column<string>(type: "json", nullable: false),
                    mui_gio = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    chi_tiet = table.Column<string>(type: "json", nullable: false),
                    co_nhac = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    nhac_truoc_phut = table.Column<int>(type: "int", nullable: false),
                    trang_thai = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
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
                    table.PrimaryKey("PK_muc_ke_hoach", x => x.Id);
                    table.CheckConstraint("CK_muc_ke_hoach_cac_thu", "JSON_TYPE(`cac_thu_trong_tuan`) = 'ARRAY'");
                    table.CheckConstraint("CK_muc_ke_hoach_loai_muc", "`loai_muc` IN ('bua_an', 'buoi_tap')");
                    table.CheckConstraint("CK_muc_ke_hoach_nhac_truoc", "`nhac_truoc_phut` >= 0");
                    table.CheckConstraint("CK_muc_ke_hoach_trang_thai", "`trang_thai` IN ('dang_ap_dung', 'tam_dung', 'ngung')");
                    table.ForeignKey(
                        name: "FK_muc_ke_hoach_ke_hoach_ke_hoach_id",
                        column: x => x.ke_hoach_id,
                        principalTable: "ke_hoach",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "buoi_tap",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ket_noi_pt_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    goi_pt_hoc_vien_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    muc_ke_hoach_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hoc_vien_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pt_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_buoi = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bat_dau_du_kien = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    bat_dau_thuc_te = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ket_thuc_thuc_te = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    trang_thai = table.Column<string>(type: "varchar(24)", maxLength: 24, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_xac_nhan_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_xac_nhan = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    noi_dung_khieu_nai = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trang_thai_khieu_nai = table.Column<string>(type: "varchar(24)", maxLength: 24, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_khieu_nai = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    nguoi_xu_ly_khieu_nai_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_xu_ly_khieu_nai = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    nguoi_tao = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_chinh_sua = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    nguoi_chinh_sua = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_buoi_tap", x => x.Id);
                    table.CheckConstraint("CK_buoi_tap_loai_buoi", "`loai_buoi` IN ('pt_truc_tiep', 'tu_tap')");
                    table.CheckConstraint("CK_buoi_tap_trang_thai", "`trang_thai` IN ('da_len_lich', 'da_hoan_thanh', 'da_xac_nhan', 'dang_khieu_nai', 'da_huy')");
                    table.CheckConstraint("CK_buoi_tap_trang_thai_khieu_nai", "`trang_thai_khieu_nai` IN ('cho_xu_ly', 'chap_nhan', 'tu_choi')");
                    table.CheckConstraint("CK_buoi_tap_tu_tap_khong_goi", "`loai_buoi` <> 'tu_tap' OR `goi_pt_hoc_vien_id` IS NULL");
                    table.ForeignKey(
                        name: "FK_buoi_tap_goi_pt_hoc_vien_goi_pt_hoc_vien_id",
                        column: x => x.goi_pt_hoc_vien_id,
                        principalTable: "goi_pt_hoc_vien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_buoi_tap_ket_noi_pt_ket_noi_pt_id",
                        column: x => x.ket_noi_pt_id,
                        principalTable: "ket_noi_pt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_buoi_tap_muc_ke_hoach_muc_ke_hoach_id",
                        column: x => x.muc_ke_hoach_id,
                        principalTable: "muc_ke_hoach",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_buoi_tap_tai_khoan_hoc_vien_id",
                        column: x => x.hoc_vien_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_buoi_tap_tai_khoan_nguoi_xac_nhan_id",
                        column: x => x.nguoi_xac_nhan_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_buoi_tap_tai_khoan_nguoi_xu_ly_khieu_nai_id",
                        column: x => x.nguoi_xu_ly_khieu_nai_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_buoi_tap_tai_khoan_pt_id",
                        column: x => x.pt_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bien_dong_so_buoi",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    goi_pt_hoc_vien_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    buoi_tap_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loai_bien_dong = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    so_buoi_thay_doi = table.Column<int>(type: "int", nullable: false),
                    ma_chong_trung = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_tao_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ly_do = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_tao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    hoan_buoi_hieu_luc = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true, computedColumnSql: "IF(`loai_bien_dong` = 'hoan_buoi', `buoi_tap_id`, NULL)", stored: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tru_buoi_hieu_luc = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true, computedColumnSql: "IF(`loai_bien_dong` = 'tru_buoi', `buoi_tap_id`, NULL)", stored: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bien_dong_so_buoi", x => x.Id);
                    table.CheckConstraint("CK_bien_dong_so_buoi_dau", "(`loai_bien_dong` = 'cap_buoi' AND `so_buoi_thay_doi` > 0) OR (`loai_bien_dong` = 'tru_buoi' AND `so_buoi_thay_doi` < 0) OR (`loai_bien_dong` = 'hoan_buoi' AND `so_buoi_thay_doi` > 0) OR (`loai_bien_dong` = 'dieu_chinh' AND `so_buoi_thay_doi` <> 0)");
                    table.CheckConstraint("CK_bien_dong_so_buoi_loai_bien_dong", "`loai_bien_dong` IN ('cap_buoi', 'tru_buoi', 'hoan_buoi', 'dieu_chinh')");
                    table.ForeignKey(
                        name: "FK_bien_dong_so_buoi_buoi_tap_buoi_tap_id",
                        column: x => x.buoi_tap_id,
                        principalTable: "buoi_tap",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_bien_dong_so_buoi_goi_pt_hoc_vien_goi_pt_hoc_vien_id",
                        column: x => x.goi_pt_hoc_vien_id,
                        principalTable: "goi_pt_hoc_vien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_bien_dong_so_buoi_tai_khoan_nguoi_tao_id",
                        column: x => x.nguoi_tao_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "nhat_ky_hoat_dong",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ket_noi_pt_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    muc_ke_hoach_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    buoi_tap_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hoc_vien_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phien_ban_ke_hoach = table.Column<int>(type: "int", nullable: true),
                    loai_hoat_dong = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_hoat_dong = table.Column<DateOnly>(type: "date", nullable: false),
                    thoi_gian_thuc_hien = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    noi_dung = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    chi_tiet = table.Column<string>(type: "json", nullable: true),
                    trang_thai = table.Column<string>(type: "varchar(24)", maxLength: 24, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nguoi_danh_gia_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nhan_xet = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_danh_gia = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ngay_tao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    nguoi_tao = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ngay_chinh_sua = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    nguoi_chinh_sua = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nhat_ky_hoat_dong", x => x.Id);
                    table.CheckConstraint("CK_nhat_ky_hoat_dong_loai_hoat_dong", "`loai_hoat_dong` IN ('an_uong', 'tap_luyen')");
                    table.CheckConstraint("CK_nhat_ky_hoat_dong_phien_ban", "`phien_ban_ke_hoach` IS NULL OR `phien_ban_ke_hoach` >= 1");
                    table.CheckConstraint("CK_nhat_ky_hoat_dong_trang_thai", "`trang_thai` IN ('nhap', 'cho_kiem_tra', 'dat', 'can_dieu_chinh', 'hoan_thanh')");
                    table.ForeignKey(
                        name: "FK_nhat_ky_hoat_dong_buoi_tap_buoi_tap_id",
                        column: x => x.buoi_tap_id,
                        principalTable: "buoi_tap",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_nhat_ky_hoat_dong_ket_noi_pt_ket_noi_pt_id",
                        column: x => x.ket_noi_pt_id,
                        principalTable: "ket_noi_pt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_nhat_ky_hoat_dong_muc_ke_hoach_muc_ke_hoach_id",
                        column: x => x.muc_ke_hoach_id,
                        principalTable: "muc_ke_hoach",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_nhat_ky_hoat_dong_tai_khoan_hoc_vien_id",
                        column: x => x.hoc_vien_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_nhat_ky_hoat_dong_tai_khoan_nguoi_danh_gia_id",
                        column: x => x.nguoi_danh_gia_id,
                        principalTable: "tai_khoan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_phien_dang_nhap_thiet_bi_id",
                table: "phien_dang_nhap",
                column: "thiet_bi_id");

            migrationBuilder.CreateIndex(
                name: "IX_bai_viet_tac_gia_id_ngay_dang",
                table: "bai_viet",
                columns: new[] { "tac_gia_id", "ngay_dang" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_bai_viet_trang_thai_ngay_dang",
                table: "bai_viet",
                columns: new[] { "trang_thai", "ngay_dang" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_bao_cao_vi_pham_loai_doi_tuong_doi_tuong_id",
                table: "bao_cao_vi_pham",
                columns: new[] { "loai_doi_tuong", "doi_tuong_id" });

            migrationBuilder.CreateIndex(
                name: "IX_bao_cao_vi_pham_nguoi_bao_cao_id",
                table: "bao_cao_vi_pham",
                column: "nguoi_bao_cao_id");

            migrationBuilder.CreateIndex(
                name: "IX_bao_cao_vi_pham_nguoi_xu_ly_id",
                table: "bao_cao_vi_pham",
                column: "nguoi_xu_ly_id");

            migrationBuilder.CreateIndex(
                name: "IX_bao_cao_vi_pham_trang_thai_ngay_tao",
                table: "bao_cao_vi_pham",
                columns: new[] { "trang_thai", "ngay_tao" });

            migrationBuilder.CreateIndex(
                name: "IX_bien_dong_so_buoi_buoi_tap_id",
                table: "bien_dong_so_buoi",
                column: "buoi_tap_id");

            migrationBuilder.CreateIndex(
                name: "IX_bien_dong_so_buoi_goi_pt_hoc_vien_id",
                table: "bien_dong_so_buoi",
                column: "goi_pt_hoc_vien_id");

            migrationBuilder.CreateIndex(
                name: "IX_bien_dong_so_buoi_hoan_buoi_hieu_luc",
                table: "bien_dong_so_buoi",
                column: "hoan_buoi_hieu_luc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bien_dong_so_buoi_ma_chong_trung",
                table: "bien_dong_so_buoi",
                column: "ma_chong_trung",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bien_dong_so_buoi_nguoi_tao_id",
                table: "bien_dong_so_buoi",
                column: "nguoi_tao_id");

            migrationBuilder.CreateIndex(
                name: "IX_bien_dong_so_buoi_tru_buoi_hieu_luc",
                table: "bien_dong_so_buoi",
                column: "tru_buoi_hieu_luc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_buoi_tap_goi_pt_hoc_vien_id_trang_thai",
                table: "buoi_tap",
                columns: new[] { "goi_pt_hoc_vien_id", "trang_thai" });

            migrationBuilder.CreateIndex(
                name: "IX_buoi_tap_hoc_vien_id_bat_dau_du_kien",
                table: "buoi_tap",
                columns: new[] { "hoc_vien_id", "bat_dau_du_kien" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_buoi_tap_ket_noi_pt_id",
                table: "buoi_tap",
                column: "ket_noi_pt_id");

            migrationBuilder.CreateIndex(
                name: "IX_buoi_tap_muc_ke_hoach_id",
                table: "buoi_tap",
                column: "muc_ke_hoach_id");

            migrationBuilder.CreateIndex(
                name: "IX_buoi_tap_nguoi_xac_nhan_id",
                table: "buoi_tap",
                column: "nguoi_xac_nhan_id");

            migrationBuilder.CreateIndex(
                name: "IX_buoi_tap_nguoi_xu_ly_khieu_nai_id",
                table: "buoi_tap",
                column: "nguoi_xu_ly_khieu_nai_id");

            migrationBuilder.CreateIndex(
                name: "IX_buoi_tap_pt_id_trang_thai",
                table: "buoi_tap",
                columns: new[] { "pt_id", "trang_thai" });

            migrationBuilder.CreateIndex(
                name: "IX_chan_tai_khoan_nguoi_bi_chan_id",
                table: "chan_tai_khoan",
                column: "nguoi_bi_chan_id");

            migrationBuilder.CreateIndex(
                name: "IX_chi_so_co_the_hoc_vien_id_ngay_do",
                table: "chi_so_co_the",
                columns: new[] { "hoc_vien_id", "ngay_do" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_chi_so_co_the_ket_noi_pt_id_ngay_do",
                table: "chi_so_co_the",
                columns: new[] { "ket_noi_pt_id", "ngay_do" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_chi_so_co_the_nguoi_nhap_id",
                table: "chi_so_co_the",
                column: "nguoi_nhap_id");

            migrationBuilder.CreateIndex(
                name: "IX_cuoc_tro_chuyen_ma_hoi_thoai_doi",
                table: "cuoc_tro_chuyen",
                column: "ma_hoi_thoai_doi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_goi_pt_hoc_vien_ket_noi_pt_id_trang_thai",
                table: "goi_pt_hoc_vien",
                columns: new[] { "ket_noi_pt_id", "trang_thai" });

            migrationBuilder.CreateIndex(
                name: "IX_goi_pt_hoc_vien_nguoi_tao_id",
                table: "goi_pt_hoc_vien",
                column: "nguoi_tao_id");

            migrationBuilder.CreateIndex(
                name: "IX_ho_so_nguoi_dung_anh_dai_dien_tep_tin_id",
                table: "ho_so_nguoi_dung",
                column: "anh_dai_dien_tep_tin_id");

            migrationBuilder.CreateIndex(
                name: "IX_ho_so_nguoi_dung_loai_nguoi_dung_trang_thai_xac_minh",
                table: "ho_so_nguoi_dung",
                columns: new[] { "loai_nguoi_dung", "trang_thai_xac_minh" });

            migrationBuilder.CreateIndex(
                name: "IX_ho_so_nguoi_dung_nguoi_xac_minh_id",
                table: "ho_so_nguoi_dung",
                column: "nguoi_xac_minh_id");

            migrationBuilder.CreateIndex(
                name: "IX_ke_hoach_ket_noi_pt_id_loai_ke_hoach_trang_thai",
                table: "ke_hoach",
                columns: new[] { "ket_noi_pt_id", "loai_ke_hoach", "trang_thai" });

            migrationBuilder.CreateIndex(
                name: "IX_ke_hoach_nguoi_tao_id",
                table: "ke_hoach",
                column: "nguoi_tao_id");

            migrationBuilder.CreateIndex(
                name: "IX_ket_noi_pt_cap_dang_mo",
                table: "ket_noi_pt",
                column: "cap_dang_mo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ket_noi_pt_hoc_vien_id_trang_thai",
                table: "ket_noi_pt",
                columns: new[] { "hoc_vien_id", "trang_thai" });

            migrationBuilder.CreateIndex(
                name: "IX_ket_noi_pt_nguoi_khoi_tao_id",
                table: "ket_noi_pt",
                column: "nguoi_khoi_tao_id");

            migrationBuilder.CreateIndex(
                name: "IX_ket_noi_pt_pt_id_trang_thai",
                table: "ket_noi_pt",
                columns: new[] { "pt_id", "trang_thai" });

            migrationBuilder.CreateIndex(
                name: "IX_lich_su_chinh_sua_loai_doi_tuong_doi_tuong_id_ngay_thuc_hien",
                table: "lich_su_chinh_sua",
                columns: new[] { "loai_doi_tuong", "doi_tuong_id", "ngay_thuc_hien" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_lich_su_chinh_sua_nguoi_thuc_hien_id",
                table: "lich_su_chinh_sua",
                column: "nguoi_thuc_hien_id");

            migrationBuilder.CreateIndex(
                name: "IX_muc_ke_hoach_ke_hoach_id_trang_thai",
                table: "muc_ke_hoach",
                columns: new[] { "ke_hoach_id", "trang_thai" });

            migrationBuilder.CreateIndex(
                name: "IX_nhat_ky_hoat_dong_buoi_tap_id",
                table: "nhat_ky_hoat_dong",
                column: "buoi_tap_id");

            migrationBuilder.CreateIndex(
                name: "IX_nhat_ky_hoat_dong_hoc_vien_id_ngay_hoat_dong",
                table: "nhat_ky_hoat_dong",
                columns: new[] { "hoc_vien_id", "ngay_hoat_dong" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_nhat_ky_hoat_dong_ket_noi_pt_id_trang_thai",
                table: "nhat_ky_hoat_dong",
                columns: new[] { "ket_noi_pt_id", "trang_thai" });

            migrationBuilder.CreateIndex(
                name: "IX_nhat_ky_hoat_dong_muc_ke_hoach_id_ngay_hoat_dong",
                table: "nhat_ky_hoat_dong",
                columns: new[] { "muc_ke_hoach_id", "ngay_hoat_dong" });

            migrationBuilder.CreateIndex(
                name: "IX_nhat_ky_hoat_dong_nguoi_danh_gia_id",
                table: "nhat_ky_hoat_dong",
                column: "nguoi_danh_gia_id");

            migrationBuilder.CreateIndex(
                name: "IX_tep_tin_loai_doi_tuong_doi_tuong_id",
                table: "tep_tin",
                columns: new[] { "loai_doi_tuong", "doi_tuong_id" });

            migrationBuilder.CreateIndex(
                name: "IX_tep_tin_nguoi_tai_len_id_ngay_tao",
                table: "tep_tin",
                columns: new[] { "nguoi_tai_len_id", "ngay_tao" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_tep_tin_s3_key",
                table: "tep_tin",
                column: "s3_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_thanh_vien_cuoc_tro_chuyen_tai_khoan_id_ngay_tham_gia",
                table: "thanh_vien_cuoc_tro_chuyen",
                columns: new[] { "tai_khoan_id", "ngay_tham_gia" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_theo_doi_nguoi_duoc_theo_doi_id_ngay_theo_doi",
                table: "theo_doi",
                columns: new[] { "nguoi_duoc_theo_doi_id", "ngay_theo_doi" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_thiet_bi_push_token",
                table: "thiet_bi",
                column: "push_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_thiet_bi_tai_khoan_id_dang_hoat_dong",
                table: "thiet_bi",
                columns: new[] { "tai_khoan_id", "dang_hoat_dong" });

            migrationBuilder.CreateIndex(
                name: "IX_thiet_bi_tai_khoan_id_ma_thiet_bi",
                table: "thiet_bi",
                columns: new[] { "tai_khoan_id", "ma_thiet_bi" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_thong_bao_tai_khoan_id_ma_chong_trung",
                table: "thong_bao",
                columns: new[] { "tai_khoan_id", "ma_chong_trung" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_thong_bao_tai_khoan_id_ngay_tao",
                table: "thong_bao",
                columns: new[] { "tai_khoan_id", "ngay_tao" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_thong_bao_trang_thai_gui_thoi_diem_du_kien",
                table: "thong_bao",
                columns: new[] { "trang_thai_gui", "thoi_diem_du_kien" });

            migrationBuilder.CreateIndex(
                name: "IX_tin_nhan_cuoc_tro_chuyen_id_ngay_gui",
                table: "tin_nhan",
                columns: new[] { "cuoc_tro_chuyen_id", "ngay_gui" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_tin_nhan_nguoi_gui_id_ma_tin_nhan_client",
                table: "tin_nhan",
                columns: new[] { "nguoi_gui_id", "ma_tin_nhan_client" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tin_nhan_tra_loi_tin_nhan_id",
                table: "tin_nhan",
                column: "tra_loi_tin_nhan_id");

            migrationBuilder.CreateIndex(
                name: "IX_tuong_tac_bai_viet_bai_viet_id_loai_tuong_tac_ngay_tao",
                table: "tuong_tac_bai_viet",
                columns: new[] { "bai_viet_id", "loai_tuong_tac", "ngay_tao" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_tuong_tac_bai_viet_tai_khoan_id",
                table: "tuong_tac_bai_viet",
                column: "tai_khoan_id");

            migrationBuilder.CreateIndex(
                name: "IX_tuong_tac_bai_viet_thich_hieu_luc",
                table: "tuong_tac_bai_viet",
                column: "thich_hieu_luc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tuong_tac_bai_viet_tuong_tac_cha_id",
                table: "tuong_tac_bai_viet",
                column: "tuong_tac_cha_id");

            migrationBuilder.AddForeignKey(
                name: "FK_phien_dang_nhap_thiet_bi_thiet_bi_id",
                table: "phien_dang_nhap",
                column: "thiet_bi_id",
                principalTable: "thiet_bi",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // Viết tay: EF không sinh trigger. bien_dong_so_buoi là sổ cái bất biến, điều chỉnh bằng giao dịch bù.
            migrationBuilder.Sql(
                "CREATE TRIGGER `trg_bien_dong_so_buoi_khong_sua` BEFORE UPDATE ON `bien_dong_so_buoi` FOR EACH ROW " +
                "SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'bien_dong_so_buoi la so cai bat bien, khong duoc sua'");
            migrationBuilder.Sql(
                "CREATE TRIGGER `trg_bien_dong_so_buoi_khong_xoa` BEFORE DELETE ON `bien_dong_so_buoi` FOR EACH ROW " +
                "SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'bien_dong_so_buoi la so cai bat bien, khong duoc xoa'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS `trg_bien_dong_so_buoi_khong_sua`");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS `trg_bien_dong_so_buoi_khong_xoa`");

            migrationBuilder.DropForeignKey(
                name: "FK_phien_dang_nhap_thiet_bi_thiet_bi_id",
                table: "phien_dang_nhap");

            migrationBuilder.DropTable(
                name: "bao_cao_vi_pham");

            migrationBuilder.DropTable(
                name: "bien_dong_so_buoi");

            migrationBuilder.DropTable(
                name: "chan_tai_khoan");

            migrationBuilder.DropTable(
                name: "chi_so_co_the");

            migrationBuilder.DropTable(
                name: "ho_so_nguoi_dung");

            migrationBuilder.DropTable(
                name: "lich_su_chinh_sua");

            migrationBuilder.DropTable(
                name: "nhat_ky_hoat_dong");

            migrationBuilder.DropTable(
                name: "thanh_vien_cuoc_tro_chuyen");

            migrationBuilder.DropTable(
                name: "theo_doi");

            migrationBuilder.DropTable(
                name: "thiet_bi");

            migrationBuilder.DropTable(
                name: "thong_bao");

            migrationBuilder.DropTable(
                name: "tin_nhan");

            migrationBuilder.DropTable(
                name: "tuong_tac_bai_viet");

            migrationBuilder.DropTable(
                name: "tep_tin");

            migrationBuilder.DropTable(
                name: "buoi_tap");

            migrationBuilder.DropTable(
                name: "cuoc_tro_chuyen");

            migrationBuilder.DropTable(
                name: "bai_viet");

            migrationBuilder.DropTable(
                name: "goi_pt_hoc_vien");

            migrationBuilder.DropTable(
                name: "muc_ke_hoach");

            migrationBuilder.DropTable(
                name: "ke_hoach");

            migrationBuilder.DropTable(
                name: "ket_noi_pt");

            migrationBuilder.DropIndex(
                name: "IX_phien_dang_nhap_thiet_bi_id",
                table: "phien_dang_nhap");

            migrationBuilder.DropColumn(
                name: "thiet_bi_id",
                table: "phien_dang_nhap");
        }
    }
}
