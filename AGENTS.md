# GymBase

Monorepo: `apps/api` (ASP.NET Core 10, modular monolith BL/DL, MySQL 8.4), `apps/mobile` (Expo), `database/`, `docs/`.
Tài liệu và comment viết tiếng Việt có dấu. Commit `type(scope): mô tả` (mô tả tiếng Việt), tách theo phần;
`apps/mobile/package-lock.json` nằm ngoài commit. Tạo PR thì nhánh đích (base) là `develop`.

Backend: quy tắc ở [apps/api/AGENTS.md](apps/api/AGENTS.md). Mobile: chạy `pnpm mobile:check` trước khi báo xong.
Code tạm (mock, bỏ qua một kiểm tra) luôn kèm `// TODO(CHỦ_ĐỀ): ...` ngay tại chỗ.

## Tài liệu: mở khi

| Khi bạn | Mở |
| --- | --- |
| Viết code tạm, mock, hoặc làm xong một chủ đề TODO | [docs/architecture/quy-uoc-todo.md](docs/architecture/quy-uoc-todo.md) |
| Cần biết một quy ước đã chốt chưa (Q1 đến Q9) | [docs/api/viec-con-lai-base.md](docs/api/viec-con-lai-base.md) |
| Làm mobile | [docs/onboarding/mobile-base.md](docs/onboarding/mobile-base.md) |
| Tìm lý do một quyết định kiến trúc | [docs/architecture/decisions.md](docs/architecture/decisions.md) |
