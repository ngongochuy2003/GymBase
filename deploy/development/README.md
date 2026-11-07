# Development

Luồng dự kiến sau MOBILE-01, API-01 và DB-01:

1. Chạy MySQL bằng cấu hình trong `infrastructure/docker/`.
2. Cấu hình backend bằng biến môi trường hoặc .NET user-secrets.
3. Áp dụng EF migration có chủ đích lên database development.
4. Chạy API tại `apps/api/` và Expo tại `apps/mobile/`.
5. Trên điện thoại thật, đặt API URL theo LAN/tunnel của máy chạy backend.

Các lệnh runtime chỉ được bổ sung sau khi có ứng dụng và kiểm chứng thực tế.
Hiện tại xem onboarding để kiểm tra khung repository.
