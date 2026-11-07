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

**Mẫu đầu tiên đi đủ các tầng:** API người dùng `GET api/v1/users` (phân trang, tìm
kiếm) và `GET api/v1/users/{id}`, có ở cả `SystemAPI` (web) và `MobAPI` (mobile), cùng
gọi `UserBL` → `IUserDL`. **`UserDL` hiện trả dữ liệu mock trong bộ nhớ**; khi làm
mục 3 thì thay thân hàm bằng SQL, giữ nguyên interface.

---

## Quyết định cần chốt trước P0

| # | Quyết định | Đề xuất | Trạng thái |
| --- | --- | --- | --- |
| Q1 | .NET 9 hay .NET 10 (.NET 9 hết hỗ trợ ngày 10/11/2026) | .NET 10 LTS | Chưa chốt |
| Q2 | Quy ước tên bảng/cột | **Tiếng Việt không dấu snake_case**, property C# trùng tên cột (`tai_khoan`, `ho_ten`, `ngay_tao`, `is_deleted`); DTO trả ra API dùng cùng tên. Riêng khóa chính vẫn là `Id`. Ví dụ đầy đủ: entity `User` (bảng `nguoi_dung`) | **Đã chốt 08/11** |
| Q3 | Thư viện chạy SQL raw | Dapper + MySqlConnector | Chưa chốt |
| Q4 | Công cụ migration (không dùng EF nên không có EF migrations) | DbUp (chạy lần lượt các script `.sql`) hoặc FluentMigrator | Chưa chốt |
| Q5 | Phiên bản MySQL | 8.x LTS | Chưa chốt |
| Q6 | Kiểu khóa chính | UUIDv7 (`Guid.CreateVersion7()`) | **Đã chốt 08/11** |
| Q7 | Múi giờ lưu trữ | UTC, chỉ đổi sang giờ địa phương khi hiển thị | **Đã chốt 08/11** |
| Q8 | Xóa mềm | Cờ `is_deleted` (bool, mặc định false) | **Đã chốt 08/11** |

---

## P0: cần có trước khi viết nghiệp vụ đầu tiên

| # | Phần base | Bên tham khảo có gì | Đặt ở đâu trong GymBase | Xong |
| --- | --- | --- | --- | --- |
| 1 | Chốt Q1 và commit khung hiện tại | (.NET 6) | Toàn solution | ☐ |
| 2 | **Cấu hình và secrets** | `ConnectionStrings`, `Redis`, `Uri` trong appsettings | Options pattern. Connection string đặt trong user-secrets hoặc biến môi trường; `2.Config` chỉ chứa file mẫu | ☐ |
| 3 | **Kết nối DB** | Pomelo + Dapper, retry khi lỗi | `AMISBase.DL.Mysql`: connection factory (MySqlConnector), Dapper, transaction. Property trùng tên cột nên Dapper không cần cấu hình map tên. Lưu UUID bằng `BINARY(16)` và **kiểm chứng** thứ tự byte giữ được tính tăng dần của UUIDv7 (MySqlConnector `GuidFormat`). Đọc `DateTime` từ DB phải gắn `DateTimeKind.Utc` | ☐ |
| 4 | **Entity gốc** | `BaseEntity` (id Guid), `BaseAuditableEntity` (ngày/người tạo và sửa), soft delete `is_deleted` | **Đã làm** trong `AMISBase.DL.Contract/Entities`: `BaseEntity` (Id UUIDv7), `BaseAuditableEntity` (`ngay_tao`, `nguoi_tao`, `ngay_chinh_sua`, `nguoi_chinh_sua`), interface `ISoftDelete` (`is_deleted`) và `EntityAuditExtensions` (`MarkCreated`, `MarkUpdated` từ chối thời gian không phải UTC; `MarkDeleted`, `Restore`). Có test `EntityTests` | ☑ |
| 5 | **CRUD dùng chung** | `BaseCrudApiController` (GetAll, GetById, Create, Update, Delete) + `GetAllQueryHandler` | `IBaseDL<T>` và `BaseDL<T>` → `BaseBL<T>` → `BaseCrudController<T>`, mỗi phần nằm ở tầng tương ứng trong AMISBASE | ☐ |
| 6 | **Phân trang, lọc, sắp xếp, tìm kiếm** | `GetAllQuery` (page, page_size, sort, filter, search) và `ResponeListDto` + `Meta` (total, total_page, ranger) | **Một phần:** `PagingRequest` (page, pageSize, search; tự đưa về giá trị hợp lệ, pageSize tối đa 100) và `PagingResult<T>` (items, totalCount, totalPages) đã có trong `AMISBase.DL.Contract/Paging`. **Còn thiếu sort và filter.** Vì viết SQL tay, sort và filter phải giới hạn trong danh sách cột cho phép để tránh SQL injection | ◐ |
| 7 | **Exception và mã lỗi** | `ErrorCode` (404, 460–464), `NotFoundException`, `RejectException`, `DataExitsException` | Exception đặt trong `AMISBase.BL`. Global exception handler trả ProblemDetails đặt trong `AMISBase.API` | ☐ |
| 8 | **Validation** | FluentValidation chạy qua pipeline của MediatR | FluentValidation gọi trong `BaseBL` trước khi xuống DL | ☐ |
| 9 | **Map DTO ↔ entity** | AutoMapper | Mapster hoặc map tay. Từ 2025 AutoMapper chuyển sang license thương mại | ☐ |
| 10 | **Người dùng hiện tại** | `IdentityService` + `CurrentUserDto` lấy từ claims của JWT | Interface `ICurrentUser` trong `AMISBase.BL`, hiện thực ở `AMISBase.API`. Dùng để tự điền người tạo/sửa | ☐ |
| 11 | **Program.cs dùng chung** | `ConfigureServices`, Swagger có nút nhập Bearer, CORS, ForwardedHeaders | Extension `AddGymBaseApi()` / `UseGymBaseApi()` trong `AMISBase.API`, để 8 host không lặp lại cấu hình | ☐ |

