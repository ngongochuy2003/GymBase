# Bàn giao Mobile

## Trạng thái hiện tại

Mobile tại `apps/mobile` dùng Expo SDK 58, React Native 0.88.1, React 19.3.0, TypeScript 6 strict, Expo Router, TanStack Query, Axios, React Hook Form/Zod và SecureStore.

Ứng dụng chạy với API thật, không nạp bài viết hoặc tài khoản mẫu ở runtime. Đã nối đăng nhập, khôi phục phiên qua `/auth/me`, refresh token, đăng xuất, tải feed, thích bài và tạo bài. Ảnh/video từ thiết bị được tải lên backend trước khi tạo bài. Bài viết và tương tác được lưu qua backend/MySQL; file dùng cấu hình lưu trữ backend.

## Kiểm tra

- `pnpm mobile:check`: lint, typecheck và 33 test trong 7 suites đều pass.
- `dotnet test GymBase.slnx`: architecture 66, integration 72 và unit 188 đều pass (chạy với `-p:NuGetAudit=false` khi NuGet audit không truy cập được).
- Expo web đã chạy được tại `http://localhost:8082`; app hiển thị màn hình đăng nhập.
- Chưa xác minh login bằng tài khoản người dùng cụ thể, thiết bị iOS hoặc build APK/IPA.

## Cấu hình chạy

1. Khởi động MySQL 8.4 và áp dụng migrations.
2. Đặt connection string và khóa JWT bằng User Secrets hoặc environment variables.
3. Chạy API trên cổng 5020 và cấu hình CORS cho nguồn Expo web.
4. Đặt `EXPO_PUBLIC_API_URL` trong `apps/mobile/.env` thành `http://localhost:5020`. Trên thiết bị thật, dùng IP LAN của máy phát triển.
5. Tạo tài khoản Admin qua lệnh `create-admin` hoặc dùng một tài khoản có sẵn trong database rồi đăng nhập trong app.

## Giới hạn

Backend chưa có đăng ký công khai, khôi phục mật khẩu qua email hoặc Google login. Tab lộ trình, khám phá và tin nhắn chưa có API nghiệp vụ. Bản web dùng SecureStore memory adapter nên phiên không tồn tại sau khi reload; native dùng SecureStore.

EAS project/signing, app identifiers và kiểm thử thiết bị iOS chưa cấu hình. Không tạo modules V2/V3 rỗng hoặc Expo project ID giả.
