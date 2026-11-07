# API contracts

Giải thích cấu trúc thư mục backend (Management / AMISBASE, DI, Biz, Mob, các tầng
API / BL / DL): [cau-truc-thu-muc.md](cau-truc-thu-muc.md).

Chưa có endpoint hoặc OpenAPI ở REPO-01. API-01 sẽ tạo host và `/health`;
API-02 bổ sung conventions, validation, ProblemDetails, logging và OpenAPI.

Nhóm cần chốt authentication/refresh, authorization, pagination, thời gian/múi giờ,
HTTP status, error codes và trace ID trước khi tích hợp Mobile.
Không trả stack trace cho production hoặc log mật khẩu/token.

OpenAPI từ backend là nguồn chuẩn cho contract client. Khi cần dùng chung,
sinh `packages/api-client` hoặc `packages/contracts`; không sửa tay generated DTOs
và không chia sẻ trực tiếp EF entities. Chưa tạo endpoint/DTO giả ở bước này.
