# Danh sách phần base backend còn phải làm

Danh sách này lập ngày 08/11/2026, sau khi đối chiếu `apps/api` với dự án tham khảo
nội bộ **dev-HuyNgo**. Dự án đó chạy .NET 6, phần base nằm trong `SharedKernel.*`
và phần cấu hình của `App.Api`. Nó dùng MediatR, EF Core (Pomelo MySQL) kèm Dapper,
FluentValidation, AutoMapper, Duende IdentityServer, Redis và NLog.

Kiến trúc của dự án tham khảo khác GymBase (CQRS so với API / BL / DL). Danh sách
dưới đây **lấy các thành phần base của nó và đặt vào đúng tầng tương ứng của
GymBase**, không chép nguyên cấu trúc. Cấu trúc thư mục hiện tại xem tại
[cau-truc-thu-muc.md](cau-truc-thu-muc.md).

**Đã xong:** khung solution (API-01), chuẩn hóa ngày 08/11 theo
[cau-truc-thu-muc.md](cau-truc-thu-muc.md): một host `GymBase.API`, ba nhóm Auth / DI / BIZ,
phần nền `GymBase.Base` và `GymBase.Base.Hosting`, test chia Unit / Integration / Architecture.
`ArchitectureTests` giữ ma trận tham chiếu project.

**Mẫu đầu tiên đi đủ các tầng:** API người dùng. Web (`/api/v1/web/users`, `Controllers/`) và
mobile (`/api/v1/mob/users`, `Mob/Controllers/`) đều có đủ CRUD qua `BaseCrudController`. Cả hai cùng
gọi `UserService : BaseService` → `IUserRepository : IBaseRepository<nguoi_dung>`. Entity `nguoi_dung` nằm ở
`GymBase.DLAuth.Contract/Entities`, DTO nằm ở `GymBase.BLAuth/NguoiDung`. `UserRepository` kế thừa `BaseRepository` dùng EF Core; DL giả cho test
nằm ở `tests/Shared/GymBase.TestDoubles`.

**Truy cập DB dùng EF Core** (chốt 08/11, thay cho hướng SQL raw + Dapper trước đó).
EF Core chỉ nằm trong các project `DL*.MySql`; BL vẫn chỉ thấy interface `IBaseRepository` /
`I*DL` ở `DL*.Contract`, không thấy `DbContext`.

---

## Quyết định cần chốt trước P0

| # | Quyết định | Đề xuất | Trạng thái |
| --- | --- | --- | --- |
| Q1 | .NET 9 hay .NET 10 (.NET 9 hết hỗ trợ ngày 10/11/2026) | .NET 10 LTS | **Đã chốt 08/11**, đã nâng lên |
| Q2 | Quy ước tên bảng/cột | **Tiếng Việt không dấu snake_case**, property C# trùng tên cột (`tai_khoan`, `ho_ten`, `ngay_tao`, `is_deleted`); DTO trả ra API dùng cùng tên. Riêng khóa chính vẫn là `Id`. Tên class entity trùng tên bảng. Ví dụ đầy đủ: entity `nguoi_dung` (bảng `nguoi_dung`) | **Đã chốt 08/11** |
| Q3 | Truy cập DB và provider MySQL | **EF Core** (đã chốt). Provider: **`Microting.EntityFrameworkCore.MySql`**, bản fork của Pomelo có hỗ trợ EF Core 10. Pomelo (dự án tham khảo dùng) mới hỗ trợ tới EF Core 9 nên không dùng được với .NET 10 / EF Core 10. Bản fork vẫn chạy trên MySqlConnector và giữ API của Pomelo (`UseMySql(...)`), nên khi Pomelo ra bản cho EF Core 10 có thể chuyển lại mà ít phải sửa | **Đã chốt 08/11** (EF Core + Microting) |
| Q4 | Công cụ migration | **EF Core migrations** (đi cùng Q3). Áp dụng bằng script idempotent (`dotnet ef migrations script --idempotent`) hoặc migration bundle, chạy ở bước riêng | **Đã chốt 08/11** |
| Q9 | Số `DbContext` và chỗ đặt migrations | **Một `DbContext` duy nhất, đặt một chỗ** cho cả DB (một DB, nhiều module), không tách theo module. Lớp `AppDbContext` đặt ở `GymBase.DLBiz.MySql/Persistence` theo chuẩn kiến trúc; `DLAuth.MySql` và `DLDI.MySql` tham chiếu `DLBiz.MySql`. `AppDbContext` nạp `IEntityTypeConfiguration` từ các assembly `DL*.MySql` khi đăng ký. Migrations đặt ở `GymBase.DLBiz.MySql/Migrations`; `dotnet ef` dùng startup project `GymBase.API` | **Đã chốt 08/11** |
| Q5 | Phiên bản MySQL | **MySQL 8.4 LTS**. Bản 8.0 đã hết hỗ trợ (04/2026). Image Docker (mục 14) ghim `mysql:8.4` | **Đã chốt 08/11** |
| Q6 | Kiểu khóa chính | UUIDv7 (`Guid.CreateVersion7()`) | **Đã chốt 08/11** |
| Q7 | Múi giờ lưu trữ | UTC, chỉ đổi sang giờ địa phương khi hiển thị | **Đã chốt 08/11** |
| Q8 | Xóa mềm | Cờ `is_deleted` (bool, mặc định false) | **Đã chốt 08/11** |

