# Logging và audit log

API có hai loại log, phục vụ hai mục đích khác nhau. Tài liệu này nói cách chúng hoạt động, cách
xem và cách dùng khi viết code.


## 1. Hai loại log

| | Log kỹ thuật | Audit log |
| --- | --- | --- |
| Dùng để | Lập trình viên xem app chạy thế nào, tìm lỗi | Tra lại ai đã thay đổi dữ liệu, lúc nào |
| Lưu ở | Console và file `logs/gymbase-<ngày>.log` | Bảng `system_log` trong MySQL |
| Ghi những gì | Mọi sự kiện đáng chú ý: khởi động, lỗi, cảnh báo | Chỉ lệnh thay đổi dữ liệu: `POST`, `PUT`, `PATCH`, `DELETE` |
| Giữ bao lâu | 30 file theo ngày | 90 ngày, tự xóa quá hạn |
| Người đọc | Lập trình viên | Quản trị, người điều tra sự cố |

Hai loại cùng ghi một **mã truy vết** (`TraceId`) cho mỗi request, nên từ một dòng audit có thể tìm
lại toàn bộ log kỹ thuật của request đó.

## 2. Log kỹ thuật (Serilog)

### Nơi ghi và cách xem

- **Console:** xuất hiện ngay trong cửa sổ chạy API.
- **File:** `logs/gymbase-20261109.log`, mỗi ngày một file. Đường dẫn tính từ thư mục làm việc của
  API. Khi chạy bằng `dotnet run` hoặc Visual Studio thì thư mục này là `src/Host/GymBase.API/logs`.
  File `*.log` đã nằm trong `.gitignore`.

Một dòng log có dạng:

```text
2026-11-09 22:48:34.293 +07:00 [INF] 6b0567b542925bfbda95e49704391986 GymBase.Base.Hosting.Errors.AppExceptionHandler: Lỗi nghiệp vụ 401 AUTH_INVALID_CREDENTIALS khi xử lý POST /api/v1/web/auth/login: ...
```

| Phần | Ý nghĩa |
| --- | --- |
| `2026-11-09 22:48:34.293 +07:00` | Thời điểm ghi, theo giờ máy chủ |
| `[INF]` | Mức log, xem bảng ở mục 2.2 |
| `6b0567b5…` | Mã truy vết của request. Để trống với log không thuộc request nào, như lúc khởi động |
| `GymBase.Base.Hosting.Errors.AppExceptionHandler` | Lớp đã ghi dòng này |
| Phần còn lại | Nội dung. Nếu có exception thì stack trace nằm ở các dòng tiếp theo |

### Các mức log

| Mức | Viết tắt | Khi nào dùng | Ví dụ trong dự án |
| --- | --- | --- | --- |
| Debug | `DBG` | Chi tiết để dò lỗi, mặc định không ghi | Client hủy request |
| Information | `INF` | Sự kiện bình thường cần biết | Lỗi nghiệp vụ như 404, 409, 422 |
| Warning | `WRN` | Bất thường nhưng app vẫn chạy | Không lưu được audit log |
| Error | `ERR` | Lỗi làm hỏng một thao tác | Exception không lường trước, trả 500 |

Mặc định ghi từ `Information` trở lên. Riêng `Microsoft.AspNetCore` và `Microsoft.EntityFrameworkCore`
chỉ ghi từ `Warning`, vì ở mức thấp hơn chúng ghi rất nhiều dòng.

### Cấu hình

Cấu hình nằm ở mục `Serilog` trong [appsettings.json](../../apps/api/src/Host/GymBase.API/appsettings.json):

| Muốn | Sửa |
| --- | --- |
| Đổi mức log chung | `Serilog:MinimumLevel:Default` |
| Xem chi tiết hơn của một thư viện | Thêm vào `Serilog:MinimumLevel:Override`, ví dụ `"Microsoft.EntityFrameworkCore.Database.Command": "Information"` để xem câu SQL |
| Giữ log lâu hơn | `retainedFileCountLimit` của sink `File` (mặc định 30 file) |
| Đổi nơi lưu file | `path` của sink `File` |

Môi trường thật đổi bằng biến môi trường, dùng hai dấu gạch dưới thay cho dấu hai chấm:

```bash
Serilog__MinimumLevel__Default=Debug
```

Cách đăng ký nằm ở [LoggingExtensions.cs](../../apps/api/src/Base/GymBase.Base.Hosting/Logging/LoggingExtensions.cs),
gọi từ `Program.cs`. Cố ý không đặt trong `AddAppApi()` để test không ghi file log.