---

## P1: hạ tầng chạy thật

| # | Phần base | Bên tham khảo có gì | GymBase | Xong |
| --- | --- | --- | --- | --- |
| 12 | **Auth** | Duende IdentityServer, JWT, middleware kiểm tra token, TOTP | JWT + refresh token trong `AuthAPI`, cấu hình xác thực dùng chung ở AMISBASE, phân quyền theo role/policy. Không dùng Duende vì cần license | ☐ |
| 13 | **Migration và seed** | EF migrations, các lớp `*DbContextInitial` seed dữ liệu lúc khởi động | Theo Q4. Seed dev đặt ở `database/seeds`. **Không** tự migrate khi khởi động production | ☐ |
| 14 | **Docker** | Compose gồm mysql, redis, vault, gateway, api, web; mỗi API có Dockerfile riêng | `infrastructure/docker`: MySQL trước, Redis sau. Dockerfile cho MobAPI, thêm `.dockerignore` | ☐ |
| 15 | **Logging** | NLog, bảng `system_log` (ai gọi lệnh gì, mất bao lâu) ghi qua `LoggingBehaviour` | Serilog hoặc NLog. Audit log ghi qua middleware hoặc action filter trong `AMISBase.API` | ☐ |
| 16 | **Health check DB** | (không có) | Thêm kiểm tra MySQL vào `/health` | ☐ |
| 17 | **Quy chuẩn code** | `.editorconfig` | `.editorconfig` + `Directory.Build.props` (nullable, coi cảnh báo là lỗi) ở gốc `apps/api` | ☐ |
| 18 | **Test** | Lệnh đo coverage | Unit test BL với DL giả lập (mock); integration test bằng `WebApplicationFactory`; đo coverage | ☐ |
| 19 | **CI** | (không có) | Workflow `api-ci.yml` chạy build và test, tương tự `mobile-ci.yml` | ☐ |

---

## P2: làm khi có nhu cầu

| Phần | Bên tham khảo có gì | GymBase |
| --- | --- | --- |
| Cache | `MemoryCacheService`, `RedisCacheService` | Interface đặt trong AMISBase, Redis thêm vào Docker khi cần |
| Job nền | Quartz (khóa người dùng, dọn thông báo, cảnh báo) | `7.Worker` |
| Import/export | CsvHelper, EPPlus | `3.Management/ImportExport` + tiện ích trong `5.Lib` |
| Tích hợp ngoài | Mail, file, đồng bộ | `6.Integration` |

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
| `BaseApiController` gán cứng route `api/nguoi-dung` | Route gốc `api/v1/[controller]` như `BaseApiController` hiện tại |
