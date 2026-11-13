# Quyết định kiến trúc và các điểm còn mở

## Cập nhật — Nâng lên .NET 10 (08/11/2026)

Backend chuyển từ .NET 9 sang **.NET 10 LTS** vì .NET 9 hết hỗ trợ ngày 10/11/2026.
Mọi project target `net10.0`; `apps/api/global.json` ghim SDK `10.0.100` với
roll-forward `latestFeature` (máy dev đang dùng SDK 10.0.402). Gói
`Microsoft.AspNetCore.OpenApi` và `Microsoft.Extensions.Hosting` lên `10.0.14`.
OpenAPI do .NET 10 sinh ra là phiên bản 3.1.

## Cập nhật — Bỏ xóa mềm (12/11/2026)

Bỏ hẳn xóa mềm: không còn `ISoftDelete`, cột `is_deleted` và query filter. Riêng `tin_nhan` và `tuong_tac_bai_viet` giữ
cột `ngay_xoa` (đánh dấu ẩn hoặc xóa theo nghiệp vụ, service tự lọc `ngay_xoa IS NULL`). Xóa là xóa hẳn dòng; dòng
đang được bảng khác tham chiếu thì từ chối bằng 409 `IN_USE` (khóa ngoại `RESTRICT`). Tên đăng nhập là unique index thường.
Tài khoản không còn cần giữ lại để hiển thị: muốn ngừng dùng thì đặt `dang_hoat_dong = false`.
Mục "Entity gốc" bên dưới đã bỏ phần xóa mềm.

## Cập nhật — Entity gốc (08/11/2026)

Tên bảng/cột dùng tiếng Việt không dấu snake_case; property C# của entity và DTO trùng
tên cột (ví dụ `ten_dang_nhap`, `ho_ten`, `ngay_tao`, `nguoi_tao`, `ngay_chinh_sua`,
`nguoi_chinh_sua`). Riêng khóa chính giữ tên `Id`.
Khóa chính là UUIDv7 (`Guid.CreateVersion7()`). Mọi mốc thời gian lưu ở UTC.
Tách `BaseEntity` (chỉ Id) và `BaseAuditableEntity` (thêm 4 trường audit) vì
bảng nối, log và lịch sử không cần audit.
Mã nguồn: `apps/api/src/Base/GymBase.Base/Entities`.

## Cập nhật — API base

Ngày 08/11/2026 chuẩn hóa `apps/api` theo tài liệu "Chuẩn kiến trúc backend .NET 10":
modular monolith theo BL / DL, **một API host** `GymBase.API` phục vụ cả web
(`Controllers/`, `/api/v1/web`) và mobile (`Mob/Controllers/`, `/api/v1/mob`), thay cho
cấu trúc nhiều host trước đó. Ba nhóm Auth / DI (danh mục) / BIZ, mỗi nhóm có BL,
DL.Contract, DL.MySql; phần nền dùng chung là `GymBase.Base` và `GymBase.Base.Hosting`.
Người dùng chuyển từ nhóm System cũ sang Auth; entity đặt ở `DL<nhóm>.Contract/Entities`;
`AppDbContext` và DL mock dùng chung đặt trong `GymBase.DLBiz.MySql`. Các nhóm khung rỗng
(ImportExport, Register, Report, Lib, Integration, Worker) bị bỏ, khi cần thì thêm lại.
SDK ban đầu ghim .NET 9, sau đó đã nâng lên .NET 10 (xem mục trên).
Chi tiết: `docs/api/cau-truc-thu-muc.md`.

Cùng ngày, gom entity vào `GymBase.Entities`, còn `AppDbContext`, cấu hình bảng và migrations vào
`GymBase.Persistence` để quản lý cấu trúc DB một chỗ, thay cho cách đặt ở `DL<nhóm>.Contract` và `GymBase.DLBiz.MySql`.
Ranh giới giữa các nhóm do `LayerReferenceTests` giữ.

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

- Bộ Expo/Node/pnpm.
  Đã chốt 08/11: SDK .NET 10; truy cập DB bằng EF Core với provider
  `Microting.EntityFrameworkCore.MySql` (Pomelo chưa hỗ trợ EF Core 10); MySQL 8.4 LTS; migration
  bằng EF Core migrations; một `AppDbContext` duy nhất ở `GymBase.Persistence`
  (xem `docs/api/viec-con-lai-base.md`, Q3, Q4, Q5, Q9).
- API auth/refresh/error contract, authorization matrix và public exports.
- ERD, quy ước tên bảng/cột, timezone, concurrency và quy tắc khiếu nại.
- GitHub reviewers thật, branch protection và required CI checks.
- Expo app identifiers, EAS project và môi trường staging/production.

Không có lựa chọn nào ở danh sách này được thay bằng credential hoặc cấu hình giả.
