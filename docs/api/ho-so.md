# Hồ sơ sau onboarding

Sau khi chọn vai trò ở onboarding, app gửi một lần "Lưu thiết lập" chứa **cả khối học viên và khối PT**. Backend ghi
`ho_so_nguoi_dung` và `chi_so_co_the` trong **một transaction**; chỉ thêm dòng chỉ số cơ thể khi số đo thay đổi.

Khối nào bắt buộc tùy `loai_nguoi_dung`: `hoc_vien` gửi khối học viên, `pt` gửi khối PT, `ca_hai` gửi cả hai. Trên
app, màn thiết lập theo vai trò hiện đúng khối đó: một khối khi chọn học viên hoặc PT, hai tab khi chọn cả hai.

Trên app, thiết lập chia hai bước (thông tin cá nhân, rồi thông tin theo vai trò) nhưng dữ liệu của hai bước được giữ ở
bản nháp phía app và gửi **một lần** ở cuối, nên API vẫn là một request, một transaction.

## 1. Bảng liên quan

| Bảng | Vai trò | Ghi chú |
| --- | --- | --- |
| `ho_so_nguoi_dung` | Hồ sơ duy nhất của mỗi tài khoản | Khóa chính là `tai_khoan_id` (kiêm khóa ngoại), nên không dùng CRUD chung |
| `chi_so_co_the` | Lịch sử chiều cao, cân nặng | Bảng chỉ thêm; đọc bản mới nhất theo `ngay_do`, rồi `ngay_tao` |
| `tai_khoan` | Nguồn `ten_hien_thi` khi tạo hồ sơ | Đọc qua `ITaiKhoanHoTenProvider` ở `GymBase.Base.Auth` (BLBiz không đọc entity của Auth) |
| `danh_muc` | Kiểm tra mã mục tiêu, khu vực, trình độ tập, chuyên môn (giới tính là enum cố định, không qua đây) | Đọc qua `IDanhMucTraCuu` ở `GymBase.ModuleContracts.DanhMuc` (BLBiz không tham chiếu BLDI) |

Cột được ghi: `loai_nguoi_dung`, `ten_hien_thi` (khi có gửi), `ngay_sinh`, `gioi_tinh`, `trinh_do_tap`,
`muc_tieu_tap_luyen`, `khu_vuc_hoat_dong`, `chuyen_mon` (JSON), `so_nam_kinh_nghiem`, `gioi_thieu`, bốn cột audit của hồ sơ; và `hoc_vien_id`, `nguoi_nhap_id`, `ngay_do`, `chieu_cao_cm`, `can_nang_kg`,
`ngay_tao` của chỉ số cơ thể.

## 2. API

Cả hai endpoint cần đăng nhập (mọi tài khoản đã đăng nhập).

### `GET /api/v1/mob/ho-so`

Hồ sơ hiện tại kèm chỉ số cơ thể mới nhất. Chưa thiết lập → `404 PROFILE_NOT_FOUND`.

### `PUT /api/v1/mob/ho-so`

```json
{
  "loai_nguoi_dung": "hoc_vien",
  "ten_hien_thi": "Huy Gym",
  "ngay_sinh": "1998-05-20",
  "gioi_tinh": "nam",
  "trinh_do_tap": "co_nen_tang",
  "muc_tieu_tap_luyen": "tang_co",
  "chieu_cao_cm": 172.5,
  "can_nang_kg": 68.4,
  "khu_vuc_hoat_dong": "ha_noi",
  "chuyen_mon": null,
  "so_nam_kinh_nghiem": null,
  "gioi_thieu": null,
  "chung_chi_tep_tin_id": null
}
```

Trả `200` cùng DTO hồ sơ vừa lưu:

```json
{
  "loai_nguoi_dung": "hoc_vien",
  "ten_hien_thi": "Huy Gym",
  "ngay_sinh": "1998-05-20",
  "gioi_tinh": "nam",
  "trinh_do_tap": "co_nen_tang",
  "muc_tieu_tap_luyen": "tang_co",
  "khu_vuc_hoat_dong": "ha_noi",
  "chuyen_mon": null,
  "so_nam_kinh_nghiem": null,
  "gioi_thieu": null,
  "trang_thai_xac_minh": "chua_xac_minh",
  "chi_so_co_the": { "ngay_do": "2026-11-15T00:00:00Z", "chieu_cao_cm": 172.5, "can_nang_kg": 68.4 },
  "chung_chi": null
}
```

Dùng `PUT` vì mỗi tài khoản chỉ có một hồ sơ; lần lưu đầu tạo mới, các lần sau cập nhật.

### Chứng chỉ PT (tệp PDF)

Khối PT có một chứng chỉ tùy chọn (nhãn trên app: "Chứng chỉ (nếu có)"), lưu dưới dạng tệp PDF theo hai bước:

1. App tải tệp lên bằng `POST /api/v1/mob/files` với `purpose = chung_chi_pt`. Tệp phải là PDF thật (backend kiểm tra phần đầu tệp là
   `%PDF-`, không tin đuôi tệp), tối đa `Storage:MaxUploadBytes` (mặc định 10 MB). Tệp ở chế độ **riêng tư**: chỉ người tải lên đọc được qua
   `GET /files/{id}/content`. Kết quả có `file_id`.
2. App gửi `file_id` đó ở `chung_chi_tep_tin_id` của `PUT /ho-so`. Backend kiểm tra tệp là của chính chủ, `muc_dich = chung_chi_pt`, kiểu PDF
   và chưa bị gỡ, rồi gắn vào hồ sơ: `tep_tin.loai_doi_tuong = ho_so_nguoi_dung`, `doi_tuong_id = tai_khoan_id`, `trang_thai = da_lien_ket`
   (theo thiết kế ở [02-tep-tin.md](../../database/diagrams/02-tep-tin.md), không thêm cột hay bảng).

Kết quả trả ở `chung_chi`: `{ "file_id", "ten_tep", "kich_thuoc_byte" }` hoặc `null`. `PUT` thay thế toàn bộ: không gửi `chung_chi_tep_tin_id` (hoặc đổi
sang `hoc_vien`) thì chứng chỉ đang có bị gỡ, tệp đánh dấu `da_xoa` và xóa khỏi kho lưu trữ. Gửi tệp khác thì tệp cũ bị gỡ như trên. Tệp không hợp lệ:
`400 VALIDATION_FAILED` ở `chung_chi_tep_tin_id`; `hoc_vien` gửi trường này: `400 ROLE_INVALID`.

Phía mobile: tab PT của màn thiết lập có ô "Chứng chỉ (nếu có)" ngay dưới số năm kinh nghiệm (chọn PDF bằng `expo-document-picker`, tải lên ngay,
bấm × để bỏ). Tệp đã tải lên mà không gắn vào hồ sơ nằm lại ở `da_tai_len`; chưa có job dọn file nên các tệp này tồn tại tới khi có job.
Lưu xong, app mở `/profile-setup/hoan-tat?loai=...`: `pt` và `ca_hai` thấy "Đã gửi hồ sơ PT!" kèm trạng thái "Chờ xác minh" (Storyboard 3, bước 6),
`hoc_vien` thấy màn chào mừng đến GymBase.

Chưa có màn quản trị để xem chứng chỉ khi xác minh PT; `ChungChiPtDto` cố ý không có đường dẫn tệp.

## 3. Quy tắc

| Quy tắc | Chi tiết | Lỗi |
| --- | --- | --- |
| Vai trò | `loai_nguoi_dung` là `hoc_vien`, `pt` hoặc `ca_hai` | `400 ROLE_INVALID` |
| Khối học viên | Bắt buộc với `hoc_vien`/`ca_hai`: `muc_tieu_tap_luyen`, `trinh_do_tap`, `chieu_cao_cm`, `can_nang_kg`. `pt` có thể bỏ khối này; nếu gửi một trong hai số đo thì phải gửi cả hai | `400 VALIDATION_FAILED` |
| Khối PT | Bắt buộc với `pt`/`ca_hai`: `chuyen_mon` (ít nhất 1) và `so_nam_kinh_nghiem`; `gioi_thieu` và `chung_chi_tep_tin_id` tùy chọn. `hoc_vien` không được gửi khối này | `400 VALIDATION_FAILED`; `hoc_vien` gửi thì `400 ROLE_INVALID` |
| Tên hiển thị | Tùy chọn. Có gửi thì ghi sau khi cắt khoảng trắng (1–100 ký tự); không gửi thì tạo mới lấy `tai_khoan.ho_ten`, cập nhật giữ nguyên | `400 VALIDATION_FAILED` |
| Ngày sinh | Tùy chọn; không gửi thì lưu `null`. Có gửi thì `yyyy-MM-dd`, không ở tương lai, tuổi 13–100 | `400 VALIDATION_FAILED` |
| Giới tính | Tùy chọn; không gửi thì lưu `null`. Có gửi thì một trong `nam`, `nu`, `khac`, `khong_muon_noi` (danh sách cố định trong code, **không** đọc từ `danh_muc`; app cũng để cố định). Lưu ở `ho_so_nguoi_dung.gioi_tinh` | `400 VALIDATION_FAILED` |
| Trình độ tập | Thuộc khối học viên; mã phải có trong `danh_muc` với `ma_dinh_danh = 'trinh_do_tap'` | `400 CATEGORY_INVALID` |
| Mục tiêu | Mã phải có trong `danh_muc` với `ma_dinh_danh = 'muc_tieu_tap_luyen'` và `tinh_trang = 1` | `400 CATEGORY_INVALID` |
| Khu vực | Tùy chọn; không gửi thì lưu `null`. Có gửi thì mã phải có trong `danh_muc` với `ma_dinh_danh = 'khu_vuc'` | `400 CATEGORY_INVALID` |
| Chuyên môn | Mỗi mã phải có trong `danh_muc` với `ma_dinh_danh = 'chuyen_mon_pt'`; 1–10 mục, tự bỏ trùng | `400 CATEGORY_INVALID` |
| Chiều cao | Khối học viên, 100–250 cm, làm tròn 1 chữ số thập phân | `400 VALIDATION_FAILED` |
| Cân nặng | Khối học viên, 20–300 kg, làm tròn 2 chữ số thập phân | `400 VALIDATION_FAILED` |
| Kinh nghiệm | Khối PT, 0–60 năm | `400 VALIDATION_FAILED` |
| Giới thiệu | Khối PT, tùy chọn, tối đa 1000 ký tự | `400 VALIDATION_FAILED` |
| Chỉ số cơ thể | Chỉ thêm dòng mới khi có số đo và chiều cao hoặc cân nặng khác bản ghi mới nhất; PT không gửi khối học viên thì không thêm | — |
| Đổi vai trò | Lưu lại với vai trò khác thì khối không còn thuộc vai trò mới bị xóa (ví dụ PT sang học viên xóa chuyên môn, kinh nghiệm, giới thiệu) | — |
| Giữ nguyên khi cập nhật | Ảnh đại diện, trạng thái xác minh của hồ sơ đã có (`ten_hien_thi` chỉ đổi khi có gửi) | — |
| Tạo mới | `ten_hien_thi` = giá trị gửi lên, không có thì `tai_khoan.ho_ten`; `trang_thai_xac_minh` = `chua_xac_minh` | — |
| Hồ sơ cũ | Hồ sơ tạo trước khi có các cột mới trả `null` cho `ngay_sinh`, `gioi_tinh`, `trinh_do_tap` | — |

