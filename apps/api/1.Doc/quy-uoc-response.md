# Quy ước response của API

Tài liệu này mô tả API trả gì khi thành công, trả gì khi lỗi, và bộ mã lỗi dùng chung cho
web và mobile. Đây là nội dung của task **API-02** và mục 6, 7 trong
[`docs/api/viec-con-lai-base.md`](../../../docs/api/viec-con-lai-base.md).

| Phần | Trạng thái |
| --- | --- |
| Dạng danh sách `{ data, meta }` | **Đã chốt 08/11, đã làm** (`PagingResult<T>`) |
| Các response thành công khác | **Đã làm** trong `BaseCrudController` |
| Dạng lỗi (ProblemDetails), bộ mã lỗi, exception | **Đề xuất**, chưa làm (mục 7) |
| Sort, filter | **Đề xuất**, chưa làm (mục 6) |

Khi một phần đề xuất được chốt hoặc làm xong, sửa bảng trên.

---

## 1. Nguyên tắc

1. **Mã HTTP cho biết loại kết quả.** Client biết thành công hay lỗi nhờ mã HTTP, không cần đọc
   cờ `success` trong body.
2. **Khi thành công, trả thẳng dữ liệu, không bọc envelope.** Không có `{ success, data, message }`.
   Riêng danh sách có dạng `{ data, meta }` vì cần trả kèm thông tin phân trang.
3. **Khi lỗi, luôn trả ProblemDetails** (RFC 9457), kèm `code` cho biết lý do cụ thể.
4. **Tên trường JSON là snake_case**, trùng tên cột (quy ước Q2): `tai_khoan`, `page_size`,
   `trace_id`. Riêng khóa chính là `id`.
5. **Không lộ chi tiết nội bộ.** Stack trace, câu SQL, tên class chỉ ghi vào log, không bao giờ
   nằm trong response.

---

## 2. Khi thành công

| Thao tác | HTTP | Body |
| --- | --- | --- |
| Lấy 1 bản ghi `GET /{id}` | 200 | DTO |
| Lấy danh sách `GET /` | 200 | `{ data, meta }` (mục 2.1) |
| Thêm `POST /` | 201, header `Location` trỏ tới bản ghi mới | DTO vừa tạo |
| Sửa `PUT /{id}` | 200 | DTO sau khi sửa |
| Xóa `DELETE /{id}` | 204 | Không có body |
| Thao tác khác (đăng nhập, đổi mật khẩu…) | 200 nếu có dữ liệu trả về, 204 nếu không | DTO hoặc không có |

Ví dụ lấy 1 bản ghi:

```json
{
  "id": "0193f0a0-0000-7000-8000-000000000001",
  "tai_khoan": "admin",
  "ho_ten": "Quản trị viên",
  "vai_tro": "Admin",
  "dang_hoat_dong": true,
  "ngay_tao": "2026-11-01T00:00:00Z",
  "ngay_chinh_sua": null
}
```

Thời gian luôn là UTC, có hậu tố `Z` (quy ước Q7). Client tự đổi sang giờ địa phương khi hiển thị.

### 2.1. Danh sách có phân trang

Request: `GET /api/web/v1/users?page=2&page_size=20&search=an`

| Tham số | Mặc định | Ghi chú |
| --- | --- | --- |
| `page` | 1 | Nhỏ hơn 1 thì tự đưa về 1 |
| `page_size` | 20 | Tối đa 100; nhỏ hơn 1 thì về 20. **Không có giá trị "lấy tất cả"** |
| `search` | (trống) | Tự cắt khoảng trắng; mỗi màn hình tự quyết định tìm theo cột nào |
| `sort` | mới tạo trước | *Đề xuất, chưa làm:* `sort=-ngay_tao,ho_ten`, dấu `-` là giảm dần |
| `filter` | (trống) | *Đề xuất, chưa làm:* dạng tham số cụ thể chốt khi làm mục 6 |

Mỗi bảng khai báo danh sách cột được phép sort và filter. Gửi cột ngoài danh sách thì trả 400
`VALIDATION_FAILED`.

Response:

```json
{
  "data": [
    { "id": "…", "tai_khoan": "an.nv", "ho_ten": "Nguyễn Văn An" }
  ],
  "meta": {
    "page": 2,
    "page_size": 20,
    "total": 57,
    "total_page": 3,
    "ranger": { "from": 21, "to": 40 }
  }
}
```

| Trường | Ý nghĩa |
| --- | --- |
| `data` | Các bản ghi của trang này; mảng rỗng nếu không có |
| `meta.page`, `meta.page_size` | Giá trị **sau khi** đã đưa về hợp lệ, có thể khác giá trị client gửi |
| `meta.total` | Tổng số bản ghi thỏa điều kiện, tính trên mọi trang |
| `meta.total_page` | Tổng số trang; 0 khi không có bản ghi |
| `meta.ranger` | Thứ tự bản ghi đầu và cuối của trang này (đếm từ 1), dùng để hiện "21–40 trên 57". Là `0`–`0` khi không có bản ghi hoặc `page` vượt quá `total_page` |

