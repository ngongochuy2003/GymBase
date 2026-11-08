# GymBase API

Backend ASP.NET Core 10 (.NET 10 LTS, SDK ghim tại `global.json`), MySQL. Solution duy nhất:
`apps/api/GymBase.slnx`. Cấu trúc theo mô hình Management / AMISBASE.

## Cấu trúc

Quy tắc đặt tên: **thư mục thật = tên solution folder bỏ dấu cách sau số thứ tự**
(ví dụ `3. Management/BIZ` → `3.Management/BIZ`, `99. AMISBASE` → `99.AMISBASE`).

```text
GymBase.slnx
global.json, .gitignore           # Bắt buộc ở gốc; không hiện trong solution
1.Doc/                            # 1. Doc — tài liệu riêng của backend (không có project)
2.Config/                         # 2. Config — cấu hình mẫu dùng chung (không có project)
3.Management/                     # 3. Management — mỗi nhóm: API / BL / DL.Contract / DL.Mysql
  Auth/                           # Xác thực, phân quyền            GymBase.AuthAPI         :5152
  Base/                           # Nền riêng của GymBase (toàn class library, không chạy được)
  BIZ/                            # Nghiệp vụ                       GymBase.BIZAPI          :5084
  DI/                             # Danh mục                        GymBase.DIAPI           :5055
  ImportExport/                   # Nhập/xuất Excel, CSV            GymBase.ImportExportAPI :5100
  Mob/                            # Chỉ có API cho mobile           GymBase.MobAPI          :5106
  Register/                       # Đăng ký, khởi tạo dữ liệu       GymBase.RegisterAPI     :5072
  Report/                         # Báo cáo, thống kê               GymBase.ReportAPI       :5150
  System/                         # Thiết lập hệ thống              GymBase.SystemAPI       :5020
4.Model/GymBase.Model             # 4. Model — entity/DTO/enum dùng chung
5.Lib/GymBase.Lib                 # 5. Lib — tiện ích không gắn nghiệp vụ
6.Integration/GymBase.Integration # 6. Integration — tích hợp hệ thống ngoài
7.Worker/GymBase.Worker           # 7. Worker — tiến trình chạy nền
8.Unitest/GymBase.UnitTests       # 8. Unitest
99.AMISBASE/                      # 99. AMISBASE — nền thêm/sửa/xóa dùng chung
  GymBase.AMISBase.API            # BaseApiController
  GymBase.AMISBase.BL
  GymBase.AMISBase.DL.Contract
  GymBase.AMISBase.DL.Mysql
```

Ví dụ nhóm BIZ gồm `GymBase.BIZAPI`, `GymBase.BLBIZ`, `GymBase.DLBIZ.Contract` và
`GymBase.DLBIZ.Mysql`; các nhóm khác đặt tên theo đúng mẫu đó. Hiện chỉ có khung:
các API host mới có `/health`, các class library còn rỗng. Giải thích chi tiết từng
thư mục: `docs/api/cau-truc-thu-muc.md`.

## Luồng phụ thuộc

```text
API  ──► BL ──► DL.Contract ◄── DL.Mysql
 └──────────────────────────────────┘   (API chỉ refer Mysql để đăng ký DI)
```

- Mỗi tầng refer tầng tương ứng trong AMISBASE.
- BL và DL.Contract **không** refer `*.Mysql`; `LayerReferenceTests` kiểm tra quy tắc này.
- Mobile: viết controller ở DIAPI/BIZAPI trước, sau đó thêm controller tương ứng ở
  MobAPI và gọi lại cùng BL → DL. MobAPI không chứa logic riêng.
- Nhóm mới trong Management dùng lại bộ 4 project API / BL / DL.Contract / DL.Mysql.
  `LayerReferenceTests` tự nhận project BL và Contract mới.
- `3.Management/Base` refer AMISBASE; các nhóm khác hiện vẫn refer thẳng AMISBASE.

## Lệnh

```bash
dotnet build GymBase.slnx
```

```bash
dotnet test GymBase.slnx
```

```bash
dotnet run --project 3.Management/BIZ/GymBase.BIZAPI
```

Mỗi host có `/health` và OpenAPI ở `/openapi/v1.json` (môi trường Development).
Route API có tiền tố kênh: host web dùng `/api/web/v1/...`, MobAPI dùng `/api/mobile/v1/...`.
Cần chạy host nào, cho mobile/điện thoại thật truy cập thế nào: xem `1.Doc/cach-chay-api.md`.

## Còn mở

- DB-01: chọn thư viện chạy SQL raw (Dapper hoặc ADO.NET) + MySQL driver, connection
  factory, migration và Docker; sau đó dựng CRUD dùng chung trong AMISBASE.
- API-02: validation, ProblemDetails, logging, chuẩn response.
- Phân các nghiệp vụ (Trainers, Trainees, Workouts, PtSessions, SessionPackages)
  vào DI hoặc BIZ.

Chỉ cấu hình credentials qua biến môi trường hoặc user-secrets.
Không tự động migrate production khi API khởi động.
NuGet quản lý dependencies .NET; pnpm không build hoặc restore backend.
