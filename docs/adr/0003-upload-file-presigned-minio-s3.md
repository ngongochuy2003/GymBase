---
status: accepted
---

# Upload file: presigned POST thẳng lên MinIO/S3, xử lý nền bằng hàng đợi trong DB

Ngày 24/11/2026. Hiện app gửi file dạng multipart qua API (`POST /api/v1/mob/files`), API nén, lưu rồi tự phát lại nội dung
qua `GET .../content`. Cách này làm API chịu toàn bộ băng thông upload và download, không có progress thật, giới hạn video
10MB, và lệch với thiết kế trong `database/diagrams/02-tep-tin.md` (presigned). Quyết định này thay cách đó. Thiết kế chi
tiết nằm ở [docs/architecture/upload-file.md](../architecture/upload-file.md).

Phạm vi: ảnh, video ngắn và PDF; vài chục nghìn người dùng; MinIO khi dev và test, S3 khi production.

Các quyết định:

1. **Upload hai bước, app gửi thẳng lên storage.** App xin form upload (`upload-url`), POST thẳng lên MinIO/S3 bằng presigned POST có policy, rồi
   báo xong (`confirm`). Byte của file không đi qua API.
2. **Bỏ upload multipart qua API.** Xóa `BLUploadAsync`, đường đọc legacy trong thư mục `uploads/`, `DevFallbackUserId`,
   và `GetContent` kiểu proxy. Dữ liệu và file cũ xóa, không viết script chuyển. `LocalFileStorage` chỉ còn cho test đơn
   vị; chạy dev thật bằng MinIO.
3. **Tin client ít nhất có thể, và upload chỉ vào key tạm.** App POST vào `tmp/{file_id}`; confirm kiểm xong thì server copy sang key chính rồi xóa key tạm, nên form upload còn hạn không đè được file đã kiểm. Lúc xin form kiểm quyền, mục đích, MIME theo mục đích và dung lượng ước lượng, rồi ràng buộc
   `Content-Type` và khoảng dung lượng vào policy. Lúc confirm `HEAD` object để đối chiếu và đọc vài byte đầu kiểm magic
   bytes. Giới hạn: ảnh 10MB, PDF 10MB, video 100MB.
4. **Mọi file đọc bằng presigned GET, quyền theo đối tượng đích.** Một đường đọc duy nhất: `GET /files/{id}/url` (và dạng
   gộp nhiều file cho danh sách). Ba nhóm quyền: công khai (mạng xã hội); chủ và PT đang kèm (check-in bữa ăn, buổi tập);
   chủ chọn `chi_minh_toi` hoặc `chia_se_pt` cho từng ảnh cơ thể. Mỗi loại đối tượng đích cài `IFileTruyCapPolicy`, kiểm
   **theo lô** để danh sách không kiểm từng file. URL hết hạn 2 đến 5 phút (riêng tư), 15 phút (công khai); không ghi URL
   vào log. Chưa dùng CDN; việc tạo URL nằm sau `IFileUrlResolver` để thêm CDN cho file công khai sau mà không sửa nơi gọi.
5. **Xử lý ảnh ở nền, không ở request; hàng đợi là cột `trang_thai_xu_ly` của `tep_tin`.** Confirm đặt `cho_xu_ly` cùng lúc với `da_tai_len` (cùng
   một lần ghi, không thể lệch), `BackgroundService` trong API nhận việc bằng cập nhật có điều kiện (như ADR-0002) rồi xoay đúng chiều, bỏ
   EXIF, tính blurhash (không tạo bản thu nhỏ, mỗi file một bản). Không dùng bảng outbox riêng vì việc luôn gắn với đúng một dòng `tep_tin`.
   Ảnh xem trước cho video (ffmpeg) làm sau; video phát nguyên bản MP4, không transcode.
6. **Liên kết bằng `file_id`.** Client gửi `file_id` khi tạo bài viết, check-in, avatar...; BL kiểm chủ sở hữu, `muc_dich` và
   trạng thái rồi chuyển `da_lien_ket`. Quyền xem file riêng tư dựa vào đối tượng đích.
7. **Job dọn file mồ côi.** `cho_tai_len` và `loi` quá 60 phút thì xóa; `da_tai_len` chưa liên kết quá khoảng
   24 giờ thì xóa. Xóa object trước rồi xóa dòng, chạy lại được an toàn.
8. **Kiểm duyệt nội dung (Amazon Rekognition) làm sau, theo kiểu quét nền, hiển thị trước, gỡ sau.** Thiết kế chừa sẵn trạng
   thái `bi_go` và một bước trong chuỗi xử lý nền; chưa làm trong đợt này.
