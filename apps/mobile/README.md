# GymBase Mobile

Base Expo/React Native theo kiến trúc feature-first. Đăng nhập và bảng tin gọi backend thật
(`/api/v1/mob`); app không còn dữ liệu demo.

## Cấu hình và chạy

1. Khởi động MySQL 8.4, áp dụng migration và cấu hình JWT theo [hướng dẫn chạy API](../../docs/api/cach-chay-api.md).
2. Đăng ký tài khoản thường, rồi đổi `vai_tro` thành `Admin` trong DB nếu cần thử quyền quản trị (xem [hướng dẫn chạy API](../../docs/api/cach-chay-api.md)).
3. Sao chép `.env.example` thành `.env`. Máy tính chạy API dùng `http://localhost:5020`; thiết bị thật cần dùng IP LAN của máy chạy API.
4. Từ repository root chạy `pnpm mobile`. Expo Go mở ứng dụng trên thiết bị/emulator.

`EXPO_PUBLIC_API_URL` là địa chỉ host gốc, không thêm `/api/v1/mob`. Không đặt mật khẩu hay token trong biến `EXPO_PUBLIC_*`.

## Kết nối hiện có

- Đăng nhập, khôi phục `/auth/me`, refresh token xoay vòng và đăng xuất.
- Bảng tin và lượt thích qua `/baiviet/feed` và `/baiviet/{id}/thich`.
- Đăng bài qua `/baiviet`; ảnh/video từ thiết bị được upload trước qua `/files`.
- Lỗi kết nối/API hiển thị trạng thái lỗi; không thay bằng dữ liệu minh họa.

# Preview cùng ứng dụng React Native trên browser:
npx --yes pnpm@10.34.9 mobile:web

# iOS simulator: cần macOS + Xcode
npx --yes pnpm@10.34.9 --filter @gymbase/mobile ios
```

Cần backend đang chạy ([cách chạy API](../../docs/api/cach-chay-api.md)) và `EXPO_PUBLIC_API_URL` trong `.env`
(sao chép từ `.env.example`; emulator Android dùng `http://10.0.2.2:5020`).

Màn hình mở đầu: **Chào mừng** → **Đăng nhập** bằng email và mật khẩu (tài khoản đã đăng ký, có thể được cấp `Admin` trong DB).
Phiên được lưu bằng SecureStore, mở lại app tự khôi phục; token tự gia hạn, hết phiên thì về màn chào mừng.
**Đăng ký tài khoản** (họ tên, email, mật khẩu, tick điều khoản; số điện thoại di động Việt Nam là tùy chọn) không có phiên: app về màn đăng nhập
với email điền sẵn. Lần đăng nhập đầu tiên hiện màn onboarding cho tới khi bấm "Thiết lập hồ sơ ngay" hoặc
"Bỏ qua, vào Trang chủ" (cờ `is_dang_nhap_lan_dau` lưu ở server). "Thiết lập hồ sơ ngay" đi qua chọn vai trò, thông tin
cá nhân và thông tin theo vai trò; lưu xong vào Trang chủ.
Ô mã giới thiệu có trên form nhưng chưa tính điểm. **Đăng nhập với Google** (Android) cần bản dev build, không chạy trong Expo Go: xem
[chay-mobile.md](../../docs/onboarding/chay-mobile.md) mục 6.
Trang chủ là bảng tin; bấm ảnh đại diện hoặc tab **Cá nhân** để đổi giao diện sáng/tối hoặc đăng xuất.

## Kiến trúc hiện có

```text
src/
  app/                       # Expo Router, thin routes, guest/protected layouts
  features/
    auth/                    # Màn chào mừng, đăng ký, đăng ký thành công, đăng nhập: RHF + Zod
    profile/                 # Cá nhân (giao diện, đăng xuất) và thiết lập hồ sơ nhiều bước
  core/
    api/                     # Axios factory, safe error mapping, API xác thực
    auth/                    # Phiên thật (SecureStore), refresh token, trạng thái loading/guest/authenticated
    config/                  # Explicit EXPO_PUBLIC env access + Zod validation
    storage/                 # Native SecureStore; web memory-only adapter
  shared/
    components/              # Screen, Card, Button, TextField, loading/empty/error
    theme/                   # Light/dark tokens and provider
  providers/                 # Query, Session, Theme, Safe Area, error boundary
__tests__/                   # Luồng form và điều hướng, API, config/storage
```

Chiều phụ thuộc: `app → features → core/shared`. Alias `@/` trỏ vào `src/`.
Feature chỉ export public entry point qua `index.ts`; không import vòng.
Route không gọi HTTP. Shared/core không chứa logic tính phí, trừ gói hoặc
authorization tin cậy. Xem [mẫu mở rộng feature](src/features/README.md).

V1 sẽ bổ sung `trainers`, `trainees`, `workouts`, `session-packages` theo use case.
V2/V3 thêm feature mới và route mỏng, giữ repository contract, không sửa toàn bộ base.
Chưa cần Zustand vì context xử lý state phiên/theme, Query quản lý dữ liệu server.

## API và môi trường

Copy `.env.example` trong thư mục này sang `.env`; Expo đọc `EXPO_PUBLIC_API_URL` (bắt buộc) và các biến `EXPO_PUBLIC_*` khác.
Không có secret trong biến `EXPO_PUBLIC_*`. App luôn gọi backend thật, không có chế độ demo: cần chạy API trước
(xem [hướng dẫn chạy API](../../docs/api/cach-chay-api.md)).

Dùng `createApiClient` với `AuthTransport`. Callback refresh phải dùng transport riêng (không tự gọi client đang
intercept 401), lưu token mới trước khi trả về.

Native token adapter dùng SecureStore, web preview chỉ dùng memory; không lưu
token ở AsyncStorage/localStorage. Lớp lưu trữ hiện được kiểm thử bằng native mock.
HTTP client đã test single-flight refresh/retry một lần; chưa nối endpoint auth thật.

Trên điện thoại thật, `localhost` là điện thoại. Dùng LAN/HTTPS backend URL phù hợp.
Không đưa Expo dev server vào Docker.

## Kiểm tra

```sh
pnpm mobile:check
```
