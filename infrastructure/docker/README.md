# Docker local

DB-01 sẽ bổ sung `compose.dev.yml`, `Dockerfile.api`, env mapping và healthchecks
cho MySQL/API. REPO-01 chưa có Docker configuration chạy được.

Compose cần dùng named volume cho MySQL, cấu hình mật khẩu từ môi trường,
và ghi rõ lệnh chạy từ repository root. Docker build context phải bao gồm toàn bộ
projects backend tham chiếu nhau. Không đưa Expo dev server vào container.