---

## P0: cần có trước khi viết nghiệp vụ đầu tiên

| # | Phần base | Bên tham khảo có gì | Đặt ở đâu trong GymBase | Xong |
| --- | --- | --- | --- | --- |
| 1 | Chốt Q1 và commit khung hiện tại | (.NET 6) | Toàn solution: đã lên .NET 10, khung đã commit | ☑ |
| 2 | **Cấu hình và secrets** | `ConnectionStrings`, `Redis`, `Uri` trong appsettings | Options pattern. Connection string đặt trong user-secrets hoặc biến môi trường; file mẫu là `apps/api/.env.example`. **Connection string đã làm** (user-secrets); Options pattern làm khi có nhóm cấu hình đầu tiên | ◐ |
| 3 | **Kết nối DB** | Pomelo + Dapper, retry khi lỗi | `GymBase.DLBiz.MySql`: `AppDbContext` duy nhất (theo Q9) và extension đăng ký `AddDbContext` (provider theo Q3, bật retry khi lỗi tạm thời, `DbContext` scoped theo request). Trong `OnModelCreating` / `ConfigureConventions`: **global query filter** `!is_deleted` cho mọi entity `ISoftDelete`; **value converter** gắn `DateTimeKind.Utc` cho mọi `DateTime` đọc từ DB. Property trùng tên cột nên EF không cần cấu hình map tên cột; tên bảng đặt bằng `ToTable(nameof(entity))` trong configuration. Lưu UUID bằng `BINARY(16)` và **kiểm chứng** thứ tự byte giữ được tính tăng dần của UUIDv7 (`GuidFormat` của MySqlConnector, provider Microting theo Q3). Đọc dữ liệu mặc định `AsNoTracking()`. **Đã làm** trong `Persistence/`, đã chạy thử CRUD trên MySQL thật; `GuidFormat=Binary16` giữ đúng thứ tự byte nên UUIDv7 tăng dần | ☑ |
| 4 | **Entity gốc** | `BaseEntity` (id Guid), `BaseAuditableEntity` (ngày/người tạo và sửa), soft delete `is_deleted` | **Đã làm** trong `GymBase.Base/Entities`: `BaseEntity` (Id UUIDv7), `BaseAuditableEntity` (`ngay_tao`, `nguoi_tao`, `ngay_chinh_sua`, `nguoi_chinh_sua`), interface `ISoftDelete` (`is_deleted`) và `EntityAuditExtensions` (`MarkCreated`, `MarkUpdated` từ chối thời gian không phải UTC; `MarkDeleted`, `Restore`). Có test `EntityTests` | ☑ |
| 5 | **CRUD dùng chung** | `BaseCrudApiController` (GetAll, GetById, Create, Update, Delete) + `GetAllQueryHandler` | **Đã làm:** `IBaseRepository<TEntity>` (DL.Contract) → `BaseService<TEntity, TDto, TCreateDto, TUpdateDto>` (BL, tự điền audit UTC, xóa mềm) → `BaseCrudController<...>` (API). `BaseRepository<TEntity>` dùng EF Core, mỗi thao tác ghi gọi `SaveChangesAsync`. Lỗi nghiệp vụ đã có (mục 7): `BaseService` ném `NotFoundException`, `UserService` chặn trùng tên đăng nhập | ☑ |
| 6 | **Phân trang, lọc, sắp xếp, tìm kiếm** | `GetAllQuery` (page, page_size, sort, filter, search) và `ResponeListDto` + `Meta` (total, total_page, ranger) | **Đã làm:** `PagingRequest` (`page`, `page_size`, `search`; tự đưa về giá trị hợp lệ, `page_size` tối đa 100, không có "lấy tất cả") và `PagingResult<T>` trả `{ data, meta }`, `meta` gồm `page`, `page_size`, `total`, `total_page`, `ranger` (`from`, `to`) theo dạng của dự án tham khảo (chốt 08/11; bỏ `Message`, `IsSuccess`, `Extra`) đã có trong `GymBase.Base/Paging`. Sort nhận dạng `sort=-ngay_tao,ho_ten`, filter nhận dạng `filter=vai_tro:eq:Trainer,ngay_tao:gte:2026-11-01` (chốt 08/11), cột ngoài danh sách cho phép trả 400. `PagingQuery` dựng biểu thức LINQ (`IQueryable`) từ `QueryColumns` của từng bảng; không ghép chuỗi vào `FromSqlRaw`. Cú pháp đầy đủ: [quy-uoc-response.md](quy-uoc-response.md) | ☑ |
| 7 | **Exception và mã lỗi** | `ErrorCode` (404, 460–464), `NotFoundException`, `RejectException`, `DataExitsException` | **Đã làm 08/11.** Exception (`BusinessException` và 7 lớp con) và `ErrorCodes` trong `GymBase.Base/Exceptions`; `AppExceptionHandler` + `ProblemDetailsCustomizer` trong `GymBase.Base.Hosting/Errors`, `GymBase.API` gọi `AddAppErrorHandling()` / `UseAppErrorHandling()`. Mọi lỗi, kể cả lỗi framework tự trả, là ProblemDetails có `code`, `detail` tiếng Việt, `trace_id`; lỗi 500 không lộ chi tiết. Mobile đọc qua `toApiError`. Quy ước và bảng mã: [quy-uoc-response.md](quy-uoc-response.md). Lỗi vi phạm unique của DB được `BaseRepository` đổi thành `ConflictException`. **Còn lại:** thông báo DataAnnotations tiếng Việt (mục 8) | ☑ |
| 8 | **Validation** | FluentValidation chạy qua pipeline của MediatR | FluentValidation gọi trong `BaseService` trước khi xuống DL | ☐ |
| 9 | **Map DTO ↔ entity** | AutoMapper | Mapster hoặc map tay. Từ 2025 AutoMapper chuyển sang license thương mại | ☐ |
| 10 | **Người dùng hiện tại** | `IdentityService` + `CurrentUserDto` lấy từ claims của JWT | Interface `ICurrentUser` trong `GymBase.Base`, hiện thực ở `GymBase.Base.Hosting`. Dùng để tự điền người tạo/sửa | ☐ |
| 11 | **Program.cs dùng chung** | `ConfigureServices`, Swagger có nút nhập Bearer, CORS, ForwardedHeaders | Extension `AddAppApi()` / `UseAppApi()` trong `GymBase.Base.Hosting`, gom cấu hình của `GymBase.API` (CORS, `LowercaseUrls`, xử lý lỗi) và cho test dùng lại | ☐ |

