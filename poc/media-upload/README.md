# POC: Hạ Tầng Lưu Trữ File GymBase Chuẩn V1 (Local Storage Trước — Sẵn Sàng S3 Sau)

> **Dự án:** GymBase Monorepo (Nền tảng kết nối PT và Học viên)  
> **Phiên bản:** Chuẩn V1 (Theo tài liệu `GymBase_Huong_Dan_Base_Luu_Tru_Local_San_Sang_S3.docx` & SRS V1)  
> **Nguyên tắc cốt lõi:** Lưu trữ trên ổ đĩa nội bộ (Local Disk / Docker Volume) ở giai đoạn đầu để tiết kiệm chi phí; khi chuyển sang Amazon S3 chỉ đổi cấu hình, **không sửa API, không sửa Frontend Mobile, không sửa code nghiệp vụ.**

---

## 1. Triết lý Thiết kế Chuẩn V1

```
+-------------------------------------------------------------------------+
|                              MOBILE / WEB                               |
|                     (Gọi duy nhất: /api/files)                          |
+------------------------------------+------------------------------------+
                                     | (1) Upload multipart
                                     | (2) Tải /api/files/{id}/content
                                     v
+-------------------------------------------------------------------------+
|                    GYMBASE BACKEND (.NET CORE 9)                         |
|                                                                         |
|  [FilesEndpoints]  ---> [FileService] ---> [SkiaImageProcessor (WebP)]  |
|                                |                                        |
|                                v                                        |
|                     [IStorageResolver]                                  |
|                      /              \                                   |
|                     v                v                                  |
|          [LocalFileStorage]     [S3FileStorage]                         |
|           (Volume Docker)        (Amazon S3 / MinIO)                    |
|                 |                        |                              |
|                 v                        v                              |
|          /app/uploads/...       gymbase-media/...                       |
+-------------------------------------------------------------------------+
                                 |
                                 v
                     +-----------------------+
                     | CSDL MySQL (tep_tin)  |
                     |  - nha_cung_cap:      |
                     |    'LOCAL' hoặc 'S3'  |
                     |  - khoa_luu_tru       |
                     |  - loai_mime, blurhash|
                     +-----------------------+
```

### Tại sao chọn Local Storage trước?
1. **Tiết kiệm chi phí ban đầu:** Chưa phát sinh chi phí S3 PUT/GET/Egress khi đang thử nghiệm và số lượng người dùng V1 còn nhỏ.
2. **Đơn giản hóa hạ tầng:** Chạy ngay trên VPS / Docker Compose của dự án mà không cần tạo IAM User, Bucket, CORS, lifecycle rule ngay ngày đầu.
3. **Sẵn sàng chuyển S3 mượt mà:** Khóa lưu trữ (`khoa_luu_tru`) được thiết kế đồng nhất giữa Local và S3 (ví dụ: `avatar/user-id/guid.webp`). Khi chuyển S3, chỉ cần đổi cấu hình `DefaultProvider = "S3"`, file mới lưu lên S3, file cũ vẫn đọc qua Local nhờ cột `nha_cung_cap`.

---

## 2. Chi Tiết API Chuẩn V1

Tất cả các route nằm trong nhóm `/api/files`:

| Phương thức | Đường dẫn | Tham số / Body | Mô tả chức năng |
|:---|:---|:---|:---|
| `POST` | `/api/files` | `multipart/form-data`: `file`, `purpose` (`avatar`, `gym_photo`, `progress`, `post`) | Nhận file, kiểm tra magic bytes, nén WebP, sinh Blurhash, lưu xuống Provider mặc định (`LOCAL`), ghi DB bảng `tep_tin`. |
| `GET` | `/api/files/{id}` | `id` (GUID) | Lấy thông tin metadata của tệp tin từ bảng `tep_tin`. |
| `GET` | `/api/files/{id}/content` | `id` (GUID) | Kiểm tra phân quyền (chỉ chủ sở hữu hoặc ảnh public `avatar`/`post` mới được xem), mở luồng stream trả về client với `Content-Type: image/webp`. |
| `DELETE` | `/api/files/{id}` | `id` (GUID) | Kiểm tra quyền sở hữu, đánh dấu xóa mềm trong CSDL và xóa file vật lý trên Storage. |
| `POST` | `/api/files/migrate-to-s3` | `batchSize` (mặc định 50) | Script chuyển đổi dữ liệu từ Local sang S3 không downtime, tự động cập nhật `nha_cung_cap = 'S3'`. |