### Tìm log của một request lỗi

Response lỗi luôn có trường `trace_id`:

```json
{ "code": "INTERNAL_ERROR", "trace_id": "00-6b0567b542925bfbda95e49704391986-2f59ef10021ed2b8-00" }
```

Lấy **phần 32 ký tự ở giữa** (`6b0567b542925bfbda95e49704391986`) rồi tìm trong file log:

```powershell
Select-String -Path logs\*.log -Pattern 6b0567b542925bfbda95e49704391986
```

Lỗi 500 chỉ trả câu chung chung cho người dùng, chi tiết thật và stack trace chỉ nằm trong log.

### Viết log trong code

Inject `ILogger<TênLớp>` và dùng **mẫu có tham số**, không nối chuỗi:

```csharp
public sealed class GoiTapService(ILogger<GoiTapService> logger, /* ... */)
{
    public async Task DoSomethingAsync(Guid id)
    {
        logger.LogInformation("Gia hạn gói tập {GoiTapId} cho {TaiKhoan}.", id, taiKhoan);
    }
}
```

- Viết `{GoiTapId}`, không viết `$"... {id}"`, để giá trị được ghi thành trường riêng và tìm kiếm được.
- Bắt exception rồi ghi log thì truyền exception làm tham số đầu: `logger.LogWarning(ex, "...")`.
- **Không ghi mật khẩu, token, refresh token, số thẻ hay dữ liệu cá nhân** vào log. File log nằm trên
  máy chủ và ai có quyền đọc file đều thấy.
- Lỗi nghiệp vụ (`BusinessException`) không cần tự ghi log, `AppExceptionHandler` đã ghi.

## 3. Audit log (bảng `system_log`)

### Khi nào có bản ghi

[AuditLogFilter](../../apps/api/src/Base/GymBase.Base.Hosting/Logging/AuditLogFilter.cs) được đăng ký
cho mọi controller trong `AddAppApi()`, nên controller mới tự có audit mà không phải làm gì thêm.

| Trường hợp | Có ghi? |
| --- | --- |
| `POST`, `PUT`, `PATCH`, `DELETE` thành công | Có |
| Lệnh thất bại do nghiệp vụ (404, 409, 422…) hoặc dữ liệu sai (400) | Có, kèm mã HTTP cuối cùng |
| Lệnh gây lỗi 500 | Có |
| Client ngắt kết nối giữa chừng | Có, mã trạng thái là 499 |
| `GET` | Không, vì ghi cả đọc thì bảng phình rất nhanh |
| Đăng nhập, làm mới token | Có, vì là `POST`. Đăng nhập sai vẫn được ghi |
| Request bị chặn ở bước xác thực hoặc phân quyền (401, 403 từ `[Authorize]`) | **Không**, vì chưa vào tới controller |
| Request bị giới hạn tốc độ (429) | **Không**, cùng lý do |

Bản ghi được lưu **sau khi response đã gửi đi**, nên người dùng không phải chờ. Nếu lưu thất bại
(ví dụ DB lỗi), request vẫn trả kết quả bình thường và chỉ có một dòng cảnh báo trong log kỹ thuật.

### Các cột

| Cột | Nội dung | Ví dụ |
| --- | --- | --- |
| `Id` | UUIDv7 | `019a1b2c-…` |
| `nguoi_dung` | Tên đăng nhập; `NULL` nếu chưa đăng nhập | `admin` |
| `phuong_thuc` | Phương thức HTTP | `DELETE` |
| `duong_dan` | Đường dẫn thực tế, **không có query string** | `/api/v1/web/users/0193f0a0-…` |
| `ma_trang_thai` | Mã HTTP cuối cùng | `204` |
| `thoi_gian_ms` | Thời gian xử lý, mili giây | `37` |
| `trace_id` | Mã truy vết, khớp với file log | `6b0567b542925bfbda95e49704391986` |
| `ip` | IP của bên gọi | `192.168.1.20` |
| `thoi_diem` | Lúc bắt đầu xử lý, giờ UTC | `2026-11-09 15:48:34` |

**Không lưu body của request** để tránh lưu mật khẩu và token vào bảng. Query string cũng bị bỏ vì
có thể chứa dữ liệu nhạy cảm. Giá trị dài hơn độ rộng cột sẽ bị cắt bớt thay vì làm lệnh lưu thất bại.

### Truy vấn thường dùng

Cột `thoi_diem` lưu UTC, giờ Việt Nam sớm hơn 7 tiếng.

