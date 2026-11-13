# Hướng dẫn thêm CRUD cho một bảng mới

Tài liệu này dành cho người mới vào dự án. Bạn không cần biết trước cấu trúc code của GymBase.
Làm lần lượt từ đầu đến cuối, bạn sẽ có một API hoàn chỉnh để xem, thêm, sửa và xóa dữ liệu của một bảng.

Ví dụ xuyên suốt là danh mục **Gói tập**, bảng `goi_tap`. Khi làm bảng của bạn, thay `goi_tap` và
`GoiTap` bằng tên tương ứng. Mọi đường dẫn file trong tài liệu tính từ thư mục `apps/api`.

## 1. Bạn sẽ có gì khi làm xong

| Yêu cầu | Tác dụng |
| --- | --- |
| `GET /api/v1/web/goitap` | Lấy danh sách có phân trang và tìm kiếm |
| `GET /api/v1/web/goitap/{id}` | Xem một gói tập |
| `POST /api/v1/web/goitap` | Thêm gói tập |
| `PUT /api/v1/web/goitap/{id}` | Sửa gói tập |
| `DELETE /api/v1/web/goitap/{id}` | Xóa gói tập |

CRUD là viết tắt của Create, Read, Update, Delete: thêm, đọc, sửa, xóa. Phần lớn logic đã có sẵn
trong các lớp nền. Bạn chỉ khai báo những gì riêng của bảng mình, như có cột nào, cột nào được sửa
và dữ liệu thế nào là hợp lệ.

## 2. Chuẩn bị

Bạn cần chạy được API và có tài khoản Admin. Nếu chưa có, làm theo phần "Chuẩn bị MySQL" trong
[cach-chay-api.md](cach-chay-api.md). Kiểm tra bằng cách chạy API rồi mở
`http://localhost:5020/health`: thấy `Healthy` là được.

Bạn cũng cần xác định bảng thuộc **nhóm** nào, vì nhóm quyết định chỗ đặt file:

| Nhóm | Chứa gì | Ví dụ |
| --- | --- | --- |
| `Auth` | Tài khoản, đăng nhập | `tai_khoan` |
| `DI` | Danh mục | `goi_tap` |
| `BIZ` | Nghiệp vụ chính | |

> Trong tên nhóm, **DI nghĩa là Danh mục**, không phải Dependency Injection.

## 3. Các khái niệm cần biết

| Tên gọi | Nói đơn giản | Trong ví dụ |
| --- | --- | --- |
| **Entity** | Class mô tả một bảng trong database. Mỗi property là một cột | `goi_tap` |
| **Migration** | File do công cụ sinh ra, chứa lệnh tạo hoặc đổi bảng trong MySQL | `AddGoiTap` |
| **Repository** | Lớp duy nhất đọc và ghi bảng | `GoiTapRepository` |
| **DTO** | Dữ liệu đi qua API. Tách khỏi entity để không lộ cột nội bộ | `GoiTapDto` |
| **Validator** | Luật kiểm tra dữ liệu gửi lên, ví dụ giá không được âm | `GoiTapCreateValidator` |
| **Service** | Nơi đặt nghiệp vụ, nằm giữa controller và repository | `GoiTapService` |
| **Controller** | Lớp nhận yêu cầu HTTP, gọi service rồi trả kết quả | `GoiTapController` |
| **Đăng ký dịch vụ** | Khai báo trong `Program.cs` để ứng dụng biết cần tạo lớp nào | Bước 7 |

Một yêu cầu đi qua các lớp theo thứ tự này:

```text
Web hoặc app
   │  POST /api/v1/web/goitap
   ▼
Controller   nhận yêu cầu
   ▼
Service      kiểm tra dữ liệu, tự điền ngày tạo và người tạo
   ▼
Repository   ghi xuống database
   ▼
MySQL        bảng goi_tap
```

## 4. Danh sách việc phải làm

