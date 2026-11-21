# GymBase

Monorepo: `apps/api` (ASP.NET Core 10, modular monolith BL/DL, MySQL 8.4), `apps/mobile` (Expo), `database/`, `docs/`.
Tài liệu và comment viết tiếng Việt có dấu. Commit `type(scope): mô tả` (mô tả tiếng Việt), tách theo phần;
`apps/mobile/package-lock.json` nằm ngoài commit. Tạo PR thì nhánh đích (base) là `develop`.

Backend: quy tắc ở [apps/api/AGENTS.md](apps/api/AGENTS.md). Mobile: chạy `pnpm mobile:check` trước khi báo xong.
Quy chuẩn giao diện (UI tokens): dùng NativeWind (`className`) và token trong `apps/mobile/global.css`; màu chuẩn lấy từ `Tài liệu dự án/GymBase_Color_Palette.html`, đổi màu thì sửa file HTML rồi đồng bộ `global.css` và `shared/theme/palette.ts`. Code mới không viết `StyleSheet`, không hardcode màu hex, không dùng `shared/theme/tokens.ts` và `theme.tsx` (hệ cũ đang được thay, xem `design-audit.md`). Không tự ý đổi thuộc tính giao diện (màu, cỡ chữ, padding, bo góc, icon) khi không được yêu cầu riêng.
Code tạm (mock, bỏ qua một kiểm tra) luôn kèm `// TODO(CHỦ_ĐỀ): ...` ngay tại chỗ.

## Tài liệu: mở khi

| Khi bạn | Mở |
| --- | --- |
| Viết code tạm, mock, hoặc làm xong một chủ đề TODO | [docs/architecture/quy-uoc-todo.md](docs/architecture/quy-uoc-todo.md) |
| Cần biết một quy ước đã chốt chưa (Q1 đến Q9) | [docs/api/viec-con-lai-base.md](docs/api/viec-con-lai-base.md) |
| Làm mobile | [docs/onboarding/mobile-base.md](docs/onboarding/mobile-base.md) |
| Chạy app mobile với backend, đăng nhập thử | [docs/onboarding/chay-mobile.md](docs/onboarding/chay-mobile.md) |
| Tìm lý do một quyết định kiến trúc | [docs/architecture/decisions.md](docs/architecture/decisions.md) |