*(Bên cạnh đó, POC vẫn bảo lưu nhóm API Direct-to-S3 `/api/v1/media/*` dùng Presigned URL phục vụ đối sánh hoặc mở rộng sau này).*

---

## 3. Cấu Trúc Bảng CSDL `tep_tin` (Đề xuất EF Core / MySQL)

```sql
CREATE TABLE `tep_tin` (
    `id`             CHAR(36)      NOT NULL,
    `nguoi_tai_len_id` CHAR(36)    NOT NULL,
    `ten_goc`        VARCHAR(255)  NOT NULL,
    `khoa_luu_tru`   VARCHAR(500)  NOT NULL,
    `nha_cung_cap`   VARCHAR(20)   NOT NULL DEFAULT 'LOCAL', -- 'LOCAL' hoặc 'S3'
    `loai_mime`      VARCHAR(100)  NOT NULL,
    `dung_luong`     BIGINT        NOT NULL,
    `muc_dich`       VARCHAR(50)   NOT NULL,                 -- 'avatar', 'gym_photo', 'progress', 'post'
    `trang_thai`     VARCHAR(20)   NOT NULL DEFAULT 'UPLOADING', -- 'UPLOADING', 'READY', 'FAILED', 'DELETING'
    `blurhash`       VARCHAR(100)  NULL,
    `ngay_tao`       DATETIME(6)   NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `ngay_cap_nhat`  DATETIME(6)   NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    `ngay_xoa`       DATETIME(6)   NULL,
    PRIMARY KEY (`id`),
    INDEX `idx_tep_tin_nguoi_tai_len` (`nguoi_tai_len_id`),
    INDEX `idx_tep_tin_nha_cung_cap_trang_thai` (`nha_cung_cap`, `trang_thai`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

---

## 4. Bảo Mật và Tối Ưu Hóa

1. **Chống Path Traversal (Directory Traversal):**
   - Lớp `LocalFileStorage` chuẩn hóa đường dẫn, từ chối mọi key chứa `..`, `\`, ký tự điều khiển, dấu `/` liên tiếp, hoặc đường dẫn tuyệt đối.
   - Luôn đảm bảo đường dẫn vật lý cuối cùng nằm trọn vẹn bên trong thư mục gốc `LocalRoot` (`/app/uploads`). Đã kiểm thử qua 7 vector tấn công trong Unit Test.
2. **Kiểm tra Magic Bytes chống giả mạo file:**
   - Không tin tưởng header `Content-Type` do client gửi lên.
   - SkiaSharp đọc 16 bytes đầu tiên để xác nhận chữ ký nhị phân thực sự của file ảnh (JPEG, PNG, WebP, HEIC). File văn bản trá hình đuôi `.jpg` sẽ bị từ chối ngay với HTTP 400.
3. **Chống tấn công Decompression Bomb:**
   - Giới hạn kích thước ảnh tối đa: chiều rộng/cao tối đa 8,192 px và tổng số điểm ảnh không vượt quá 36,000,000 pixels.
4. **Tối ưu dung lượng với WebP & SkiaSharp:**
   - Nén ảnh về định dạng WebP chất lượng 82, giúp giảm 70% – 85% dung lượng so với JPEG gốc của điện thoại chụp mà mắt thường không phân biệt được.
5. **Blurhash Placeholder tức thì:**
   - Tự động trích xuất chuỗi đại diện 20-30 ký tự (Blurhash) lưu cùng metadata. Mobile hiển thị placeholder mờ ngay lập tức khi cuộn danh sách (tránh hiện tượng màn hình trắng chớp nháy).

---

## 5. Hướng Dẫn Chạy Thử Nghiệm

Toàn bộ POC hoàn toàn độc lập, không cần Docker hay S3 thật để chạy thử.

### Cách 1: Chạy trực tiếp với Local Storage (Khuyến nghị cho Dev)
```bash
# 1. Chạy Backend API (Tự động tạo thư mục uploads/)
dotnet run --project poc/media-upload/backend/src/GymBase.Poc.Media.Api

