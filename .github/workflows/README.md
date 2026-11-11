# CI workflows

`mobile-ci.yml` đã có: frozen-lockfile install, lint, typecheck, Jest, Prettier
và export bundle Android/iOS/web. Chạy khi PR hoặc push develop/main thay đổi
Mobile, packages hoặc cấu hình liên quan. Chưa chạy trên GitHub trong lần bàn giao local.
`api-ci.yml` chạy khi PR hoặc push develop/main thay đổi `apps/api`: restore, build Release, toàn bộ test
(unit, kiến trúc, tích hợp, không cần MySQL) và kiểm tra model EF khớp migration. Chưa chạy trên GitHub.

- Mobile: install từ lockfile, lint, typecheck và tests thực tế.
- API: restore, build, tests thực tế và `dotnet ef migrations has-pending-model-changes` theo SDK đã chốt.
- Repository: có thể chạy `node scripts/check-repo.mjs`.

Không deploy production hoặc submit stores trong CI base.
CODEOWNERS hiện là mẫu comment; leader phải điền GitHub handles hợp lệ
và thiết lập branch protection/required checks sau khi workflows hoạt động.
