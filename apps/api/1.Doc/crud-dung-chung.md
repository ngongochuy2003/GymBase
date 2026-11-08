# CRUD dùng chung

Phần nền CRUD nằm trong `99.AMISBASE`. Mỗi bảng mới chỉ cần khai báo entity, DTO và
vài hàm map là có đủ API thêm/sửa/xóa/xem. Mẫu đầy đủ đang chạy: **người dùng** (bảng `nguoi_dung`).

> **Trạng thái:** DL hiện là **mock lưu trong bộ nhớ** (`InMemoryBaseDL`), tắt host là mất
> dữ liệu. Khi nối MySQL sẽ thêm `BaseDL<TEntity>` dùng EF Core; DL của từng bảng chỉ đổi lớp
> cha, BL và API giữ nguyên.

## 1. Các lớp nền

```text
HTTP ──► BaseCrudController<TDto, TCreateDto, TUpdateDto>     AMISBase.API
            │ gọi ICrudBL
            ▼
         BaseBL<TEntity, TDto, TCreateDto, TUpdateDto>         AMISBase.BL
            │ gọi IBaseDL<TEntity>
            ▼
         IBaseDL<TEntity>                                       AMISBase.DL.Contract
            ▲ hiện thực
         InMemoryBaseDL<TEntity> + InMemoryStore<TEntity>       AMISBase.DL.Mysql (mock)
```

| Lớp | Project | Việc làm |
| --- | --- | --- |
| `BaseCrudController<TDto, TCreateDto, TUpdateDto>` | `AMISBase.API` | Đủ 5 action: `GET` (phân trang, tìm kiếm), `GET {id}`, `POST`, `PUT {id}`, `DELETE {id}`. `TDto` phải có `Id` (`IHasId`). Web và mobile dùng chung |
| `ICrudBL<TDto, TCreateDto, TUpdateDto>` | `AMISBase.BL` | Interface mà controller gọi; controller không biết entity |
| `BaseBL<TEntity, TDto, TCreateDto, TUpdateDto>` | `AMISBase.BL` | Chuẩn hóa phân trang, map entity ↔ DTO, tự điền audit (UTC), xóa mềm |
| `IBaseDL<TEntity>` | `AMISBase.DL.Contract` | `GetPagingAsync`, `GetByIdAsync`, `InsertAsync`, `UpdateAsync`, `DeleteAsync` |
| `InMemoryBaseDL<TEntity>` | `AMISBase.DL.Mysql` | Mock của `IBaseDL`: ẩn bản ghi xóa mềm, tìm kiếm, mới tạo trước, phân trang |
| `InMemoryStore<TEntity>` | `AMISBase.DL.Mysql` | "Bảng" trong bộ nhớ, đăng ký singleton; luôn trả bản sao như khi đọc DB |

## 2. Đặt entity và DTO ở đâu

```text
4.Model/GymBase.Model/
└── Entities/                         namespace GymBase.Model.Entities
    ├── System/nguoi_dung.cs          class nguoi_dung  (bảng nguoi_dung)
    └── DI/goi_tap.cs                 class goi_tap     (bảng goi_tap)

3.Management/System/GymBase.BLSystem/
└── NguoiDung/                        namespace GymBase.BLSystem.NguoiDung
    ├── UserBL.cs
    └── NguoiDungDtos.cs              NguoiDungDto, NguoiDungCreateDto, NguoiDungUpdateDto
```

| Loại | Đặt ở | Quy ước |
| --- | --- | --- |
| **Entity** (cấu trúc bảng) | `GymBase.Model/Entities/<phân hệ>/<tên_bảng>.cs` | Tên class **trùng tên bảng** (`nguoi_dung`), property trùng tên cột, riêng `Id` giữ nguyên. Sau này `BaseDL` lấy tên bảng từ tên class |
| **DTO** (dữ liệu vào/ra API) | BL của phân hệ: `GymBase.BL<Nhóm>/<TênPascal>/<TênPascal>Dtos.cs` | Tên PascalCase: `NguoiDungDto`, `NguoiDungCreateDto`, `NguoiDungUpdateDto`. Property cùng tên cột |

