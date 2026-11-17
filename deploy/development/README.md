# Development

Chạy MySQL, MinIO (S3 cục bộ) và API trên máy dev bằng Docker. Môi trường **thật** nằm ở [../production](../production/README.md); hai môi trường
tách hẳn nhau:

| | Development (thư mục này) | Production (`../production`) |
| --- | --- | --- |
| Project Docker | `gymbase-dev` | `gymbase-prod` |
| File cấu hình | `.env` ở thư mục này | `.env` ở `../production` |
| API | build từ mã nguồn, `ASPNETCORE_ENVIRONMENT=Development` (có OpenAPI) | kéo image từ GHCR theo tag, `Production` |
| Truy cập | `API_PORT` (5020), MySQL ở `127.0.0.1:3307`, MinIO ở `127.0.0.1:9100`, HTTP | chỉ nginx (80, 443), HTTPS |
| Migration | tự chạy mỗi lần `up` | chạy tay, sao lưu trước |
| MySQL | không binary log | binary log, sao lưu định kỳ |
| File tải lên | MinIO trong Docker (bucket `gymbase-files`, tự tạo ở lần tải lên đầu) | S3 bên ngoài |

Dev và production không dùng chung container, volume, mạng hay mật khẩu; có thể chạy cùng lúc trên một máy mà không đụng nhau.

## Chạy

Cần Docker. Tạo `.env` từ `.env.example` (điền mật khẩu MySQL và `Jwt__SigningKey`) rồi:

```bash
cd deploy/development
cp .env.example .env
docker compose up --build -d      # build, áp migration, chạy API; API nghe ở http://localhost:5020
docker compose logs -f api
docker compose down               # giữ dữ liệu
docker compose down -v            # xóa cả dữ liệu (reset DB sạch)
```

Chưa có script seed Admin. Đăng ký tài khoản thường (app hoặc `POST /auth/register`) rồi cấp quyền trong DB:

```bash
docker compose exec -T mysql sh -c 'mysql -u root -p"$MYSQL_ROOT_PASSWORD" "$MYSQL_DATABASE"' <<'SQL'
UPDATE tai_khoan SET vai_tro = 'Admin' WHERE email = '<email-vừa-đăng-ký>';
SQL
```

Thêm migration mới thì chạy lại `docker compose up --build -d`: `migrate` build lại bundle và áp phần còn thiếu.

## File tải lên (MinIO)

- Giao diện web quản lý bucket: `http://localhost:9101`, đăng nhập bằng `MINIO_ROOT_USER` và `MINIO_ROOT_PASSWORD` (mặc định
  `minioadmin`/`minioadmin`). S3 API ở `http://localhost:9100`.
- Mobile không cần thấy MinIO: API tự đọc object rồi trả qua `GET /api/v1/web/files/{id}/content`.
- `docker compose down -v` xóa cả `minio_data` (mất file đã tải lên), nên nhớ xóa luôn dữ liệu MySQL tương ứng.
- File tải lên **trước khi** chuyển sang MinIO (cột `tep_tin.s3_bucket` là `LOCAL`) vẫn đọc được vì volume `uploads_data` còn giữ;
  muốn dồn sang MinIO thì gọi `POST /api/v1/web/files/migrate-to-s3`.
- MinIO chạy riêng ở `infrastructure/docker/docker-compose.minio.yml` (cổng 9000, 9001) dành cho lúc chạy API bằng tay; hai
  cái không đụng nhau vì khác cổng và khác volume.

## Ghi chú

- Debug bằng Visual Studio hay `dotnet run` vẫn nhanh hơn (breakpoint, hot reload); xem `docs/api/cach-chay-api.md`. Nếu API chạy tay
  đang giữ cổng 5020 thì đặt `API_PORT` khác trong `.env`.
- Dockerfile dùng chung với production: `infrastructure/docker/Dockerfile.api`.
- Mobile: `EXPO_PUBLIC_API_URL` trỏ `http://10.0.2.2:5020` (emulator Android) hoặc IP LAN của máy chạy Docker (thiết bị thật).
- Expo dev server chạy ngoài Docker để debug emulator/thiết bị thật.
