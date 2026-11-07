# GymBase

Nền tảng kết nối phòng gym, PT và học viên; hỗ trợ trao đổi, quản lý tập luyện
và theo dõi tiến trình. V1 ưu tiên PT/học viên; phòng gym, web và các tích hợp
thương mại mở rộng theo roadmap.

**Trạng thái: REPO-01 — khung monorepo.** Chưa scaffold Expo, Next.js, .NET hoặc
database. Đây là cấu trúc để nhóm bắt đầu triển khai, chưa phải ứng dụng chạy được.

## Cấu trúc hiện có

Tất cả thư mục dưới đây nằm cùng cấp với `GymBase.docx`, ngay tại root Git:

```text
GymBase/
├── apps/
│   ├── mobile/                 # Hướng dẫn vị trí Expo / React Native
│   └── api/                    # Hướng dẫn vị trí ASP.NET Core 9
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
│   ├── workflows/             # Hướng dẫn CI-01, chưa có workflow
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
`apps/web`, các package con, solution .NET, source modules, monitoring và môi trường
staging/production chỉ được tạo khi triển khai task tương ứng theo tài liệu.

## Kiểm tra sau khi clone

Chạy từ root bằng Node.js (script không cần cài dependencies):

```sh
node scripts/check-repo.mjs
```

Nếu đã có pnpm, có thể chạy `pnpm check:repo` và `pnpm -r list --depth -1`.
Workspace hiện chưa có package ứng dụng; chỉ thư mục có `package.json` mới là
package. Chưa cần `pnpm install`; phiên bản pnpm/Node và lockfile sẽ được chốt
cùng Expo ở MOBILE-01. Dependencies .NET được quản lý riêng bằng NuGet.

## Hướng dẫn cho nhóm

- [Onboarding Windows/macOS và Git workflow](docs/onboarding/README.md).
- [Kiến trúc và ranh giới module](docs/architecture/README.md).
- [Quyết định, khác biệt tài liệu và việc còn mở](docs/architecture/decisions.md).
- [Roadmap các task tiếp theo](docs/architecture/roadmap.md).
- [Quy tắc nghiệp vụ cần xác nhận](docs/business/README.md).
- [Hợp đồng API cần xây dựng](docs/api/README.md).
- [Báo cáo bàn giao REPO-01](docs/onboarding/repo-01-handoff.md).

Không commit `.env`, tokens, mật khẩu, signing keys hoặc dữ liệu người dùng.
`.env.example` chỉ khai báo tên biến dự kiến; chưa có runtime đọc file này.

<!-- Ghi chú ban đầu của repository: nnHuy comment -->
