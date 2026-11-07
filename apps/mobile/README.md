# GymBase Mobile

Base Expo/React Native theo kiến trúc feature-first. Chạy độc lập với backend bằng
dữ liệu demo rõ ràng; không tạo tài khoản hoặc xác nhận buổi PT tính phí thật.

## Chạy từ repository root

Yêu cầu Node 24.13.0 (hoặc Node 22.13+), pnpm 10.34.9.
Không cần cài pnpm global nếu dùng các lệnh npx dưới đây.

```sh
npx --yes pnpm@10.34.9 install --frozen-lockfile
npx --yes pnpm@10.34.9 mobile
```

Trên Windows PowerShell dùng `npx.cmd` nếu execution policy chặn script npm.
Mở Expo Go tương thích SDK 58, quét QR trên cùng mạng với máy phát triển.

```sh
# Emulator Android đã khởi động, Android SDK / adb đã cài:
npx --yes pnpm@10.34.9 mobile:android

# Preview cùng ứng dụng React Native trên browser:
npx --yes pnpm@10.34.9 mobile:web

# iOS simulator: cần macOS + Xcode
npx --yes pnpm@10.34.9 --filter @gymbase/mobile ios
```

Màn hình mở đầu: nhập tên ít nhất 2 ký tự → **Khám phá bản demo**.
Tab **Hôm nay**: lịch tập mẫu, đánh dấu/bỏ đánh dấu đã tập để cập nhật tiến trình.
Tab **Cá nhân**: đổi giao diện sáng/tối hoặc thoát demo.
Phiên demo không lưu tài khoản/token; reload hoặc thoát sẽ bắt đầu lại.

## Kiến trúc hiện có

```text
src/
  app/                       # Expo Router, thin routes, guest/protected layouts
  features/
    auth/                    # Demo entry form: RHF + Zod
    pt-sessions/             # Typed repository → query hook → screen/components
    profile/                 # Theme settings / exit demo
  core/
    api/                     # Axios factory, safe error mapping, refresh seam
    auth/                    # Ephemeral demo session, bootstrap/loading state
    config/                  # Explicit EXPO_PUBLIC env access + Zod validation
    storage/                 # Native SecureStore; web memory-only adapter
  shared/
    components/              # Screen, Card, Button, TextField, loading/empty/error
    theme/                   # Light/dark tokens and provider
  providers/                 # Query, Session, Theme, Safe Area, error boundary
__tests__/                   # Form/demo flow, API, config/storage
```

Chiều phụ thuộc: `app → features → core/shared`. Alias `@/` trỏ vào `src/`.
Feature chỉ export public entry point qua `index.ts`; không import vòng.
Route không gọi HTTP. Shared/core không chứa logic tính phí, trừ gói hoặc
authorization tin cậy. Xem [mẫu mở rộng feature](src/features/README.md).

V1 sẽ bổ sung `trainers`, `trainees`, `workouts`, `session-packages` theo use case.
V2/V3 thêm feature mới và route mỏng, giữ repository contract, không sửa toàn bộ base.
Chưa cần Zustand vì context xử lý state phiên/theme, Query quản lý dữ liệu server.

## API và môi trường

Demo chạy không cần `.env`. Khi cần cấu hình, copy `.env.example` trong thư mục này
sang `.env`; Expo đọc `EXPO_PUBLIC_DATA_MODE` và `EXPO_PUBLIC_API_URL`.
Không có secret trong biến `EXPO_PUBLIC_*`.

`demo` là chế độ mặc định. `api` yêu cầu URL hợp lệ và **không cho vào demo**;
chưa có login/backend adapter thật. Đừng bật API mode rồi coi auth đã hoạt động.

Khi backend sẵn sàng:

1. Chốt OpenAPI, login/refresh/logout, DTO/error contract và authorization.
2. Implement auth adapter: restore/verify session, rotation/revocation, xử lý
   logout trong khi refresh còn pending. Thay demo session bằng adapter đó.
3. Dùng `createApiClient` với `AuthTransport`. Callback refresh phải dùng transport
   riêng (không tự gọi client đang intercept 401), lưu token mới trước khi trả về.
4. Inject API implementation của `SessionsRepository` và query keys theo user/filter.
5. Thay thao tác đánh dấu demo bằng mutation thật, invalidate query sau response;
   backend quyết định buổi tính phí và số dư gói.
6. Kiểm tra E2E với backend thật, auth concurrency và permissions trước phát hành.

Native token adapter dùng SecureStore, web preview chỉ dùng memory; không lưu
token ở AsyncStorage/localStorage. Lớp lưu trữ hiện được kiểm thử bằng native mock.
HTTP client đã test single-flight refresh/retry một lần; chưa nối endpoint auth thật.

Trên điện thoại thật, `localhost` là điện thoại. Dùng LAN/HTTPS backend URL phù hợp.
Không đưa Expo dev server vào Docker.

## Kiểm tra

```sh
npx --yes pnpm@10.34.9 mobile:check
npx --yes pnpm@10.34.9 --filter @gymbase/mobile format:check
npx --yes pnpm@10.34.9 --filter @gymbase/mobile exec expo install --check
npx --yes pnpm@10.34.9 --filter @gymbase/mobile export
```

Export tạo JS/Hermes bundles cho Android/iOS/web; không phải APK/IPA hoặc xác nhận
đã chạy trên iOS. `dist/`, `.expo/`, `node_modules/` được Git ignore.
Workflow `.github/workflows/mobile-ci.yml` chạy lint/typecheck/tests/format/export.

Xem [báo cáo kiểm thử và ảnh Android](../../docs/onboarding/mobile-base.md).
EAS/signing chưa cấu hình vì chưa có Expo project ID, identifier và credentials
của chủ dự án. Không tự submit store hoặc deploy production.

## Nguồn kỹ thuật đã đối chiếu

- [Expo monorepo](https://docs.expo.dev/guides/monorepos/)
- [Expo unit testing](https://docs.expo.dev/develop/unit-testing/)
- [Expo Router](https://docs.expo.dev/router/introduction/)