Mã danh mục lấy từ [`GET /api/v1/mob/danh-muc`](danh-muc.md), app hiển thị `ten` nhưng gửi lại `ma`.

## 3a. Quyền riêng tư

`ngay_sinh`, tuổi và `gioi_tinh` là thông tin riêng tư: **chỉ** `GET/PUT /ho-so` của chính chủ trả chúng. Mọi DTO hiển
thị người khác (tác giả bài viết, bảng tin, bình luận, sau này là trang hồ sơ công khai) không được chứa ngày sinh, tuổi
hay giới tính, kể cả dạng đã tính sẵn. Test `Public_post_responses_never_expose_private_profile_fields` giữ quy tắc này
cho bảng tin, chi tiết bài viết và bình luận; thêm DTO công khai mới thì thêm vào test đó.

## 4. Phân tầng

```text
GET/PUT /api/v1/mob/ho-so → Mob/Controllers/HoSoController
    → HoSoService.BLXemHoSoAsync / BLLuuHoSoAsync     (GymBase.BLBiz/BLHoSo)
    → IHoSoNguoiDungRepository / IChiSoCoTheRepository → MySQL (GymBase.DLBiz.MySql)
    → IDanhMucTraCuu (BLDI) · ITaiKhoanHoTenProvider (BLAuth) · IChungChiPtLienKet (BLFile, ModuleContracts)
```

`IHoSoNguoiDungRepository.DLUpsertAsync` nhận cả `chi_so_co_the?` và mở transaction, để một lần bấm "Lưu" không ghi
nửa chừng. Đăng ký: `AddBizBusiness()` (BL) và `AddBizData()` (DL.MySql, gồm các repository của hồ sơ và chỉ số cơ thể), cả hai được gọi ở `Program.cs`.

## 5. Test

- `dotnet test GymBase.slnx` tại `apps/api` — `HoSoTests` phủ: lưu lần đầu tạo hồ sơ + chỉ số; lưu lại cùng số đo không
  thêm dòng; đổi số đo thêm đúng một dòng; giữ `ten_hien_thi`/trạng thái xác minh; PT gửi chuyên môn hợp lệ; mã danh mục
  sai; chiều cao ngoài khoảng; thiếu hoặc ngoài khoảng ngày sinh, giới tính ngoài danh sách cố định, mã trình độ sai; gửi hoặc bỏ `ten_hien_thi`;
  hồ sơ cũ trả `null` cho trường mới; học viên gửi thông tin PT; vai trò sai; `GET` khi chưa có hồ sơ; 401 khi chưa đăng nhập; chứng chỉ PT
  (gắn và đọc lại, tệp của người khác hoặc sai mục đích hoặc kiểu bị từ chối, thay và gỡ chứng chỉ, học viên gửi chứng chỉ). `FileServiceTests`
  phủ upload PDF (riêng tư, giữ nguyên nội dung, từ chối tệp giả PDF).
