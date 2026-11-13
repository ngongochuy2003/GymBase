# Hướng dẫn thêm API nghiệp vụ (không phải CRUD)

CRUD ([huong-dan-them-crud.md](huong-dan-them-crud.md)) chỉ thêm, sửa, xóa một bảng. Tài liệu này dành cho API làm một
**việc nghiệp vụ**: chạm nhiều bảng, có điều kiện, chuyển trạng thái, phải thành công hoặc thất bại trọn vẹn. Ví dụ xuyên
suốt: **PT xác nhận một buổi tập và trừ 1 buổi của gói**.

> `BuoiTapService` và các repository trong ví dụ **chưa có trong code**; entity `buoi_tap`, `goi_pt_hoc_vien`,
> `bien_dong_so_buoi` và các enum thì đã có. Giao dịch (mục 4) cũng chưa có sẵn.

## 1. Khác CRUD ở đâu

| | CRUD | API nghiệp vụ |
| --- | --- | --- |
| Tên API | Danh từ: thêm, sửa, xóa một bản ghi | Động từ: một việc (`xac-nhan`, `huy`, `gia-han`) |
| Service | Kế thừa `BaseService`, có sẵn 5 hàm | Lớp riêng như `XacThucService`, mỗi việc một hàm `BL...` |
| Quy tắc | Gần như không có | Nằm hết trong hàm BL |
| Dữ liệu | Một bảng | Nhiều bảng, thường cần giao dịch |

## 2. Viết bảng quy tắc trước khi viết code

Làm bảng này trước, vì mỗi dòng sau đó thành một nhánh trong hàm BL và một test.

| Điều kiện | Kết quả khi vi phạm | Mã HTTP | Mã lỗi |
| --- | --- | --- | --- |
| Chưa đăng nhập | `UnauthorizedException` | 401 | `UNAUTHORIZED` |
| Buổi không tồn tại | `NotFoundException` | 404 | `NOT_FOUND` |
| Người gọi không phải PT của buổi | `ForbiddenException` | 403 | `FORBIDDEN` |
| Buổi không phải `pt_truc_tiep` hoặc không gắn gói | `BusinessRuleException` | 422 | mã riêng, ví dụ `SESSION_NOT_BILLABLE` |
| Buổi chưa ở `da_hoan_thanh` | `BusinessRuleException` | 422 | mã riêng |
| Gói không còn buổi | `BusinessRuleException` | 422 | mã riêng, ví dụ `PACKAGE_EXHAUSTED` |
| Gọi lại khi buổi đã `da_xac_nhan` | Trả kết quả cũ, không trừ thêm | 200 | không có |
| Hai request cùng lúc | Một thắng, một nhận lỗi trùng | 409 | `DUPLICATE` |

Danh sách exception và mã chung: [quy-uoc-response.md](quy-uoc-response.md) mục 4.

## 3. Phân việc giữa các tầng

```text
Controller   chỉ gọi BL, không có điều kiện nghiệp vụ
BL           đọc, kiểm tra từng dòng của bảng quy tắc, ghi, trả DTO; không SQL, không EF
DL           mỗi hàm một việc nhỏ: đọc một bản ghi, tính tổng, thêm một dòng
```

Luật ở BL, câu truy vấn ở DL. Đừng viết một hàm DL "xác nhận buổi" làm hết mọi việc, vì khi đó luật nằm ở DL và test BL
không còn ý nghĩa.

## 4. Giao dịch và chống chạy trùng

Việc trừ buổi ghi vào ba bảng (`bien_dong_so_buoi`, `buoi_tap`, `lich_su_chinh_sua`). Chúng phải cùng thành công hoặc cùng
bị hủy, nên cần **giao dịch**. Có hai lớp bảo vệ, dùng cả hai:

- **Kiểm tra trong BL** để báo lỗi rõ ràng cho người dùng (đọc số dư, kiểm tra trạng thái).
- **Ràng buộc trong database** để dù hai request chạy cùng lúc cũng không bao giờ sai. Với ví dụ này: cột sinh tự động
  `tru_buoi_hieu_luc` kèm unique index chỉ cho một lần trừ mỗi buổi, và `ma_chong_trung` unique. Request thua nhận
  `ConflictException` từ `BaseRepository` (trùng unique index).

**Chưa có trong code:** một abstraction giao dịch cho BL. BL không được tham chiếu EF Core, nên cần thêm interface (gợi ý
`IGiaoDich`, có `BeginAsync` và `CommitAsync`; thoát mà chưa `Commit` thì hủy) ở `GymBase.Base`, hiện thực bằng `Database.BeginTransactionAsync` trong
`GymBase.Persistence`. Làm một lần khi cần API nghiệp vụ đầu tiên, rồi mọi API sau dùng lại. Để lại comment
`// TODO(DB): ...` ở nơi tạm bỏ giao dịch (xem [quy-uoc-todo.md](../architecture/quy-uoc-todo.md)).

