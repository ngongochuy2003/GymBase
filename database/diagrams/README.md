# Sơ đồ database

Sơ đồ ERD viết bằng [Mermaid](https://mermaid.js.org/), xem trực tiếp trên GitHub hoặc bằng preview Markdown của VS Code.
Nguồn thiết kế: `Tài liệu dự án/GymBase_V1_Thiet_ke_Database_21_bang.docx`.

> **Trạng thái:** đây là thiết kế, chưa triển khai hết. Entity của Base đã đổi `nguoi_dung` thành `tai_khoan` (chưa commit),
> nhưng các migration hiện có vẫn tạo `nguoi_dung`. `dang_nhap_ngoai` và 21 bảng nghiệp vụ chưa có entity hay migration.
> Dự kiến hai migration: `InitialBase` (4 bảng nền, tạo lại cho khớp tên mới), sau đó `AddBizTables` (toàn bộ 21 bảng
> nghiệp vụ, kèm cột `phien_dang_nhap.thiet_bi_id`). Tổng cộng 25 bảng.
>
> Sơ đồ có **một số thay đổi đề xuất so với tài liệu thiết kế** (bảng bên dưới). Tài liệu docx chưa được cập nhật theo.

| File | Nội dung | Bảng |
| --- | --- | --- |
| [00-tong-quan.md](00-tong-quan.md) | Sơ đồ tổng: các nhóm bảng và mọi khóa ngoại | Tất cả 25 bảng |
| [01-tai-khoan-ho-so.md](01-tai-khoan-ho-so.md) | Tài khoản, phiên đăng nhập, đăng nhập Google, log hệ thống, hồ sơ, luồng đăng ký và chọn nhóm | `tai_khoan`, `phien_dang_nhap`, `dang_nhap_ngoai`, `system_log`, `ho_so_nguoi_dung` |
| [02-tep-tin.md](02-tep-tin.md) | File trên S3, vòng đời, luồng upload | `tep_tin` |
| [03-mang-xa-hoi.md](03-mang-xa-hoi.md) | Bài viết, thích và bình luận, theo dõi | `bai_viet`, `tuong_tac_bai_viet`, `theo_doi` |
| [04-ket-noi-pt.md](04-ket-noi-pt.md) | Kết nối PT với học viên, vòng đời | `ket_noi_pt` |
| [05-nhan-tin.md](05-nhan-tin.md) | Chat 1–1 | `cuoc_tro_chuyen`, `thanh_vien_cuoc_tro_chuyen`, `tin_nhan` |
| [06-huan-luyen.md](06-huan-luyen.md) | Kế hoạch, nhật ký, lịch sử chỉnh sửa, chỉ số cơ thể | `ke_hoach`, `muc_ke_hoach`, `nhat_ky_hoat_dong`, `lich_su_chinh_sua`, `chi_so_co_the` |
| [07-goi-pt-buoi-tap.md](07-goi-pt-buoi-tap.md) | Gói buổi PT, buổi tập, sổ cái số buổi | `goi_pt_hoc_vien`, `buoi_tap`, `bien_dong_so_buoi` |
| [08-thong-bao.md](08-thong-bao.md) | Thiết bị, thông báo, luồng nhắc giờ | `thiet_bi`, `thong_bao` |
| [09-an-toan.md](09-an-toan.md) | Báo cáo vi phạm, chặn tài khoản | `bao_cao_vi_pham`, `chan_tai_khoan` |
| [10-lien-ket-da-hinh.md](10-lien-ket-da-hinh.md) | Các liên kết không có khóa ngoại thật | `tep_tin`, `lich_su_chinh_sua`, `thong_bao`, `bao_cao_vi_pham` |

## Quy ước trong sơ đồ

- Kiểu cột viết gọn: `guid` (lưu `varchar(36)`), `string`, `text`, `json`, `datetime` (UTC), `date`, `time`, `int`, `decimal`, `bool`.
  Độ dài cụ thể và `CHECK` xem trong tài liệu thiết kế, mục 4 "Từ điển dữ liệu".
- Quan hệ chỉ vẽ cho **khóa ngoại thật**. Liên kết đa hình nằm ở [10-lien-ket-da-hinh.md](10-lien-ket-da-hinh.md).
- Bảng ghi chú "đa hình, không có FK" nghĩa là cột đó không có ràng buộc ở database, service phải kiểm tra.
- Sơ đồ đã chuyển từ PostgreSQL trong tài liệu sang MySQL 8.4 của dự án: partial unique index thành cột sinh tự động kèm unique index.

## Thay đổi đề xuất so với tài liệu thiết kế

Chưa có trong docx, đã vẽ vào sơ đồ, cần duyệt trước khi viết migration.

| Bảng | Thay đổi | Lý do |
| --- | --- | --- |
| `dang_nhap_ngoai` | Bảng nền mới (Google) | Đăng nhập Google, định danh bằng `sub` |
| `phien_dang_nhap` | Thêm `thiet_bi_id` (FK, NULL) | Tắt push đúng thiết bị khi đăng xuất |
| `thiet_bi` | `push_token` từ text thành `varchar(512)` kèm `UNIQUE` | Một token không nằm ở hai tài khoản, tránh thông báo lọt sang người khác |
| `ho_so_nguoi_dung` | Thêm `ngay_xac_minh`, `nguoi_xac_minh_id`, `ly_do_tu_choi` | Biết ai duyệt PT, khi nào, vì sao từ chối |
| `tep_tin` | Thêm `muc_dich` | Phân biệt ảnh đại diện với chứng chỉ PT cùng gắn vào hồ sơ |
| `nhat_ky_hoat_dong` | Thêm `phien_ban_ke_hoach` | Biết nhật ký làm theo phiên bản kế hoạch nào |
| `ho_so_nguoi_dung`, `bai_viet`, `ke_hoach`, `muc_ke_hoach`, `nhat_ky_hoat_dong`, `goi_pt_hoc_vien`, `buoi_tap`, `thiet_bi` | Thay `ngay_cap_nhat` bằng bốn cột audit của Base: `ngay_tao`, `nguoi_tao`, `ngay_chinh_sua`, `nguoi_chinh_sua` | Dùng chung `BaseAuditableEntity`; `goi_pt_hoc_vien` có mốc đổi trạng thái |
| `buoi_tap` | Thêm `ngay_khieu_nai`, `nguoi_xu_ly_khieu_nai_id`, `ngay_xu_ly_khieu_nai` | Truy vấn nhanh trạng thái khiếu nại |
| `lich_su_chinh_sua` | Thêm `muc_ke_hoach`, `goi_pt_hoc_vien`, `bao_cao_vi_pham` vào `loai_doi_tuong`; thêm `xu_ly_bao_cao`, `huy`, `ket_thuc` vào `hanh_dong` | Tài liệu để danh sách mở, nhưng yêu cầu ghi lịch sử các việc này |
| `bien_dong_so_buoi` | `CHECK` dấu `so_buoi_thay_doi` theo `loai_bien_dong` | Dữ liệu sổ cái đúng ngay ở database |

## Quy ước chung khi làm migration

- **Cột audit (đã chốt):** bảng nào có `ngay_tao` / `ngay_cap_nhat` thì kế thừa `BaseAuditableEntity`, tức có bốn cột
  `ngay_tao`, `nguoi_tao`, `ngay_chinh_sua`, `nguoi_chinh_sua`. `ngay_cap_nhat` của tài liệu thiết kế được thay bằng
  `ngay_chinh_sua` (cho phép NULL cho tới lần sửa đầu). Áp dụng cho `bai_viet`, `ke_hoach`, `muc_ke_hoach`, `nhat_ky_hoat_dong`,
  `goi_pt_hoc_vien`, `buoi_tap`, `thiet_bi`. `ho_so_nguoi_dung` có khóa chính `tai_khoan_id` nên không kế thừa được, phải khai
  báo bốn cột này và ghi bằng tay.
- **Không kế thừa `BaseAuditableEntity`:** bảng nối và bản ghi chỉ thêm (`theo_doi`, `chan_tai_khoan`, `thanh_vien_cuoc_tro_chuyen`,
  `lich_su_chinh_sua`, `bien_dong_so_buoi`, `chi_so_co_the`, `tin_nhan`, `tuong_tac_bai_viet`, `thong_bao`, `tep_tin`, `ket_noi_pt`)
  giữ cột thời gian riêng như tài liệu. `cuoc_tro_chuyen.ngay_cap_nhat` cũng giữ nguyên vì nghĩa là thời điểm hoạt động gần nhất
  (tin nhắn mới), không phải lần sửa bản ghi. Đây là phần tôi tự xếp, cần bạn xem lại.
- **Cột `ngay_xoa`:** `tin_nhan` và `tuong_tac_bai_viet` giữ `ngay_xoa` (NULL là còn hiển thị). Không có query filter nên service
  phải lọc `ngay_xoa IS NULL`. Không còn xóa mềm kiểu `is_deleted`.
- **Khóa ngoại:** dùng `RESTRICT` cho mọi FK tới `tai_khoan`, để xóa bản ghi cha không kéo theo xóa con; cha đang được tham chiếu thì không xóa được (409 `IN_USE`). MySQL cấm `CHECK` trên cột đang dùng trong FK
  có `ON DELETE SET NULL` hoặc `CASCADE`, nên cột FK nào có `CHECK` (như `theo_doi`, `chan_tai_khoan`) phải để `RESTRICT`.
- **Trạng thái:** `varchar` kèm `CHECK`, danh sách giá trị khai báo một chỗ trong code và dùng chung cho `CHECK` và validator.
- **MySQL:** `timestamptz` thành `datetime(6)` UTC. `jsonb` thành `json`, index GIN của `chuyen_mon` không có, V1 lọc bằng
  `JSON_CONTAINS`. `s3_key` dùng collation `utf8mb4_bin`. Unique cho nhiều cột NULL hoạt động bình thường.

## Điểm còn mở

Các điểm tài liệu chưa nói rõ, đã ghi ở từng sơ đồ:

1. Khi nào `nhat_ky_hoat_dong` chuyển từ `dat` sang `hoan_thanh`.
2. `buoi_tap.trang_thai` sau khi xử lý khiếu nại, và ai được quyết định khiếu nại. **Bỏ qua** (11/11), chưa làm ở V1.
3. Danh sách `loai_doi_tuong` của `thong_bao`.
4. Người dùng có đổi nhóm sau khi đã chọn được không (đề xuất: chọn một lần, đổi sang PT phải xác minh lại).
5. V1 xác minh PT bằng gì, có cần upload chứng chỉ không (kéo theo giá trị của `tep_tin.muc_dich`).
6. `phien_ban_ke_hoach` trong nhật ký: thêm cột hay lưu trong JSON `chi_tiet`. **Bỏ qua** (11/11): cột vẫn nằm trong sơ đồ như
   đề xuất, chưa duyệt.
7. ~~Tự đăng ký bằng Google~~ **Đã chốt 11/11:** có, nằm trong V1. [xac-thuc.md](../../docs/api/xac-thuc.md) mục 8 đã sửa theo.
8. ~~Bốn bảng `lo_trinh`, `tham_gia_lo_trinh`, `ma_gioi_thieu`, `bien_dong_diem`~~ **Đã chốt 11/11: bỏ qua, V1 chỉ làm 21 bảng.**
   Bốn bảng này chỉ có trong [GymBase_V1_De_Xuat_Kien_Truc_Phan_Quyen.docx](../../Tài%20liệu%20dự%20án/Tổng%20quan%20kiến%20trúc%20dự%20án/GymBase_V1_De_Xuat_Kien_Truc_Phan_Quyen.docx),
   tài liệu đó cũng đề xuất giá trị `chua_thiet_lap` cho `loai_nguoi_dung` và bộ giá trị xác minh PT khác. Các đề xuất này không
   vào sơ đồ. Khi làm lại thì phải thiết kế từ đầu: tài liệu chưa có cột nào và chưa có quy tắc cộng điểm.

Điểm đã giải quyết: `vai_tro` chỉ còn `Admin` hoặc `User`, quyền làm PT dựa vào `loai_nguoi_dung` và `trang_thai_xac_minh`.
Cách nối `thiet_bi` với `phien_dang_nhap` đã đề xuất là cột `phien_dang_nhap.thiet_bi_id`.
