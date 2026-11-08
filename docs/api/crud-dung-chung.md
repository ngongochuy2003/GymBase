# CRUD dùng chung

Phần nền CRUD nằm trong `GymBase.Base` và `GymBase.Base.Hosting`. Mỗi bảng mới chỉ cần khai báo
entity, DTO và vài hàm map là có đủ API thêm/sửa/xóa/xem. Mẫu đầy đủ đang chạy: **người dùng**
(bảng `nguoi_dung`, nhóm Auth).

## 1. Các lớp nền

```text
HTTP ──► BaseCrudController<TDto, TCreateDto, TUpdateDto>     GymBase.Base.Hosting
            │ gọi ICrudService
            ▼
         BaseService<TEntity, TDto, TCreateDto, TUpdateDto>         GymBase.Base (Crud/)
            │ gọi IBaseRepository<TEntity>
            ▼
         IBaseRepository<TEntity>                                       GymBase.Base (Crud/)
            ▲ hiện thực
         BaseRepository<TEntity> + AppDbContext (EF Core)               GymBase.DLBiz.MySql (Persistence/)
```

| Lớp | Project | Việc làm |
| --- | --- | --- |
| `BaseCrudController<TDto, TCreateDto, TUpdateDto>` | `GymBase.Base.Hosting` | Đủ 5 action: `GET` (phân trang, tìm kiếm), `GET {id}`, `POST`, `PUT {id}`, `DELETE {id}`. `TDto` phải có `Id` (`IHasId`). Controller web và mobile cùng kế thừa |
| `ICrudService<TDto, TCreateDto, TUpdateDto>` | `GymBase.Base` | Interface mà controller gọi; controller không biết entity |
| `BaseService<TEntity, TDto, TCreateDto, TUpdateDto>` | `GymBase.Base` | Chuẩn hóa phân trang, map entity ↔ DTO, tự điền audit (UTC), xóa mềm |
| `IBaseRepository<TEntity>` | `GymBase.Base` | `GetPagingAsync`, `GetByIdAsync`, `InsertAsync`, `UpdateAsync`, `DeleteAsync` |
| `BaseRepository<TEntity>` | `GymBase.DLBiz.MySql` | Hiện thực `IBaseRepository` bằng EF Core: ẩn bản ghi xóa mềm (query filter), tìm kiếm, mới tạo trước, phân trang; trùng unique index thì ném `ConflictException` |
| `InMemoryBaseRepository<TEntity>`, `InMemoryStore<TEntity>` | `tests/Shared/GymBase.TestDoubles` | DL giả để test BL không cần MySQL; hành vi phải khớp `BaseRepository` |

`AppDbContext` và `BaseRepository` nằm trong `DLBiz.MySql` (quyết định Q9), nên `DLAuth.MySql` và
`DLDI.MySql` tham chiếu `DLBiz.MySql`.

## 2. Đặt entity và DTO ở đâu

```text
src/Auth/GymBase.DLAuth.Contract/
└── Entities/nguoi_dung.cs            class nguoi_dung (bảng nguoi_dung)
                                      namespace GymBase.DLAuth.Contract.Entities
src/Auth/GymBase.BLAuth/
└── NguoiDung/                        namespace GymBase.BLAuth.NguoiDung
    ├── UserService.cs
    └── NguoiDungDtos.cs              NguoiDungDto, NguoiDungCreateDto, NguoiDungUpdateDto
```

| Loại | Đặt ở | Quy ước |
| --- | --- | --- |
| **Entity** (cấu trúc bảng) | `GymBase.DL<Nhóm>.Contract/Entities/<tên_bảng>.cs` của **nhóm sở hữu bảng** | Tên class **trùng tên bảng** (`nguoi_dung`), property trùng tên cột, riêng `Id` giữ nguyên |
| **DTO** (dữ liệu vào/ra API) | BL của nhóm: `GymBase.BL<Nhóm>/<TênPascal>/<TênPascal>Dtos.cs` | Tên PascalCase: `NguoiDungDto`, `NguoiDungCreateDto`, `NguoiDungUpdateDto`. Property cùng tên cột |

