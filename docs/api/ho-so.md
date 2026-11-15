# Hồ sơ sau onboarding

Sau khi chọn vai trò ở onboarding, app gửi một lần "Lưu thiết lập" chứa **cả khối học viên và khối PT**. Backend ghi
`ho_so_nguoi_dung` và `chi_so_co_the` trong **một transaction**; chỉ thêm dòng chỉ số cơ thể khi số đo thay đổi.

Đợt này app chỉ gửi khối học viên cho cả ba lựa chọn vai trò; khối PT đã có ở API nhưng chưa dựng ở UI —
`// TODO(PROFILE-PT)` trong `HoSoService`.

## 1. Bảng liên quan

| Bảng | Vai trò | Ghi chú |
| --- | --- | --- |
| `ho_so_nguoi_dung` | Hồ sơ duy nhất của mỗi tài khoản | Khóa chính là `tai_khoan_id` (kiêm khóa ngoại), nên không dùng CRUD chung |
| `chi_so_co_the` | Lịch sử chiều cao, cân nặng | Bảng chỉ thêm; đọc bản mới nhất theo `ngay_do`, rồi `ngay_tao` |
| `tai_khoan` | Nguồn `ten_hien_thi` khi tạo hồ sơ | Đọc qua `ITaiKhoanHoTenProvider` ở `GymBase.Base.Auth` (BLBiz không đọc entity của Auth) |
| `danh_muc` | Kiểm tra mã mục tiêu, khu vực, chuyên môn | Đọc qua `IDanhMucTraCuu` ở `GymBase.ModuleContracts.DanhMuc` (BLBiz không tham chiếu BLDI) |

Cột được ghi: `loai_nguoi_dung`, `muc_tieu_tap_luyen`, `khu_vuc_hoat_dong`, `chuyen_mon` (JSON), `so_nam_kinh_nghiem`,
`gioi_thieu`, bốn cột audit của hồ sơ; và `hoc_vien_id`, `nguoi_nhap_id`, `ngay_do`, `chieu_cao_cm`, `can_nang_kg`,
`ngay_tao` của chỉ số cơ thể.

## 2. API

Cả hai endpoint cần đăng nhập (mọi tài khoản đã đăng nhập).

### `GET /api/v1/mob/ho-so`

Hồ sơ hiện tại kèm chỉ số cơ thể mới nhất. Chưa thiết lập → `404 PROFILE_NOT_FOUND`.

### `PUT /api/v1/mob/ho-so`

```json
{
  "loai_nguoi_dung": "hoc_vien",
  "muc_tieu_tap_luyen": "tang_co",
  "chieu_cao_cm": 172.5,
  "can_nang_kg": 68.4,
  "khu_vuc_hoat_dong": "ha_noi",
  "chuyen_mon": null,
  "so_nam_kinh_nghiem": null,
  "gioi_thieu": null
}
```

Trả `200` cùng DTO hồ sơ vừa lưu:

```json
{
  "loai_nguoi_dung": "hoc_vien",
  "ten_hien_thi": "Ngô Ngọc Huy",
  "muc_tieu_tap_luyen": "tang_co",
  "khu_vuc_hoat_dong": "ha_noi",
  "chuyen_mon": null,
  "so_nam_kinh_nghiem": null,
  "gioi_thieu": null,
  "trang_thai_xac_minh": "chua_xac_minh",
  "chi_so_co_the": { "ngay_do": "2026-11-15T00:00:00Z", "chieu_cao_cm": 172.5, "can_nang_kg": 68.4 }
}
```

Dùng `PUT` vì mỗi tài khoản chỉ có một hồ sơ; lần lưu đầu tạo mới, các lần sau cập nhật.

## 3. Quy tắc

| Quy tắc | Chi tiết | Lỗi |
| --- | --- | --- |
| Vai trò | `loai_nguoi_dung` là `hoc_vien`, `pt` hoặc `ca_hai` | `400 ROLE_INVALID` |
| Khối PT | Chỉ tài khoản `pt`/`ca_hai` mới gửi `chuyen_mon`, `so_nam_kinh_nghiem`, `gioi_thieu` | `400 ROLE_INVALID` |
| Mục tiêu | Mã phải có trong `danh_muc` với `ma_dinh_danh = 'muc_tieu_tap_luyen'` và `tinh_trang = 1` | `400 CATEGORY_INVALID` |
| Khu vực | Mã phải có trong `danh_muc` với `ma_dinh_danh = 'khu_vuc'` | `400 CATEGORY_INVALID` |
| Chuyên môn | Mỗi mã phải có trong `danh_muc` với `ma_dinh_danh = 'chuyen_mon_pt'`; tối đa 10 mục, tự bỏ trùng | `400 CATEGORY_INVALID` |
| Chiều cao | Bắt buộc, 100–250 cm, làm tròn 1 chữ số thập phân | `400 VALIDATION_FAILED` |
| Cân nặng | Bắt buộc, 20–300 kg, làm tròn 2 chữ số thập phân | `400 VALIDATION_FAILED` |
| Kinh nghiệm | 0–60 năm | `400 VALIDATION_FAILED` |
| Chỉ số cơ thể | Chỉ thêm dòng mới khi chiều cao hoặc cân nặng khác bản ghi mới nhất | — |
| Giữ nguyên khi cập nhật | `ten_hien_thi`, ảnh đại diện, trạng thái xác minh của hồ sơ đã có | — |
| Tạo mới | `ten_hien_thi` = `tai_khoan.ho_ten`; `trang_thai_xac_minh` = `chua_xac_minh` | — |

Mã danh mục lấy từ [`GET /api/v1/mob/danh-muc`](danh-muc.md), app hiển thị `ten` nhưng gửi lại `ma`.

## 4. Phân tầng

```text
GET/PUT /api/v1/mob/ho-so → Mob/Controllers/HoSoController
    → HoSoService.BLXemHoSoAsync / BLLuuHoSoAsync     (GymBase.BLBiz/BLHoSo)
    → IHoSoNguoiDungRepository / IChiSoCoTheRepository → MySQL (GymBase.DLBiz.MySql)
    → IDanhMucTraCuu (BLDI) · ITaiKhoanHoTenProvider (BLAuth)
```

`IHoSoNguoiDungRepository.DLUpsertAsync` nhận cả `chi_so_co_the?` và mở transaction, để một lần bấm "Lưu" không ghi
nửa chừng. Đăng ký ở `Program.cs`: `AddBizBusiness()` (BL) và hai repository (DL).

## 5. Test

- `dotnet test GymBase.slnx` tại `apps/api` — `HoSoTests` phủ: lưu lần đầu tạo hồ sơ + chỉ số; lưu lại cùng số đo không
  thêm dòng; đổi số đo thêm đúng một dòng; giữ `ten_hien_thi`/trạng thái xác minh; PT gửi chuyên môn hợp lệ; mã danh mục
  sai; chiều cao ngoài khoảng; học viên gửi thông tin PT; vai trò sai; `GET` khi chưa có hồ sơ; 401 khi chưa đăng nhập.
- Bruno: `docs/api/bruno/mobile/ho-so/`.
