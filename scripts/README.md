# Repository scripts

`node scripts/check-repo.mjs` kiểm tra root manifest, workspace patterns và các
đường dẫn tài liệu bắt buộc của khung REPO-01. Chạy từ mọi working directory,
không cài dependencies, không ghi file và trả exit code khác 0 nếu có lỗi.

Script không thay thế lint, build, tests, kiểm tra secrets hoặc chạy app.
Thêm các lệnh đó khi apps được scaffold; không tạo lệnh giả luôn trả thành công.
