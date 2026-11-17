# Development

Chạy MySQL và API trên máy dev bằng Docker. Môi trường **thật** nằm ở [../production](../production/README.md); hai môi trường
tách hẳn nhau:

| | Development (thư mục này) | Production (`../production`) |
| --- | --- | --- |
| Project Docker | `gymbase-dev` | `gymbase-prod` |
| File cấu hình | `.env` ở thư mục này | `.env` ở `../production` |
| API | build từ mã nguồn, `ASPNETCORE_ENVIRONMENT=Development` (có OpenAPI) | kéo image từ GHCR theo tag, `Production` |
| Truy cập | `API_PORT` (5020) và MySQL ở `127.0.0.1:3307`, HTTP | chỉ nginx (80, 443), HTTPS |
| Migration | tự chạy mỗi lần `up` | chạy tay, sao lưu trước |
| MySQL | không binary log | binary log, sao lưu định kỳ |
| File tải lên | volume cục bộ | S3 bên ngoài |

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

Tạo tài khoản Admin đầu tiên (mật khẩu lấy từ biến môi trường để không nằm trong lịch sử lệnh):

```bash
GYMBASE_ADMIN_PASSWORD='<mật khẩu>' docker compose run --rm -e GYMBASE_ADMIN_PASSWORD api create-admin admin@gymbase.local
```

Thêm migration mới thì chạy lại `docker compose up --build -d`: `migrate` build lại bundle và áp phần còn thiếu.

## Ghi chú

- Debug bằng Visual Studio hay `dotnet run` vẫn nhanh hơn (breakpoint, hot reload); xem `docs/api/cach-chay-api.md`. Nếu API chạy tay
  đang giữ cổng 5020 thì đặt `API_PORT` khác trong `.env`.
- Dockerfile dùng chung với production: `infrastructure/docker/Dockerfile.api`.
- Mobile: `EXPO_PUBLIC_API_URL` trỏ `http://10.0.2.2:5020` (emulator Android) hoặc IP LAN của máy chạy Docker (thiết bị thật).
- Expo dev server chạy ngoài Docker để debug emulator/thiết bị thật.
