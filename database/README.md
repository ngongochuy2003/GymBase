# Database

MySQL là database dự kiến. REPO-01 chưa tạo schema hoặc kết nối database.

- `diagrams/`: ERD và quyết định mô hình dữ liệu.
- `scripts/`: SQL vận hành thủ công có mục đích và hướng dẫn rõ ràng.
- `seeds/`: dữ liệu giả cho development/test.

EF Core migrations tại
`apps/api/src/Data/GymBase.Persistence/Migrations/` (cùng chỗ với `AppDbContext`) là nguồn thay đổi
schema duy nhất. Không duy trì một bộ SQL migration song song.
Không commit MySQL volumes, backup thật hoặc dữ liệu người dùng.
