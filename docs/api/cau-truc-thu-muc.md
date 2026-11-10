# Chuẩn kiến trúc backend

Backend GymBase là **modular monolith theo mô hình BL / DL** trên .NET 10: một API host, một
solution, các nhóm chức năng là class library. Tài liệu này là chuẩn cho nhóm phát triển,
review PR và mở rộng V1 → V3. Cấu trúc trong `apps/api` hiện đã theo đúng chuẩn này.

## 1. Quyết định kiến trúc

| Quyết định | Chuẩn áp dụng |
| --- | --- |
| Kiểu triển khai | Modular monolith; **một API host** (`GymBase.API`); một Docker image cho V1 |
| Nhóm chức năng | Auth / DI (Danh mục) / BIZ, cùng phần nền Base |
| Tầng project | Mỗi nhóm có BL, DL.Contract, DL.MySql |
| Dữ liệu | Một MySQL 8.4 dùng chung ở V1; mỗi bảng thuộc đúng một nhóm |
| Entity, DbContext, migration | Gom một chỗ: entity ở `GymBase.Entities`, `AppDbContext`, cấu hình bảng và migrations ở `GymBase.Persistence` |
| Web và mobile | Dùng chung host, BL, DL; chỉ tách controller (`Controllers/` và `Mob/Controllers/`) |
| Định hướng | Không chuyển sang microservices sớm; chỉ tách module khi có nhu cầu thực tế |

**"DI" trong tên nhóm nghĩa là Danh mục, không phải Dependency Injection.** Khi nói về cơ chế
injection của .NET, tài liệu dùng "đăng ký dịch vụ (IoC)".

## 2. Cây thư mục

```text
apps/api/
├── GymBase.slnx                    # Solution duy nhất của backend
├── global.json                     # Ghim .NET SDK
├── Directory.Build.props           # net10.0, nullable, implicit usings, coi cảnh báo là lỗi
├── Directory.Packages.props        # Phiên bản NuGet tập trung
├── .editorconfig                   # Quy ước C# (namespace theo thư mục, tên snake_case cho entity/DTO)
├── src/
│   ├── Host/GymBase.API/           # HOST DUY NHẤT (Web SDK)
│   │   ├── Program.cs              # Composition root: đăng ký dịch vụ, middleware
│   │   ├── Controllers/            # Controller cho web       → /api/v1/web/...
│   │   ├── Mob/Controllers/        # Controller cho mobile    → /api/v1/mob/...
│   │   └── appsettings.json
│   ├── Base/
│   │   ├── GymBase.Base/           # Entity gốc, phân trang, CRUD dùng chung, exception
│   │   └── GymBase.Base.Hosting/   # Controller gốc, route theo kênh, xử lý lỗi ProblemDetails
│   ├── Contracts/
│   │   └── GymBase.ModuleContracts/    # Interface công khai cho giao tiếp giữa các nhóm
│   ├── Data/
│   │   ├── GymBase.Entities/       # Entity và enum của cột, chia theo nhóm: Auth/, DI/, BIZ/
│   │   └── GymBase.Persistence/    # AppDbContext, BaseRepository, Configurations/, Migrations/
│   ├── Auth/
│   │   ├── GymBase.BLAuth/
│   │   ├── GymBase.DLAuth.Contract/
│   │   └── GymBase.DLAuth.MySql/
│   ├── DI/
│   │   ├── GymBase.BLDI/
│   │   ├── GymBase.DLDI.Contract/
│   │   └── GymBase.DLDI.MySql/
│   └── BIZ/
│       ├── GymBase.BLBiz/
│       ├── GymBase.DLBiz.Contract/
│       └── GymBase.DLBiz.MySql/
└── tests/
    ├── Shared/GymBase.TestDoubles/ # DL giả lưu trong bộ nhớ, chỉ project test tham chiếu
    ├── Unit/GymBase.UnitTests/
    ├── Integration/GymBase.IntegrationTests/
    └── Architecture/GymBase.ArchitectureTests/
```

Ngoài `apps/api`: `database/` (seed, backup, script phụ trợ), `deploy/` (compose, Nginx, CI/CD),
`docs/` (tài liệu, ADR, collection Bruno ở `docs/api/bruno`).

