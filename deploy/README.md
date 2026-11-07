# Deployment

`development/` ghi kế hoạch local hiện tại. Chưa có release manifest.
Chỉ tạo `staging/` và `production/` khi nhóm có môi trường thật, người quản lý,
secret store và quy trình migration/rollback.

API, web và mobile phát hành độc lập. EAS/store credentials, domain, server và
pipeline deployment sẽ được cấu hình ở task riêng; không tự publish từ base repo.
