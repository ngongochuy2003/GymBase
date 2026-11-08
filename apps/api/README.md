# GymBase API

Backend ASP.NET Core 10 (.NET 10 LTS, SDK ghim tại `global.json`), EF Core, MySQL 8.4.
Kiến trúc **modular monolith theo BL / DL**: một API host (`GymBase.API`) phục vụ cả web và
mobile, một solution `GymBase.slnx`. Chuẩn kiến trúc đầy đủ:
[`docs/api/cau-truc-thu-muc.md`](../../docs/api/cau-truc-thu-muc.md).

## Cấu trúc

```text
apps/api/
├── GymBase.slnx                  # Solution duy nhất
├── global.json                   # Ghim .NET SDK
├── Directory.Build.props         # net10.0, nullable, implicit usings, coi cảnh báo là lỗi
├── Directory.Packages.props      # Phiên bản NuGet tập trung (Central Package Management)
├── .editorconfig                 # Quy ước C# của backend
├── src/
│   ├── Host/GymBase.API/         # HOST DUY NHẤT: Controllers/ (web), Mob/Controllers/ (mobile)
│   ├── Base/
│   │   ├── GymBase.Base/         # Entity gốc, phân trang, CRUD dùng chung, exception
│   │   └── GymBase.Base.Hosting/ # Controller gốc, route theo kênh, xử lý lỗi ProblemDetails
│   ├── Contracts/
│   │   └── GymBase.ModuleContracts/  # Interface công khai giữa các nhóm
│   ├── Auth/                     # Tài khoản, xác thực, phân quyền: BLAuth, DLAuth.Contract, DLAuth.MySql
│   ├── DI/                       # Danh mục: BLDI, DLDI.Contract, DLDI.MySql
│   └── BIZ/                      # Nghiệp vụ PT, học viên, buổi tập: BLBiz, DLBiz.Contract, DLBiz.MySql
└── tests/
    ├── Unit/GymBase.UnitTests/
    ├── Integration/GymBase.IntegrationTests/
    └── Architecture/GymBase.ArchitectureTests/
```

"DI" trong tên nhóm nghĩa là **Danh mục**, không phải Dependency Injection.

## Luồng phụ thuộc

```text
Controllers/ (web) ─┐
                    ├─► BLAuth / BLDI / BLBiz ──► DL<nhóm>.Contract ◄── DL<nhóm>.MySql
Mob/Controllers/ ───┘
```

- Controller chỉ gọi BL. BL chỉ gọi `DL<nhóm>.Contract`, không biết EF/MySQL.
- `GymBase.API` tham chiếu các `DL*.MySql` chỉ để đăng ký dịch vụ.
- `AppDbContext` và DL mock dùng chung nằm trong `GymBase.DLBiz.MySql`; `DLAuth.MySql`,
  `DLDI.MySql` được tham chiếu riêng project này.
- `GymBase.ArchitectureTests` kiểm tra ma trận tham chiếu project mỗi lần chạy test.

## Lệnh

Chạy tại `apps/api`:

```bash
dotnet build GymBase.slnx
```

```bash
dotnet test GymBase.slnx
```

```bash
dotnet run --project src/Host/GymBase.API --launch-profile http
```

Host nghe ở `http://localhost:5020`. Route web: `/api/v1/web/...`, route mobile:
`/api/v1/mob/...`. Có `/health` và OpenAPI ở `/openapi/v1.json` (môi trường Development).
Hướng dẫn chạy chi tiết: [`docs/api/cach-chay-api.md`](../../docs/api/cach-chay-api.md).
Gọi thử API bằng collection Bruno ở `docs/api/bruno`.

## Tài liệu khác

| File | Nội dung |
| --- | --- |
| `docs/api/cau-truc-thu-muc.md` | Chuẩn kiến trúc: trách nhiệm từng project, ma trận tham chiếu, quy tắc review |
| `docs/api/crud-dung-chung.md` | Lớp nền CRUD và các bước thêm CRUD cho một bảng mới |
| `docs/api/quy-uoc-response.md` | Dạng response khi thành công và khi lỗi, bộ mã lỗi |
| `docs/api/viec-con-lai-base.md` | Phần base còn phải làm, quyết định đã chốt |

Chỉ cấu hình credentials qua biến môi trường hoặc user-secrets.
Không tự động migrate production khi API khởi động.
NuGet quản lý dependencies .NET; pnpm không build hoặc restore backend.
