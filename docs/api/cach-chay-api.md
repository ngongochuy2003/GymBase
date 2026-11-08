# Cách chạy API cho web và mobile

Backend chỉ có **một host**: `GymBase.API` (`apps/api/src/Host/GymBase.API`), nghe ở cổng
**5020** (profile `http`). Web và mobile dùng chung host, khác nhau ở **tiền tố route**, do
`ChannelRouteConvention` (`GymBase.Base.Hosting`) gắn theo thư mục của controller:

| Kênh | Controller đặt ở | Tiền tố | Ví dụ |
| --- | --- | --- | --- |
| Web (trang quản trị) | `Controllers/` | `/api/v1/web` | `GET http://localhost:5020/api/v1/web/users` |
| Mobile | `Mob/Controllers/` | `/api/v1/mob` | `GET http://localhost:5020/api/v1/mob/users` |

Gọi thiếu tiền tố hoặc sai tiền tố sẽ nhận **404**. `/health` và `/openapi/v1.json` không có tiền tố.

## 1. Chạy bằng Visual Studio

1. Mở `apps/api/GymBase.slnx`.
2. Chuột phải vào `GymBase.API` (thư mục `src/Host`), chọn **Set as Startup Project**.
3. Chọn profile **http** ở nút Run, rồi nhấn F5.

## 2. Chạy bằng VS Code (task)

Mở **thư mục gốc repo** (`GymBase`) trong VS Code. Các task khai báo ở `.vscode/tasks.json`.
Nhấn `Ctrl+Shift+P`, chọn **Tasks: Run Task**, rồi chọn task:

| Task | Việc làm |
| --- | --- |
| `API: chạy` | Build rồi chạy `GymBase.API` (5020) |
| `API: chạy cho điện thoại thật (LAN)` | Như trên nhưng nghe trên mọi địa chỉ mạng (xem mục 4) |
| `API: build` | Build solution. Phím tắt `Ctrl+Shift+B` |
| `API: test` | Chạy toàn bộ test (Unit, Integration, Architecture) |
| **`Dev: API + Expo`** | Build API, rồi chạy song song `GymBase.API` (5020) và Expo (8081). Dùng khi làm app mobile |
| `Mobile: chạy Expo` | `pnpm mobile`. Trong terminal của task nhấn `a` (Android), `w` (web), `r` (reload) |
| `Mobile: chạy Expo (xóa cache)` | Như trên kèm `-c`, dùng khi app hiển thị code cũ hoặc lỗi bundle lạ |
| `Mobile: kiểm tra (lint + typecheck + test)` | `pnpm mobile:check` |

App mobile mặc định chạy chế độ demo (không gọi API). Muốn app gọi API thì cấu hình
`apps/mobile/.env` theo mục 4. Muốn dừng API thì bấm vào terminal của task rồi nhấn Ctrl+C,
hoặc chạy **Tasks: Terminate Task**.

## 3. Chạy bằng dòng lệnh

Tại `apps/api`:

```bash
dotnet run --project src/Host/GymBase.API --launch-profile http
```

Dừng bằng Ctrl+C.

## 4. Kiểm tra đã chạy

- Mở `http://localhost:5020/health`, thấy `Healthy` là được.
- **Bruno:** mở [Bruno](https://www.usebruno.com/), chọn **Open Collection**, trỏ tới thư mục
  `docs/api/bruno`, chọn environment **local** ở góc trên bên phải. Collection có hai thư mục
  `Web (/api/v1/web)` và `Mobile (/api/v1/mob)`, mỗi request đã có sẵn assert mã trả về. Chạy cả
  collection bằng **Run** để kiểm tra nhanh.
- **Postman:** **Import** URL `http://localhost:5020/openapi/v1.json` để tạo collection tự động.
  Trang OpenAPI chỉ có ở môi trường Development.

Id người dùng mẫu (dữ liệu mock): `0193f0a0-0000-7000-8000-000000000001` (admin).

## 5. Cho app mobile gọi được API

Cấu hình `apps/mobile/.env` (xem `.env.example`). Base URL **gồm cả tiền tố `/api/v1/mob`**:

```text
EXPO_PUBLIC_DATA_MODE=api
EXPO_PUBLIC_API_URL=<địa chỉ ở bảng dưới>
```

| App chạy trên | `EXPO_PUBLIC_API_URL` | Cách chạy API |
| --- | --- | --- |
| Trình duyệt (Expo web), iOS Simulator | `http://localhost:5020/api/v1/mob` | Như mục 1–3 |
| Android Emulator | `http://10.0.2.2:5020/api/v1/mob` | Như mục 1–3 |
| Điện thoại thật | `http://<IP-máy-tính>:5020/api/v1/mob` | Phải lắng nghe trên mọi địa chỉ mạng, xem bên dưới |

`10.0.2.2` là địa chỉ mà Android Emulator dùng để trỏ về máy tính chạy nó.

**Điện thoại thật:** mặc định API chỉ nghe ở `localhost`, nên điện thoại không gọi tới được.

1. Chạy API cho nghe trên mọi địa chỉ mạng (hoặc dùng task `API: chạy cho điện thoại thật (LAN)`):

   ```bash
   dotnet run --project src/Host/GymBase.API --launch-profile http --urls "http://0.0.0.0:5020"
   ```

2. Lấy IP của máy tính bằng lệnh `ipconfig`. Dùng dòng **IPv4 Address** của card Wi-Fi
   hoặc Ethernet đang dùng. Đừng dùng IP của card `vEthernet (WSL)` hay `Hyper-V`.
3. Điện thoại và máy tính phải dùng **chung một mạng Wi-Fi**.
4. Lần đầu chạy, nếu Windows Firewall hỏi thì cho phép `dotnet` truy cập mạng **Private**.
5. Thử mở `http://<IP-máy-tính>:5020/health` bằng trình duyệt trên điện thoại.

Đang phát triển với Expo Go thì gọi `http://` được. Bản build phát hành cần API chạy HTTPS.

## 6. Cho trang web gọi được API

- Gọi qua Bruno, Postman hoặc curl không cần cấu hình gì thêm.
- Trang web chạy trong trình duyệt ở origin khác (ví dụ `http://localhost:3000`) sẽ bị
  trình duyệt chặn vì **CORS chưa được cấu hình**. Việc này nằm trong mục 11 của
  `docs/api/viec-con-lai-base.md`. Khi làm, CORS phải đọc danh sách origin từ cấu hình,
  không mở cho mọi origin.

## 7. Lỗi thường gặp

| Hiện tượng | Nguyên nhân / cách xử lý |
| --- | --- |
| 404 khi gọi API | Thiếu hoặc sai tiền tố `/api/v1/web`, `/api/v1/mob` |
| `address already in use` / cổng 5020 bị chiếm | API đang chạy ở cửa sổ khác. Tắt cửa sổ đó, hoặc tìm và dừng tiến trình `GymBase.API` |
| Điện thoại không kết nối được | Chưa chạy với `--urls http://0.0.0.0:5020`, sai IP, khác mạng Wi-Fi, hoặc bị firewall chặn |
| Lỗi chứng chỉ khi gọi `https://localhost:7016` | Chạy `dotnet dev-certs https --trust` một lần, hoặc dùng profile `http` |
| `A project with an Output Type of Class Library cannot be started directly` | Startup project đang là class library. Chọn `GymBase.API` |
