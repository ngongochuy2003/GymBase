# Danh sách phần base backend còn phải làm

Danh sách này lập ngày 08/11/2026, sau khi đối chiếu `apps/api` với dự án tham khảo
nội bộ **dev-HuyNgo**. Dự án đó chạy .NET 6, phần base nằm trong `SharedKernel.*`
và phần cấu hình của `App.Api`. Nó dùng MediatR, EF Core (Pomelo MySQL) kèm Dapper,
FluentValidation, AutoMapper, Duende IdentityServer, Redis và NLog.

Kiến trúc của dự án tham khảo khác GymBase (CQRS so với API / BL / DL). Danh sách
dưới đây **lấy các thành phần base của nó và đặt vào đúng tầng tương ứng của
GymBase**, không chép nguyên cấu trúc. Cấu trúc thư mục hiện tại xem tại
[cau-truc-thu-muc.md](cau-truc-thu-muc.md).

**Đã xong:** khung solution (API-01). Có 42 project, tham chiếu giữa các tầng đã
nối đúng chiều, mỗi API host có `/health`, và có test giữ quy tắc phân tầng.

**Mẫu đầu tiên đi đủ các tầng:** API người dùng. Web (`/api/web/v1/users`, SystemAPI) và
mobile (`/api/mobile/v1/users`, MobAPI) đều có đủ CRUD qua `BaseCrudController`. Cả hai cùng
gọi `UserBL : BaseBL` → `IUserDL : IBaseDL<nguoi_dung>`. Entity
`nguoi_dung` nằm ở `GymBase.Model/Entities/System`, DTO nằm ở `GymBase.BLSystem/NguoiDung`. **`UserDL` hiện lưu trong bộ nhớ** (`InMemoryBaseDL`);
khi làm mục 3 thì đổi lớp cha sang `BaseDL` dùng EF Core, giữ nguyên interface.

**Truy cập DB dùng EF Core** (chốt 08/11, thay cho hướng SQL raw + Dapper trước đó).
EF Core chỉ nằm trong các project `*.DL.Mysql`; BL vẫn chỉ thấy interface `IBaseDL` /
`I*DL` ở `*.DL.Contract`, không thấy `DbContext`.

---

## Quyết định cần chốt trước P0

| # | Quyết định | Đề xuất | Trạng thái |
| --- | --- | --- | --- |
| Q1 | .NET 9 hay .NET 10 (.NET 9 hết hỗ trợ ngày 10/11/2026) | .NET 10 LTS | **Đã chốt 08/11**, đã nâng lên |
| Q2 | Quy ước tên bảng/cột | **Tiếng Việt không dấu snake_case**, property C# trùng tên cột (`tai_khoan`, `ho_ten`, `ngay_tao`, `is_deleted`); DTO trả ra API dùng cùng tên. Riêng khóa chính vẫn là `Id`. Tên class entity trùng tên bảng. Ví dụ đầy đủ: entity `nguoi_dung` (bảng `nguoi_dung`) | **Đã chốt 08/11** |
| Q3 | Truy cập DB và provider MySQL | **EF Core** (đã chốt). Provider: `Pomelo.EntityFrameworkCore.MySql` (chạy trên MySqlConnector, dự án tham khảo cũng dùng) nếu đã có bản hỗ trợ EF Core 10; nếu chưa thì `MySql.EntityFrameworkCore` của Oracle. **Kiểm tra bản phát hành trước khi chốt provider** | EF Core **đã chốt 08/11**; provider chưa chốt |
| Q4 | Công cụ migration | **EF Core migrations** (đi cùng Q3). Áp dụng bằng script idempotent (`dotnet ef migrations script --idempotent`) hoặc migration bundle, chạy ở bước riêng | **Đã chốt 08/11** |
| Q9 | Số `DbContext` và chỗ đặt migrations | Một `GymBaseDbContext` cho cả DB (một DB, nhiều module). Lớp nền đặt ở `AMISBase.DL.Mysql`, nạp `IEntityTypeConfiguration` từ các assembly `DL*.Mysql` khi đăng ký. Migrations đặt trong một project riêng tham chiếu mọi `DL*.Mysql`, để `dotnet ef` có một chỗ duy nhất | Chưa chốt |
| Q5 | Phiên bản MySQL | 8.x LTS | Chưa chốt |
| Q6 | Kiểu khóa chính | UUIDv7 (`Guid.CreateVersion7()`) | **Đã chốt 08/11** |
| Q7 | Múi giờ lưu trữ | UTC, chỉ đổi sang giờ địa phương khi hiển thị | **Đã chốt 08/11** |
| Q8 | Xóa mềm | Cờ `is_deleted` (bool, mặc định false) | **Đã chốt 08/11** |

---

## P0: cần có trước khi viết nghiệp vụ đầu tiên