- Namespace theo thư mục (`GymBase.DLAuth.Contract.Entities`). `.editorconfig` của `apps/api` tắt
  gợi ý "tên phải PascalCase" vì entity và DTO cố ý đặt tên snake_case.
- **DTO để trong BL, không để ở DL**: DTO đổi theo nghiệp vụ/màn hình còn entity đổi theo DB;
  BL là nơi map nên DTO ở cạnh BL; API đã tham chiếu BL nên dùng được DTO; DL không nhìn thấy DTO.
- DTO HTTP riêng cho mobile (gọn hơn web) đặt cạnh controller trong `Mob/Controllers/`, hoặc trong
  BL nếu nhiều nơi cùng dùng, ví dụ `NguoiDungMobileDto`.
- Nhóm khác cần dữ liệu của bảng này thì đi qua interface trong `GymBase.ModuleContracts`, không
  tham chiếu thẳng BL/DL của nhóm sở hữu. Hạ tầng chung (`PagingRequest`, `PagingResult`, `IHasId`)
  ở `GymBase.Base`.

## 3. Hành vi HTTP

Route có tiền tố kênh: `/api/v1/web/...` cho controller trong `Controllers/`, `/api/v1/mob/...` cho
controller trong `Mob/Controllers/`. Bảng dưới ghi phần sau tiền tố.

| Request | Thành công | Lỗi |
| --- | --- | --- |
| `GET /users?page=1&page_size=20&search=abc` | 200, `PagingResult<TDto>`: `data` (mảng `TDto`) và `meta` (`page`, `page_size`, `total`, `total_page`, `ranger` gồm `from`, `to`) | |
| `GET /users/{id}` | 200, `TDto` | 404 `NOT_FOUND` nếu không có hoặc đã xóa |
| `POST /users` | 201, `TDto`, header `Location` trỏ tới bản ghi mới | 400 `VALIDATION_FAILED` nếu body vi phạm DataAnnotations; 409 `USER_USERNAME_TAKEN` nếu trùng `tai_khoan` |
| `PUT /users/{id}` | 200, `TDto` sau khi sửa | 400 như trên; 404 `NOT_FOUND` nếu không có hoặc đã xóa |
| `DELETE /users/{id}` | 204 | 404 `NOT_FOUND` nếu không có hoặc đã xóa |

Mọi lỗi trả ProblemDetails có `code`, `detail` tiếng Việt và `trace_id`. Lỗi 400 do `[ApiController]`
tự trả trước khi vào BL, có `errors` theo từng trường. Các lỗi khác do BL ném exception, global
exception handler đổi thành ProblemDetails; controller không try/catch.

Ví dụ danh sách (`?page=2&page_size=20`, có 57 bản ghi):

```json
{
  "data": [ { "id": "…", "tai_khoan": "an.nv", "ho_ten": "Nguyễn Văn An" } ],
  "meta": { "page": 2, "page_size": 20, "total": 57, "total_page": 3, "ranger": { "from": 21, "to": 40 } }
}
```

`ranger` là 0–0 khi không có bản ghi hoặc trang vượt quá `total_page`. `page_size` tối đa 100,
không có giá trị "lấy tất cả". Quy ước response đầy đủ, gồm dạng lỗi và mã lỗi:
[quy-uoc-response.md](quy-uoc-response.md).

## 4. BaseService tự làm gì

| Thao tác | Việc BaseService làm |
| --- | --- |
| Danh sách | `request.Normalize()` (page ≥ 1, page_size 1–100, trim search), gọi DL, map sang DTO |
| Thêm | `CreateEntity(dto)` → Id UUIDv7 có sẵn → nếu là `BaseAuditableEntity` thì gán `ngay_tao`, `nguoi_tao` → `InsertAsync` |
| Sửa | Đọc bản ghi → `ApplyUpdate(entity, dto)` → gán `ngay_chinh_sua`, `nguoi_chinh_sua` → `UpdateAsync` |
| Xóa | Entity có `ISoftDelete` thì đặt `is_deleted = true` rồi `UpdateAsync` (xóa mềm); không có thì `DeleteAsync` (xóa hẳn) |
| Xem, sửa, xóa bản ghi không có | Ném `NotFoundException` ("Không tìm thấy {EntityDisplayName} hoặc … đã bị xóa."); override `EntityDisplayName` để câu báo lỗi nói đúng tên đối tượng |