Trang vượt quá số trang **không phải lỗi**: trả 200, `data` rỗng.

---

## 3. Khi lỗi

> Phần này là **đề xuất**, sẽ làm ở mục 7. Hiện tại chỉ có lỗi 400 (do `[ApiController]` tự trả)
> và 404 (body rỗng).

Mọi lỗi trả header `Content-Type: application/problem+json` và body:

```json
{
  "type": "https://gymbase.local/errors/duplicate",
  "title": "Dữ liệu bị trùng",
  "status": 409,
  "detail": "Tên đăng nhập 'an.nv' đã tồn tại.",
  "instance": "/api/web/v1/users",
  "code": "USER_USERNAME_TAKEN",
  "trace_id": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01",
  "errors": {
    "tai_khoan": ["Tên đăng nhập đã tồn tại."]
  }
}
```

| Trường | Bắt buộc | Ý nghĩa |
| --- | --- | --- |
| `status` | Có | Trùng mã HTTP |
| `title` | Có | Tên ngắn của loại lỗi, cố định theo `status` |
| `type` | Có | URI định danh loại lỗi (chỉ để định danh, không cần mở được). Tên miền `gymbase.local` là tạm, đổi khi có tên miền thật |
| `detail` | Có | Câu tiếng Việt **hiển thị được ngay cho người dùng**. Lỗi 500 chỉ có câu chung chung |
| `instance` | Có | Đường dẫn của request bị lỗi |
| `code` | Có | Mã lỗi (mục 4). **Client rẽ nhánh theo `code`**, không so chuỗi `detail` |
| `trace_id` | Có | Dùng để tìm log tương ứng. Khi báo lỗi, gửi kèm giá trị này |
| `errors` | Không | Chỉ có khi lỗi gắn với từng ô nhập. Khóa là tên trường JSON, giá trị là danh sách thông báo |

Lỗi 400 do `[ApiController]` tự trả (body sai kiểu, vi phạm DataAnnotations) cũng phải theo đúng
dạng trên, tức là có thêm `code` = `VALIDATION_FAILED` và `trace_id`. Phần này cấu hình bằng
`AddProblemDetails(o => o.CustomizeProblemDetails = …)` trong `AddGymBaseApi()` (mục 11).

---

## 4. Mã lỗi

### 4.1. Mã chung

| HTTP | `code` | `title` | Khi nào dùng | Ví dụ |
| --- | --- | --- | --- | --- |
| 400 | `VALIDATION_FAILED` | Dữ liệu không hợp lệ | Nhập sai hoặc thiếu trường, sai định dạng, cột sort/filter không được phép. **Có `errors`** | Thiếu `tai_khoan`, email sai định dạng |
| 401 | `UNAUTHORIZED` | Chưa xác thực | Chưa đăng nhập, token sai hoặc hết hạn, sai tên đăng nhập hoặc mật khẩu | Gọi API không kèm token |
| 403 | `FORBIDDEN` | Không có quyền | Đã đăng nhập nhưng không đủ quyền, tài khoản bị khóa | Học viên gọi API quản trị |
| 404 | `NOT_FOUND` | Không tìm thấy | Không có bản ghi theo id. Bản ghi đã xóa mềm cũng tính là không có | `GET /users/{id}` với id không tồn tại |
| 409 | `DUPLICATE` | Dữ liệu bị trùng | Trùng giá trị phải duy nhất | Trùng `tai_khoan` |
| 409 | `CONCURRENCY_CONFLICT` | Dữ liệu đã thay đổi | Bản ghi đã bị người khác sửa trước, client cần tải lại | Hai người cùng sửa một hội viên |
| 422 | `BUSINESS_RULE` | Không thực hiện được | Dữ liệu đúng định dạng nhưng vi phạm quy tắc nghiệp vụ | Gia hạn gói tập đã hủy |
| 500 | `INTERNAL_ERROR` | Lỗi hệ thống | Lỗi không lường trước, kể cả lỗi DB. `detail` luôn là câu chung chung | Mất kết nối MySQL |

Hai mã có thể dùng sau, khi có nhu cầu: 429 `TOO_MANY_REQUESTS` (giới hạn số lần gọi, ví dụ đăng
nhập sai nhiều lần) và 503 `SERVICE_UNAVAILABLE` (bảo trì).

**Tại sao lỗi nhập liệu là 400 chứ không phải 422?** `[ApiController]` của ASP.NET Core tự trả 400
khi body sai. Dùng 400 cho mọi lỗi nhập liệu thì lỗi do framework bắt và lỗi do FluentValidation
bắt (mục 8) cùng một mã. 422 để riêng cho lỗi nghiệp vụ, nên client phân biệt được "nhập sai"
(hiện lỗi dưới từng ô) với "không được phép làm việc này" (hiện thông báo chung).

**Tại sao 409 có hai mã?** Cả hai đều là xung đột với dữ liệu đang có, nhưng client xử lý khác
nhau: `DUPLICATE` thì báo người dùng sửa giá trị, `CONCURRENCY_CONFLICT` thì tải lại bản ghi.
Client phân biệt bằng `code`.