Mỗi thư mục `GymBase.*` trong `src` là một project có `.csproj` riêng. Thư mục bên trong project
(`NguoiDung/`, `Paging/`…) **không** có `.csproj`. Không tạo project mới chỉ vì có
thêm thư mục hay chức năng; phải có lý do về ranh giới.

**Quy ước đặt tên:** `BIZ` cho thư mục nhóm, `Biz` trong tên project (`GymBase.BLBiz`);
`MySql` (không phải `Mysql` hay `MySQL`) trong tên project; `DI`, `Auth` giữ nguyên.

**Tên lớp theo chuẩn Service / Repository** (chốt 08/11): nghiệp vụ là `UserService`, truy cập dữ liệu là
`IUserRepository` / `UserRepository`, lớp nền là `BaseService`, `BaseRepository`. "BL" và "DL" chỉ còn là tên
tầng và tên project (`GymBase.BLAuth`, `GymBase.DLAuth.MySql`).

## 3. Trách nhiệm từng project

| Project | Được chứa | Không được chứa |
| --- | --- | --- |
| `GymBase.API` | `Controllers/` cho web, `Mob/Controllers/` cho mobile; request/response HTTP, policy, đăng ký dịch vụ, OpenAPI | Quy tắc nghiệp vụ, SQL, DbContext; BL/DL riêng cho mobile |
| `GymBase.Base` | Entity gốc (`BaseEntity`, `BaseAuditableEntity`, `ISoftDelete`), phân trang, CRUD dùng chung (`IBaseRepository`, `BaseService`, `ICrudService`), exception nghiệp vụ và mã lỗi | Logic PT, gói tập; tham chiếu tới Auth, DI, BIZ |
| `GymBase.Base.Hosting` | `BaseApiController`, `BaseCrudController`, route theo kênh, xử lý lỗi ProblemDetails, extension đăng ký dịch vụ nền | Quy tắc nghiệp vụ riêng |
| `GymBase.ModuleContracts` | Interface công khai và DTO tối thiểu cho giao tiếp giữa các nhóm | DbContext, repository nội bộ, controller |
| `GymBase.BLAuth` | Người dùng, đăng nhập, refresh/logout, phân quyền, chính sách tài khoản | Truy vấn EF trực tiếp, controller |
| `GymBase.Entities` | Entity và enum của cột, chia thư mục theo nhóm sở hữu bảng | Nghiệp vụ, EF Core |
| `GymBase.Persistence` | `AppDbContext`, `BaseRepository`, cấu hình bảng, migrations, đăng ký EF Core | Nghiệp vụ, repository riêng của từng bảng |
| `GymBase.DLAuth.Contract` | Interface DL (`IUserRepository`), danh sách cột sort/filter | Hiện thực MySQL |
| `GymBase.DLAuth.MySql` | Hiện thực DL của Auth, truy vấn | Quy tắc nghiệp vụ, gọi BLAuth |
| `GymBase.BLDI` | CRUD và ràng buộc danh mục, phân loại, kiểm tra mã danh mục | Quy tắc trừ buổi tập |
| `GymBase.DLDI.Contract` / `.MySql` | Interface DL của danh mục / hiện thực MySQL | Nghiệp vụ thanh toán, PT |
| `GymBase.BLBiz` | Nghiệp vụ PT, học viên, buổi tập, gói PT, nhật ký, khiếu nại | SQL, DbContext, logic HTTP |
| `GymBase.DLBiz.Contract` / `.MySql` | Interface DL của BIZ / hiện thực MySQL | Điều phối nghiệp vụ xuyên nhóm |

## 4. Bên trong BL và DL

Một chức năng có vị trí song song trong BL và DL. Mẫu đang có (người dùng):

```text
GymBase.BLAuth/
├── NguoiDung/
│   ├── NguoiDungDtos.cs        # DTO trả ra API và body thêm/sửa
│   └── UserService.cs               # Nghiệp vụ, kế thừa BaseService
└── AuthErrorCodes.cs           # Mã lỗi chi tiết của nhóm
GymBase.DLAuth.Contract/
└── IUserRepository.cs
GymBase.DLAuth.MySql/
└── UserRepository.cs                   # Hiện thực IUserRepository, kế thừa BaseRepository
GymBase.Entities/Auth/
└── nguoi_dung.cs               # Tên class trùng tên bảng, property trùng tên cột
GymBase.Persistence/Configurations/Auth/
└── NguoiDungConfiguration.cs   # Cấu hình bảng nguoi_dung
```