- Giờ lấy từ `TimeProvider` (đăng ký `TimeProvider.System`), test truyền giờ cố định.
- `nguoi_tao` / `nguoi_chinh_sua` hiện là `null`; sẽ lấy từ `ICurrentUser` khi làm xác thực
  (override `CurrentUserName`).

## 5. Thêm CRUD cho một bảng mới

Ví dụ danh mục **Gói tập** (bảng `goi_tap`) thuộc nhóm DI.

### Bước 1: Entity (`GymBase.DLDI.Contract/Entities/goi_tap.cs`)

```csharp
namespace GymBase.DLDI.Contract.Entities;

public sealed class goi_tap : BaseAuditableEntity, ISoftDelete
{
    public string ma_goi { get; set; } = string.Empty;
    public string ten_goi { get; set; } = string.Empty;
    public decimal gia { get; set; }
    public bool is_deleted { get; set; }
}
```

### Bước 2: DL (`DLDI.Contract` và `DLDI.MySql`)

```csharp
// GymBase.DLDI.Contract/IGoiTapRepository.cs
public interface IGoiTapRepository : IBaseRepository<goi_tap>
{
    // Truy vấn riêng nếu cần, ví dụ: Task<bool> ExistsByCodeAsync(string ma_goi, CancellationToken ct);
}

// GymBase.DLDI.MySql/GoiTapRepository.cs
public sealed class GoiTapRepository(AppDbContext db) : BaseRepository<goi_tap>(db), IGoiTapRepository
{
    protected override IReadOnlyList<Expression<Func<goi_tap, string?>>> SearchColumns { get; } =
        [e => e.ma_goi, e => e.ten_goi];
}

// GymBase.DLDI.MySql/Configurations/GoiTapConfiguration.cs
public sealed class GoiTapConfiguration : IEntityTypeConfiguration<goi_tap>
{
    public void Configure(EntityTypeBuilder<goi_tap> builder)
    {
        builder.ToTable(nameof(goi_tap));
        builder.Property(e => e.ma_goi).HasMaxLength(20);
        builder.Property(e => e.ten_goi).HasMaxLength(100);
    }
}
```

`SearchColumns` chọn cột được tìm kiếm; không override thì tham số `search` bị bỏ qua. Thêm bảng
thì tạo migration mới theo [cach-chay-api.md](cach-chay-api.md). Dữ liệu mẫu đặt ở `database/seeds`.

### Bước 3: DTO và BL (`GymBase.BLDI/GoiTap/`)

```csharp
// GoiTapDtos.cs
namespace GymBase.BLDI.GoiTap;

public sealed record GoiTapDto(Guid Id, string ma_goi, string ten_goi, decimal gia, DateTime ngay_tao) : IHasId;

public sealed class GoiTapCreateDto
{
    [Required, StringLength(20)] public string ma_goi { get; init; } = string.Empty;
    [Required, StringLength(100)] public string ten_goi { get; init; } = string.Empty;
    [Range(0, double.MaxValue)] public decimal gia { get; init; }
}

public sealed class GoiTapUpdateDto
{
    [Required, StringLength(100)] public string ten_goi { get; init; } = string.Empty;
    [Range(0, double.MaxValue)] public decimal gia { get; init; }
}

// GoiTapService.cs
public sealed class GoiTapService(IGoiTapRepository goiTapRepository, TimeProvider timeProvider)
    : BaseService<goi_tap, GoiTapDto, GoiTapCreateDto, GoiTapUpdateDto>(goiTapRepository, timeProvider)
{
    protected override GoiTapDto ToDto(goi_tap e) => new(e.Id, e.ma_goi, e.ten_goi, e.gia, e.ngay_tao);

    protected override goi_tap CreateEntity(GoiTapCreateDto dto) =>
        new() { ma_goi = dto.ma_goi.Trim(), ten_goi = dto.ten_goi.Trim(), gia = dto.gia };

    protected override void ApplyUpdate(goi_tap e, GoiTapUpdateDto dto)
    {
        e.ten_goi = dto.ten_goi.Trim();
        e.gia = dto.gia;
    }
}
```