| Bước | Tạo hoặc sửa | Project |
| --- | --- | --- |
| 1 | Entity `goi_tap.cs` | `GymBase.Entities` |
| 2 | Cấu hình bảng `GoiTapConfiguration.cs` và migration | `GymBase.Persistence` |
| 3 | Repository `IGoiTapRepository.cs`, `GoiTapRepository.cs` | thư mục `DLGoiTap/` trong `GymBase.DLDI.Contract` và `GymBase.DLDI.MySql` |
| 4 | DTO `GoiTapDtos.cs` | thư mục `BLGoiTap/` trong `GymBase.BLDI` |
| 5 | Validator `GoiTapValidators.cs` và service `GoiTapService.cs` | thư mục `BLGoiTap/` trong `GymBase.BLDI` |
| 6 | Controller `GoiTapController.cs` | `GymBase.API` |
| 7 | Đăng ký trong `Program.cs` | `GymBase.API` |
| 8 | Chạy thử | |
| 9 | Viết test và request Bruno | `GymBase.UnitTests`, `docs/api/bruno` |

Các project `BLDI`, `DLDI.Contract`, `DLDI.MySql` đã có sẵn và đã tham chiếu đúng, bạn không phải tạo mới.

## 5. Làm từng bước

### Bước 1: Tạo entity

Entity mô tả bảng `goi_tap`.

```csharp
// src/Data/GymBase.Entities/DI/goi_tap.cs
using GymBase.Base.Entities;

namespace GymBase.Entities.DI;

/// <summary>Gói tập mà phòng gym bán cho học viên.</summary>
public sealed class goi_tap : BaseAuditableEntity
{
    public string ma_goi { get; set; } = string.Empty;
    public string ten_goi { get; set; } = string.Empty;
    public decimal gia { get; set; }
}
```

Những điểm cần chú ý:

- **Tên class và property viết giống tên bảng và tên cột**, nên dùng `snake_case`. Đây là quy ước
  của dự án, không phải lỗi đặt tên.
- `BaseAuditableEntity` cho bạn sẵn các cột `Id`, `ngay_tao`, `nguoi_tao`, `ngay_chinh_sua` và
  `nguoi_chinh_sua`. Không khai báo lại các cột này.
- `DELETE` xóa hẳn dòng khỏi bảng. Dòng đang được bảng khác tham chiếu thì API trả 409 `IN_USE`.

### Bước 2: Cấu hình bảng và tạo migration

Lớp cấu hình cho biết tên bảng và độ dài từng cột.

```csharp
// src/Data/GymBase.Persistence/Configurations/DI/GoiTapConfiguration.cs
using GymBase.Entities.DI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymBase.Persistence.Configurations.DI;

/// <summary>Bảng <c>goi_tap</c>.</summary>
public sealed class GoiTapConfiguration : IEntityTypeConfiguration<goi_tap>
{
    public void Configure(EntityTypeBuilder<goi_tap> builder)
    {
        builder.ToTable(nameof(goi_tap));

        builder.Property(e => e.ma_goi).HasMaxLength(20);
        builder.Property(e => e.ten_goi).HasMaxLength(100);
        builder.Property(e => e.gia).HasPrecision(12, 2);
    }
}
```

`HasPrecision(12, 2)` nghĩa là tổng cộng 12 chữ số, trong đó có 2 chữ số sau dấu phẩy. Lớp này được
nạp tự động, không cần đăng ký ở đâu.

Tiếp theo tạo migration và áp dụng vào database. Chạy hai lệnh này tại `apps/api`:

```bash
dotnet ef migrations add AddGoiTap --project src/Data/GymBase.Persistence --startup-project src/Host/GymBase.API
```

```bash
dotnet ef database update --project src/Data/GymBase.Persistence --startup-project src/Host/GymBase.API
```

Lệnh đầu sinh file trong `src/Data/GymBase.Persistence/Migrations`. Mở file đó xem nhanh, phải thấy
`CreateTable` với `name: "goi_tap"`. Commit cả các file migration. Lệnh sau tạo bảng thật trong MySQL.

### Bước 3: Tạo repository

Interface mô tả những gì lớp nghiệp vụ được phép gọi. Interface rỗng vẫn có đủ thao tác CRUD, vì đã
kế thừa từ `IBaseRepository`.

```csharp
// src/DI/GymBase.DLDI.Contract/DLGoiTap/IGoiTapRepository.cs
using GymBase.Base.Crud;
using GymBase.Entities.DI;

namespace GymBase.DLDI.Contract.DLGoiTap;

/// <summary>Truy cập dữ liệu gói tập.</summary>
public interface IGoiTapRepository : IBaseRepository<goi_tap>
{
}
```

Lớp hiện thực dùng lại toàn bộ `BaseRepository`. Bạn chỉ cần chọn cột được tìm kiếm.

