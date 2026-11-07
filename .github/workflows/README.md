# CI workflows

Chưa thêm workflow giả khi ứng dụng/lockfile chưa tồn tại.
CI-01 sẽ tạo `mobile-ci.yml` và `api-ci.yml`, có path filters cho ứng dụng,
packages dùng chung và cấu hình liên quan.

- Mobile: install từ lockfile, lint, typecheck và tests thực tế.
- API: restore, build và tests thực tế theo SDK đã chốt.
- Repository: có thể chạy `node scripts/check-repo.mjs`.

Không deploy production hoặc submit stores trong CI base.
CODEOWNERS hiện là mẫu comment; leader phải điền GitHub handles hợp lệ
và thiết lập branch protection/required checks sau khi workflows hoạt động.
