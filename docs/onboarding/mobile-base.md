# Bàn giao Mobile base

## Kết quả

Mobile đặt tại `apps/mobile`, giữ nguyên root monorepo và tài liệu Word.
Stack: Expo SDK 58, React Native 0.88.1, React 19.3.0, TypeScript 6 strict,
Expo Router, TanStack Query, Axios, React Hook Form/Zod và SecureStore.
Root pin pnpm 10.34.9, có `pnpm-lock.yaml`, hoisted workspace và Node version file.

Đã tạo route khách/protected, demo session, theme sáng/tối, UI primitives,
API error mapping/refresh seam, storage adapters và feature lịch tập mẫu.
Màn hình demo không gọi backend; trạng thái hoàn thành chỉ nằm trong Query cache
và không trừ gói buổi PT. API mode chủ động chặn demo khi chưa có auth adapter thật.

## Kiểm thử

- TypeScript strict, ESLint (0 warnings), Prettier: **PASS**.
- Jest/RNTL/Expo Router: **15/15 tests PASS** (4 suites), gồm form validation, vào/thoát demo,
  cập nhật tiến trình, xóa dữ liệu phiên cũ, đổi theme, empty/error/retry,
  API status mapping, timeout/offline, concurrent refresh, retry giới hạn,
  config validation và secure storage/corrupt data.
- Router integration test dùng routes/layouts thật: guest deep-link tới profile
  được chuyển về login; vào demo tới today; logout quay về login, không rơi vào 404.
- Expo export: đã tạo bundles **Android + iOS + web** thành công.
- `expo install --check`: dependencies tương thích.
- Đã mở app trên **Pixel_6a_API_35, Android emulator**, bằng Expo Go.
- Chưa chạy thiết bị iOS, chưa build APK/IPA, chưa tích hợp backend thật.
- Workflow GitHub Actions đã viết; chưa push nên chưa có kết quả CI remote.

Ảnh chụp từ emulator, không phải mockup:

- [Màn hình vào demo](screenshots/mobile-login-android.png)
- [Màn hình lịch tập](screenshots/mobile-dashboard-android.png)

## Các file chính

| Khu vực | Thay đổi |
| --- | --- |
| Root manifest/config | Scripts mobile, packageManager, engines, .npmrc, .node-version, lockfile |
| apps/mobile/config | Expo, TypeScript, Metro, Jest, ESLint, Prettier, env example |
| apps/mobile/src/app | Router và layouts; routes chỉ gọi feature exports |
| apps/mobile/src/features | auth, pt-sessions, profile, hướng dẫn thêm module |
| apps/mobile/src/core | Config, session, API factory/errors, storage adapters |
| apps/mobile/src/shared | UI và light/dark theme |
| apps/mobile/src/providers | Composition, Query defaults, cache clear và error boundary |
| apps/mobile/__tests__ | Kiểm thử hành vi và adapter |
| .github/workflows/mobile-ci.yml | Install frozen lockfile, checks và bundle export |
| README/docs | Hướng dẫn chạy, trạng thái kiến trúc và roadmap cập nhật |

## Giới hạn và công việc tiếp theo

Đây là base mobile chạy demo, chưa phải toàn bộ base production/auth trong tài liệu.
Các phần còn chờ: backend API contract, restore phiên thật, refresh rotation/revoke,
logout-vs-refresh race, permissions, API repository, integration/E2E thật,
EAS project/signing, app identifiers và tests trên thiết bị iOS.

Không tạo modules V2/V3 rỗng, native projects hoặc Expo project ID giả.
Đọc [README Mobile](../../apps/mobile/README.md) trước khi phát triển feature mới.
Không thay đổi backend/database, không commit/push/merge trong lần thi công này.
