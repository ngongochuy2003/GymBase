# CRUD dùng chung

Phần nền CRUD nằm trong `GymBase.Base` và `GymBase.Base.Hosting`. Mỗi bảng mới chỉ cần khai báo
entity, DTO và vài hàm map là có đủ API thêm/sửa/xóa/xem. Mẫu đầy đủ đang chạy: **người dùng**
(bảng `tai_khoan`, nhóm Auth).

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
         BaseRepository<TEntity> + AppDbContext (EF Core)               GymBase.Persistence
```

| Lớp | Project | Việc làm |
| --- | --- | --- |
| `BaseCrudController<TDto, TCreateDto, TUpdateDto>` | `GymBase.Base.Hosting` | Đủ 5 action: `GET` (phân trang, tìm kiếm), `GET {id}`, `POST`, `PUT {id}`, `DELETE {id}`. `TDto` phải có `Id` (`IHasId`). Controller web và mobile cùng kế thừa |
| `ICrudService<TDto, TCreateDto, TUpdateDto>` | `GymBase.Base` | Interface mà controller gọi; controller không biết entity |
| `BaseService<TEntity, TDto, TCreateDto, TUpdateDto>` | `GymBase.Base` | Chuẩn hóa phân trang, map entity ↔ DTO, tự điền audit (UTC) |
| `IBaseRepository<TEntity>` | `GymBase.Base` | `DLGetPagingAsync`, `DLGetByIdAsync`, `DLInsertAsync`, `DLUpdateAsync`, `DLDeleteAsync` |
| `BaseRepository<TEntity>` | `GymBase.Persistence` | Hiện thực `IBaseRepository` bằng EF Core: tìm kiếm, mới tạo trước, phân trang; trùng unique index thì ném `ConflictException` |
| `InMemoryBaseRepository<TEntity>`, `InMemoryStore<TEntity>` | `tests/Shared/GymBase.TestDoubles` | DL giả để test BL không cần MySQL; hành vi phải khớp `BaseRepository` |

`AppDbContext` và `BaseRepository` nằm trong `GymBase.Persistence` (quyết định Q9), mọi `DL*.MySql` tham chiếu project này.

## 2. Đặt entity và DTO ở đâu

```text
src/Data/GymBase.Entities/
└── Auth/tai_khoan.cs                class tai_khoan (bảng tai_khoan)
                                      namespace GymBase.Entities.Auth
src/Auth/GymBase.BLAuth/
└── BLTaiKhoan/                      namespace GymBase.BLAuth.BLTaiKhoan
    ├── TaiKhoanService.cs
    └── TaiKhoanDtos.cs              TaiKhoanDto, TaiKhoanCreateDto, TaiKhoanUpdateDto
```

| Loại | Đặt ở | Quy ước |
| --- | --- | --- |
| **Entity** (cấu trúc bảng) | `GymBase.Entities/<Nhóm>/<tên_bảng>.cs` theo **nhóm sở hữu bảng** | Tên class **trùng tên bảng** (`tai_khoan`), property trùng tên cột, riêng `Id` giữ nguyên |
| **DTO** (dữ liệu vào/ra API) | BL của nhóm: `GymBase.BL<Nhóm>/<TênPascal>/<TênPascal>Dtos.cs` | Tên PascalCase: `TaiKhoanDto`, `TaiKhoanCreateDto`, `TaiKhoanUpdateDto`. Property cùng tên cột |

- Namespace theo thư mục (`GymBase.Entities.Auth`). `.editorconfig` của `apps/api` tắt
  gợi ý "tên phải PascalCase" vì entity và DTO cố ý đặt tên snake_case.
- **DTO để trong BL, không để ở DL**: DTO đổi theo nghiệp vụ/màn hình còn entity đổi theo DB;
  BL là nơi map nên DTO ở cạnh BL; API đã tham chiếu BL nên dùng được DTO; DL không nhìn thấy DTO.
- DTO HTTP riêng cho mobile (gọn hơn web) đặt cạnh controller trong `Mob/Controllers/`, hoặc trong
  BL nếu nhiều nơi cùng dùng, ví dụ `TaiKhoanMobileDto`.
- Nhóm khác cần dữ liệu của bảng này thì đi qua interface trong `GymBase.ModuleContracts`, không
  tham chiếu thẳng BL/DL của nhóm sở hữu. Hạ tầng chung (`PagingRequest`, `PagingResult`, `IHasId`)
  ở `GymBase.Base`.

## 3. Hành vi HTTP

Route có tiền tố kênh: `/api/v1/web/...` cho controller trong `Controllers/`, `/api/v1/mob/...` cho
controller trong `Mob/Controllers/`. Bảng dưới ghi phần sau tiền tố.

| Request | Thành công | Lỗi |
| --- | --- | --- |
| `GET /users?page=1&page_size=20&search=abc` | 200, `PagingResult<TDto>`: `data` (mảng `TDto`) và `meta` (`page`, `page_size`, `total`, `total_page`, `ranger` gồm `from`, `to`) | |
| `GET /users/{id}` | 200, `TDto` | 404 `NOT_FOUND` nếu không có |
| `POST /users` | 201, `TDto`, header `Location` trỏ tới bản ghi mới | 400 `VALIDATION_FAILED` nếu body không qua validator; 409 `USER_USERNAME_TAKEN` nếu trùng `ten_dang_nhap` |
| `PUT /users/{id}` | 200, `TDto` sau khi sửa | 400 như trên; 404 `NOT_FOUND` nếu không có |
| `DELETE /users/{id}` | 204 | 404 `NOT_FOUND` nếu không có; 409 `IN_USE` nếu đang được bảng khác tham chiếu |

Mọi lỗi trả ProblemDetails có `code`, `detail` tiếng Việt và `trace_id`. Lỗi 400 do `[ApiController]`
tự trả trước khi vào BL, có `errors` theo từng trường. Các lỗi khác do BL ném exception, global
exception handler đổi thành ProblemDetails; controller không try/catch.

Ví dụ danh sách (`?page=2&page_size=20`, có 57 bản ghi):

```json
{
  "data": [ { "id": "…", "ten_dang_nhap": "an.nv", "ho_ten": "Nguyễn Văn An" } ],
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
| Thêm | `BLCreateEntity(dto)` → Id UUIDv7 có sẵn → nếu là `BaseAuditableEntity` thì gán `ngay_tao`, `nguoi_tao` → `DLInsertAsync` |
| Sửa | Đọc bản ghi → `BLApplyUpdate(entity, dto)` → gán `ngay_chinh_sua`, `nguoi_chinh_sua` → `DLUpdateAsync` |
| Xóa | `DLDeleteAsync` xóa hẳn; bản ghi đang được bảng khác tham chiếu thì `ConflictException` (409 `IN_USE`) |
| Xem, sửa, xóa bản ghi không có | Ném `NotFoundException` ("Không tìm thấy {EntityDisplayName} hoặc … đã bị xóa."); override `EntityDisplayName` để câu báo lỗi nói đúng tên đối tượng |

- Giờ lấy từ `TimeProvider` (đăng ký `TimeProvider.System`), test truyền giờ cố định.
- `nguoi_tao` / `nguoi_chinh_sua` lấy từ `ICurrentUser`. Chưa có đăng nhập nên hiện vẫn là `null`.

## 5. Thêm CRUD cho một bảng mới

Xem hướng dẫn từng bước, kèm code đầy đủ, cách chạy thử và lỗi thường gặp:
[huong-dan-them-crud.md](huong-dan-them-crud.md).

## 6. Còn thiếu

| Việc | Ghi chú |
| --- | --- |
