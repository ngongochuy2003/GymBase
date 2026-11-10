# Feature boundaries

`auth` là màn hình vào demo; `feed` là Trang chủ (bảng tin) và `composer` là màn Đăng bài, cả hai dựng từ `ScreenConfig` qua `ScreenRenderer` (`shared/screen-config`); `profile` chứa tùy chỉnh giao diện/thoát phiên.

## Thêm section/chức năng vào màn hình

1. Component nhận props là mảng cấu hình (items/actions/options...).
2. `registerSection('type', Component)` (trong screen của feature).
3. Thêm một object `{ id, type, props }` vào `sections` của config màn hình. Chip, tab, action, chủ đề mới = thêm một phần tử vào mảng mock/config tương ứng.

Design tokens ở `shared/theme/tokens.ts`, icon ở `shared/icons` (`registerIcon`).

Các module V1 tiếp theo: `trainers`, `trainees`, `workouts`, `session-packages`.
Tạo thư mục khi triển khai use case thực tế; không tạo screen rỗng để giả đã có chức năng.

Mẫu một feature:

```text
features/<feature>/
  api/          # Repository/adapter HTTP hoặc demo tách biệt
  components/   # UI riêng của feature
  hooks/        # Query/mutation, query key factory
  schemas/      # Zod validation
  screens/     # Compose UI/hook, không HTTP trực tiếp
  types/        # DTO/domain/view models tách khi cần
  __tests__/    # Tests hành vi
  index.ts      # Public exports
```

Route chỉ import public exports. Dùng alias `@/` cho import liên tầng; relative imports
chỉ dùng bên trong cùng feature. Không import vòng hoặc deep-import feature khác.
ESLint chặn deep import và import ngược vào routes/providers; review vẫn cần kiểm tra
ranh giới nghiệp vụ. `core` không chứa quy tắc trừ buổi, tính phí hoặc quyền đáng tin cậy.

V2 thêm `gyms`, `branches`, `memberships`; V3 thêm social/messaging khi có task.
Giữ UI phụ thuộc typed repository để thay transport/backend mà không viết lại screens.
