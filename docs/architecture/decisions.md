# Quyết định kiến trúc và các điểm còn mở

## Cập nhật — Nâng lên .NET 10 (08/11/2026)

Backend chuyển từ .NET 9 sang **.NET 10 LTS** vì .NET 9 hết hỗ trợ ngày 10/11/2026.
Mọi project target `net10.0`; `apps/api/global.json` ghim SDK `10.0.100` với
roll-forward `latestFeature` (máy dev đang dùng SDK 10.0.402). Gói
`Microsoft.AspNetCore.OpenApi` và `Microsoft.Extensions.Hosting` lên `10.0.14`.
OpenAPI do .NET 10 sinh ra là phiên bản 3.1.

## Cập nhật — Entity gốc (08/11/2026)

Tên bảng/cột dùng tiếng Việt không dấu snake_case; property C# của entity và DTO trùng
tên cột (ví dụ `tai_khoan`, `ho_ten`, `ngay_tao`, `nguoi_tao`, `ngay_chinh_sua`,
`nguoi_chinh_sua`). Riêng khóa chính giữ tên `Id`.
Khóa chính là UUIDv7 (`Guid.CreateVersion7()`). Mọi mốc thời gian lưu ở UTC.
Xóa mềm bằng cờ `is_deleted` (property trùng tên cột).
Tách `BaseEntity` (chỉ Id) và `BaseAuditableEntity` (thêm 4 trường audit) vì
bảng nối, log và lịch sử không cần audit; xóa mềm là interface `ISoftDelete` riêng.
Mã nguồn: `apps/api/99.AMISBASE/GymBase.AMISBase.DL.Contract/Entities`.

## Cập nhật — API base

Theo yêu cầu trực tiếp của chủ repo, `apps/api` dùng cấu trúc Management / AMISBASE
thay cho modular monolith `GymBase.Modules` ghi trước đó. Mỗi nhóm (DI = danh mục,
Biz = nghiệp vụ) có 4 project: API (controller), BL (logic, không query DB),
DL.Contract, DL.Mysql (SQL raw). `Mob/GymBase.MobAPI` là API duy nhất cho mobile,
gọi lại BL của DI/Biz. AMISBASE chứa nền CRUD dùng chung theo cùng 4 tầng.
SDK ban đầu ghim .NET 9, sau đó đã nâng lên .NET 10 (xem mục trên).
Chi tiết: `apps/api/README.md`.

## Cập nhật — Mobile base

Người dùng đã yêu cầu triển khai Mobile tiếp sau REPO-01. Dùng Expo SDK 58,
React Native 0.88.1, React 19.3.0 theo template Expo đã kiểm chứng từ npm.
Root pin pnpm 10.34.9, Node tham chiếu 24.13.0; Node 22.13+ cũng tương thích.
Có lockfile, Router, demo feature, TanStack Query, RHF/Zod, Axios, SecureStore,
ESLint/Prettier và Jest/RNTL. Metro dùng cấu hình Expo mặc định cho monorepo;
pnpm hoisted tránh trùng React/native dependencies.

Demo session chỉ nằm trong memory và không tạo token. API mode không cho vào
demo; đăng nhập/refresh thật phải nối backend adapter trước khi dùng production.
Không thêm Zustand khi state hiện tại chỉ cần context và Query cache.
Chưa cấu hình EAS project ID, signing hoặc app identifier giả.
Các ADR REPO-01 bên dưới ghi lại trạng thái lịch sử của bước dựng repo.

## ADR-001 — Root monorepo

Chọn repository hiện tại làm root; `apps`, `packages`, `database`, `infrastructure`,
`deploy`, `docs`, `scripts`, `.github` nằm cùng cấp `GymBase.docx`.
Không tạo thêm thư mục `gymbase/` bọc ngoài hoặc Git repo lồng.

## ADR-002 — Phạm vi REPO-01

Theo mục 10 của `GymBase.docx`, chỉ dựng khung và cấu hình root, chưa scaffold
ứng dụng hoặc triển khai nghiệp vụ. README tạo ranh giới hữu ích và giúp Git lưu
thư mục; không tạo hàng loạt module rỗng hoặc file source giả.
Web, packages dùng chung, monitoring, IaC và môi trường chưa có nằm trong roadmap.

## ADR-003 — Ưu tiên tài liệu

Yêu cầu trực tiếp của chủ repo ưu tiên cao nhất, tiếp đến `GymBase.docx` cho cấu
trúc monorepo. Các tài liệu trong `Tài liệu dự án/` bổ sung chi tiết các task sau.

| Khác biệt | Quyết định |
| --- | --- |
| Tài liệu mobile cũ có hướng dẫn scaffold ở root | Dùng `apps/mobile` theo bản monorepo |
| Một số tài liệu dùng tên feature số ít hoặc `shared/ui` | Theo bản chính: `trainers`, `trainees`, `workouts`, `pt-sessions`, `shared/components` |
| Kế hoạch phụ để mở mono/multi repo | Chọn một monorepo theo bản chính |
| Kế hoạch phụ đề xuất .NET LTS được chốt sau | Ban đầu giữ .NET 9 theo yêu cầu trực tiếp; ngày 08/11/2026 nâng lên .NET 10 LTS |
| Cây minh họa có solution ở root | Theo ghi chú bản chính: `apps/api/GymBase.slnx` |

## ADR-004 — Tooling

pnpm workspace bao gồm `apps/*` và `packages/*`; chỉ quét package có manifest JS.
NuGet độc lập với pnpm. Root `package.json` là private và không có dependencies.
Chưa pin pnpm/Node/Expo hoặc tạo lockfile khi chưa có app; MOBILE-01 phải chốt
bộ phiên bản tương thích, `packageManager`, engines và commit `pnpm-lock.yaml`.
`check:repo` dùng Node built-ins, chạy được không cần install.

## ADR-005 — Git trong lần bàn giao này

Repo đang ở `nnhuy_init_base`, HEAD `403233a`; `develop` ở `24e6551` lúc khảo sát.
Giữ nhánh hiện tại để không thay đổi nền công việc của chủ repo. Đây là khác biệt
với hướng dẫn `chore/monorepo-base` từ `develop` trong tài liệu. Không commit,
push hoặc merge. Leader chọn luồng PR phù hợp trước khi chia việc cho nhóm.

## Cần chốt ở các task sau

- MySQL provider (Dapper + MySqlConnector), MySQL version và bộ Expo/Node/pnpm. SDK đã chốt: .NET 10.
- API auth/refresh/error contract, authorization matrix và public exports.
- ERD, quy ước tên bảng/cột, timezone, concurrency và quy tắc khiếu nại.
- GitHub reviewers thật, branch protection và required CI checks.
- Expo app identifiers, EAS project và môi trường staging/production.

Không có lựa chọn nào ở danh sách này được thay bằng credential hoặc cấu hình giả.
