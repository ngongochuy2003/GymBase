# Sort và filter

API danh sách (`GET /`) nhận hai tham số `sort` và `filter`, dùng chung cho web và mobile. Mỗi bảng
tự khai báo những cột được phép; cột ngoài danh sách trả 400.

Phần 1 đến 4 dành cho người gọi API, phần 5 trở đi dành cho người viết backend. Các tham số khác của
danh sách (`page`, `page_size`, `search`) và dạng response: [quy-uoc-response.md](quy-uoc-response.md).

## 1. Sort

```text
GET /api/v1/web/users?sort=-ngay_tao,ho_ten
```

- Liệt kê các cột cách nhau bằng dấu phẩy, ưu tiên từ trái sang phải.
- Dấu `-` trước tên cột là giảm dần, không có dấu là tăng dần.
- Không gửi `sort` thì bản ghi mới tạo xếp trước.
- Server luôn sắp thêm theo `Id` giảm dần ở cuối, để thứ tự giữa các trang không bị đảo khi nhiều bản
  ghi có cùng giá trị.

## 2. Filter

```text
GET /api/v1/web/users?filter=vai_tro:eq:Trainer,ngay_tao:gte:2026-11-01
```

Mỗi điều kiện có dạng `cot:toan_tu:gia_tri`. Nhiều điều kiện cách nhau bằng dấu phẩy và kết hợp theo
**AND**; hiện chưa có OR. `filter` cũng kết hợp AND với `search`.

| Toán tử | Ý nghĩa | Dùng cho kiểu |
| --- | --- | --- |
| `eq` | Bằng | Mọi kiểu |
| `ne` | Khác | Mọi kiểu |
| `gt` | Lớn hơn | Số, thời gian |
| `gte` | Lớn hơn hoặc bằng | Số, thời gian |
| `lt` | Nhỏ hơn | Số, thời gian |
| `lte` | Nhỏ hơn hoặc bằng | Số, thời gian |
| `contains` | Chứa chuỗi | Chuỗi |

Tên cột và toán tử phân biệt hoa thường, viết đúng như trong bảng.

| Kiểu cột | Cách viết giá trị | Ví dụ |
| --- | --- | --- |
| Chuỗi | Nguyên văn; so sánh **không phân biệt hoa thường** theo collation của cột | `vai_tro:eq:trainer` |
| Boolean | `true`, `false` | `dang_hoat_dong:eq:false` |
| Số | Dấu chấm cho phần thập phân, không có phân cách hàng nghìn | `gia:gte:150000.50` |
| Thời gian | ISO 8601; không ghi múi giờ thì hiểu là **UTC** | `ngay_tao:gte:2026-11-01T07:00:00Z` |
| Guid | Dạng chuẩn có dấu gạch | `Id:eq:0193f0a0-0000-7000-8000-000000000001` |

Dấu `:` trong giá trị vẫn dùng được, nên thời gian có giờ phút không cần xử lý gì thêm. Với
`contains`, ký tự `%` và `_` được hiểu đúng là ký tự đó, không phải ký tự đại diện.

**Giới hạn:**

- Giá trị không được chứa dấu phẩy.
- Chưa lọc được giá trị NULL: `email:eq:null` so sánh với chuỗi chữ `null`.
- Khoảng trắng ở đầu và cuối tên cột, toán tử, giá trị bị bỏ qua.

## 3. Gọi từ client

Giá trị có ký tự đặc biệt (khoảng trắng, `+`, `&`, `#`, tiếng Việt) phải được mã hóa URL. Truyền qua
`params` của axios hoặc `URLSearchParams` thì thư viện tự làm việc này:

```ts
await axios.get(`${baseUrl}/users`, {
  params: {
    page: 1,
    page_size: 20,
    sort: '-ngay_tao',
    filter: 'vai_tro:eq:Trainer,dang_hoat_dong:eq:true',
  },
});
```

Các request mẫu trong Bruno: `sap-xep.bru`, `loc.bru`, `sap-xep-sai-cot.bru`
(thư mục `docs/api/bruno/web/nguoi-dung` và `docs/api/bruno/mobile/nguoi-dung`).

## 4. Khi sai