```csharp
// src/DI/GymBase.DLDI.MySql/DLGoiTap/GoiTapRepository.cs
using System.Linq.Expressions;
using GymBase.DLDI.Contract.DLGoiTap;
using GymBase.Entities.DI;
using GymBase.Persistence;

namespace GymBase.DLDI.MySql.DLGoiTap;

/// <summary>Truy cập bảng <c>goi_tap</c>.</summary>
public sealed class GoiTapRepository(AppDbContext db) : BaseRepository<goi_tap>(db), IGoiTapRepository
{
    protected override IReadOnlyList<Expression<Func<goi_tap, string?>>> SearchColumns { get; } =
        [e => e.ma_goi, e => e.ten_goi];
}
```

Nhờ `SearchColumns`, `GET /goitap?search=vip` sẽ tìm trong mã gói và tên gói. Không khai báo thì tham
số `search` bị bỏ qua.

### Bước 4: Tạo DTO

Mỗi bảng có 3 DTO, ứng với 3 dạng dữ liệu đi qua API.

```csharp
// src/DI/GymBase.BLDI/BLGoiTap/GoiTapDtos.cs
using GymBase.Base.Entities;

namespace GymBase.BLDI.BLGoiTap;

/// <summary>Dữ liệu API trả về.</summary>
public sealed record GoiTapDto(Guid Id, string ma_goi, string ten_goi, decimal gia, DateTime ngay_tao) : IHasId;

/// <summary>Dữ liệu gửi lên khi thêm.</summary>
public sealed class GoiTapCreateDto
{
    public string ma_goi { get; init; } = string.Empty;
    public string ten_goi { get; init; } = string.Empty;
    public decimal gia { get; init; }
}

/// <summary>Dữ liệu gửi lên khi sửa. Không có ma_goi nên mã gói không sửa được.</summary>
public sealed class GoiTapUpdateDto
{
    public string ten_goi { get; init; } = string.Empty;
    public decimal gia { get; init; }
}
```

Việc tách DTO giúp người gọi API không thể tự gửi `Id` hay `ngay_tao`. Muốn một cột
không sửa được, chỉ cần không đưa nó vào `GoiTapUpdateDto`.

### Bước 5: Tạo validator và service

Validator kiểm tra dữ liệu trước khi xử lý. `WithName` là tên cột hiển thị trong câu báo lỗi tiếng Việt.

```csharp
// src/DI/GymBase.BLDI/BLGoiTap/GoiTapValidators.cs
using FluentValidation;
using GymBase.Base.Validation;

namespace GymBase.BLDI.BLGoiTap;

public sealed class GoiTapCreateValidator : AppValidator<GoiTapCreateDto>
{
    public GoiTapCreateValidator()
    {
        RuleFor(x => x.ma_goi).NotEmpty().MaximumLength(20).WithName("Mã gói");
        RuleFor(x => x.ten_goi).NotEmpty().MaximumLength(100).WithName("Tên gói");
        RuleFor(x => x.gia).GreaterThanOrEqualTo(0).WithName("Giá");
    }
}

public sealed class GoiTapUpdateValidator : AppValidator<GoiTapUpdateDto>
{
    public GoiTapUpdateValidator()
    {
        RuleFor(x => x.ten_goi).NotEmpty().MaximumLength(100).WithName("Tên gói");
        RuleFor(x => x.gia).GreaterThanOrEqualTo(0).WithName("Giá");
    }
}
```

Giữ `MaximumLength` bằng đúng độ dài cột đã khai báo ở bước 2.

Service kế thừa `BaseService`, và bạn viết 3 hàm chuyển đổi giữa entity và DTO:

```csharp
// src/DI/GymBase.BLDI/BLGoiTap/GoiTapService.cs
using FluentValidation;
using GymBase.Base.Auth;
using GymBase.Base.Crud;
using GymBase.DLDI.Contract.DLGoiTap;
using GymBase.Entities.DI;

namespace GymBase.BLDI.BLGoiTap;

/// <summary>Nghiệp vụ gói tập, dùng chung cho controller web và mobile.</summary>
public sealed class GoiTapService(
    IGoiTapRepository goiTapRepository,
    TimeProvider timeProvider,
    ICurrentUser currentUser,
    IValidator<GoiTapCreateDto> createValidator,
    IValidator<GoiTapUpdateDto> updateValidator)
    : BaseService<goi_tap, GoiTapDto, GoiTapCreateDto, GoiTapUpdateDto>(
        goiTapRepository, timeProvider, currentUser, createValidator, updateValidator)
{
    protected override string EntityDisplayName => "gói tập";

    protected override GoiTapDto BLToDto(goi_tap e) => new(e.Id, e.ma_goi, e.ten_goi, e.gia, e.ngay_tao);

    protected override goi_tap BLCreateEntity(GoiTapCreateDto dto) =>
        new() { ma_goi = dto.ma_goi.Trim(), ten_goi = dto.ten_goi.Trim(), gia = dto.gia };

    protected override void BLApplyUpdate(goi_tap e, GoiTapUpdateDto dto)
    {
        e.ten_goi = dto.ten_goi.Trim();
        e.gia = dto.gia;
    }
}
```

