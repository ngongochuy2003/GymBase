---
trigger: always_on
---

# Chuẩn dự án GymBase (tóm tắt từ `docs/`)

Nguồn đầy đủ: `docs/`. Khi có mâu thuẫn, ưu tiên: yêu cầu trực tiếp của chủ repo > `GymBase.docx` > tài liệu trong `docs/`.
Đọc đúng tài liệu liên quan trước khi sửa khu vực tương ứng.

## Tổng quan & kiến trúc (`docs/architecture`, `docs/business`)
- Một Git monorepo: `apps/mobile` (Expo/React Native), `apps/api` (ASP.NET Core 10, modular monolith BL/DL, một host `GymBase.API`), `packages`, `database`, `infrastructure`, `deploy`, `docs`, `scripts`.
- Web (Next.js) là V2; V3 thêm cộng đồng/messaging/notifications. Chỉ tạo thư mục/module khi có use case thật; không tạo screen/module rỗng, không credential hay config giả.
- Backend quyết định quyền, số buổi PT và tính phí; client chỉ trình bày và gửi yêu cầu.
- Module V1: auth, trainers, trainees, workouts, pt-sessions, session-packages. Dữ liệu danh mục thuộc nhóm DI.
- Tài liệu module mới phải có actors, use cases, trạng thái, quyền, chuyển trạng thái, trường hợp lỗi trước khi làm schema.

## Mobile (`docs/onboarding/mobile-base.md`, `apps/mobile/README.md`, `apps/mobile/src/features/README.md`)
- Stack: Expo SDK 58, RN 0.88.1, React 19.3, TypeScript strict, Expo Router, TanStack Query, Axios, RHF + Zod, SecureStore. pnpm 10.34.9.
- Feature-first. Chiều phụ thuộc: `app → features → core/shared`. Route mỏng, chỉ import public exports của feature (`@/features/<tên>`), không deep-import, không import vòng. `core`/`shared` không import `features`/`app`/`providers` (ESLint chặn).
- Mẫu feature: `api/ components/ hooks/ schemas/ screens/ types/ __tests__/ index.ts`. Screen không gọi HTTP trực tiếp; dùng typed repository để thay được backend.
- Alias `@/` cho import liên tầng; relative chỉ trong cùng feature.
- Demo session chỉ trong memory, không token; API mode không cho vào demo. Không thêm Zustand khi chưa cần.
- Kiểm tra trước khi xong: `pnpm lint` (0 warning), `pnpm typecheck`, `pnpm test`, `pnpm format:check` trong `apps/mobile`.

## Backend (`docs/api`)
- Cấu trúc: `cau-truc-thu-muc.md`; CRUD: `crud-dung-chung.md`, `huong-dan-them-crud.md`; response/lỗi/phân trang: `quy-uoc-response.md`; sort/filter: `sort-va-filter.md`; log: `logging.md`; auth: `xac-thuc.md`; chạy API: `cach-chay-api.md`; việc còn lại: `viec-con-lai-base.md`.
- Web route `/api/v1/web` (`Controllers/`), mobile `/api/v1/mob` (`Mob/Controllers/`).
- Tên bảng/cột: tiếng Việt không dấu snake_case; property C# trùng tên cột; khóa chính `Id` UUIDv7; thời gian UTC; xóa mềm `is_deleted`; audit `ngay_tao`, `nguoi_tao`, `ngay_chinh_sua`, `nguoi_chinh_sua`.
- .NET 10, EF Core + `Microting.EntityFrameworkCore.MySql`, MySQL 8.4, một `AppDbContext` ở `GymBase.Persistence`, migrations ở `GymBase.Persistence/Migrations`.
- OpenAPI (`/openapi/v1.json`) là nguồn contract; không chia sẻ EF entities, không log mật khẩu/token, không trả stack trace production.
- Collection thử API: `docs/api/bruno`.

## Quy trình
- Nhánh `feat/*`, `fix/*`, `chore/*`, PR vào `develop`. Không commit/push/merge nếu chủ repo không yêu cầu.
- `.env` bị Git ignore; biến `EXPO_PUBLIC_*` không chứa secret.
- Cập nhật `docs/` khi thay đổi hành vi hoặc cấu trúc.