# 2. Mở trình duyệt xem Swagger UI:
# http://localhost:5242
```

### Cách 2: Chạy Backend Tests (100% Pass)
```bash
dotnet test poc/media-upload/backend/GymBase.Poc.Media.slnx
```
**Kết quả thực tế:**
- **59 tests PASSED** (0 failed, duration: ~700 ms). Bao gồm:
  - Unit tests: `LocalFileStorageTests`, `StorageResolverTests`, `MediaValidatorTests`, `MediaKeyBuilderTests`, `DeduplicationTests`, `SkiaImageProcessorTests`, `FileServiceTests`, `FileMigrationTests`.
  - Integration tests: `FilesApiIntegrationTests` (Upload, GetMetadata, Stream Content, Delete, Magic Bytes), `SecurityValidationIntegrationTests`, `MediaUploadFlowIntegrationTests`.

### Cách 3: Chạy Mobile Jest Tests (100% Pass)
```bash
npm.cmd test --prefix poc/media-upload/mobile
```
**Kết quả thực tế:**
- **9 tests PASSED** (2 test suites, duration: ~12s):
  - `uploadMultipartFile` (POST `/api/files`, xử lý kết quả, xử lý lỗi 400).
  - `uploadImage` (Presigned URL, Retry XHR, Abort signal).
  - `selectImageVariant` (chọn thumb, medium, large theo kích thước khung hình).

---

## 6. Sử Dụng Phía Mobile React Native (Expo)

### 1. Upload ảnh bằng `uploadMultipartFile` (Chuẩn V1)
```typescript
import { uploadMultipartFile } from './services/image-uploader';

async function handleAvatarPicked(imageUri: string, userToken: string) {
  try {
    const result = await uploadMultipartFile(imageUri, 'avatar', {
      apiBaseUrl: 'http://10.0.2.2:5242', // IP máy dev trên Android Emulator
      authToken: userToken,
    });

    console.log('Upload thành công!', result.file_id, result.content_url);
    // Lưu result.file_id vào thông tin người dùng trong CSDL
  } catch (error) {
    console.error('Lỗi upload:', error);
  }
}
```

### 2. Hiển thị ảnh bằng `OptimizedFileImage`
```tsx
import React from 'react';
import { OptimizedFileImage } from './components/optimized-image';

export const UserAvatar = ({ fileId, blurhash }: { fileId: string; blurhash?: string }) => {
  return (
    <OptimizedFileImage
      fileId={fileId}
      apiBaseUrl="http://10.0.2.2:5242"
      blurhash={blurhash}
      style={{ width: 80, height: 80, borderRadius: 40 }}
      contentFit="cover"
    />
  );
};
```

---

## 7. Kế Hoạch Chuyển Đổi Sang Amazon S3 (Không Downtime)

Khi dự án GymBase bước vào giai đoạn mở rộng:
1. **Bước 1:** Cấu hình biến môi trường trên Production:
   ```json
   "Storage": {
     "DefaultProvider": "S3",
     "BucketName": "gymbase-media-prod",
     "Region": "ap-southeast-1"
   }
   ```
   *Kể từ thời điểm này, toàn bộ ảnh mới tải lên sẽ tự động ghi vào S3.*
2. **Bước 2:** Chạy tiến trình di chuyển nền (Background Migration):
   - Gọi API nội bộ: `POST /api/files/migrate-to-s3?batchSize=100` (hoặc chạy HostedService `FileMigrationService`).
   - Tiến trình sẽ đọc các file có `nha_cung_cap = 'LOCAL'`, đẩy sang S3, kiểm tra kích thước xác thực và cập nhật `nha_cung_cap = 'S3'`.
   - Nếu có lỗi trong quá trình đẩy một file cụ thể, file đó vẫn giữ `nha_cung_cap = 'LOCAL'`, người dùng vẫn xem bình thường mà không hề bị gián đoạn dịch vụ.

---

## 8. Điểm Ghép Vào Base Code Chính Của Monorepo

Khi đưa vào nhánh chính (`apps/api` và `apps/mobile`), chỉ cần thực hiện 4 bước sau:

1. **Backend Dependency Injection (`apps/api/src/GymBase.Api/Program.cs`):**
   ```csharp
   // Thêm 1 dòng duy nhất
   builder.Services.AddGymBaseFileStorage(builder.Configuration);

   // Map endpoints
   app.MapFilesEndpoints();
   ```
2. **Entity Framework Core Migration (`GymBase.Infrastructure/Data`):**
   - Đưa class `TepTin.cs` vào thư mục Entities.
   - Thêm `public DbSet<TepTin> TepTin { get; set; }` vào `AppDbContext`.
   - Thay thế `InMemoryTepTinRepository` bằng `EfTepTinRepository`.
3. **Cấu hình AppSettings (`appsettings.json`):**
   - Thêm khối cấu hình `"Storage": { "DefaultProvider": "LOCAL", "LocalRoot": "uploads" }`.
4. **Mobile Base (`apps/mobile/src/shared/components`):**
   - Copy thư mục `poc/media-upload/mobile/src/` vào `apps/mobile/src/shared/media/`.
   - Cài đặt `expo-image` và `expo-image-manipulator` vào `apps/mobile`.
