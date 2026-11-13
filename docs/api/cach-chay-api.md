# Cách chạy API cho web và mobile

Backend chỉ có **một host**: `GymBase.API` (`apps/api/src/Host/GymBase.API`), nghe ở cổng
**5020** (profile `http`). Web và mobile dùng chung host, khác nhau ở **tiền tố route**, do
`ChannelRouteConvention` (`GymBase.Base.Hosting`) gắn theo thư mục của controller:

| Kênh | Controller đặt ở | Tiền tố | Ví dụ |
| --- | --- | --- | --- |
| Web (trang quản trị) | `Controllers/` | `/api/v1/web` | `GET http://localhost:5020/api/v1/web/users` |
| Mobile | `Mob/Controllers/` | `/api/v1/mob` | `GET http://localhost:5020/api/v1/mob/users` |

Gọi thiếu tiền tố hoặc sai tiền tố sẽ nhận **404**. `/health` và `/openapi/v1.json` không có tiền tố.

## Chuẩn bị MySQL (làm một lần)

API cần MySQL mới khởi động được. Dự án chốt MySQL 8.4 LTS. Chạy các lệnh sau tại `apps/api`.

1. Đặt connection string vào user-secrets (không ghi vào `appsettings.json`), thay thông tin của bạn:

   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=gymbase;User=root;Password=<mật-khẩu>" --project src/Host/GymBase.API
   ```

2. Tạo database và bảng bằng migration:

   ```bash
   dotnet ef database update --project src/Data/GymBase.Persistence --startup-project src/Host/GymBase.API
   ```

   Database dev tạo từ các migration cũ (`InitialCreate`, `AddPhienDangNhap`, `AddSystemLog`, `ChangeGuidToVarchar36`)
   không cập nhật tiếp được, vì các migration đó đã được thay bằng `InitialBase` và `AddBizTables`. Xóa database đó
   (`dotnet ef database drop`) rồi tạo lại.

3. Nạp dữ liệu mẫu nếu cần: chạy `database/seeds/dev_tai_khoan.sql` trên database `gymbase`. Các tài khoản mẫu không
   đăng nhập được vì không có mật khẩu thật.

4. Tạo khóa ký JWT (chuỗi ngẫu nhiên, mỗi môi trường một khóa, không dùng chung):

   ```powershell
   $bytes = New-Object byte[] 48; [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
   dotnet user-secrets set "Jwt:SigningKey" ([Convert]::ToBase64String($bytes)) --project src/Host/GymBase.API
   ```

5. Tạo tài khoản Admin, đăng nhập bằng email. Mật khẩu lấy từ biến môi trường để không nằm trong lịch sử lệnh. Email đã
   tồn tại thì đặt lại mật khẩu và chuyển thành Admin:

   ```powershell
   $env:GYMBASE_ADMIN_PASSWORD = Read-Host "Mật khẩu Admin"
   dotnet run --project src/Host/GymBase.API --launch-profile http -- create-admin admin@gymbase.local
   Remove-Item Env:GYMBASE_ADMIN_PASSWORD
   ```

6. Đăng nhập Google (tùy chọn, khi làm app nối Google): khai báo client ID, một hoặc nhiều giá trị. Chưa khai báo thì
   `POST /auth/google` luôn trả 401 `AUTH_GOOGLE_TOKEN_INVALID`:

   ```bash
   dotnet user-secrets set "Google:ClientIds:0" "<client-id-web>.apps.googleusercontent.com" --project src/Host/GymBase.API
   ```

Môi trường thật đặt khóa ký bằng biến `Jwt__SigningKey` và client ID Google bằng `Google__ClientIds__0`. Đổi khóa thì mọi access token cũ hết hiệu lực ngay.

Khi đổi entity, tạo migration mới (đặt tên rõ theo nội dung thay đổi) rồi lặp lại bước 2:

```bash
dotnet ef migrations add <TenThayDoi> --project src/Data/GymBase.Persistence --startup-project src/Host/GymBase.API
```

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
| `API: chạy cho điện thoại thật (LAN)` | Như trên nhưng nghe trên mọi địa chỉ mạng (xem mục 5) |
| `API: build` | Build solution. Phím tắt `Ctrl+Shift+B` |
| `API: test` | Chạy toàn bộ test (Unit, Integration, Architecture) |
| **`Dev: API + Expo`** | Build API, rồi chạy song song `GymBase.API` (5020) và Expo (8081). Dùng khi làm app mobile |
| **`Dev: API + Expo (LAN)`** | Như `Dev: API + Expo` nhưng API nghe `0.0.0.0:5020`, dùng cho điện thoại thật (mục 5). Chỉ bật khi cần, vì mọi máy cùng mạng gọi được API |
| `Mobile: chạy Expo` | `pnpm mobile`. Trong terminal của task nhấn `a` (Android), `w` (web), `r` (reload) |
| `Mobile: chạy Expo (xóa cache)` | Như trên kèm `-c`, dùng khi app hiển thị code cũ hoặc lỗi bundle lạ |
| `Mobile: kiểm tra (lint + typecheck + test)` | `pnpm mobile:check` |

App mobile cần `apps/mobile/.env` trỏ tới API (xem mục 5 và [chay-mobile.md](../onboarding/chay-mobile.md)). Muốn dừng API thì bấm vào terminal của task rồi nhấn Ctrl+C,
hoặc chạy **Tasks: Terminate Task**.

## 3. Chạy bằng dòng lệnh

Tại `apps/api`:

```bash
dotnet run --project src/Host/GymBase.API --launch-profile http
```

Dừng bằng Ctrl+C.

## 4. Kiểm tra đã chạy

- Mở `http://localhost:5020/health`, thấy `Healthy` là API đã kết nối được MySQL.
- **Bruno:** mở [Bruno](https://www.usebruno.com/), chọn **Open Collection**, trỏ tới thư mục
  `docs/api/bruno`, chọn environment **local** ở góc trên bên phải. Collection chia thành `Hệ thống`,
  `Web (/api/v1/web)` và `Mobile (/api/v1/mob)`; trong mỗi kênh có thư mục theo chức năng (`Xác thực`,
  `Người dùng`). Đặt mật khẩu Admin vào biến bí mật `adminPassword` của environment, rồi chạy **Đăng nhập**
  trước khi gọi các request khác. Mỗi request đã có sẵn assert mã trả về. Chạy cả collection bằng **Run** để
  kiểm tra nhanh; các endpoint xác thực giới hạn 20 lần mỗi phút, nên chờ một phút rồi mới chạy lại.
- **Postman:** **Import** URL `http://localhost:5020/openapi/v1.json` để tạo collection tự động.
  Trang OpenAPI chỉ có ở môi trường Development.

Id người dùng mẫu (sau khi nạp seed): `0193f0a0-0000-7000-8000-000000000001` (admin).

## 5. Cho app mobile gọi được API

Cấu hình `apps/mobile/.env` (xem `.env.example`). `EXPO_PUBLIC_API_URL` là địa chỉ **host của API, không kèm** tiền tố
`/api/v1/mob` (app tự thêm). Đăng nhập luôn gọi API thật; `EXPO_PUBLIC_DATA_MODE=demo` chỉ chọn dữ liệu mẫu cho lịch tập.
Hướng dẫn từng bước từ đầu: [chay-mobile.md](../onboarding/chay-mobile.md).

```text
EXPO_PUBLIC_DATA_MODE=demo
EXPO_PUBLIC_API_URL=<địa chỉ ở bảng dưới>
```

| App chạy trên | `EXPO_PUBLIC_API_URL` | Cách chạy API |
| --- | --- | --- |
| Trình duyệt (Expo web), iOS Simulator | `http://localhost:5020` | Như mục 1–3 |
| Android Emulator | `http://10.0.2.2:5020` | Như mục 1–3 |
| Điện thoại thật | `http://<IP-máy-tính>:5020` | Phải lắng nghe trên mọi địa chỉ mạng, xem bên dưới |

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
- Trang web chạy trong trình duyệt phải có origin nằm trong cấu hình `Cors:AllowedOrigins`. Khi dev,
  `appsettings.Development.json` đã có `http://localhost:3000` và `http://localhost:8081` (Expo web).
- Môi trường thật đặt bằng biến môi trường, mỗi origin một biến: `Cors__AllowedOrigins__0`,
  `Cors__AllowedOrigins__1`… Không khai báo thì trình duyệt không gọi được từ origin khác.
- Không dùng `*`: API sẽ báo lỗi ngay khi khởi động.

## 7. Lỗi thường gặp

| Hiện tượng | Nguyên nhân / cách xử lý |
| --- | --- |
| API báo `Chưa cấu hình ConnectionStrings:DefaultConnection` khi khởi động | Chưa đặt user-secrets, xem phần Chuẩn bị MySQL. User-secrets chỉ được đọc ở môi trường Development |
| API báo `Chưa cấu hình Jwt:SigningKey` khi khởi động | Chưa tạo khóa ký, xem phần Chuẩn bị MySQL bước 4 |
| Mọi request trả 401, kể cả sai đường dẫn | Chưa gửi token. Đăng nhập qua `POST /auth/login` rồi gửi header `Authorization: Bearer <access_token>` |
| Gọi `/users` trả 403 | API này chỉ dành cho `Admin` |
| Đăng nhập trả 429 | Vượt 20 lần gọi mỗi phút, chờ một phút |
| `/health` trả `Unhealthy` | MySQL chưa chạy, sai thông tin kết nối, hoặc chưa tạo database |
| 404 khi gọi API | Thiếu hoặc sai tiền tố `/api/v1/web`, `/api/v1/mob` |
| `address already in use` / cổng 5020 bị chiếm | API đang chạy ở cửa sổ khác. Tắt cửa sổ đó, hoặc tìm và dừng tiến trình `GymBase.API` |
| Điện thoại không kết nối được | Chưa chạy với `--urls http://0.0.0.0:5020`, sai IP, khác mạng Wi-Fi, hoặc bị firewall chặn |
| Lỗi chứng chỉ khi gọi `https://localhost:7016` | Chạy `dotnet dev-certs https --trust` một lần, hoặc dùng profile `http` |
| `A project with an Output Type of Class Library cannot be started directly` | Startup project đang là class library. Chọn `GymBase.API` |
