# Chạy app mobile với backend

Hướng dẫn từ máy mới đến lúc đăng nhập được trong app. Chi tiết về API (route, Bruno, lỗi thường gặp) ở
[cach-chay-api.md](../api/cach-chay-api.md); tổng quan mobile ở [apps/mobile/README.md](../../apps/mobile/README.md).

## Cần có

- Node 24.13.0 (hoặc 22.13 trở lên) và pnpm 10.34.9. Cài thư viện một lần ở thư mục gốc repo: `pnpm install`.
- .NET 10 SDK và MySQL 8.4 đang chạy.
- Android Studio với một Android Emulator (ví dụ Pixel 6a, API 35), hoặc Expo Go trên điện thoại thật cùng mạng Wi-Fi.

## 1. Chuẩn bị backend (làm một lần)

Chạy tại `apps/api`. Mỗi bước có giải thích đầy đủ ở [cach-chay-api.md](../api/cach-chay-api.md), mục "Chuẩn bị MySQL".

```bash
# 1. Connection string và khóa ký JWT nằm trong user-secrets, không ghi vào file.
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=gymbase;User=root;Password=<mật-khẩu>" --project src/Host/GymBase.API

# 2. Tạo hoặc cập nhật bảng bằng migration (chạy lại mỗi khi có migration mới).
dotnet ef database update --project src/Data/GymBase.Persistence --startup-project src/Host/GymBase.API
```

Khóa ký JWT (`Jwt:SigningKey`) tạo theo bước 4 của cach-chay-api.md.

Tạo tài khoản Admin để đăng nhập thử. **Đăng nhập bằng email**, mật khẩu tối thiểu 8 ký tự và không quá phổ biến:

```powershell
$env:GYMBASE_ADMIN_PASSWORD = Read-Host "Mật khẩu Admin"
dotnet run --project src/Host/GymBase.API --launch-profile http -- create-admin admin@gymbase.local
Remove-Item Env:GYMBASE_ADMIN_PASSWORD
```

Nếu DB cũ có tài khoản mẫu, migration `EmailLaDinhDanhDangNhap` đã cấp email dạng `<tên-đăng-nhập>@gymbase.local` cho các
tài khoản chưa có email. Tài khoản mẫu không có mật khẩu thật nên không đăng nhập được; dùng tài khoản Admin vừa tạo.

## 2. Cấu hình app

Tạo `apps/mobile/.env` từ `.env.example`:

```text
EXPO_PUBLIC_DATA_MODE=demo
EXPO_PUBLIC_API_URL=http://10.0.2.2:5020
```

- `EXPO_PUBLIC_API_URL` là địa chỉ **host của API, không kèm** `/api/v1/mob`: app tự thêm tiền tố đó. Để thừa tiền tố
  thì mọi request bị 404.
- Địa chỉ theo nơi app chạy:

| App chạy trên | `EXPO_PUBLIC_API_URL` |
| --- | --- |
| Android Emulator | `http://10.0.2.2:5020` |
| Trình duyệt (Expo web), iOS Simulator | `http://localhost:5020` |
| Điện thoại thật | `http://<IP-máy-tính>:5020`, cần chạy API theo mục "Điện thoại thật" của cach-chay-api.md |

- `EXPO_PUBLIC_DATA_MODE=demo` chỉ quyết định nguồn dữ liệu **lịch tập** (dữ liệu mẫu, chưa có API lịch tập). Đăng nhập luôn
  gọi API thật.
- Biến `EXPO_PUBLIC_*` được nhúng lúc build. Đổi `.env` xong phải chạy lại Expo với cache sạch (`pnpm --filter @gymbase/mobile start -c`, hoặc task
  `Mobile: chạy Expo (xóa cache)`), nếu không app vẫn dùng giá trị cũ.

## 3. Chạy

Cách nhanh nhất trong VS Code: mở thư mục gốc repo, **Tasks: Run Task**, chọn **`Dev: API + Expo`**. Task build API rồi chạy
song song API (cổng 5020) và Expo (cổng 8081).

Hoặc chạy bằng hai terminal:

```bash
# Terminal 1, tại apps/api
dotnet run --project src/Host/GymBase.API --launch-profile http

# Terminal 2, tại thư mục gốc repo
pnpm mobile:android      # emulator Android đã khởi động
# hoặc: pnpm mobile      # rồi nhấn a (Android), w (web) hoặc quét QR bằng Expo Go
```

Kiểm tra API trước: mở `http://localhost:5020/health`, thấy `Healthy` là API đã nối được MySQL.

## 4. Đăng nhập thử

1. Mở app, thấy màn chào mừng, bấm **Đăng nhập**.
2. Nhập email Admin và mật khẩu vừa đặt. Vào được tab **Hôm nay** là thành công.
3. Tab **Cá nhân** có tên và email của tài khoản; **Đăng xuất** thu hồi phiên và về màn chào mừng.
4. Đóng hẳn app rồi mở lại: app khôi phục phiên, không hỏi đăng nhập.

Màn đăng ký trong app và đăng nhập Google chưa nối. Muốn có thêm tài khoản thường để thử thì gọi API đăng ký bằng Bruno
(request **Đăng ký** trong `Mobile (/api/v1/mob)` → `Xác thực`), rồi đăng nhập bằng email đó trong app.

## Lỗi thường gặp

| Hiện tượng | Nguyên nhân / cách xử lý |
| --- | --- |
| App báo "Không thể kết nối. Kiểm tra mạng và thử lại." | API chưa chạy, sai `EXPO_PUBLIC_API_URL`, hoặc (điện thoại thật) khác mạng / API chưa nghe trên `0.0.0.0` |
| Đăng nhập báo 404 | `EXPO_PUBLIC_API_URL` còn kèm `/api/v1/mob`, hoặc chưa chạy lại Expo với `-c` sau khi sửa `.env` |
| "Email hoặc mật khẩu không đúng." với đúng thông tin | Tài khoản chưa có email (DB chưa chạy migration), hoặc chưa tạo Admin; thử `create-admin` lại để đặt lại mật khẩu |
| Đúng mật khẩu mà vẫn báo sai sau nhiều lần thử | Đăng nhập sai từ lần thứ 5 bị khóa tạm 1 đến 15 phút (vẫn báo email hoặc mật khẩu không đúng); chờ rồi thử lại. Quá 20 lần gọi mỗi phút theo IP thì trả 429 |
| App báo `EXPO_PUBLIC_API_URL là bắt buộc` khi mở | Chưa tạo `apps/mobile/.env` |
| `/health` trả `Unhealthy` | MySQL chưa chạy, sai connection string, hoặc chưa chạy `dotnet ef database update` |