Chức năng lớn hơn trong BIZ thì chia thêm thư mục con, ví dụ:

```text
GymBase.BLBiz/
├── PTSessions/
│   ├── PTSessionService.cs          # Hoàn thành, hủy buổi tập…
│   ├── Rules/                  # Điều kiện chuyển trạng thái
│   ├── Dtos/
│   └── Validators/
└── SessionPackages/
    ├── SessionPackageService.cs     # Mua, trừ buổi, khiếu nại…
    └── Rules/                  # Điều kiện trừ gói, giới hạn buổi
GymBase.DLBiz.Contract/
├── PTSessions/IPTSessionRepository.cs
└── SessionPackages/ISessionPackageRepository.cs
GymBase.DLBiz.MySql/
└── PTSessions/PTSessionRepository.cs
```

Controller chỉ gọi BL. Interface DL nằm ở `DL<nhóm>.Contract`, hiện thực nằm ở `DL<nhóm>.MySql`.
Entity đặt ở `GymBase.Entities/<Nhóm>/`, cấu hình bảng ở `GymBase.Persistence/Configurations/<Nhóm>/`.

**Enum:**

| Enum dùng cho | Đặt ở |
| --- | --- |
| Cột trong bảng | `GymBase.Entities/<Nhóm>/Enums/`, lưu DB dạng chuỗi bằng `HasConversion<string>()` |
| Chỉ dùng trong nghiệp vụ hoặc DTO | Cạnh DTO trong `BL<nhóm>` |
| Nhóm khác cần dùng | `GymBase.ModuleContracts` |
| Hạ tầng chung | `GymBase.Base` |

**Ranh giới giữa các nhóm:** mọi nhóm cùng tham chiếu `GymBase.Entities`, nhưng BL và DL của một nhóm chỉ được dùng
entity của chính nhóm đó (`LayerReferenceTests` kiểm tra). Cần dữ liệu nhóm khác thì đi qua `ModuleContracts`.

## 5. Ma trận tham chiếu project

| Project | Được tham chiếu | Không được tham chiếu |
| --- | --- | --- |
| `GymBase.API` | `Base.Hosting`, mọi `BL*`; các `DL*.MySql` và `Persistence` **chỉ để đăng ký dịch vụ** | Controller gọi thẳng DL hoặc DbContext |
| `BL<nhóm>` | `Base`, `ModuleContracts`, `DL<cùng nhóm>.Contract` | `DL*.MySql`, API, BL nhóm khác |
| `GymBase.Entities` | `Base` | Mọi project khác |
| `GymBase.Persistence` | `Base`, `Entities`, EF Core | `BL*`, `DL*`, API |
| `DL<nhóm>.Contract` | `Base`, `Entities` | `BL*`, API, `DL*.MySql` |
| `DL<nhóm>.MySql` | `Base`, `DL<cùng nhóm>.Contract`, `Persistence` | `BL*`, API, Contract của nhóm khác |
| `GymBase.ModuleContracts` | `Base` | `BL*`, `DL*`, API, EF Core |
| `GymBase.Base` | Thư viện .NET chuẩn, FluentValidation | Auth, DI, BIZ, API |
| `GymBase.Base.Hosting` | `Base`, ASP.NET Core | Logic nghiệp vụ BIZ |

`GymBase.ArchitectureTests/LayerReferenceTests` kiểm tra bảng này mỗi lần chạy test, tự quét mọi
`.csproj` trong `src/`, nên nhóm hay project mới không cần sửa test.

**BIZ cần dữ liệu danh mục:** `BLBiz` phụ thuộc interface tra cứu công khai trong
`ModuleContracts`; `BLDI` hiện thực interface đó và được host đăng ký. Không tham chiếu `BLDI`
trực tiếp để tránh phụ thuộc chéo.

## 6. Controller cho web và mobile

Web và mobile dùng chung `GymBase.API` và toàn bộ BL/DL; chỉ tách lớp HTTP. Không tạo
`BLMob`, `DLMob`, cũng không dựng host thứ hai.

```text
GymBase.API/
├── Controllers/                    # WEB  → GymBase.API.Controllers
│   └── UsersController.cs
└── Mob/Controllers/                # MOBILE → GymBase.API.Mob.Controllers
    └── UsersController.cs

Cả hai nhóm controller → BLAuth / BLDI / BLBiz → DL<nhóm>.Contract ◄── DL<nhóm>.MySql
```