| # | Phần base | Bên tham khảo có gì | Đặt ở đâu trong GymBase | Xong |
| --- | --- | --- | --- | --- |
| 1 | Chốt Q1 và commit khung hiện tại | (.NET 6) | Toàn solution: đã lên .NET 10, khung đã commit | ☑ |
| 2 | **Cấu hình và secrets** | `ConnectionStrings`, `Redis`, `Uri` trong appsettings | Options pattern. Connection string đặt trong user-secrets hoặc biến môi trường; `2.Config` chỉ chứa file mẫu | ☐ |
| 3 | **Kết nối DB** | Pomelo + Dapper, retry khi lỗi | `AMISBase.DL.Mysql`: lớp nền `DbContext` (theo Q9) và extension đăng ký `AddDbContext` (provider theo Q3, bật retry khi lỗi tạm thời, `DbContext` scoped theo request). Trong `OnModelCreating` / `ConfigureConventions`: **global query filter** `!is_deleted` cho mọi entity `ISoftDelete`; **value converter** gắn `DateTimeKind.Utc` cho mọi `DateTime` đọc từ DB. Property trùng tên cột nên EF không cần cấu hình map tên cột; tên bảng đặt bằng `ToTable(nameof(entity))` trong configuration. Lưu UUID bằng `BINARY(16)` và **kiểm chứng** thứ tự byte giữ được tính tăng dần của UUIDv7 (`GuidFormat` của MySqlConnector nếu dùng Pomelo). Đọc dữ liệu mặc định `AsNoTracking()` | ☐ |
| 4 | **Entity gốc** | `BaseEntity` (id Guid), `BaseAuditableEntity` (ngày/người tạo và sửa), soft delete `is_deleted` | **Đã làm** trong `AMISBase.DL.Contract/Entities`: `BaseEntity` (Id UUIDv7), `BaseAuditableEntity` (`ngay_tao`, `nguoi_tao`, `ngay_chinh_sua`, `nguoi_chinh_sua`), interface `ISoftDelete` (`is_deleted`) và `EntityAuditExtensions` (`MarkCreated`, `MarkUpdated` từ chối thời gian không phải UTC; `MarkDeleted`, `Restore`). Có test `EntityTests` | ☑ |
| 5 | **CRUD dùng chung** | `BaseCrudApiController` (GetAll, GetById, Create, Update, Delete) + `GetAllQueryHandler` | **Khung đã làm:** `IBaseDL<TEntity>` (DL.Contract) → `BaseBL<TEntity, TDto, TCreateDto, TUpdateDto>` (BL, tự điền audit UTC, xóa mềm) → `BaseCrudController<...>` (API). DL tạm là `InMemoryBaseDL<TEntity>` lưu trong bộ nhớ. **Còn thiếu:** `BaseDL<TEntity>` dùng `DbSet<TEntity>` của EF Core, mỗi thao tác ghi gọi `SaveChangesAsync` (chờ mục 3). Lỗi nghiệp vụ đã có (mục 7): `BaseBL` ném `NotFoundException`, `UserBL` chặn trùng tên đăng nhập | ◐ |
| 6 | **Phân trang, lọc, sắp xếp, tìm kiếm** | `GetAllQuery` (page, page_size, sort, filter, search) và `ResponeListDto` + `Meta` (total, total_page, ranger) | **Một phần:** `PagingRequest` (`page`, `page_size`, `search`; tự đưa về giá trị hợp lệ, `page_size` tối đa 100, không có "lấy tất cả") và `PagingResult<T>` trả `{ data, meta }`, `meta` gồm `page`, `page_size`, `total`, `total_page`, `ranger` (`from`, `to`) theo dạng của dự án tham khảo (chốt 08/11; bỏ `Message`, `IsSuccess`, `Extra`) đã có trong `AMISBase.DL.Contract/Paging`. **Còn thiếu sort và filter.** Sort nhận dạng `sort=-ngay_tao,ho_ten` (dấu `-` là giảm dần), cột ngoài danh sách cho phép trả 400. Dựng bằng LINQ (`IQueryable`), EF tự tham số hóa giá trị. Tên cột sort/filter do client gửi vẫn phải giới hạn trong danh sách cột cho phép rồi map sang biểu thức; không ghép chuỗi vào `FromSqlRaw` | ◐ |
| 7 | **Exception và mã lỗi** | `ErrorCode` (404, 460–464), `NotFoundException`, `RejectException`, `DataExitsException` | **Đã làm 08/11.** Exception (`BusinessException` và 7 lớp con) và `ErrorCodes` trong `AMISBase.BL/Exceptions`; `AppExceptionHandler` + `ProblemDetailsCustomizer` trong `AMISBase.API/Errors`, 8 host gọi `AddAppErrorHandling()` / `UseAppErrorHandling()`. Mọi lỗi, kể cả lỗi framework tự trả, là ProblemDetails có `code`, `detail` tiếng Việt, `trace_id`; lỗi 500 không lộ chi tiết. Mobile đọc qua `toApiError`. Quy ước và bảng mã: [`apps/api/1.Doc/quy-uoc-response.md`](../../apps/api/1.Doc/quy-uoc-response.md). **Còn lại:** thông báo DataAnnotations tiếng Việt (mục 8); đổi lỗi vi phạm unique của DB thành `ConflictException` (mục 3) | ☑ |
| 8 | **Validation** | FluentValidation chạy qua pipeline của MediatR | FluentValidation gọi trong `BaseBL` trước khi xuống DL | ☐ |
| 9 | **Map DTO ↔ entity** | AutoMapper | Mapster hoặc map tay. Từ 2025 AutoMapper chuyển sang license thương mại | ☐ |
| 10 | **Người dùng hiện tại** | `IdentityService` + `CurrentUserDto` lấy từ claims của JWT | Interface `ICurrentUser` trong `AMISBase.BL`, hiện thực ở `AMISBase.API`. Dùng để tự điền người tạo/sửa | ☐ |
| 11 | **Program.cs dùng chung** | `ConfigureServices`, Swagger có nút nhập Bearer, CORS, ForwardedHeaders | Extension `AddAppApi()` / `UseAppApi()` trong `AMISBase.API`, để 8 host không lặp lại cấu hình | ☐ |

