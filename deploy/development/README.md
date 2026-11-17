# Development

Luồng local:

1. Tạo `.env` ở gốc repo từ `.env.example`.
2. Chạy MySQL và API bằng Docker (`infrastructure/docker/compose.dev.yml`, lệnh trong `infrastructure/docker/README.md`):
   compose tự áp EF migration rồi mới khởi động API. Hoặc chạy ngoài Docker theo `docs/api/cach-chay-api.md`
   (cấu hình bằng .NET user-secrets, áp migration có chủ đích).
3. Chạy Expo tại `apps/mobile/` (ngoài Docker).
4. Trên điện thoại thật, đặt API URL theo LAN/tunnel của máy chạy backend.
