# Deployment

Hai môi trường tách bạch, mỗi môi trường có `compose.yml`, `.env.example` và README riêng (project Docker, volume và mật khẩu
không dùng chung):

- `development/`: MySQL + API build từ mã nguồn trên máy dev, migration tự chạy, HTTP.
- `production/`: một VPS với nginx (HTTPS), API build từ mã nguồn trên VPS, MySQL, migration chạy tay, sao lưu và rollback.

Chưa có `staging/`: chỉ tạo khi nhóm có môi trường thật và người quản lý.

API, web và mobile phát hành độc lập. EAS/store credentials, domain, server và
pipeline deployment sẽ được cấu hình ở task riêng; không tự publish từ base repo.
