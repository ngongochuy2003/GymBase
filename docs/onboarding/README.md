# Onboarding

## Hiện trạng

Mobile đã chạy được; xem [hướng dẫn chạy](../../apps/mobile/README.md) và
[bàn giao Mobile](mobile-base.md). Backend/DB chưa được triển khai.
Phần kiểm tra REPO-01 dưới đây chỉ kiểm tra cấu trúc, không cần Docker hoặc .NET.

Windows PowerShell:

```powershell
Set-Location D:\Project\GymBase-Code\GymBase
node scripts/check-repo.mjs
```

macOS/Linux (thay đường dẫn bằng nơi clone):

```sh
cd /path/to/GymBase
node scripts/check-repo.mjs
```

Nếu đã có pnpm: `pnpm check:repo` là lệnh tương đương.
`pnpm -r list --depth -1` hiện có `@gymbase/mobile`.
Chưa có .NET project hoặc Docker Compose để chạy backend.

## Nhận việc

1. Đọc root README, kiến trúc và README của khu vực phụ trách.
2. Thống nhất nhánh nền với leader. Sau khi base đã merge vào `develop`, dùng
   workflow dưới đây. Không coi nhánh đang dựng base là nhánh ổn định cho team.
3. Phát triển Mobile tại `apps/mobile`; khởi tạo backend tại `apps/api`, giữ README;
   không tạo thêm một thư mục ứng dụng bọc bên trong.
4. Commit dependencies/lockfile và scripts thật cùng task khởi tạo ứng dụng.
5. Cập nhật hướng dẫn chạy, test và env sau khi kiểm chứng trên môi trường local.

```sh
git fetch origin
git switch develop
git pull --ff-only origin develop
git switch -c feat/ten-chuc-nang
```

Quy ước nhánh: `feat/*`, `fix/*`, `chore/*`; PR vào `develop` theo quy trình nhóm.
Riêng task base, tài liệu đề xuất `chore/monorepo-base` từ `develop`.
CODEOWNERS hiện chỉ có ví dụ comment; leader cấu hình reviewers và branch
protection thật trên GitHub. Workflow Mobile đã có, leader kiểm tra lần chạy đầu
trên GitHub rồi cấu hình required checks.

## Môi trường ở bước tiếp theo

Mobile đã pin phiên bản và có lockfile; setup Android/iOS theo README Mobile.
API dùng .NET 10 SDK (ghim ở `apps/api/global.json`); DB-01 chốt MySQL và Docker Compose.
Cách chạy API: `apps/api/1.Doc/cach-chay-api.md`.
Sau khi có cấu hình runtime, copy env example vào đúng phạm vi app/service.
Các file `.env` bị Git ignore; không lưu credential vào appsettings được commit.
Biến public trong Expo không được chứa secret.
