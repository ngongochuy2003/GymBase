# Infrastructure

`docker/` là nơi triển khai cấu hình local ở DB-01.
Expo dev server chạy ngoài Docker để thuận tiện debug emulator/thiết bị thật.

Chưa tạo `nginx/`, `monitoring/` hoặc IaC: chỉ bổ sung khi có hạ tầng sử dụng thật.
Cấu hình release theo môi trường thuộc `deploy/`.
