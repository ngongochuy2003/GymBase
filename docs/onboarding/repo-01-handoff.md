# Bàn giao REPO-01

## Phạm vi

Dựng bộ thư mục tại root theo `GymBase.docx`, mục 10; không scaffold ứng dụng,
không nghiệp vụ, không cài dependencies, không commit/push/merge.
Tài liệu Word, `Tài liệu dự án/` và `Document.txt` được giữ nguyên.

## File tạo/sửa và mục đích

| File hoặc nhóm file | Mục đích |
| --- | --- |
| `README.md` (cập nhật) | Điểm vào, cây thư mục, trạng thái và lệnh kiểm tra |
| `.editorconfig`, `.gitattributes` | UTF-8, indentation, line endings, binary documents |
| `.gitignore`, `.env.example` | Loại trừ cấu hình riêng/build outputs; tên biến dự kiến |
| `package.json`, `pnpm-workspace.yaml` | Workspace JS private và lệnh kiểm tra root |
| `apps/README.md`, `apps/{mobile,api}/README.md` | Ranh giới app, cây source mục tiêu, task khởi tạo |
| `packages/README.md` | Điều kiện tạo thư viện TypeScript dùng chung |
| `database/README.md`, `{diagrams,scripts,seeds}/README.md` | ERD, SQL hỗ trợ, seed và nguồn EF migration duy nhất |
| `infrastructure/README.md`, `docker/README.md` | Trách nhiệm hạ tầng và task Docker local |
| `deploy/README.md`, `development/README.md` | Quy trình môi trường và kế hoạch local |
| `docs/architecture/{README,decisions,roadmap}.md` | Kiến trúc, ADR/khác biệt tài liệu, lộ trình |
| `docs/{business,api}/README.md` | Đầu vào nghiệp vụ và hợp đồng cần chốt |
| `docs/onboarding/README.md`, file bàn giao này | Hướng dẫn thành viên và trạng thái thực tế |
| `scripts/{README.md,check-repo.mjs}` | Kiểm tra cấu trúc không cần dependencies |
| `.github/CODEOWNERS`, `workflows/README.md` | Mẫu reviewer và yêu cầu CI, chưa kích hoạt workflow |

## Quyết định và giới hạn

Ưu tiên bản monorepo khi tài liệu mobile cũ khác đường dẫn; backend .NET 9 theo
yêu cầu chủ repo. Solution sau này đặt tại `apps/api/GymBase.slnx`.
Giữ nhánh hiện tại `nnhuy_init_base` vì HEAD khác `develop`; leader quyết định
nhánh/PR khi push. Không có reviewer giả, CI giả hoặc lệnh app chưa hoạt động.

## Kiểm tra

- `node scripts/check-repo.mjs`: PASS, 29 file bắt buộc và cấu hình root hợp lệ.
- `node --check scripts/check-repo.mjs`: PASS, cú pháp JavaScript hợp lệ.
- `git diff --check`: PASS trên thay đổi đã được Git theo dõi (README).
- Kiểm tra bằng Node: PASS, 7 liên kết tài liệu local tồn tại.
- `git check-ignore` qua Node: PASS, 9 trường hợp; secrets/build bị ignore,
  env examples, SQL source và tài liệu Word không bị loại trừ.

Lệnh Git local dùng `-c safe.directory=D:/Project/GymBase-Code/GymBase` vì
tài khoản chạy kiểm tra khác chủ sở hữu repo; không sửa Git config global.
Các kiểm tra build/test API, chạy Expo, migrations và Docker chưa áp dụng ở REPO-01.
Môi trường khảo sát có Node v24.13.0; chưa tìm thấy lệnh pnpm trong PATH,
nên không tuyên bố đã chạy `pnpm install` hoặc workspace listing bằng pnpm.

## Bước tiếp theo

Leader review và push theo quy trình nhóm. Triển khai MOBILE-01 và API-01 tại
các đường dẫn đã quy định, sau đó DB-01 và các task trong roadmap.
Chỉ coi toàn bộ base sẵn sàng chạy khi đạt checklist ứng dụng/DB/CI của tài liệu.