---

## P1: hạ tầng chạy thật

| # | Phần base | Bên tham khảo có gì | GymBase | Xong |
| --- | --- | --- | --- | --- |
| 12 | **Auth** | Duende IdentityServer, JWT, middleware kiểm tra token, TOTP | JWT + refresh token: nghiệp vụ trong `BLAuth`, endpoint ở `GymBase.API` (cả `Controllers/` và `Mob/Controllers/`), cấu hình xác thực ở `GymBase.Base.Hosting`, phân quyền theo role/policy. Không dùng Duende vì cần license | ☐ |
| 13 | **Migration và seed** | EF migrations, các lớp `*DbContextInitial` seed dữ liệu lúc khởi động | EF Core migrations theo Q4, đặt theo Q9. Dữ liệu danh mục cố định dùng `HasData` trong migration; dữ liệu mẫu cho dev đặt ở `database/seeds`, chạy có chủ đích. **Không** gọi `Database.Migrate()` khi khởi động production. **Đã có** migration `InitialCreate` và `database/seeds/dev_nguoi_dung.sql`, đã áp dụng trên MySQL local | ☑ |
| 14 | **Docker** | Compose gồm mysql, redis, vault, gateway, api, web; mỗi API có Dockerfile riêng | `infrastructure/docker`: MySQL trước (image `mysql:8.4` theo Q5), Redis sau. Một Dockerfile multi-stage ở `apps/api`, chỉ publish `GymBase.API`; thêm `.dockerignore` | ☐ |
| 15 | **Logging** | NLog, bảng `system_log` (ai gọi lệnh gì, mất bao lâu) ghi qua `LoggingBehaviour` | Serilog hoặc NLog. Audit log ghi qua middleware hoặc action filter trong `GymBase.Base.Hosting` | ☐ |
| 16 | **Health check DB** | (không có) | Thêm kiểm tra `DbContext` vào `/health` (`AddDbContextCheck`). **Đã làm** | ☑ |
| 17 | **Quy chuẩn code** | `.editorconfig` | **Đã làm 08/11:** `.editorconfig`, `Directory.Build.props` (nullable, coi cảnh báo là lỗi) và `Directory.Packages.props` (phiên bản NuGet tập trung) ở gốc `apps/api` | ☑ |
| 18 | **Test** | Lệnh đo coverage | Đã chia `tests/Unit`, `tests/Integration` (TestServer), `tests/Architecture`. Còn lại: integration test bằng `WebApplicationFactory` chạy với MySQL thật (Testcontainers hoặc container Docker), không dùng provider InMemory của EF vì nó không kiểm được SQL, query filter và kiểu cột; đo coverage | ☐ |
| 19 | **CI** | (không có) | Workflow `api-ci.yml` chạy build và test, tương tự `mobile-ci.yml` | ☐ |

