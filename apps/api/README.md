# GymBase API

Thư mục đích cho API-01 trở đi. Backend mục tiêu: ASP.NET Core 9,
modular monolith; EF Core + MySQL. Chưa có solution, project hay endpoint.

Cấu trúc khi triển khai:

```text
GymBase.slnx
src/
  GymBase.Api/                  # Host, DI, middleware, endpoints
  GymBase.Modules/
    Identity/
    Trainers/
    Trainees/
    Workouts/
    PtSessions/
    SessionPackages/
  GymBase.Infrastructure/
    Persistence/Migrations/    # Nguồn migration duy nhất
  GymBase.SharedKernel/        # Domain primitives dùng chung
tests/
  GymBase.UnitTests/
  GymBase.IntegrationTests/
appsettings.Example.json
```

Đặt solution tại `apps/api/GymBase.slnx`, không tạo thêm solution ở root.
API-01 cần chốt SDK cụ thể, cấu hình project references/DI, `/health`, build và test.
DB-01 bổ sung DbContext, MySQL provider tương thích EF Core, migration và Docker.
API-02 chuẩn hóa validation, ProblemDetails, OpenAPI và logging.

Module sở hữu nghiệp vụ của mình; trao đổi qua service/contract nội bộ,
không truy cập tùy tiện bảng private của module khác. SharedKernel không chứa
mọi DTO/entity. Không chia sẻ trực tiếp EF entities cho frontend.
NuGet quản lý dependencies .NET; pnpm không build hoặc restore backend.

Chỉ cấu hình credentials qua biến môi trường hoặc user-secrets sau khi có project.
Không tự động migrate production khi API khởi động.
