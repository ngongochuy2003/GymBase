# Sơ đồ database

Sơ đồ ERD viết bằng [Mermaid](https://mermaid.js.org/), xem trực tiếp trên GitHub hoặc bằng preview Markdown của VS Code.
Nguồn thiết kế: `Tài liệu dự án/GymBase_V1_Thiet_ke_Database_21_bang.docx`.

> **Trạng thái:** đây là thiết kế đã chốt, chưa triển khai hết. Trong code mới có `nguoi_dung` (sẽ đổi thành `tai_khoan`),
> `phien_dang_nhap` và `system_log`. `dang_nhap_ngoai` và 21 bảng nghiệp vụ chưa có entity hay migration.
> Dự kiến hai migration: `InitialBase` (4 bảng nền), sau đó `AddBizTables` (toàn bộ 21 bảng nghiệp vụ).
> Tổng cộng 25 bảng.

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

## Điểm còn mở

Các điểm tài liệu chưa nói rõ, đã ghi ở từng sơ đồ:

1. Khi nào `nhat_ky_hoat_dong` chuyển từ `dat` sang `hoan_thanh`.
2. `buoi_tap.trang_thai` sau khi xử lý khiếu nại.
3. Cách nối `thiet_bi` với `phien_dang_nhap` khi đăng xuất.
4. Danh sách `loai_doi_tuong` đầy đủ của `lich_su_chinh_sua` và `thong_bao`.
5. Người dùng có đổi nhóm sau khi đã chọn được không (đề xuất: chọn một lần, đổi sang PT phải xác minh lại).

Điểm đã giải quyết: `vai_tro` chỉ còn `Admin` hoặc `User`, quyền làm PT dựa vào `loai_nguoi_dung` và `trang_thai_xac_minh`.