- **Namespace của entity luôn là `GymBase.Model.Entities`**, không theo thư mục con. Thư mục
  `System`, `DI`… chỉ để xếp file theo phân hệ. Nếu theo thư mục thì namespace thành
  `GymBase.Model.Entities.System`, che namespace `System` của .NET. Tên bảng vốn duy nhất nên
  class không bị trùng. `GymBase.Model/.editorconfig` đã tắt gợi ý "namespace không khớp thư mục"
  và "tên class phải PascalCase" cho riêng project này.
- **DTO để trong BL, không để ở Model**: DTO đổi theo nghiệp vụ/màn hình còn entity đổi theo DB;
  BL là nơi map nên DTO ở cạnh BL; API host đã tham chiếu BL nên dùng được DTO; DL không nhìn thấy DTO.
- DTO riêng cho mobile (gọn hơn web) cũng đặt trong BL đó, ví dụ `NguoiDungMobileDto`.
- Kiểu dữ liệu mà **nhiều phân hệ** cùng cần thì mới đưa lên `GymBase.Model`, không để BL này
  tham chiếu BL khác. Hạ tầng chung (`PagingRequest`, `PagingResult`, `IHasId`) ở AMISBASE.

## 3. Hành vi HTTP

Route có tiền tố kênh (`/api/web` hoặc `/api/mobile`), ví dụ `/api/web/v1/users`.

| Request | Thành công | Lỗi |
| --- | --- | --- |
| `GET /v1/users?page=1&page_size=20&search=abc` | 200, `PagingResult<TDto>`: `data` (mảng `TDto`) và `meta` (`page`, `page_size`, `total`, `total_page`, `ranger` gồm `from`, `to`) | |
| `GET /v1/users/{id}` | 200, `TDto` | 404 nếu không có hoặc đã xóa |
| `POST /v1/users` | 201, `TDto`, header `Location` trỏ tới bản ghi mới | 400 nếu body vi phạm DataAnnotations |
| `PUT /v1/users/{id}` | 200, `TDto` sau khi sửa | 400 như trên; 404 nếu không có hoặc đã xóa |
| `DELETE /v1/users/{id}` | 204 | 404 nếu không có hoặc đã xóa |

Lỗi 400 do `[ApiController]` tự trả (ProblemDetails, có `errors` theo từng trường) trước khi vào BL.

Ví dụ danh sách (`?page=2&page_size=20`, có 57 bản ghi):

```json
{
  "data": [ { "id": "…", "tai_khoan": "an.nv", "ho_ten": "Nguyễn Văn An" } ],
  "meta": { "page": 2, "page_size": 20, "total": 57, "total_page": 3, "ranger": { "from": 21, "to": 40 } }
}
```

`ranger` là 0–0 khi không có bản ghi hoặc trang vượt quá `total_page`. `page_size` tối đa 100,
không có giá trị "lấy tất cả".

## 4. BaseBL tự làm gì

| Thao tác | Việc BaseBL làm |
| --- | --- |
| Danh sách | `request.Normalize()` (page ≥ 1, page_size 1–100, trim search), gọi DL, map sang DTO |
| Thêm | `CreateEntity(dto)` → Id UUIDv7 có sẵn → nếu là `BaseAuditableEntity` thì gán `ngay_tao`, `nguoi_tao` → `InsertAsync` |
| Sửa | Đọc bản ghi → `ApplyUpdate(entity, dto)` → gán `ngay_chinh_sua`, `nguoi_chinh_sua` → `UpdateAsync` |
| Xóa | Entity có `ISoftDelete` thì đặt `is_deleted = true` rồi `UpdateAsync` (xóa mềm); không có thì `DeleteAsync` (xóa hẳn) |

- Giờ lấy từ `TimeProvider` (đăng ký `TimeProvider.System`), test truyền giờ cố định.
- `nguoi_tao` / `nguoi_chinh_sua` hiện là `null`; sẽ lấy từ `ICurrentUser` khi làm xác thực
  (override `CurrentUserName`).

## 5. Thêm CRUD cho một bảng mới

Ví dụ danh mục **Gói tập** (bảng `goi_tap`) thuộc nhóm DI.

### Bước 1: Entity (`GymBase.Model/Entities/DI/goi_tap.cs`)