- **Route:** controller chỉ khai báo `[controller]` (kế thừa `BaseApiController`).
  `ChannelRouteConvention` gắn tiền tố theo namespace: `api/v1/mob` nếu namespace có đoạn `Mob`,
  còn lại `api/v1/web`. Ví dụ `/api/v1/web/users` và `/api/v1/mob/users`.
- Controller web và mobile được **trùng tên** ở hai namespace. Kênh được gắn làm route value
  `area`, nên link sinh ra (header `Location` của `POST`) luôn đúng kênh.
- `Mob/Controllers/` được tùy chỉnh response, DTO hoặc gộp dữ liệu cho app, nhưng gọi cùng BL và
  **không chép logic nghiệp vụ**.
- DTO HTTP riêng của từng kênh (khi schema khác nhau) đặt cạnh controller của kênh đó; không đưa
  DTO thuần HTTP xuống DL.
- Quy tắc khác nhau theo vai trò thì kiểm tra theo user, role và policy, không dựa vào đường dẫn
  `/mob`. **Tiền tố `mob` không phải ranh giới bảo mật**: backend phải kiểm tra quyền cho cả hai kênh.
- Endpoint có cùng contract và hành vi trên cả hai kênh thì không bắt buộc tạo bản sao cho Mob.

Ví dụ luồng:

```text
GET /api/v1/web/users/{id} → Controllers/UsersController     ┐
GET /api/v1/mob/users/{id} → Mob/Controllers/UsersController ┴→ UserService.GetByIdAsync → IUserRepository → MySQL
```

## 7. Ví dụ một luồng nghiệp vụ V1: PT hoàn thành buổi tập

1. PT gọi `POST /api/v1/mob/pt-sessions/{id}/complete`; API kiểm tra xác thực rồi gọi `BLBiz`.
2. `BLBiz` tải buổi tập và gói qua interface trong `DLBiz.Contract`; kiểm tra đúng PT, đúng trạng
   thái, buổi có PT trực tiếp, gói còn lượt.
3. `BLBiz` cập nhật trạng thái và số buổi trong cùng một unit of work; chống hoàn thành hoặc trừ
   buổi hai lần bằng điều kiện trạng thái hoặc concurrency token.
4. `DLBiz.MySql` lưu mọi thay đổi trong một transaction; ghi audit/outbox trong cùng transaction
   nếu cần.
5. API trả response theo [quy-uoc-response.md](quy-uoc-response.md); thông báo gửi bất đồng bộ
   sau commit qua outbox/worker nếu có.

Quy tắc V1: chỉ trừ lượt khi có PT trực tiếp; PT xác nhận hoàn thành; học viên được khiếu nại;
cả PT và học viên nhập nhật ký nhưng phải có lịch sử chỉnh sửa. Các trường hợp đồng thời phải có test.

## 8. Sở hữu dữ liệu và migration

- Auth sở hữu tài khoản, refresh session và các bảng xác thực. DI sở hữu bảng danh mục. BIZ sở hữu
  bảng nghiệp vụ. Không sửa bảng của nhóm khác nếu chưa qua review.
- Một DB, **một `AppDbContext`** (trong `GymBase.Persistence`), tự nạp mọi `IEntityTypeConfiguration` trong
  project đó. Không có hai DbContext cùng migrate một bảng.
- Migration phải được review, đặt tên rõ, chỉ roll forward; không sửa migration đã áp dụng trên
  môi trường dùng chung. Không gọi `Database.Migrate()` khi khởi động production.
- Seed danh mục phải idempotent. Không lưu bí mật trong `appsettings.json` hay trong repo.

## 9. Build, test, deploy

Chạy tại `apps/api`:

```bash
dotnet restore GymBase.slnx
```

```bash
dotnet build GymBase.slnx -c Release --no-restore
```

```bash
dotnet test GymBase.slnx -c Release --no-restore
```

```bash
dotnet run --project src/Host/GymBase.API
```

