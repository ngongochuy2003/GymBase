# Kiến trúc GymBase

Một Git monorepo, nhiều ứng dụng có vòng đời chạy/deploy độc lập:

```text
Mobile (Expo / React Native) ─┐
                             ├─ HTTPS → ASP.NET Core 9 API → MySQL
Web (Next.js, dự kiến V2) ───┘
```

Mobile đã có demo chạy được; API/MySQL/Web vẫn là kiến trúc mục tiêu.
Backend dùng modular monolith, chưa chia microservices. Mobile tổ chức feature-first.

| Khu vực | Trách nhiệm và ranh giới |
| --- | --- |
| `apps/mobile` | UI, navigation, state, API adapters; không truy cập SQL |
| `apps/api` | Authentication, authorization, business rules, persistence |
| `packages` | TypeScript dùng chung có consumer thật; không chứa C# |
| `database` | ERD, SQL hỗ trợ, seed giả; không phải nguồn migration thứ hai |
| `infrastructure` | Cách dựng dịch vụ; Docker trước, monitoring khi cần |
| `deploy` | Cấu hình và runbook từng môi trường có thật |
| `docs` | Quyết định kiến trúc, nghiệp vụ, API, onboarding |
| `scripts` | Tự động hóa công việc repo, không chứa business runtime |

## Module V1

| Backend dự kiến | Mobile tương ứng | Phạm vi |
| --- | --- | --- |
| Identity | auth | Danh tính, session, phân quyền |
| Trainers | trainers | Hồ sơ và tác vụ PT |
| Trainees | trainees | Hồ sơ và tác vụ học viên |
| Workouts | workouts | Kế hoạch và nhật ký tập |
| PtSessions | pt-sessions | Buổi PT, xác nhận, khiếu nại |
| SessionPackages | Bổ sung feature khi có màn hình | Gói và số lượt buổi PT |

Mobile: `app → features → core/shared`; route chỉ gọi screen. Feature không import
vòng hoặc tùy ý truy cập nội bộ feature khác. Backend trao đổi liên module bằng
contract/service nội bộ; API là nơi quyết định quyền, số buổi và tính phí.
EF migrations đặt trong Infrastructure, solution nằm tại `apps/api/GymBase.slnx`.

Xem README của từng app để biết cây source sẽ được tạo ở các task tiếp theo.
