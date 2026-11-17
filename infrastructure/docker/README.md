# Docker local

`compose.dev.yml` chạy MySQL 8.4 và API. Expo dev server chạy ngoài Docker để debug emulator/thiết bị thật.

| Service | Việc làm |
| --- | --- |
| `mysql` | `mysql:8.4`, named volume `mysql_data`, healthcheck bằng `mysqladmin ping`, mở `127.0.0.1:3307` |
| `migrate` | EF migrations bundle chạy một lần rồi thoát; `api` chỉ khởi động khi bước này thành công |
| `api` | `GymBase.API` (không chạy bằng root), cổng `5020` (đổi bằng `API_PORT`), file tải lên ở volume `uploads_data` |

Mọi lệnh chạy từ gốc repo, với `.env` tạo từ `.env.example` (cần `MYSQL_*` và `Jwt__SigningKey`):

```bash
docker compose --env-file .env -f infrastructure/docker/compose.dev.yml up --build -d
docker compose --env-file .env -f infrastructure/docker/compose.dev.yml logs -f api
docker compose --env-file .env -f infrastructure/docker/compose.dev.yml down        # giữ dữ liệu
docker compose --env-file .env -f infrastructure/docker/compose.dev.yml down -v     # xóa cả dữ liệu
```

Tạo tài khoản Admin đầu tiên (mật khẩu lấy từ biến môi trường để không nằm trong lịch sử lệnh):

```bash
GYMBASE_ADMIN_PASSWORD='<mật khẩu>' docker compose --env-file .env -f infrastructure/docker/compose.dev.yml run --rm -e GYMBASE_ADMIN_PASSWORD api create-admin admin@gymbase.local
```

## Ghi chú

- `Dockerfile.api` có ba target: `runtime` (API), `migrator` (EF bundle) và `publish`; build context là `apps/api`
  (`apps/api/.dockerignore` bỏ `bin`, `obj`, `logs`, `uploads`, `tests`). Cần BuildKit (mặc định trong Docker hiện nay).
- Phiên bản `dotnet-ef` trong Dockerfile (`DOTNET_EF_VERSION`) phải khớp `Microsoft.EntityFrameworkCore.Design` ở
  `apps/api/Directory.Packages.props`; nâng EF thì nâng cả hai.
- MySQL chạy với `--skip-log-bin` vì migration tạo trigger, mà binary log (bật mặc định ở 8.4) chặn việc đó với user không có quyền SUPER.
- Storage dùng `LOCAL` (volume `uploads_data`) vì MinIO chưa nằm trong compose này; compose MinIO riêng ở `docker-compose.minio.yml`.
- Thêm migration mới thì chạy lại `up --build`: `migrate` build lại bundle và áp migration còn thiếu.
- Điện thoại thật gọi API qua IP LAN của máy chạy Docker, cổng `API_PORT` (xem `docs/api/cach-chay-api.md`, mục 5).
