# API contracts

Tài liệu backend:

| File | Nội dung |
| --- | --- |
| [cau-truc-thu-muc.md](cau-truc-thu-muc.md) | Chuẩn kiến trúc: một host `GymBase.API`, nhóm Auth / DI / BIZ, trách nhiệm từng project, ma trận tham chiếu, quy tắc review |
| [cach-chay-api.md](cach-chay-api.md) | Chạy API cho web và mobile, điện thoại thật, lỗi thường gặp |
| [crud-dung-chung.md](crud-dung-chung.md) | Lớp nền CRUD và các bước thêm CRUD cho một bảng mới |
| [quy-uoc-response.md](quy-uoc-response.md) | Dạng response khi thành công và khi lỗi, phân trang, bộ mã lỗi |
| [sort-va-filter.md](sort-va-filter.md) | Cú pháp `sort` và `filter`, lỗi trả về, cách bật cho một bảng mới |
| [viec-con-lai-base.md](viec-con-lai-base.md) | Phần base còn phải làm theo mức ưu tiên, quyết định đã chốt |
| [bruno/](bruno) | Collection [Bruno](https://www.usebruno.com/) để gọi thử API |

Không trả stack trace cho production hoặc log mật khẩu/token.

OpenAPI từ backend (`/openapi/v1.json`) là nguồn chuẩn cho contract client. Khi cần dùng chung,
sinh `packages/api-client` hoặc `packages/contracts`; không sửa tay generated DTOs
và không chia sẻ trực tiếp EF entities.