## 5. Làm từng bước

### Bước 1: Hàm DL

Chỉ truy vấn và ghi. Đặt trong thư mục `DL<Danh từ>/` ở cả `DL<Nhóm>.Contract` (interface) và `DL<Nhóm>.MySql` (hiện thực).
Tên hàm bắt đầu bằng `DL`. Hàm đã có sẵn trong `IBaseRepository` (`DLGetByIdAsync`, `DLInsertAsync`, `DLUpdateAsync`)
dùng luôn, chỉ thêm hàm còn thiếu:

```csharp
// DL<Nhóm>.Contract/DLBienDongSoBuoi/IBienDongSoBuoiRepository.cs
public interface IBienDongSoBuoiRepository : IBaseRepository<bien_dong_so_buoi>
{
    /// <summary>Số buổi còn lại của gói: tổng cột so_buoi_thay_doi của mọi biến động.</summary>
    Task<int> DLSumSoBuoiAsync(Guid goiPtHocVienId, CancellationToken cancellationToken);
}

// DL<Nhóm>.MySql/DLBienDongSoBuoi/BienDongSoBuoiRepository.cs
public Task<int> DLSumSoBuoiAsync(Guid goiPtHocVienId, CancellationToken cancellationToken) =>
    Set.Where(b => b.goi_pt_hoc_vien_id == goiPtHocVienId).SumAsync(b => b.so_buoi_thay_doi, cancellationToken);
```

### Bước 2: Validator (chỉ khi có body)

Id của đối tượng lấy từ **đường dẫn**, người gọi lấy từ `ICurrentUser`. Không đọc `pt_id` hay `hoc_vien_id` từ body, vì client
sửa được. Ví dụ này không có body nên bỏ bước. Khi có body, viết validator kế thừa `AppValidator<T>` như
`DangNhapValidator`, rồi gọi `validator.ValidateOrThrowAsync(dto, cancellationToken)` ở đầu hàm BL.

### Bước 3: Service BL

Lớp thường, không kế thừa `BaseService`, đặt trong `BL<Nhóm>/BL<Danh từ>/`. Tên hàm bắt đầu bằng `BL`. Thứ tự cố định:
**người gọi → đọc → kiểm tra từng dòng bảng quy tắc → ghi → trả DTO**.

```csharp
public sealed class BuoiTapService(
    IBuoiTapRepository buoiTaps,
    IBienDongSoBuoiRepository bienDongs,
    ILichSuChinhSuaRepository lichSus,
    IGiaoDich giaoDich,                       // chưa có: xem mục 4
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    private DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;

    public async Task<BuoiTapDto> BLXacNhanBuoiTapAsync(Guid buoiTapId, CancellationToken cancellationToken)
    {
        var ptId = currentUser.UserId ?? throw new UnauthorizedException();
        await using var scope = await giaoDich.BeginAsync(cancellationToken);

        var buoi = await buoiTaps.DLGetByIdAsync(buoiTapId, cancellationToken) ?? throw new NotFoundException();
        if (buoi.pt_id != ptId)
        {
            throw new ForbiddenException();
        }

        if (buoi.trang_thai == TrangThaiBuoiTap.da_xac_nhan)
        {
            return ToDto(buoi);                // gọi lại: trả kết quả cũ, không trừ thêm
        }

        if (buoi.loai_buoi != LoaiBuoi.pt_truc_tiep || buoi.goi_pt_hoc_vien_id is not { } goiId)
        {
            throw new BusinessRuleException("Buổi này không tính vào gói PT.", BizErrorCodes.SessionNotBillable);
        }

        if (buoi.trang_thai != TrangThaiBuoiTap.da_hoan_thanh)
        {
            throw new BusinessRuleException("Buổi chưa hoàn thành.", BizErrorCodes.SessionNotCompleted);
        }

        if (await bienDongs.DLSumSoBuoiAsync(goiId, cancellationToken) < 1)
        {
            throw new BusinessRuleException("Gói đã hết buổi.", BizErrorCodes.PackageExhausted);
        }

        await bienDongs.DLInsertAsync(new bien_dong_so_buoi
        {
            goi_pt_hoc_vien_id = goiId,
            buoi_tap_id = buoi.Id,
            loai_bien_dong = LoaiBienDong.tru_buoi,
            so_buoi_thay_doi = -1,
            ma_chong_trung = $"tru-buoi:{buoi.Id}",     // cùng buổi thì cùng khóa
            nguoi_tao_id = ptId,
            ngay_tao = UtcNow,
        }, cancellationToken);

        buoi.trang_thai = TrangThaiBuoiTap.da_xac_nhan;
        buoi.nguoi_xac_nhan_id = ptId;
        buoi.ngay_xac_nhan = UtcNow;
        await buoiTaps.DLUpdateAsync(buoi, cancellationToken);

        await lichSus.DLInsertAsync(/* hanh_dong = xac_nhan, nguoi_thuc_hien_id = ptId */ ..., cancellationToken);

        await scope.CommitAsync(cancellationToken);
        return ToDto(buoi);
    }
}
```