### 4.2. Mã chi tiết của từng nghiệp vụ

Khi client cần xử lý riêng một tình huống, module được thêm mã chi tiết hơn, **giữ nguyên mã
HTTP của mã chung tương ứng**.

- Dạng: `<ĐỐI_TƯỢNG>_<LÝ_DO>`, chữ in hoa, nối bằng `_`, tiếng Anh.
- Đã đưa ra thì không đổi tên và không đổi nghĩa, vì client đã rẽ nhánh theo nó. Bỏ mã thì đánh
  dấu ngừng dùng trong bảng, không dùng lại tên đó cho nghĩa khác.
- Không cần mã chi tiết thì dùng mã chung ở mục 4.1.

| HTTP | `code` | Khi nào |
| --- | --- | --- |
| 409 | `USER_USERNAME_TAKEN` | Thêm người dùng trùng `tai_khoan` |
| 401 | `AUTH_INVALID_CREDENTIALS` | Sai tên đăng nhập hoặc mật khẩu |
| 403 | `AUTH_ACCOUNT_LOCKED` | Tài khoản đang bị khóa |
| 422 | `MEMBERSHIP_EXPIRED` | *(ví dụ)* Thao tác trên gói tập đã hết hạn |

Thêm mã mới thì thêm một dòng vào bảng này, cùng commit với code dùng mã đó.

---

## 5. Hiện thực ở backend (đề xuất cho mục 7)

| Exception (`AMISBase.BL`) | HTTP | `code` mặc định |
| --- | --- | --- |
| `ValidationException` | 400 | `VALIDATION_FAILED` |
| `UnauthorizedException` | 401 | `UNAUTHORIZED` |
| `ForbiddenException` | 403 | `FORBIDDEN` |
| `NotFoundException` | 404 | `NOT_FOUND` |
| `ConflictException` | 409 | `DUPLICATE` |
| `ConcurrencyException` | 409 | `CONCURRENCY_CONFLICT` |
| `BusinessRuleException` | 422 | `BUSINESS_RULE` |
| Exception khác | 500 | `INTERNAL_ERROR` |

- Tất cả kế thừa `BusinessException(string code, string detail)`. Hàm khởi tạo nhận `code` để
  ghi đè bằng mã chi tiết, ví dụ `throw new ConflictException("USER_USERNAME_TAKEN", "Tên đăng nhập 'an.nv' đã tồn tại.")`.
- Hằng số mã lỗi đặt trong lớp tĩnh `ErrorCodes`: mã chung ở AMISBase, mã chi tiết ở BL của
  từng module.
- Một **global exception handler** (`IExceptionHandler`) ở `AMISBase.API` đổi exception thành
  ProblemDetails. Controller **không** viết `try/catch`.
- Lỗi 4xx ghi log mức Information hoặc Warning. Lỗi 500 ghi log mức Error kèm stack trace, rồi
  trả `detail` chung chung.
- BL ném `NotFoundException` thay vì trả `null`, nên `BaseCrudController` bỏ được các nhánh
  `dto is null ? NotFound() : …`.

**So với dự án tham khảo (dev-HuyNgo).** Dự án đó có `ErrorCode` 404, 460–464, nhưng các số này
không thành mã HTTP: mọi lỗi nghiệp vụ, kể cả không tìm thấy, đều trả 400, còn lỗi khác trả 500
kèm stack trace. Các mã `CreateFailed`, `UpdateFailed`, `DeleteFailed` không nói lý do lỗi nên bị
dùng lẫn (ví dụ dùng `UpdateFailed` cho "không tìm thấy"). Tiền tố `"Show:"` trong thông báo dùng
để báo cho frontend biết câu nào hiện được. GymBase thay tất cả bằng mã HTTP đúng nghĩa, `code`
rõ lý do, và quy tắc "`detail` của lỗi 4xx luôn hiện được".

---

## 6. Phía client

- **Lỗi 4xx:** hiện `detail`. Nếu có `errors` thì hiện lỗi dưới từng ô nhập.
- **Lỗi 500, lỗi mạng, hết thời gian chờ, response không có body:** hiện câu chung chung của
  client, không hiện nội dung từ server.
- **401:** thử refresh token một lần. Không được thì đăng xuất.
- Rẽ nhánh theo `code`. Gặp `code` lạ thì xử lý theo mã HTTP.
- Khi báo lỗi cho người dùng hoặc gửi log, kèm `trace_id`.

**Mobile cần sửa khi làm mục 7:** `apps/mobile/src/core/api/api-error.ts` hiện chỉ đoán thông
báo theo mã HTTP. Nó hiểu 422 là "thông tin chưa hợp lệ" và 409 là "dữ liệu đã thay đổi", tức là
lệch với bảng ở mục 4.1. Cần sửa `toApiError` để đọc `code`, `detail`, `errors`, `trace_id` từ
ProblemDetails. Thông báo theo mã HTTP chỉ giữ làm phương án dự phòng khi không có body.