```sql
-- 50 thao tác gần nhất
SELECT thoi_diem, nguoi_dung, phuong_thuc, duong_dan, ma_trang_thai, thoi_gian_ms
FROM system_log
ORDER BY thoi_diem DESC
LIMIT 50;

-- Một người đã làm gì trong 1 ngày
SELECT thoi_diem, phuong_thuc, duong_dan, ma_trang_thai
FROM system_log
WHERE nguoi_dung = 'admin' AND thoi_diem >= UTC_TIMESTAMP() - INTERVAL 1 DAY
ORDER BY thoi_diem;

-- Ai đã xóa bản ghi có id này
SELECT thoi_diem, nguoi_dung, ip
FROM system_log
WHERE phuong_thuc = 'DELETE' AND duong_dan LIKE '%0193f0a0-0000-7000-8000-000000000001';

-- Các lệnh chậm hoặc lỗi hệ thống
SELECT thoi_diem, nguoi_dung, duong_dan, ma_trang_thai, thoi_gian_ms, trace_id
FROM system_log
WHERE thoi_gian_ms > 1000 OR ma_trang_thai >= 500
ORDER BY thoi_diem DESC;
```

Có `trace_id` thì tìm tiếp trong file log để xem lỗi chi tiết, theo mục 2.

### Giữ và xóa

[SystemLogCleanupService](../../apps/api/src/Data/GymBase.Persistence/Logging/SystemLogCleanupService.cs)
chạy một lần khi app khởi động rồi mỗi ngày một lần, xóa các dòng cũ hơn số ngày cấu hình:

```json
"AuditLog": { "RetentionDays": 90 }
```

Môi trường thật đặt bằng biến `AuditLog__RetentionDays`. Giá trị phải là số nguyên lớn hơn 0, nếu
không app báo lỗi ngay lúc khởi động để tránh xóa nhầm toàn bộ log.

## 4. Cách các phần ghép với nhau

```text
Request ──► Controller ──► Service ──► Repository
              │
              │ AuditLogFilter ghi nhận method, đường dẫn, người dùng
              ▼
         Response gửi đi ──► OnCompleted ──► IAuditLogWriter ──► system_log
```

| Thành phần | Project | Việc làm |
| --- | --- | --- |
| `AuditLogFilter` | `GymBase.Base.Hosting` | Quyết định request nào cần ghi, đo thời gian, lấy mã trạng thái cuối |
| `IAuditLogWriter`, `AuditLogEntry` | `GymBase.Base` | Hợp đồng lưu audit; Hosting chỉ biết interface này, không phụ thuộc EF Core |
| `SystemLogWriter` | `GymBase.Persistence` | Lưu vào bảng bằng một `AppDbContext` riêng, để không kéo theo thay đổi dang dở của request vừa lỗi |
| `SystemLogCleanupService` | `GymBase.Persistence` | Xóa dòng quá hạn |
| `system_log` | `GymBase.Entities/Logging` | Entity của bảng, kế thừa `BaseEntity` (bảng log không cần 4 cột audit) |

## 5. Test

- `AuditLogTests` (integration) kiểm tra ghi đúng method, đường dẫn, mã trạng thái, tên người dùng;
  không ghi `GET`; lỗi khi lưu không ảnh hưởng response.
- Test dùng `RecordingAuditLogWriter` trong `GymBase.TestDoubles` thay cho ghi DB. Vì audit ghi sau
  khi response xong, test đợi bằng `await audit.NextAsync()` chứ không đọc ngay.
- `AppDbContextModelTests` kiểm tra bảng `system_log` và việc cắt giá trị quá dài.

## 6. Giới hạn đã biết

- Request bị chặn ở xác thực, phân quyền hoặc giới hạn tốc độ (401, 403, 429) không có audit. Muốn
  ghi cả các trường hợp này thì cần thêm một middleware đặt trước `UseAuthentication()`.
- Audit chỉ cho biết **ai gọi lệnh gì**, không lưu dữ liệu trước và sau khi sửa.
- Cột `ip` là IP của proxy khi đặt sau Nginx, cho đến khi bật ForwardedHeaders (mục 11 và 14 trong
  [viec-con-lai-base.md](viec-con-lai-base.md)). Chỗ cần sửa đã có `TODO(HOST)` trong `AuditLogFilter`.
- Ghi audit dùng chung MySQL với dữ liệu chính. Nếu lượng ghi lớn, cần chuyển sang kho riêng hoặc
  ghi theo lô.
- File log chưa được gom về một nơi tập trung; khi chạy nhiều bản API thì phải xem từng máy.

Quy ước lỗi và `trace_id`: [quy-uoc-response.md](quy-uoc-response.md).
