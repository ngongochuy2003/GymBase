# GymBase

Nền tảng kết nối phòng gym, PT và học viên; hỗ trợ trao đổi, quản lý tập luyện
và theo dõi tiến trình. V1 ưu tiên PT/học viên; phòng gym, web và các tích hợp
thương mại mở rộng theo roadmap.

**Trạng thái: Mobile base đã chạy được bằng Expo.** Có demo lịch tập, phiên demo,
theme sáng/tối, API/storage adapters và tests. Backend .NET, MySQL và Next.js chưa
triển khai. Xem [hướng dẫn Mobile](apps/mobile/README.md).

## Cấu trúc hiện có

Tất cả thư mục dưới đây nằm cùng cấp với `GymBase.docx`, ngay tại root Git:

```text
GymBase/
├── apps/
│   ├── mobile/                 # Expo / React Native chạy được, feature-first
│   └── api/                    # Backend ASP.NET Core 10, một host GymBase.API
├── packages/                  # Quy ước packages TypeScript dùng chung
├── database/
│   ├── diagrams/
│   ├── scripts/
│   └── seeds/
├── infrastructure/docker/     # Hướng dẫn cấu hình Docker ở DB-01
├── deploy/development/        # Kế hoạch chạy local
├── docs/
│   ├── architecture/
│   ├── business/
│   ├── api/
│   └── onboarding/
├── scripts/                   # Kiểm tra khung repo không cần dependencies
├── .github/
│   ├── workflows/             # Mobile CI; backend CI bổ sung khi có API
│   └── CODEOWNERS             # Mẫu comment, chưa gán reviewer
├── .editorconfig
├── .gitattributes
├── .gitignore
├── .env.example
├── package.json
├── pnpm-workspace.yaml
├── GymBase.docx               # Tài liệu monorepo gốc, giữ nguyên
└── Tài liệu dự án/             # Các tài liệu gốc bổ sung, giữ nguyên
```

Các thư mục có README để được lưu trong Git và giải thích trách nhiệm.
`apps/web`, các package con, solution .NET, monitoring và môi trường
staging/production chỉ được tạo khi triển khai task tương ứng theo tài liệu.

## Kiểm tra sau khi clone

Chạy từ root bằng Node.js (script không cần cài dependencies):

```sh
node scripts/check-repo.mjs
```

Chạy demo Mobile từ root (Node 24.13.0, pnpm 10.34.9):

```sh
npx --yes pnpm@10.34.9 install --frozen-lockfile
npx --yes pnpm@10.34.9 mobile
```

Mở Expo Go tương thích SDK 58 và quét QR; hoặc dùng `mobile:android` khi emulator
đã chạy. `mobile:web` mở bản preview React Native Web, không phải ứng dụng Next.js.
Chạy `npx --yes pnpm@10.34.9 mobile:check` để lint, typecheck và test.
PowerShell có thể dùng `npx.cmd` nếu policy chặn `npx.ps1`.
Workspace hiện có `@gymbase/mobile`; NuGet vẫn quản lý dependencies .NET riêng.

## Hướng dẫn cho nhóm

- [Onboarding Windows/macOS và Git workflow](docs/onboarding/README.md).
- [Kiến trúc và ranh giới module](docs/architecture/README.md).
- [Quyết định, khác biệt tài liệu và việc còn mở](docs/architecture/decisions.md).
- [Roadmap các task tiếp theo](docs/architecture/roadmap.md).
- [Quy tắc nghiệp vụ cần xác nhận](docs/business/README.md).
- [Hợp đồng API cần xây dựng](docs/api/README.md).
- [Báo cáo bàn giao REPO-01](docs/onboarding/repo-01-handoff.md).
- [Mobile base: kiến trúc, giới hạn và kiểm thử](docs/onboarding/mobile-base.md).

Không commit `.env`, tokens, mật khẩu, signing keys hoặc dữ liệu người dùng.
Mobile sử dụng `apps/mobile/.env.example`; `.env.example` ở root dành cho hạ tầng
tương lai. Demo chạy ngay mà không cần `.env` hoặc backend.

<!-- Ghi chú ban đầu của repository: nnHuy comment -->
