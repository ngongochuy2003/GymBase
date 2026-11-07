# GymBase Mobile

Thư mục đích cho MOBILE-01 trở đi. Chưa có Expo project hoặc dependencies.
Khi scaffold vào đây, giữ nội dung hướng dẫn này và không tạo app ở root repo.

Cấu trúc cần tạo khi có mã nguồn tương ứng:

```text
app.json, eas.json, package.json, tsconfig.json
assets/
src/
  app/                 # Expo Router: route và layout mỏng
  features/
    auth/
    trainers/
    trainees/
    workouts/
    pt-sessions/
  core/
    api/
    auth/
    config/
    storage/
  shared/
    components/
    hooks/
    theme/
    utils/
  providers/
__tests__/
```

Một feature có `api`, `components`, `screens`, `hooks`, `schemas`, `types`,
`utils`, `__tests__` khi thực sự sử dụng; `index.ts` là public API nếu cần.
Chiều phụ thuộc: `app → features → core/shared`. Không import vòng giữa features.
Không đặt logic trừ buổi, thanh toán hoặc quyền truy cập đáng tin cậy ở client.

MOBILE-01 chốt phiên bản Expo/React Native/Node/pnpm tương thích, commit lockfile,
thiết lập Expo Router và TypeScript strict. MOBILE-02 bổ sung màn hình demo,
alias `@/` và cấu trúc feature. MOBILE-03 bổ sung HTTP client, TanStack Query,
secure token storage và kiểm thử tích hợp mock.

`EXPO_PUBLIC_*` là dữ liệu công khai. Điện thoại thật phải gọi địa chỉ LAN/tunnel
của backend; `localhost` trên điện thoại không trỏ đến máy phát triển.
