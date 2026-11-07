# Quyết định kiến trúc và các điểm còn mở

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
| Kế hoạch phụ đề xuất .NET LTS được chốt sau | Giữ .NET 9 theo yêu cầu trực tiếp; API-01 xác minh SDK/provider tương thích |
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

- SDK .NET 9 cụ thể, EF Core/MySQL provider, MySQL version và bộ Expo/Node/pnpm.
- API auth/refresh/error contract, authorization matrix và public exports.
- ERD, quy ước tên bảng/cột, timezone, concurrency và quy tắc khiếu nại.
- GitHub reviewers thật, branch protection và required CI checks.
- Expo app identifiers, EAS project và môi trường staging/production.

Không có lựa chọn nào ở danh sách này được thay bằng credential hoặc cấu hình giả.