| Hàm hoặc property | Khi nào chạy | Bạn viết gì |
| --- | --- | --- |
| `BLToDto` | Trước khi trả dữ liệu | Chọn các cột trả ra API |
| `BLCreateEntity` | Khi thêm | Chép từ DTO sang entity. Không gán `Id`, `ngay_tao` |
| `BLApplyUpdate` | Khi sửa | Chỉ chép những cột được phép sửa |
| `EntityDisplayName` | Khi không tìm thấy | Tên trong câu báo lỗi, ví dụ "Không tìm thấy gói tập…" |

`BaseService` tự lo phần còn lại: chạy validator, điền ngày và người tạo hoặc sửa, báo 404.

### Bước 6: Tạo controller

Controller chỉ cần vài dòng, vì 5 API đã có sẵn trong `BaseCrudController`.

```csharp
// src/Host/GymBase.API/Controllers/GoiTapController.cs
using GymBase.Base.Auth;
using GymBase.Base.Hosting;
using GymBase.BLDI.BLGoiTap;
using Microsoft.AspNetCore.Authorization;

namespace GymBase.API.Controllers;

/// <summary>API quản lý gói tập cho web: /api/v1/web/goitap, chỉ Admin.</summary>
[Authorize(Roles = AppRoles.Admin)]
public sealed class GoiTapController(GoiTapService goiTapService)
    : BaseCrudController<GoiTapDto, GoiTapCreateDto, GoiTapUpdateDto>(goiTapService);
```

- **Đường dẫn** lấy từ tên class bỏ chữ `Controller` rồi viết thường: `GoiTapController` thành `/goitap`.
- **Tiền tố** lấy từ thư mục. Đặt trong `Controllers/` thì là `/api/v1/web`, đặt trong
  `Mob/Controllers/` thì là `/api/v1/mob`.
- **Quyền truy cập:** dòng `[Authorize(Roles = AppRoles.Admin)]` chỉ cho Admin gọi. Bỏ dòng này thì mọi
  người đã đăng nhập đều xem, sửa, xóa được. Vì vậy hãy luôn cân nhắc quyền cho từng controller.

App mobile cần API này thì tạo thêm một file tương tự trong `src/Host/GymBase.API/Mob/Controllers/`, với
`namespace GymBase.API.Mob.Controllers`. Chưa cần thì bỏ qua.

### Bước 7: Đăng ký trong `Program.cs`

Mở `src/Host/GymBase.API/Program.cs` và sửa 3 chỗ.

**Thêm `using` ở đầu file:**

```csharp
using GymBase.BLDI.BLGoiTap;
using GymBase.DLDI.Contract.DLGoiTap;
using GymBase.DLDI.MySql.DLGoiTap;
```

**Thêm assembly của BLDI vào `AddAppApi`**, để ứng dụng tìm thấy validator. Mỗi BL chỉ cần làm một lần:

```csharp
builder.Services.AddAppApi(builder.Configuration, typeof(TaiKhoanService).Assembly, typeof(GoiTapService).Assembly);
```

**Đăng ký repository và service**, đặt sau các dòng đăng ký của Auth:

```csharp
// Danh mục: gói tập
builder.Services.AddScoped<IGoiTapRepository, GoiTapRepository>();
builder.Services.AddScoped<GoiTapService>();
```

### Bước 8: Chạy thử

Chạy API:

```bash
dotnet run --project src/Host/GymBase.API --launch-profile http
```

