# API contracts

Tài liệu backend:

| File | Nội dung |
| --- | --- |
| [cau-truc-thu-muc.md](cau-truc-thu-muc.md) | Chuẩn kiến trúc: một host `GymBase.API`, nhóm Auth / DI / BIZ, trách nhiệm từng project, ma trận tham chiếu, quy tắc review |
| [cach-chay-api.md](cach-chay-api.md) | Chạy API cho web và mobile, điện thoại thật, lỗi thường gặp |
| [crud-dung-chung.md](crud-dung-chung.md) | Lớp nền CRUD: các lớp, nơi đặt entity và DTO, hành vi HTTP |
| [huong-dan-them-crud.md](huong-dan-them-crud.md) | Hướng dẫn từng bước thêm CRUD cho một bảng mới, dành cho người mới |
| [huong-dan-them-api-nghiep-vu.md](huong-dan-them-api-nghiep-vu.md) | Hướng dẫn thêm API không phải CRUD, có nhiều quy tắc và nhiều bảng (giao dịch, mã lỗi riêng, test theo bảng quy tắc) |
| [quy-uoc-response.md](quy-uoc-response.md) | Dạng response khi thành công và khi lỗi, phân trang, bộ mã lỗi |
| [sort-va-filter.md](sort-va-filter.md) | Cú pháp `sort` và `filter`, lỗi trả về, cách bật cho một bảng mới |
| [logging.md](logging.md) | Log kỹ thuật (Serilog) và audit log (bảng `system_log`): cách xem, cấu hình, truy vấn, giới hạn |
| [phan-quyen.md](phan-quyen.md) | Phân quyền ba lớp: vai trò hệ thống, policy theo hồ sơ, quyền trên dữ liệu học viên; cách thêm endpoint có quyền, mã lỗi |
| [xac-thuc.md](xac-thuc.md) | Thiết kế xác thực và phân quyền: JWT, refresh token, mật khẩu, đăng nhập Google (chưa triển khai) |
| [viec-con-lai-base.md](viec-con-lai-base.md) | Phần base còn phải làm theo mức ưu tiên, quyết định đã chốt |
| [bruno/](bruno) | Collection [Bruno](https://www.usebruno.com/) để gọi thử API |

Không trả stack trace cho production hoặc log mật khẩu/token.

OpenAPI từ backend (`/openapi/v1.json`) là nguồn chuẩn cho contract client. Khi cần dùng chung,
sinh `packages/api-client` hoặc `packages/contracts`; không sửa tay generated DTOs
và không chia sẻ trực tiếp EF entities.
