# Lộ trình theo GymBase.docx

| Task | Kết quả cần tạo | Trạng thái |
| --- | --- | --- |
| REPO-01 | Khung root, workspace, quy ước và onboarding | Đã dựng |
| MOBILE-01 | Expo TS tại apps/mobile, Router, dependencies/lockfile | Đã dựng và chạy demo Android |
| MOBILE-02 | Feature-first, demo screen, alias, typecheck | Đã dựng |
| API-01 | Solution/projects .NET, DI, health, build/test | Chưa làm |
| DB-01 | MySQL Docker, DbContext, migration mẫu, env mapping | Chưa làm |
| API-02 | Validation, ProblemDetails, OpenAPI, logging | Chưa làm |
| MOBILE-03 | HTTP client, Query, secure storage, integration mock test | Có base và mock tests; auth thật chờ backend |
| DOCS-01 | Hoàn thiện onboarding bằng lệnh runtime đã kiểm chứng | Có hướng dẫn chạy Mobile; API chờ scaffold |
| CI-01 | Mobile/API workflows độc lập, lint/typecheck/build/test | Có workflow Mobile, chưa chạy trên GitHub; API chưa làm |
| RELEASE-01 | EAS profiles, signing hướng dẫn, deployment design | Chưa làm |

V2 thêm Web Next.js và nghiệp vụ phòng gym (gyms, branches, memberships).
V3 thêm cộng đồng, messaging/notifications theo nhu cầu. V3+ có commerce/payments
và các integrations. Chỉ tạo source folders khi task tương ứng bắt đầu.

Checklist nghiệm thu toàn bộ base trong tài liệu chỉ hoàn tất sau các task trên;
REPO-01 không đồng nghĩa với app chạy được hoặc CI/deployment đã hoạt động.