Mở [Bruno](https://www.usebruno.com/), mở collection ở `docs/api/bruno`, chọn environment **local**
rồi chạy request **Đăng nhập** trong thư mục `Web > Xác thực` để lấy token. Sau đó tạo request mới:

- Method `POST`, URL `{{webApi}}/goitap`
- Auth chọn **Bearer**, token là `{{accessToken}}`
- Body chọn **JSON**:

```json
{
  "ma_goi": "G01",
  "ten_goi": "Gói 1 tháng",
  "gia": 500000
}
```

Thành công sẽ nhận mã **201** và dữ liệu như sau:

```json
{
  "id": "019a0000-0000-7000-8000-000000000001",
  "ma_goi": "G01",
  "ten_goi": "Gói 1 tháng",
  "gia": 500000,
  "ngay_tao": "2026-11-09T15:00:00Z"
}
```

Thử tiếp các tình huống sau để chắc mọi thứ đúng:

| Thử | Kết quả mong đợi |
| --- | --- |
| `GET {{webApi}}/goitap` | 200, gói vừa thêm nằm trong `data` |
| `GET {{webApi}}/goitap?search=G01` | 200, tìm thấy gói vừa thêm |
| `POST` với `"gia": -1` | 400, kèm lỗi ở trường `gia` |
| `PUT {{webApi}}/goitap/{id}` với `ten_goi` và `gia` mới | 200, dữ liệu đã đổi |
| `DELETE {{webApi}}/goitap/{id}` | 204 |
| `GET {{webApi}}/goitap/{id}` sau khi xóa | 404 |
| Gọi mà không có token | 401 |

### Bước 9: Viết test và lưu request Bruno

Test kiểm tra service mà không cần MySQL, nhờ một repository giả lưu trong bộ nhớ.

Thêm dòng này vào `tests/Unit/GymBase.UnitTests/GymBase.UnitTests.csproj`, trong nhóm `ProjectReference`.
Mỗi nhóm chỉ cần thêm một lần:

```xml
<ProjectReference Include="..\..\..\src\DI\GymBase.BLDI\GymBase.BLDI.csproj" />
```

Sau đó tạo file test:

```csharp
// tests/Unit/GymBase.UnitTests/DI/GoiTapServiceTests.cs
using GymBase.Base.Exceptions;
using GymBase.BLDI.BLGoiTap;
using GymBase.DLDI.Contract.DLGoiTap;
using GymBase.Entities.DI;
using GymBase.TestDoubles;

namespace GymBase.UnitTests.DI;

public class GoiTapServiceTests
{
    private sealed class FakeGoiTapRepository(InMemoryStore<goi_tap> store)
        : InMemoryBaseRepository<goi_tap>(store), IGoiTapRepository;

    private readonly InMemoryStore<goi_tap> _store = new();

    private GoiTapService CreateService() => new(
        new FakeGoiTapRepository(_store),
        TimeProvider.System,
        new FakeCurrentUser("nv.thu"),
        new GoiTapCreateValidator(),
        new GoiTapUpdateValidator());

    private static GoiTapCreateDto ValidBody(decimal gia = 500_000) =>
        new() { ma_goi = "G01", ten_goi = "Gói 1 tháng", gia = gia };

    [Fact]
    public async Task Create_saves_row_and_returns_it()
    {
        var created = await CreateService().BLCreateAsync(ValidBody(), CancellationToken.None);

        Assert.Equal("G01", created.ma_goi);
        Assert.Single(_store.All());
    }

    [Fact]
    public async Task Negative_price_is_rejected()
    {
        await Assert.ThrowsAsync<ValidationFailedException>(
            () => CreateService().BLCreateAsync(ValidBody(gia: -1), CancellationToken.None));
    }

    [Fact]
    public async Task Deleted_row_is_no_longer_found()
    {
        var service = CreateService();
        var created = await service.BLCreateAsync(ValidBody(), CancellationToken.None);

        await service.BLDeleteAsync(created.Id, CancellationToken.None);

        await Assert.ThrowsAsync<NotFoundException>(() => service.BLGetByIdAsync(created.Id, CancellationToken.None));
    }
}
```

Chạy toàn bộ test tại `apps/api`:

```bash
dotnet test GymBase.slnx
```

Cuối cùng, lưu các request đã thử vào một thư mục riêng của chức năng, ví dụ `docs/api/bruno/web/goi-tap`, để
người khác dùng lại. Thư mục cần có `folder.bru` để đặt tên và thứ tự. Xem mẫu ở `docs/api/bruno/web/nguoi-dung`.

## 6. Khi cần thêm tính năng

### Chặn trùng dữ liệu, ví dụ mã gói

Thêm hàm kiểm tra vào `IGoiTapRepository` (tên bắt đầu bằng `DL`, ví dụ `DLExistsByMaGoiAsync`), viết hàm đó trong `GoiTapRepository` bằng `Set.AnyAsync(...)`,
rồi override `BLBeforeCreateAsync` trong service và ném `ConflictException` khi trùng. Mẫu đầy đủ là
`DLExistsByTenDangNhapAsync` trong `TaiKhoanRepository` và `BLBeforeCreateAsync` trong `TaiKhoanService`.
Chặn đổi sang giá trị đã có khi sửa thì override `BLBeforeUpdateAsync` (chạy sau validator, trước khi ghi), như
`TaiKhoanService` làm với email.

Nên thêm cả unique index ở database để chặn hai yêu cầu gửi cùng lúc. Mẫu là `HasIndex(e => e.ten_dang_nhap).IsUnique()`
trong `TaiKhoanConfiguration`.

### Kiểm tra nghiệp vụ khi sửa hoặc xóa

Override `BLUpdateAsync` hoặc `BLDeleteAsync` trong service, kiểm tra xong gọi `base.BLUpdateAsync(...)` hoặc
`base.BLDeleteAsync(...)`. Vi phạm thì ném exception trong `GymBase.Base.Exceptions`. Danh sách exception
và mã lỗi: [quy-uoc-response.md](quy-uoc-response.md). Không viết SQL trong service.

### Sắp xếp và lọc

Override `AllowedColumns` trong repository, theo mẫu `TaiKhoanQueryColumns`. Chưa làm bước này mà gửi
`sort` hoặc `filter` thì API trả 400. Chi tiết: [sort-va-filter.md](sort-va-filter.md).

### Thêm API riêng hoặc đổi quyền từng API

Viết thêm action trong controller như bình thường. Các action có sẵn đều là `virtual` nên có thể
override, ví dụ để đặt quyền riêng cho `GET`. Mẫu thêm action: `ResetPassword` trong `UsersController`.

### Thêm dữ liệu mẫu

Đặt script SQL trong `database/seeds`. Mẫu là `dev_tai_khoan.sql`.

## 7. Lỗi thường gặp

| Hiện tượng | Nguyên nhân và cách xử lý |
| --- | --- |
| Khi khởi động hoặc khi gọi API báo `Unable to resolve service for type 'FluentValidation.IValidator...'` | Chưa thêm assembly của BL vào `AddAppApi` (bước 7) |
| Khi khởi động hoặc khi gọi API báo `Unable to resolve service for type '...GoiTapService'` hoặc `IGoiTapRepository` | Thiếu dòng `AddScoped` trong `Program.cs` (bước 7) |
| Gọi API báo 500, log có `Table 'gymbase.goi_tap' doesn't exist` | Chưa chạy `dotnet ef database update` (bước 2) |
| `dotnet ef migrations add` tạo migration rỗng | Thiếu lớp cấu hình bảng ở bước 2, hoặc chưa lưu file |
| 404 khi gọi API | Sai tiền tố `/api/v1/web` hoặc `/api/v1/mob`, hoặc sai tên controller |
| 401 | Chưa gửi token hoặc token đã hết hạn. Chạy lại request **Đăng nhập** |
| 403 | Tài khoản không có vai trò mà `[Authorize]` yêu cầu |
| 400 khi gửi `sort` hoặc `filter` | Bảng chưa khai báo `AllowedColumns` |
| Test kiến trúc `LayerReferenceTests` báo lỗi | Project tham chiếu sai lớp. Xem ma trận trong [cau-truc-thu-muc.md](cau-truc-thu-muc.md) |

## 8. Kiểm tra lần cuối trước khi commit

- [ ] Entity, cấu hình bảng và các file migration đều đã được thêm.
- [ ] `MaximumLength` trong validator khớp độ dài cột.
- [ ] Controller đã khai báo đúng quyền truy cập.
- [ ] `Program.cs` đã có assembly của BL và 2 dòng `AddScoped`.
- [ ] `dotnet test GymBase.slnx` chạy qua hết.
- [ ] Đã lưu request vào `docs/api/bruno`.

Muốn hiểu sâu hơn về các lớp nền, đọc [crud-dung-chung.md](crud-dung-chung.md).