| Tệp / công cụ | Quy định |
| --- | --- |
| `global.json` | Ghim SDK; CI và máy dev dùng cùng SDK |
| `Directory.Build.props` | Nullable, implicit usings, coi cảnh báo là lỗi, cấu hình project test |
| `Directory.Packages.props` | Central Package Management; `.csproj` chỉ ghi tên gói, không ghi phiên bản |
| `.editorconfig` | Định dạng, đặt tên, cảnh báo; thống nhất trước khi review |
| CI | Restore → Build → Unit/Architecture test → Integration test → Docker build |
| Docker | Build từ `apps/api` bằng một Dockerfile multi-stage; chỉ publish `GymBase.API` |

## 10. Quy tắc review bắt buộc

1. Controller trong `Controllers/` và `Mob/Controllers/` chỉ gọi BL; không chứa SQL hay xử lý trừ buổi PT.
2. BL chỉ tham chiếu `DL<nhóm>.Contract` hoặc `ModuleContracts`; không dùng trực tiếp EF hay MySQL.
3. `DL*.MySql` không tham chiếu BL, không cài quy tắc nghiệp vụ.
4. `Base` không thành "sọt chung" cho DTO và logic của BIZ.
5. DI (Danh mục) chỉ quản lý dữ liệu danh mục và quy tắc nội tại của danh mục.
6. Không tạo `.csproj` mới chỉ vì có thêm thư mục hay chức năng.
7. Không có tham chiếu vòng; mọi project restore và build được từ bản clone mới.
8. Nghiệp vụ nhiều bảng phải có transaction và test chạy đồng thời; mọi bug sửa phải kèm test hồi quy.
9. Không commit mật khẩu, JWT secret, connection string thật.
10. PR đổi schema phải kèm migration và mô tả ảnh hưởng tương thích.

## 11. Thêm chức năng mới

### Thêm danh mục "Gói tập" (nhóm DI)

Làm theo mẫu người dùng đang có. Hướng dẫn kèm code: [huong-dan-them-crud.md](huong-dan-them-crud.md).

| Bước | Project | File | Nội dung |
| --- | --- | --- | --- |
| 1 | `GymBase.Entities` | `DI/goi_tap.cs` | `class goi_tap : BaseAuditableEntity, ISoftDelete`, property trùng tên cột |
| 2 | `GymBase.DLDI.Contract` | `IGoiTapRepository.cs` | `interface IGoiTapRepository : IBaseRepository<goi_tap>` |
| 3 | `GymBase.DLDI.MySql`, `GymBase.Persistence` | `GoiTapRepository.cs`, `Configurations/DI/GoiTapConfiguration.cs` | `class GoiTapRepository : BaseRepository<goi_tap>, IGoiTapRepository`; cấu hình bảng; tạo migration mới |
| 4 | `GymBase.BLDI` | `GoiTap/GoiTapDtos.cs`, `GoiTap/GoiTapValidators.cs`, `GoiTap/GoiTapService.cs` | DTO và validator; `GoiTapService : BaseService<...>` viết 3 hàm map, quy tắc riêng thì override |
| 5 | `GymBase.API` | `Controllers/GoiTapController.cs` | Web: kế thừa `BaseCrudController<...>` là đủ 5 endpoint |
| 6 | `GymBase.API` | `Mob/Controllers/GoiTapController.cs` | Mobile (nếu app cần): như trên, gọi lại `GoiTapService` |
| 7 | `GymBase.API` | `Program.cs` | Đăng ký `IGoiTapRepository → GoiTapRepository`, `GoiTapService` |
| 8 | `GymBase.UnitTests` | `DI/GoiTapServiceTests.cs` | Test BL trên DL giả (`GymBase.TestDoubles`), mỗi test một `InMemoryStore` riêng |

### Thêm một nhóm mới

Chỉ thêm nhóm khi ranh giới đủ độc lập; V2 ưu tiên thêm chức năng vào nhóm sẵn có (thường là BIZ).
Khi cần, tạo bộ 3 project `GymBase.BL<Nhóm>`, `GymBase.DL<Nhóm>.Contract`, `GymBase.DL<Nhóm>.MySql`
trong `src/<Nhóm>/`, tham chiếu theo bảng ở mục 5, thêm vào `GymBase.slnx`, rồi chạy
`dotnet test GymBase.slnx`. `LayerReferenceTests` tự nhận các project mới.

## 12. Còn phải chốt

- **Tên route:** route lấy theo tên controller, chữ thường (`/users`). Tên nhiều từ hiện viết liền
  (`/goitap`). Có dùng dạng kebab-case (`/pt-sessions`) hay không thì chốt trước khi thêm controller đó.