Hai điểm hay quên: mốc thời gian lấy từ `TimeProvider` (UTC), và hàm BL không nhận `pt_id` từ ngoài.

### Bước 4: Mã lỗi riêng

Mã chung (`BUSINESS_RULE`) chưa đủ để client rẽ nhánh, nên thêm mã riêng vào `<Nhóm>ErrorCodes` (ví dụ `BizErrorCodes`), giữ
mã HTTP của mã chung tương ứng, và thêm một dòng vào bảng mục 4.2 của [quy-uoc-response.md](quy-uoc-response.md). Mã đã công
bố thì không đổi tên.

### Bước 5: Controller

Chỉ gọi BL. Đặt `[Route("buoi-tap")]` tường minh (chữ thường, gạch nối, danh từ tiếng Việt), tên action là việc
(`XacNhan`), id lấy từ đường dẫn, quyền ghi bằng `[Authorize]` (xem [xac-thuc.md](xac-thuc.md)). Route tường minh thay hẳn
route lấy theo tên controller của `BaseApiController`.

```csharp
[Route("buoi-tap")]
public abstract class BuoiTapControllerBase(BuoiTapService service) : BaseApiController
{
    /// <summary>PT xác nhận buổi đã tập và trừ 1 buổi của gói. Gọi lại không trừ thêm.</summary>
    [HttpPost("{id:guid}/xac-nhan")]
    public async Task<ActionResult<BuoiTapDto>> XacNhan(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.BLXacNhanBuoiTapAsync(id, cancellationToken));
}
```

Controller web và mobile kế thừa lớp gốc này như `AuthControllerBase` (mỗi kênh một lớp con rỗng, đặt trong `Controllers/`
và `Mob/Controllers/`).

### Bước 6: Đăng ký

Thêm `services.AddScoped<BuoiTapService>()` vào extension của nhóm (như `AuthBusinessExtensions`) và các repository vào
`Program.cs`. Quên bước này thì ứng dụng báo không tạo được service (lỗi lúc khởi động, hoặc 500 khi gọi).

### Bước 7: Test

Mỗi dòng của bảng quy tắc là một test BL, dùng DL giả `InMemory*` trong `tests/Shared/GymBase.TestDoubles`:

| Test | Kỳ vọng |
| --- | --- |
| PT đúng, buổi hợp lệ | Có 1 dòng `-1`, buổi `da_xac_nhan`, có lịch sử |
| Người gọi không phải PT của buổi | `ForbiddenException`, không ghi gì |
| Buổi `tu_tap` | `BusinessRuleException`, không trừ |
| Buổi chưa `da_hoan_thanh` | `BusinessRuleException` |
| Gói hết buổi | `BusinessRuleException` |
| Gọi hai lần | Chỉ một dòng `-1` |

Chạy cùng lúc hai request thật cần MySQL (unique index chặn), `InMemory*` không làm được. Ghi rõ test nào chưa phủ được.
Rồi thêm một request vào `docs/api/bruno` như các API hiện có.

## 6. Lỗi thường gặp

| Triệu chứng | Nguyên nhân |
| --- | --- |
| Luật nằm trong controller hoặc DL | Chuyển về hàm BL, để test BL bắt được |
| Hàm BL gọi `AppDbContext` hoặc viết LINQ | BL không tham chiếu EF; thêm hàm vào DL |
| Client truyền `pt_id` trong body và API tin | Lấy từ `ICurrentUser`, kiểm tra với dữ liệu trong DB |
| Hai bảng cập nhật lệch nhau | Thiếu giao dịch (mục 4) |
| Trừ hai lần khi mạng gửi lại | Thiếu khóa chống trùng hoặc unique index |
| Client không phân biệt được lý do lỗi | Dùng mã chung; thêm mã riêng (bước 4) |
| Trả thẳng entity | Trả DTO |

## 7. Kiểm tra lần cuối trước khi commit

- Bảng quy tắc ở mục 2 đủ, mỗi dòng có test.
- Tên hàm BL/DL có tiền tố, tên file khớp tên lớp, thư mục theo quy tắc ở [cau-truc-thu-muc.md](cau-truc-thu-muc.md).
- Mã lỗi mới có trong `<Nhóm>ErrorCodes` và bảng mục 4.2.
- `dotnet test GymBase.slnx` tại `apps/api` qua.
