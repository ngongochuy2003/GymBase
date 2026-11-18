# Bản build production: từ code đến container chạy

> **Ngày viết:** 17/11/2026 · **Cập nhật:** 18/11/2026 · **Trạng thái:** PoC (đã chạy thử ở local; chưa chạy trên VPS thật, Let's Encrypt chưa thử với tên miền thật, xem mục 5) · **Đối chiếu code:** 169fd49 (+ thay đổi chưa commit)

Tài liệu giải thích cách backend được đóng gói và đưa lên máy chủ thật. Cách vận hành từng bước (lần đầu, cập nhật, sao
lưu) nằm ở [deploy/production/README.md](../../deploy/production/README.md); tài liệu này giải thích **vì sao** nó được dựng như vậy.

Mã nguồn được **build ngay trên VPS**: kéo code bằng `git pull` rồi `deploy.sh` build image, migrate và khởi động lại. Không qua
registry và không có CI build image. Đổi lại, VPS cần cài thêm SDK .NET trong cache build (khoảng 5 đến 6 GB đĩa tổng cộng thay vì 2 đến 2,5 GB nếu chỉ kéo image).

```text
VPS: git pull ──▶ deploy.sh
                  sao lưu → build 2 image → migrate → up
```

## Thư mục nào làm gì

| Thư mục | Vai trò |
| --- | --- |
| `infrastructure/docker/` | Cách **đóng gói** API (`Dockerfile.api`). Không biết gì về môi trường, dev và production dùng chung. |
| `deploy/development/` | Cách **chạy** ở máy dev: build từ mã nguồn, mở cổng HTTP, tự migrate. |
| `deploy/production/` | Cách **chạy** ở máy chủ thật: nginx, HTTPS, MySQL, build từ mã nguồn, migrate chạy tay. |

## 1. Build: `Dockerfile.api` (multi-stage)

Dockerfile chia thành nhiều tầng (stage). Tầng sau lấy kết quả của tầng trước, nên image cuối chỉ chứa phần cần để chạy, không có SDK hay mã nguồn.
Build context là `apps/api`.

| Stage | Base | Việc làm |
| --- | --- | --- |
| `restore` | dotnet **sdk** | Chỉ chép các file `.csproj`, `Directory.Packages.props`, `global.json` rồi `dotnet restore` (tải NuGet). |
| `publish` | từ `restore` | Chép mã nguồn rồi `dotnet publish -c Release` ra `/out`. |
| `bundle` | từ `publish` | Cài `dotnet-ef` rồi `dotnet ef migrations bundle`, tạo file thực thi `efbundle`. |
| `migrator` | dotnet **aspnet** | Chỉ chép `efbundle`; `ENTRYPOINT ./efbundle`. |
| `runtime` | dotnet **aspnet** | Chép `/out` từ `publish`, chạy bằng user không phải root. |

Lý do của từng lựa chọn:

- **Tách `restore` riêng.** Tải NuGet là bước chậm. Tầng này chỉ phụ thuộc vào các file khai báo package nên sửa code không làm
  tải lại. `--mount=type=cache` giữ thêm cache NuGet giữa các lần build.
- **Image cuối dùng `aspnet`, không dùng `sdk`.** Image nhỏ hơn nhiều (khoảng 220 MB so với khoảng 800 MB) và ít thành phần thừa.
- **`runtime` chạy bằng `$APP_UID`.** Image aspnet có sẵn user này. Nếu API bị khai thác thì kẻ tấn công không có quyền root trong
  container. `logs/` và `uploads/` được `chown` trước để user đó ghi được.
- **Cổng 8080.** User không phải root không bind được cổng dưới 1024; nginx proxy tới cổng này.
- **`migrator` tách riêng.** Image `runtime` không có SDK nên không chạy được `dotnet ef`. `migrations bundle` đóng gói toàn bộ
  migration thành một file chạy độc lập: nhận connection string, áp migration rồi thoát. Nhờ vậy API không tự đổi schema khi khởi
  động; áp migration là bước có kiểm soát, đi kèm sao lưu.
- **Hai biến giả ở stage `bundle`** (`ConnectionStrings__DefaultConnection`, `Jwt__SigningKey`) chỉ để host dựng được lúc tạo
  bundle vì code kiểm tra chúng khi khởi động. Không có kết nối database nào xảy ra ở bước này và chúng không nằm trong image cuối.
- **Phiên bản `dotnet-ef` (`DOTNET_EF_VERSION`) phải khớp** `Microsoft.EntityFrameworkCore.Design` trong
  `apps/api/Directory.Packages.props`. Nâng EF thì nâng cả hai.

Build thử ở local:

```bash
docker build -f infrastructure/docker/Dockerfile.api --target runtime -t gymbase-api apps/api
docker build -f infrastructure/docker/Dockerfile.api --target migrator -t gymbase-migrator apps/api
```

## 2. Triển khai: `deploy/production/scripts/deploy.sh`

Chạy sau `git pull`. Bốn bước theo thứ tự, dừng ngay nếu có lỗi (`set -euo pipefail`):

1. **Sao lưu** database bằng `backup.sh` (mysqldump nén ra `backups/`). Cần MySQL đang chạy, nên lần đầu không dùng script này (xem README).
2. **Build** hai image (`api` và `migrate`) từ mã nguồn đang checkout. Build lỗi thì dừng, **API cũ vẫn chạy**.
3. **Áp migration** bằng service `migrate` (chạy xong thoát). Lỗi thì dừng ở đây, API cũ vẫn chạy.
4. **`up -d`** tạo lại container API với image mới.

Script in ra commit đang triển khai (kèm cảnh báo nếu có thay đổi chưa commit ở `apps/api` hay `infrastructure/docker`).

**Rollback code:** `git checkout <commit cũ>` rồi chạy lại script. **Schema không tự quay lui:** nếu migration mới đã đổi bảng,
khôi phục từ bản sao lưu ở bước 1 hoặc viết migration sửa tiếp.

## 3. Các container khi chạy

Trong `deploy/production/compose.yml` (project `gymbase-prod`):

```text
Internet ──443──▶ nginx ──▶ api ──▶ mysql          certbot gia hạn chứng chỉ
                  (HTTPS)  (nội bộ)  (nội bộ)      file tải lên: S3 bên ngoài
```

| Service | Vai trò |
| --- | --- |
| `nginx` | Service duy nhất mở cổng (80, 443). Chuyển HTTP sang HTTPS, proxy vào `api:8080`, đặt `X-Forwarded-For` bằng IP thật của client để rate limit và audit log thấy đúng IP. |
| `certbot` | Vòng lặp gia hạn chứng chỉ Let's Encrypt. nginx tự nạp lại cấu hình mỗi 6 giờ để nhận chứng chỉ mới. |
| `mysql` | MySQL 8.4.6, không mở cổng ra ngoài. Bật binary log (giữ 7 ngày) để khôi phục theo thời điểm. Healthcheck để `api` chờ MySQL sẵn sàng. |
| `api` | `ASPNETCORE_ENVIRONMENT=Production`: không có OpenAPI, file tải lên đi vào S3, `Storage__AllowAnonymousForDev=false`. Healthcheck gọi `/health` (có kiểm tra DB). Bí mật lấy từ `.env`. |
| `migrate` | Thuộc profile `tools`: không tự chạy khi `up`, chỉ chạy khi gọi. |

Vì API tin header `X-Forwarded-For` do nginx đặt, **không được mở cổng API hay MySQL ra ngoài**: mở trực tiếp thì client tự giả IP được.

## 4. Lý do thiết kế

| Lựa chọn | Lý do |
| --- | --- |
| Build ngay trên VPS | Một máy, một quy trình, không cần registry hay CI cho triển khai. Đánh đổi: tốn đĩa cho cache build và phải dọn định kỳ. |
| Rollback bằng commit git | Biết chính xác đang chạy commit nào; quay lại bằng `git checkout`. |
| Migration chạy tay, kèm sao lưu | Đổi schema là việc rủi ro nhất, không để API tự làm khi khởi động. |
| User không phải root; API và MySQL không mở cổng | Giảm thiệt hại nếu một lớp bị phá. |

## 5. Chưa kiểm chứng

- Let's Encrypt và đường gia hạn chưa thử với tên miền thật: chạy `init-letsencrypt.sh` với `LETSENCRYPT_STAGING=1` trước.
- Chưa build và chạy trên VPS thật, chưa đo dung lượng thực tế của cache build.

Đã chạy thử ở local: `deploy.sh` (sao lưu, build, migrate, `up`; API healthy), migration trên MySQL 8.4 có binary log. nginx với chứng
chỉ tự ký đã thử ở lần viết đầu (HTTPS 200, chuyển hướng 301, OpenAPI không phục vụ, IP thật vào audit log dù client giả header).
