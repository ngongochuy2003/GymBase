# Onboarding

## Hiện trạng

Repo hiện có cấu trúc thư mục và hướng dẫn, chưa có ứng dụng chạy được.
Không cần Docker, .NET SDK hoặc pnpm để kiểm tra REPO-01; cần Node.js.

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

Nếu đã có pnpm: `pnpm check:repo` là lệnh tương đương;
`pnpm -r list --depth -1` chưa liệt kê package ứng dụng nào là đúng ở bước này.
Không chạy `dotnet build`, Expo start hoặc Docker Compose trước khi scaffold.

## Nhận việc

1. Đọc root README, kiến trúc và README của khu vực phụ trách.
2. Thống nhất nhánh nền với leader. Sau khi base đã merge vào `develop`, dùng
   workflow dưới đây. Không coi nhánh đang dựng base là nhánh ổn định cho team.
3. Khởi tạo app tại đúng `apps/mobile` hoặc `apps/api`, giữ README hiện có;
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
protection thật trên GitHub. Chưa có CI bắt buộc đến khi hoàn thành CI-01.

## Môi trường ở bước tiếp theo

MOBILE-01 chốt Node/pnpm/Expo tương thích và setup Android/iOS theo môi trường
thành viên. API-01 chốt .NET 9 SDK; DB-01 chốt MySQL và Docker Compose.
Sau khi có cấu hình runtime, copy env example vào đúng phạm vi app/service.
Các file `.env` bị Git ignore; không lưu credential vào appsettings được commit.
Biến public trong Expo không được chứa secret.