- `Create` và `Update` dùng DTO riêng để client không gửi được `Id`, `ngay_tao`, `is_deleted`…
- Quy tắc nghiệp vụ riêng (ví dụ mã gói không trùng) thì override `CreateAsync` / `UpdateAsync`,
  kiểm tra rồi gọi `base.CreateAsync(...)`. Vi phạm thì ném exception trong
  `GymBase.Base.Exceptions`, ví dụ trùng thì
  `throw new ConflictException(nameof(GoiTapCreateDto.ma_goi), "Mã gói 'G01' đã tồn tại.", DIErrorCodes.GoiTapCodeTaken)`.
  Mẫu đầy đủ: `UserService.CreateAsync`. Bảng exception và mã lỗi: [quy-uoc-response.md](quy-uoc-response.md).
  BL **không** viết SQL.

### Bước 4: Controller

```csharp
// GymBase.API/Controllers/GoiTapController.cs      →  /api/v1/web/goitap
namespace GymBase.API.Controllers;

public sealed class GoiTapController(GoiTapService goiTapService)
    : BaseCrudController<GoiTapDto, GoiTapCreateDto, GoiTapUpdateDto>(goiTapService);

// GymBase.API/Mob/Controllers/GoiTapController.cs  →  /api/v1/mob/goitap (chỉ khi app mobile cần)
namespace GymBase.API.Mob.Controllers;

public sealed class GoiTapController(GoiTapService goiTapService)
    : BaseCrudController<GoiTapDto, GoiTapCreateDto, GoiTapUpdateDto>(goiTapService);
```

Route lấy theo tên controller (`[controller]`), tiền tố kênh lấy theo thư mục (namespace).
Hai controller cùng tên ở web và mob không xung đột. Muốn đổi hành vi một action thì override
(các action đều `virtual`); muốn thêm endpoint riêng thì viết thêm action như bình thường.
Mọi action public của lớp cha đều thành endpoint, nên controller nào kế thừa
`BaseCrudController` cũng có đủ 5 action.

### Bước 5: Đăng ký dịch vụ (`GymBase.API/Program.cs`)

```csharp
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddAppPersistence(builder.Configuration, typeof(UserRepository).Assembly, typeof(GoiTapRepository).Assembly);
builder.Services.AddScoped<IGoiTapRepository, GoiTapRepository>();
builder.Services.AddScoped<GoiTapService>();
```

Hai dòng đầu đã có sẵn trong `Program.cs`. Thêm hai dòng sau, và thêm assembly vào
`AddAppPersistence` nếu đây là bảng đầu tiên của nhóm đó.

### Bước 6: Test và Bruno

- Test BL trên DL giả trong `GymBase.TestDoubles`, mỗi test một `InMemoryStore` riêng để không ảnh hưởng nhau
  (xem `tests/Unit/GymBase.UnitTests/Auth/UserServiceTests.cs`).
- Thêm request vào `docs/api/bruno/web` (và `mobile` nếu có controller mobile) theo mẫu `nguoi-dung-them.bru`, `nguoi-dung-sua.bru`,
  `nguoi-dung-xoa.bru`.

## 6. Còn thiếu

| Việc | Ghi chú |
| --- | --- |
| Thông báo lỗi DataAnnotations bằng tiếng Việt | Mục 8: FluentValidation. Hiện `errors` của lỗi 400 vẫn là câu tiếng Anh mặc định |
| Sort, filter | Mục 6; chỉ cho phép cột trong danh sách trắng |
| `nguoi_tao` / `nguoi_chinh_sua` | Mục 10: `ICurrentUser` |
| URL chữ thường (`Location` đang là `/Users/...`) | Mục 11: `AddAppApi()` dùng chung, bật `LowercaseUrls` |