9. **Mobile: progress thật, hủy và thử lại.** Dùng `createUploadTask` của `expo-file-system`, `AbortSignal`, retry có
   backoff; URL hết hạn thì xin URL mới. Giao diện của `useFileUpload` giữ nguyên để các màn hình hiện tại ít phải đổi.
   Cache ảnh theo `file_id`, không theo URL, vì presigned URL đổi mỗi lần.
10. **`IFileStorage` là seam.** Thêm `CreatePresignedUpload`, `CreatePresignedGetUrl`, `CopyAsync`, `ReadHeadAsync`; `S3FileStorage` chạy
    được với MinIO qua `ServiceUrl` và `ForcePathStyle`.

11. **Hạn mức chống lạm dụng.** Tối đa 20 file chờ đồng thời, mỗi tài khoản mỗi ngày tối đa 50 ảnh, 10 video, 5 PDF và 500MB, rate limit `upload-url` 30 lần mỗi phút (số cụ thể ở [upload-file.md](../architecture/upload-file.md), chỉnh được trong `StorageOptions`).

12. **Dùng presigned POST có policy, không dùng presigned PUT.** Đã thử trên MinIO (24/11/2026, `S3FileStorageMinioTests`): presigned PUT chỉ ép được `Content-Type`, còn `Content-Length` đã ký vẫn nhận body lớn hơn. Presigned POST ràng buộc bằng policy: đúng key, đúng `Content-Type`, `content-length-range` từ 1 byte đến mức tối đa của loại file (không ép đúng một số byte, vì app không biết chính xác dung lượng sau khi nén; kích thước thật ghi lúc confirm); sai một điều kiện là bị từ chối ngay tại storage, nên không ai nhét được file lớn vào key tạm. Policy ký thủ công bằng SigV4 vì SDK .NET không hỗ trợ `content-length-range`.

## Hệ quả

- API hết chịu tải băng thông file; thêm một worker nền.
- Mỗi ảnh chỉ có một bản (cạnh dài 2048px do app nén), nên danh sách ảnh tải bản lớn hơn mức cần; chấp nhận để đơn giản, thêm bản thu nhỏ sau nếu số liệu cho thấy tốn data.
- File xuất hiện trong storage trước khi API biết (`cho_tai_len`), nên bắt buộc có job dọn.
- URL presigned không thu hồi được trước khi hết hạn: hạn phải ngắn (2 đến 5 phút riêng tư, 15 phút công khai); gỡ file hay hết quan hệ PT có hiệu lực sau tối đa chừng đó.
- App gửi form `multipart/form-data` (không phải PUT thuần): trường policy trước, trường `file` đặt cuối cùng.
- Mỗi đối tượng gắn file phải cài `IFileTruyCapPolicy`; thêm loại đối tượng mới là thêm một policy.
- Confirm tốn thêm một lần copy phía server (tối đa 100MB).
- App phải xóa cache ảnh riêng tư khi đăng xuất.
- App phải tự xử lý hết hạn URL và cache theo `file_id`.
- Cần cấu hình CORS cho bucket nếu có client web; mobile native không cần.

## Còn mở

- Hạn cụ thể của presigned GET (riêng tư, công khai) và form upload (presigned POST).
- Ảnh xem trước của video tạo bằng ffmpeg trong worker hay lấy từ client lúc upload.
- Khi nào thêm CDN cho file công khai.
- Chính sách xóa file khi đối tượng gắn bị xóa.

## Considered Options

- Giữ multipart qua API và chỉ dọn lại: ít việc nhất, nhưng API vẫn gánh băng thông, không có progress thật, video bị kẹt
  ở giới hạn thấp. Loại.
- CDN ngay từ đầu cho file công khai: nhanh hơn khi xem, nhưng thêm CloudFront, origin access và vấn đề file "công khai"
  không thu hồi được. Loại ở đợt này, chừa seam.
- Xử lý ảnh đồng bộ lúc confirm: không cần worker nhưng confirm chậm, tốn CPU API. Loại.
- Chỉ nén ở client, không xử lý server: không tin được client (EXIF/GPS, kích thước). Loại.
- Hàng đợi ngoài (SQS, RabbitMQ) hoặc worker tiến trình riêng: bền và tách tải hơn, nhưng thêm hạ tầng chưa cần ở quy mô
  vài chục nghìn người dùng. Loại; hàng đợi trong DB và worker trong API đủ, chuyển sau nếu CPU xử lý ảnh chen vào API.
- Bảng outbox riêng cho việc xử lý: cần thêm bảng và transaction ghi hai nơi, trong khi mỗi việc chỉ gắn với một dòng `tep_tin`. Loại.
- Backend proxy stream khi xem file riêng tư: kiểm quyền dễ nhưng giữ lại đúng vấn đề tải của hiện tại. Loại.
- Upload chia mảnh / resumable cho video lớn: không cần với video ngắn dưới 100MB. Loại; mở lại nếu cho phép video dài.