Trả 400 `VALIDATION_FAILED`, `errors` có khóa `sort` hoặc `filter`. `detail` hiển thị được ngay cho
người dùng. Client nên hiển thị dưới dạng văn bản, vì câu lỗi có thể lặp lại giá trị đã gửi.

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Dữ liệu không hợp lệ",
  "status": 400,
  "detail": "Không được sắp xếp theo cột 'mat_khau_hash'.",
  "instance": "/api/v1/web/users",
  "code": "VALIDATION_FAILED",
  "trace_id": "00-…",
  "errors": { "sort": ["Không được sắp xếp theo cột 'mat_khau_hash'."] }
}
```

| Trường hợp | `detail` |
| --- | --- |
| Sort cột không được phép | `Không được sắp xếp theo cột '…'.` |
| Filter cột không được phép | `Không được lọc theo cột '…'.` |
| Thiếu phần trong điều kiện | `Điều kiện lọc '…' phải có dạng cot:toan_tu:gia_tri.` |
| Toán tử lạ hoặc không hợp với kiểu cột | `Cột '…' không dùng được toán tử '…'.` |
| Giá trị sai kiểu | `Giá trị '…' của cột '…' không hợp lệ.` |

## 5. Cột được phép của từng bảng

| Bảng | Cột sort và filter | Khai báo ở |
| --- | --- | --- |
| `tai_khoan` | `ten_dang_nhap`, `ho_ten`, `email`, `so_dien_thoai`, `vai_tro`, `dang_hoat_dong`, `ngay_tao`, `ngay_chinh_sua` | `TaiKhoanQueryColumns` |

Thêm bảng mới thì thêm một dòng vào bảng này.

## 6. Bật sort và filter cho một bảng

**Bước 1:** khai báo danh sách cột trong `DL<Nhóm>.Contract`, cạnh interface repository:

```csharp
public static class GoiTapQueryColumns
{
    public static QueryColumns<goi_tap> Allowed { get; } = new QueryColumns<goi_tap>()
        .Add(e => e.ma_goi)
        .Add(e => e.ten_goi)
        .Add(e => e.gia)
        .Add(e => e.ngay_tao);
}
```

**Bước 2:** override `AllowedColumns` trong repository:

```csharp
public sealed class GoiTapRepository(AppDbContext db) : BaseRepository<goi_tap>(db), IGoiTapRepository
{
    protected override QueryColumns<goi_tap> AllowedColumns => GoiTapQueryColumns.Allowed;
}
```

Controller và service không phải sửa gì. Repository giả dùng trong test cũng override `AllowedColumns`
bằng cùng danh sách, để test chạy đúng với cột thật.

**Quy tắc chọn cột:**

- Chỉ khai báo cột được phép lộ ra API. **Không** khai báo mật khẩu, token, cờ xóa, dữ liệu nội bộ. Một
  cột lọc được thì dò được giá trị của nó, kể cả khi response không trả cột đó.
- Không override thì bảng không có sort và filter: client gửi lên sẽ nhận 400.
- Kiểu cột hỗ trợ: `string`, `bool`, `int`, `long`, `decimal`, `double`, `DateTime`, `Guid` và dạng
  nullable của chúng. Kiểu khác cần thêm vào `PagingQuery.ParseValue`, nếu không request lọc theo cột
  đó sẽ lỗi 500.
- Cột hay dùng để sort hoặc lọc thì thêm index trong migration. `contains` không dùng được index nên
  luôn quét bảng; cân nhắc khi bảng lớn.

## 7. Cách hoạt động

```text
?sort=&filter=  →  PagingRequest  →  BaseService (Normalize)  →  BaseRepository.DLGetPagingAsync
                                                                     ├─ PagingQuery.ApplyFilter
                                                                     └─ PagingQuery.ApplySort
```

| Thành phần | Vị trí | Việc làm |
| --- | --- | --- |
| `PagingRequest` | `GymBase.Base/Paging` | Nhận `sort`, `filter` từ query string, cắt khoảng trắng |
| `QueryColumns<TEntity>` | `GymBase.Base/Paging` | Danh sách cột được phép |
| `PagingQuery` | `GymBase.Base/Paging` | Kiểm tra cú pháp, đổi thành biểu thức LINQ, ném `ValidationFailedException` khi sai |

`PagingQuery` làm việc trên `IQueryable`, nên EF Core và repository giả dùng chung một logic. Khác biệt
duy nhất: repository giả so sánh chuỗi có phân biệt hoa thường, còn MySQL thì không.

## 8. Bảo mật

- Tên cột client gửi chỉ dùng để tra trong `QueryColumns`, không bao giờ ghép vào SQL. Không dùng
  `FromSqlRaw` cho sort và filter.
- Giá trị đi qua closure như biến trong LINQ viết tay. Provider MySQL tham số hóa hoặc escape: giá trị
  chuỗi có thể nằm trực tiếp trong SQL sau khi escape, các kiểu còn lại là tham số.
- **Chưa phân quyền theo vai trò:** danh sách cột giống nhau với mọi người gọi. Khi làm xác thực
  (mục 12 trong [viec-con-lai-base.md](viec-con-lai-base.md)) cần xem lại, nhất là các cột như `email`,
  `so_dien_thoai`.
- **Chưa giới hạn tải:** chưa giới hạn số điều kiện trong `filter`, và chưa có rate limit.

## 9. Test

| File | Kiểm tra |
| --- | --- |
| `tests/Unit/GymBase.UnitTests/Base/PagingQueryTests.cs` | Cú pháp, toán tử, kiểu giá trị, các trường hợp bị từ chối |
| `tests/Unit/GymBase.UnitTests/Auth/AppDbContextModelTests.cs` | Câu SQL sinh ra, escape dấu nháy |
| `tests/Integration/GymBase.IntegrationTests/SortFilterTests.cs` | Đi từ query string qua controller, mã lỗi 400 |
