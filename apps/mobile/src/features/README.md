# Ranh giới feature

`auth` là màn chào mừng và đăng nhập; `profile` chứa tùy chỉnh giao diện/thoát phiên và các bước thiết lập hồ sơ.

`feed/config` và `composer/config` chỉ chứa cấu hình giao diện tĩnh (nhãn bộ lọc, chủ đề, hành động). Dữ liệu nghiệp vụ đi qua repository API, không có bộ dữ liệu mẫu ở runtime.

## Thêm section/chức năng vào màn hình

1. Component nhận props có kiểu rõ ràng.
2. Đăng ký bằng `registerSection('type', Component)`.
3. Thêm `{ id, type, props }` vào cấu hình màn hình.

Màu và token ở `global.css` (dùng qua `className`), `shared/theme/palette.ts` chỉ cho prop không nhận class; `shared/theme/tokens.ts` là hệ cũ đang được thay, không dùng cho code mới. Icon ở `shared/icons`. Route chỉ gọi public exports của feature. Dùng alias `@/` cho import liên tầng; không import vòng hoặc deep-import feature khác.

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