```csharp
namespace GymBase.Model.Entities;

public sealed class goi_tap : BaseAuditableEntity, ISoftDelete
{
    public string ma_goi { get; set; } = string.Empty;
    public string ten_goi { get; set; } = string.Empty;
    public decimal gia { get; set; }
    public bool is_deleted { get; set; }
}
```

### Bước 2: DL (`DLDI.Contract` và `DLDI.Mysql`)

```csharp
// GymBase.DLDI.Contract/IGoiTapDL.cs
public interface IGoiTapDL : IBaseDL<goi_tap>
{
    // Truy vấn riêng nếu cần, ví dụ: Task<bool> ExistsByCodeAsync(string ma_goi, CancellationToken ct);
}

// GymBase.DLDI.Mysql/GoiTapDL.cs
public sealed class GoiTapDL(InMemoryStore<goi_tap> store) : InMemoryBaseDL<goi_tap>(store), IGoiTapDL
{
    protected override IEnumerable<string?> GetSearchValues(goi_tap e) => [e.ma_goi, e.ten_goi];
}
```

`GetSearchValues` chọn cột được tìm kiếm; không override thì tham số `search` bị bỏ qua.
Muốn có dữ liệu mẫu thì gọi `store.SeedOnce(...)` trong constructor (xem `UserDL`).
Project `DLDI.Contract` cần tham chiếu `GymBase.Model`.

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

// GoiTapBL.cs
public sealed class GoiTapBL(IGoiTapDL goiTapDL, TimeProvider timeProvider)
    : BaseBL<goi_tap, GoiTapDto, GoiTapCreateDto, GoiTapUpdateDto>(goiTapDL, timeProvider)
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
  kiểm tra rồi gọi `base.CreateAsync(...)`. BL **không** viết SQL.

### Bước 4: Controller

```csharp
// GymBase.DIAPI/Controllers/GoiTapController.cs  →  /api/web/v1/goitap
public sealed class GoiTapController(GoiTapBL goiTapBL)
    : BaseCrudController<GoiTapDto, GoiTapCreateDto, GoiTapUpdateDto>(goiTapBL);

// GymBase.MobAPI/Controllers/GoiTapController.cs →  /api/mobile/v1/goitap
public sealed class GoiTapController(GoiTapBL goiTapBL)
    : BaseCrudController<GoiTapDto, GoiTapCreateDto, GoiTapUpdateDto>(goiTapBL);
```

Route lấy theo tên controller (`[controller]`). Muốn đổi hành vi một action thì override
(các action đều `virtual`); muốn thêm endpoint riêng thì viết thêm action như bình thường.
Mọi action public của lớp cha đều thành endpoint, nên controller nào kế thừa
`BaseCrudController` cũng có đủ 5 action.

### Bước 5: Đăng ký DI (`Program.cs` của từng host dùng tới)

```csharp
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(typeof(InMemoryStore<>)); // MOCK, bỏ khi nối DB
builder.Services.AddScoped<IGoiTapDL, GoiTapDL>();
builder.Services.AddScoped<GoiTapBL>();
```

Hai dòng đầu chỉ cần một lần cho mỗi host.

### Bước 6: Test và Bruno

- Test BL trên DL mock, mỗi test một `InMemoryStore` riêng để không ảnh hưởng nhau
  (xem `8.Unitest/GymBase.UnitTests/System/UserBLTests.cs`).
- Thêm request vào `1.Doc/bruno/web` theo mẫu `nguoi-dung-them.bru`, `nguoi-dung-sua.bru`,
  `nguoi-dung-xoa.bru`.

## 6. Còn thiếu

| Việc | Ghi chú |
| --- | --- |
| `BaseDL<TEntity>` dùng EF Core + MySQL | Mục 3 trong `docs/api/viec-con-lai-base.md` |
| Lỗi nghiệp vụ (trùng mã, không được phép…) trả mã lỗi chuẩn | Mục 7: exception + ProblemDetails. Hiện chưa chặn trùng `tai_khoan` |
| Sort, filter | Mục 6; chỉ cho phép cột trong danh sách trắng |
| `nguoi_tao` / `nguoi_chinh_sua` | Mục 10: `ICurrentUser` |
| URL chữ thường (`Location` đang là `/Users/...`) | Mục 11: `AddGymBaseApi()` dùng chung, bật `LowercaseUrls` |