---

## P2: làm khi có nhu cầu

| Phần | Bên tham khảo có gì | GymBase |
| --- | --- | --- |
| Cache | `MemoryCacheService`, `RedisCacheService` | Interface đặt trong `GymBase.Base`, Redis thêm vào Docker khi cần |
| Job nền | Quartz (khóa người dùng, dọn thông báo, cảnh báo) | Chạy trong `GymBase.API` bằng hosted service trước; tách project worker khi cần scale riêng |
| Import/export | CsvHelper, EPPlus | Chức năng trong nhóm sở hữu dữ liệu (thường là BIZ) |
| Tích hợp ngoài | Mail, file, đồng bộ | Interface ở `ModuleContracts` hoặc BL; tách project riêng khi có lý do về ranh giới |
| Truy vấn báo cáo nặng | Dapper chạy song song EF Core | Khi LINQ không đủ: `FromSql` (có tham số) hoặc Dapper dùng chung connection của `DbContext`, chỉ trong `DL*.MySql` của nhóm sở hữu dữ liệu |

**Chưa cần cho GymBase lúc này:** API Gateway (Ocelot/YARP; chỉ có một host), WebSocket, Vault, middleware chặn IP, license phần cứng.

---

## Những chỗ trong dự án tham khảo **không nên chép theo**

| Vấn đề | Cách làm đúng ở GymBase |
| --- | --- |
| Lỗi trả về client kèm nguyên stack trace | ProblemDetails chỉ có mã lỗi và thông báo; stack trace chỉ ghi vào log |
| Nhiều khối `catch { }` nuốt lỗi | Luôn log lỗi hoặc ném tiếp, không bỏ qua im lặng |
| Mật khẩu chứng chỉ ghi trong code, mật khẩu DB ghi trong file text | Secret chỉ đặt trong user-secrets hoặc biến môi trường, không commit |
| CORS cho phép mọi nguồn (`AllowAll`) | Danh sách origin đọc từ cấu hình |
| Bỏ kiểm tra khóa ký JWT, bỏ qua kiểm tra chứng chỉ SSL | Luôn kiểm tra chữ ký, issuer, audience, thời hạn |
| `Database.Migrate()` chạy mỗi lần khởi động | Migration chạy bằng lệnh hoặc pipeline riêng |
| `BaseApiController` gán cứng route `api/nguoi-dung` | Route gốc `[controller]` như `BaseApiController` hiện tại, kèm tiền tố kênh `/api/v1/web` hoặc `/api/v1/mob` |
