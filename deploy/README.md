# Deployment

`development/` ghi kế hoạch local hiện tại. `production/` là cấu hình một VPS (nginx, API, MySQL) kèm quy trình
migration, sao lưu và rollback. Chưa có `staging/`: chỉ tạo khi nhóm có môi trường thật và người quản lý.

API, web và mobile phát hành độc lập. EAS/store credentials, domain, server và
pipeline deployment sẽ được cấu hình ở task riêng; không tự publish từ base repo.
