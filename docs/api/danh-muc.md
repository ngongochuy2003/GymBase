# Danh mục (nhóm DI)

Danh mục là dữ liệu tham chiếu cho dropdown của app: mục tiêu tập luyện, khu vực hoạt động, chuyên môn PT. Bảng
`danh_muc` do nhóm **DI (Danh mục)** sở hữu — DI ở đây nghĩa là danh mục, không phải dependency injection.

Vì mã danh mục được kiểm tra khi lưu hồ sơ, dữ liệu tham chiếu nằm trong migration (`ThemBangDanhMuc`), không chỉ trong
seed của môi trường dev.

## 1. Bảng `danh_muc`

| Cột | Kiểu | Null | Ghi chú |
| --- | --- | --- | --- |
| `Id` | varchar(36) | không | Khóa chính, UUIDv7 |
| `ma_dinh_danh` | varchar(255) | không | Loại danh mục (namespace), ví dụ `muc_tieu_tap_luyen` |
| `ma` | varchar(255) | không | Mã mục trong loại, ví dụ `tang_co` |
| `ten` | varchar(255) | không | Tên hiển thị |
| `mo_ta` | longtext | có | Mô tả thêm |
| `muc_luc` | varchar(512) | có | Đường dẫn trong cây theo tên |
| `muc_luc_ma` | varchar(512) | có | Đường dẫn theo mã, có index |
| `cap` | int | có | Độ sâu; cấp gốc là 1 |
| `danh_muc_cap_tren_id` | varchar(36) | có | Mục cha (FK tự thân, `RESTRICT`), có index |
| `tinh_trang` | tinyint(1) | không | 1 = đang dùng, 0 = ẩn |
| `ghi_chu` | longtext | có | Ghi chú nội bộ |
| `ngay_tao`, `nguoi_tao`, `ngay_chinh_sua`, `nguoi_chinh_sua` | | | Bốn cột audit của `BaseAuditableEntity`, giờ UTC |

Ràng buộc: unique `(ma_dinh_danh, ma)` — cùng một mã được dùng ở nhiều loại (`tang_co` có ở cả `muc_tieu_tap_luyen`
và `chuyen_mon_pt`).

**Quy ước đợt này là danh mục phẳng:** `cap = 1`, `danh_muc_cap_tren_id = NULL`, `muc_luc = ma`,
`muc_luc_ma = '<ma_dinh_danh>/<ma>'`. Cây nhiều cấp (khu vực → quận/huyện) để sau, xem `// TODO(DANH-MUC)` trong entity.

## 2. Dữ liệu tham chiếu

| `ma_dinh_danh` | `ma` (tên hiển thị) |
| --- | --- |
| `muc_tieu_tap_luyen` | `tang_co` (Tăng cơ), `giam_mo` (Giảm mỡ), `duy_tri` (Duy trì vóc dáng), `tang_suc_ben` (Tăng sức bền), `linh_hoat` (Linh hoạt, giảm đau mỏi) |
| `khu_vuc` | `ha_noi` (Hà Nội), `tp_hcm` (TP. Hồ Chí Minh), `da_nang` (Đà Nẵng), `hai_phong` (Hải Phòng), `can_tho` (Cần Thơ) |
| `chuyen_mon_pt` | `tang_co` (Tăng cơ), `giam_mo` (Giảm mỡ), `cai_thien_voc_dang` (Cải thiện vóc dáng), `phuc_hoi_chuc_nang` (Phục hồi chức năng), `dinh_duong` (Dinh dưỡng) |
| `trinh_do_tap` | `moi_bat_dau` (Mới bắt đầu), `co_nen_tang` (Có nền tảng), `lau_nam` (Tập lâu năm) |

Sửa `ten` hoặc `tinh_trang` trực tiếp trong DB là app đổi theo ngay, không cần build lại. Đổi **mã** thì phải thêm
migration mới (mã đã lưu trong `ho_so_nguoi_dung`).

Thêm một mục:

```sql
INSERT INTO danh_muc (Id, ma_dinh_danh, ma, ten, muc_luc, muc_luc_ma, cap, tinh_trang, ngay_tao)
VALUES (UUID(), 'khu_vuc', 'hue', 'Huế', 'hue', 'khu_vuc/hue', 1, 1, UTC_TIMESTAMP(6));
```

Ẩn một mục (giữ dữ liệu hồ sơ cũ): `UPDATE danh_muc SET tinh_trang = 0 WHERE ma_dinh_danh = 'khu_vuc' AND ma = 'hue';`

## 3. API

`GET /api/v1/mob/danh-muc` — cần đăng nhập (mọi tài khoản đã đăng nhập đều đọc được).

| Tham số | Bắt buộc | Ghi chú |
| --- | --- | --- |
| `ma_dinh_danh` | không | Một hoặc nhiều loại, cách nhau bằng dấu phẩy: `muc_tieu_tap_luyen,khu_vuc`. Bỏ trống thì trả mọi loại |

| `ma_cap_tren` | không | Mã mục cha, trả mục con trực tiếp, ví dụ `ma_dinh_danh=khu_vuc&ma_cap_tren=ha_noi`. Phải kèm đúng một `ma_dinh_danh`, nếu không trả `400` |

Không có `ma_cap_tren` thì **chỉ trả mục cấp 1**. Mã cha không tồn tại hoặc chưa có mục con thì trả mảng rỗng. Mỗi mục
chỉ xét `tinh_trang` của chính nó: ẩn một tỉnh thì ẩn cả mục con của tỉnh đó.

<!-- TODO(DANH-MUC): chưa nạp dữ liệu xã/phường (cấp 2 của khu_vuc); cần danh mục hành chính chính thức và migration. -->

Chỉ trả mục có `tinh_trang = 1`; loại không có mục nào đang dùng thì không xuất hiện trong kết quả. Trong mỗi loại,
mục sắp theo `ten` (bảng chưa có cột thứ tự).

```json
[
  {
    "ma_dinh_danh": "khu_vuc",
    "items": [
      { "ma": "can_tho", "ten": "Cần Thơ", "mo_ta": null, "cap": 1, "danh_muc_cap_tren_id": null },
      { "ma": "da_nang", "ten": "Đà Nẵng", "mo_ta": null, "cap": 1, "danh_muc_cap_tren_id": null }
    ]
  }
]
```

Lỗi: `401 UNAUTHORIZED` khi chưa đăng nhập; các lỗi khác theo [quy-uoc-response.md](quy-uoc-response.md).

## 4. Phân tầng

```text
GET /api/v1/mob/danh-muc → Mob/Controllers/DanhMucController
    → DanhMucService.BLLayDanhMucAsync   (GymBase.BLDI/BLDanhMuc)
    → IDanhMucRepository.DLFindDangDungAsync → MySQL (GymBase.DLDI.MySql/DLDanhMuc)
```

Đăng ký: `AddDIBusiness()` (BL) và `AddDIData()` (DL.MySql, đăng ký `IDanhMucRepository → DanhMucRepository`), cả hai được gọi ở `Program.cs`. Mobile chỉ cần kênh
`mob`; chưa có màn quản trị danh mục trên web.

## 5. Test

- `dotnet test GymBase.slnx` tại `apps/api` — `DanhMucTests` phủ: trả đủ các loại đang dùng, lọc theo `ma_dinh_danh`,
  bỏ mục `tinh_trang = 0`, 401 khi chưa đăng nhập.
