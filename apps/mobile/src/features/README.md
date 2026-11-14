# Ranh giới feature

`auth` là màn chào mừng và đăng nhập; `pt-sessions` có màn hình lịch tập, typed repository,
query keys, hook và UI; `profile` chứa tùy chỉnh giao diện/thoát phiên.

`feed/config` và `composer/config` chỉ chứa cấu hình giao diện tĩnh (nhãn bộ lọc, chủ đề, hành động). Dữ liệu nghiệp vụ đi qua repository API, không có bộ dữ liệu mẫu ở runtime.

## Thêm section/chức năng vào màn hình

1. Component nhận props có kiểu rõ ràng.
2. Đăng ký bằng `registerSection('type', Component)`.
3. Thêm `{ id, type, props }` vào cấu hình màn hình.

Design tokens ở `shared/theme/tokens.ts`, icon ở `shared/icons`. Route chỉ gọi public exports của feature. Dùng alias `@/` cho import liên tầng; không import vòng hoặc deep-import feature khác.

Mẫu feature mới:

```text
features/<feature>/
  api/          # Repository HTTP
  components/   # UI riêng
  hooks/        # Query/mutation và query keys
  schemas/      # Validation
  screens/      # Compose UI và hooks
  index.ts      # Public exports
```
