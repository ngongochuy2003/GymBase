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
| Điện thoại thật | `http://<IP-máy-tính>:5020`, API phải nghe trên mọi địa chỉ mạng: chạy task **`Dev: API + Expo (LAN)`** (xem mục "Điện thoại thật" của cach-chay-api.md) |

- `EXPO_PUBLIC_TERMS_URL` và `EXPO_PUBLIC_PRIVACY_URL` (không bắt buộc) là liên kết Điều khoản và Chính sách bảo mật ở màn đăng ký;
  để trống thì dùng URL giữ chỗ.
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

## 5. Đăng ký tài khoản mới trong app

1. Ở màn chào mừng bấm **Đăng ký tài khoản**.
2. Nhập họ và tên, email, số điện thoại di động Việt Nam (`0912 345 678`, `+84912345678`...), mật khẩu (tối thiểu 8 ký tự, không quá
   phổ biến, không chứa phần trước `@` của email), xác nhận mật khẩu, tick đồng ý điều khoản. Mã giới thiệu để trống cũng được
   (chưa tính điểm).
3. Bấm **Đăng ký**: app có phiên ngay và hiện màn **Đăng ký thành công**. **Bỏ qua, vào Trang chủ** vào tab Hôm nay; **Thiết lập hồ sơ
   ngay** mở màn giữ chỗ (phần thiết lập hồ sơ chưa làm). Màn này chỉ hiện một lần, các lần đăng nhập sau vào thẳng app.
4. Email đã dùng thì form báo "Email này đã được dùng" kèm nút **Đã có tài khoản? Đăng nhập**.

Đăng nhập Google cần bản dev build, xem mục 6.

## 6. Đăng nhập với Google (dev build, chỉ Android)

Nút **Đăng nhập với Google** dùng thư viện native nên **không chạy trong Expo Go**; app phải chạy từ một bản dev build. Trong Expo
Go nút chỉ báo "cần bản dev build". iOS chưa làm (`TODO(IOS)`).

**Làm một lần, trên Google Cloud Console** (ticket #15):

1. Tạo project, cấu hình màn hình đồng ý OAuth (OAuth consent screen).
2. Tạo OAuth client ID loại **Web application**. Giá trị này là `aud` của ID token và là client ID dùng cho cả app lẫn backend.
3. Tạo OAuth client ID loại **Android** với package name `vn.gymbase.app` (đặt ở `apps/mobile/app.json`, phải trùng) và SHA-1 của
   keystore dev (bước dưới). Client ID Android không đưa vào code, nhưng phải tồn tại để Google chấp nhận app.

**Tạo dev build và lấy SHA-1.** Cần JDK 17 và Android SDK (đã có nếu chạy được emulator):

```bash
# Tại thư mục gốc repo. Lần đầu sinh thư mục native apps/mobile/android (không commit), build rồi cài lên emulator.
pnpm mobile:android:dev
```

```powershell
# SHA-1 của keystore debug, dán vào client ID Android ở bước 3:
keytool -list -v -keystore apps\mobile\android\app\debug.keystore -alias androiddebugkey -storepass android -keypass android
```

**Khai báo client ID** (cùng một Web client ID cho cả hai nơi):

```bash
# Backend, tại apps/api
dotnet user-secrets set "Google:ClientIds:0" "<web-client-id>.apps.googleusercontent.com" --project src/Host/GymBase.API
```

```text
# apps/mobile/.env
EXPO_PUBLIC_GOOGLE_WEB_CLIENT_ID=<web-client-id>.apps.googleusercontent.com
```

Sau khi sửa `.env`, build lại hoặc chạy Metro với `-c`. Từ đó mở app dev build (không phải Expo Go), chạy API (task `Dev: API + Expo`)
và bấm **Đăng nhập với Google**: lần đầu app hiện màn "Đăng ký thành công", các lần sau vào thẳng app. Google báo `DEVELOPER_ERROR`
thường do package name hoặc SHA-1 của client ID Android không khớp bản build.

Email Google trùng email của tài khoản đã đăng ký bằng mật khẩu thì app báo "Email này đã đăng ký bằng mật khẩu"; việc liên kết
Google vào tài khoản có sẵn từ màn Cá nhân chưa làm.

## Lỗi thường gặp

| Hiện tượng | Nguyên nhân / cách xử lý |
| --- | --- |
| App báo "Không thể kết nối. Kiểm tra mạng và thử lại." | API chưa chạy, sai `EXPO_PUBLIC_API_URL`, hoặc (điện thoại thật) khác mạng / API chưa nghe trên `0.0.0.0` |
| Đăng nhập báo 404 | `EXPO_PUBLIC_API_URL` còn kèm `/api/v1/mob`, hoặc chưa chạy lại Expo với `-c` sau khi sửa `.env` |
| "Email hoặc mật khẩu không đúng." với đúng thông tin | Tài khoản chưa có email (DB chưa chạy migration), hoặc chưa tạo Admin; thử `create-admin` lại để đặt lại mật khẩu |
| Đúng mật khẩu mà vẫn báo sai sau nhiều lần thử | Đăng nhập sai từ lần thứ 5 bị khóa tạm 1 đến 15 phút (vẫn báo email hoặc mật khẩu không đúng); chờ rồi thử lại. Quá 20 lần gọi mỗi phút theo IP thì trả 429 |
| App báo `EXPO_PUBLIC_API_URL là bắt buộc` khi mở | Chưa tạo `apps/mobile/.env` |
| Đăng ký báo email đã dùng nhưng bạn chưa từng đăng ký | Email đó đã có trong DB (tài khoản mẫu hoặc Admin); dùng email khác |
| Nút Google báo "cần bản dev build" | Đang chạy trong Expo Go; build và mở bản dev build (mục 6) |
| Nút Google báo "chưa được cấu hình" | Thiếu `EXPO_PUBLIC_GOOGLE_WEB_CLIENT_ID` trong `.env` (build lại hoặc chạy Metro với `-c`) |
| `/health` trả `Unhealthy` | MySQL chưa chạy, sai connection string, hoặc chưa chạy `dotnet ef database update` |