---

## P1: hạ tầng chạy thật

| # | Phần base | Bên tham khảo có gì | GymBase | Xong |
| --- | --- | --- | --- | --- |
| 12 | **Auth** | Duende IdentityServer, JWT, middleware kiểm tra token, TOTP | JWT + refresh token trong `AuthAPI`, cấu hình xác thực dùng chung ở AMISBASE, phân quyền theo role/policy. Không dùng Duende vì cần license | ☐ |
| 13 | **Migration và seed** | EF migrations, các lớp `*DbContextInitial` seed dữ liệu lúc khởi động | EF Core migrations theo Q4, đặt theo Q9. Dữ liệu danh mục cố định dùng `HasData` trong migration; dữ liệu mẫu cho dev đặt ở `database/seeds`, chạy có chủ đích. **Không** gọi `Database.Migrate()` khi khởi động production | ☐ |
| 14 | **Docker** | Compose gồm mysql, redis, vault, gateway, api, web; mỗi API có Dockerfile riêng | `infrastructure/docker`: MySQL trước, Redis sau. Dockerfile cho MobAPI, thêm `.dockerignore` | ☐ |
| 15 | **Logging** | NLog, bảng `system_log` (ai gọi lệnh gì, mất bao lâu) ghi qua `LoggingBehaviour` | Serilog hoặc NLog. Audit log ghi qua middleware hoặc action filter trong `AMISBase.API` | ☐ |
| 16 | **Health check DB** | (không có) | Thêm kiểm tra `DbContext` vào `/health` (`AddDbContextCheck`) | ☐ |
| 17 | **Quy chuẩn code** | `.editorconfig` | `.editorconfig` + `Directory.Build.props` (nullable, coi cảnh báo là lỗi) ở gốc `apps/api` | ☐ |
| 18 | **Test** | Lệnh đo coverage | Unit test BL với DL giả lập (mock); integration test bằng `WebApplicationFactory` chạy với MySQL thật (Testcontainers hoặc container Docker), không dùng provider InMemory của EF vì nó không kiểm được SQL, query filter và kiểu cột; đo coverage | ☐ |
| 19 | **CI** | (không có) | Workflow `api-ci.yml` chạy build và test, tương tự `mobile-ci.yml` | ☐ |

---

## P2: làm khi có nhu cầu

| Phần | Bên tham khảo có gì | GymBase |
| --- | --- | --- |
| Cache | `MemoryCacheService`, `RedisCacheService` | Interface đặt trong AMISBase, Redis thêm vào Docker khi cần |
| Job nền | Quartz (khóa người dùng, dọn thông báo, cảnh báo) | `7.Worker` |
| Import/export | CsvHelper, EPPlus | `3.Management/ImportExport` + tiện ích trong `5.Lib` |
| Tích hợp ngoài | Mail, file, đồng bộ | `6.Integration` |
| Truy vấn báo cáo nặng | Dapper chạy song song EF Core | Khi LINQ không đủ: `FromSql` (có tham số) hoặc Dapper dùng chung connection của `DbContext`, chỉ trong `DLReport.Mysql` |

**Chưa cần cho GymBase lúc này:** API Gateway (Ocelot/YARP; mobile chỉ gọi
MobAPI), WebSocket, Vault, middleware chặn IP, license phần cứng.

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
| `BaseApiController` gán cứng route `api/nguoi-dung` | Route gốc `v1/[controller]` như `BaseApiController` hiện tại, kèm tiền tố kênh `/api/web` hoặc `/api/mobile` |
