# GymBase Mobile

Ứng dụng Expo/React Native kết nối API ASP.NET Core thật. Không nạp dữ liệu bài viết mẫu; bảng tin, lượt thích, tạo bài và phiên đăng nhập dùng backend. Dữ liệu bài viết nằm trong MySQL; ảnh upload được backend lưu theo cấu hình hiện tại.

## Cấu hình và chạy

1. Khởi động MySQL 8.4, áp dụng migration và cấu hình JWT theo [hướng dẫn chạy API](../../docs/api/cach-chay-api.md).
2. Tạo tài khoản Admin bằng lệnh `create-admin` trong tài liệu API, hoặc dùng một tài khoản đã có trong DB.
3. Sao chép `.env.example` thành `.env`. Máy tính chạy API dùng `http://localhost:5020`; thiết bị thật cần dùng IP LAN của máy chạy API.
4. Từ repository root chạy `pnpm mobile`. Expo Go mở ứng dụng trên thiết bị/emulator.

`EXPO_PUBLIC_API_URL` là địa chỉ host gốc, không thêm `/api/v1/mob`. Không đặt mật khẩu hay token trong biến `EXPO_PUBLIC_*`.

## Kết nối hiện có

- Đăng nhập, khôi phục `/auth/me`, refresh token xoay vòng và đăng xuất.
- Bảng tin và lượt thích qua `/baiviet/feed` và `/baiviet/{id}/thich`.
- Đăng bài qua `/baiviet`; ảnh/video từ thiết bị được upload trước qua `/files`.
- Lỗi kết nối/API hiển thị trạng thái lỗi; không thay bằng dữ liệu minh họa.

Backend hiện chưa có đăng ký tài khoản công khai, quên mật khẩu hoặc Google login. Cần tạo tài khoản trong DB qua Admin trước khi đăng nhập. Các tab chưa có API nghiệp vụ như lộ trình, khám phá, tin nhắn hiện chưa được triển khai.

## Kiểm tra

```sh
pnpm mobile:check
```
