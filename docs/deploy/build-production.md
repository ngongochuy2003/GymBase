# Bản build production: từ code đến container chạy

> **Ngày viết:** 17/11/2026 · **Trạng thái:** PoC (đã chạy thử ở local; chưa chạy trên VPS thật, workflow CI chưa chạy lần nào, Let's Encrypt chưa thử với tên miền thật, xem mục 6) · **Đối chiếu code:** 71cef01

Tài liệu giải thích cách backend được đóng gói và đưa lên máy chủ thật. Cách vận hành từng bước (lần đầu, cập nhật, sao
lưu) nằm ở [deploy/production/README.md](../../deploy/production/README.md); tài liệu này giải thích **vì sao** nó được dựng như vậy.

Quy trình có ba giai đoạn: **build** (trên GitHub), **lưu image** (trên GHCR), **chạy** (trên VPS). VPS không bao giờ build, chỉ kéo image về.

```text
push main ──▶ GitHub Actions ──▶ GHCR (registry) ────────────────▶ VPS: deploy.sh <tag>
              build 2 image       gymbase-api:sha-1a2b3c4           sao lưu → pull → migrate → up
                                  gymbase-migrator:sha-1a2b3c4
```

## Thư mục nào làm gì

| Thư mục | Vai trò |
| --- | --- |
| `infrastructure/docker/` | Cách **đóng gói** API (`Dockerfile.api`). Không biết gì về môi trường, dev và production dùng chung. |
| `deploy/development/` | Cách **chạy** ở máy dev: build từ mã nguồn, mở cổng HTTP, tự migrate. |
| `deploy/production/` | Cách **chạy** ở máy chủ thật: nginx, HTTPS, MySQL, kéo image theo tag, migrate chạy tay. |
| `.github/workflows/api-image.yml` | CI build và đẩy image lên GHCR. |

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

## 2. CI: `.github/workflows/api-image.yml`

- **Khi nào chạy:** push lên `main` (chỉ khi đổi `apps/api/**`, `Dockerfile.api` hoặc chính workflow), push tag `v*`, hoặc chạy tay (`workflow_dispatch`).
- **Làm gì:** đăng nhập GHCR bằng `GITHUB_TOKEN` (không cần bí mật tự tạo), build và đẩy **hai image**:
  `ghcr.io/<owner>/gymbase-api` (target `runtime`) và `ghcr.io/<owner>/gymbase-migrator` (target `migrator`).
- **Tag:** luôn có `sha-<7 ký tự đầu của commit>`; chạy từ tag git thì thêm tag đó (ví dụ `v1.2.3`).
- **Không có `latest`.** Có chủ ý: `latest` không cho biết đang chạy bản nào và rollback không biết quay về đâu. Mỗi phiên bản có một tag cụ thể.
- **Cache:** `type=gha` lưu cache layer trên GitHub nên các lần build sau nhanh hơn.
- **`concurrency`:** hai lần chạy cùng ref xếp hàng, không chạy chồng lên nhau.

## 3. Triển khai: `deploy/production/scripts/deploy.sh <tag>`

Script chạy năm bước theo thứ tự và dừng ngay nếu có lỗi (`set -euo pipefail`):

1. **Sao lưu** database bằng `backup.sh` (mysqldump nén ra `backups/`).
2. **Ghi tag mới** vào `.env` (`IMAGE_TAG=...`); compose đọc tag từ đó để biết kéo image nào.
3. **`pull`** image `api` và `migrator`.
4. **Áp migration** bằng service `migrate` (chạy xong thoát). Nếu lỗi thì script dừng ở đây, **API cũ vẫn đang chạy**.
5. **`up -d`** tạo lại container API với image mới.

Cuối cùng script in ra tag cũ để dùng khi rollback.

**Rollback code:** `./scripts/deploy.sh <tag cũ>`. **Schema không tự quay lui:** nếu migration mới đã đổi bảng, khôi phục từ bản
sao lưu ở bước 1 hoặc viết migration sửa tiếp.

## 4. Các container khi chạy

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

## 5. Lý do thiết kế

| Lựa chọn | Lý do |
| --- | --- |
| Build ở CI, VPS chỉ pull | VPS không phải gánh việc build; bản chạy giống hệt bản đã build. |
| Tag bất biến, không `latest` | Biết chính xác đang chạy gì; rollback rõ ràng. |
| Migration chạy tay, kèm sao lưu | Đổi schema là việc rủi ro nhất, không để API tự làm khi khởi động. |
| User không phải root; API và MySQL không mở cổng | Giảm thiệt hại nếu một lớp bị phá. |

## 6. Chưa kiểm chứng

- Workflow `api-image.yml` chưa chạy lần nào vì chỉ kích hoạt khi push lên `main`. Lần đầu nên chạy tay (`workflow_dispatch`) để kiểm tra.
- Let's Encrypt và đường gia hạn chưa thử với tên miền thật: chạy `init-letsencrypt.sh` với `LETSENCRYPT_STAGING=1` trước.
- Package GHCR để riêng tư thì VPS cần `docker login ghcr.io` với PAT có quyền `read:packages`.

Đã chạy thử ở local: build hai image, nginx với chứng chỉ tự ký (HTTPS 200, chuyển hướng 301, OpenAPI không phục vụ, IP thật vào
audit log dù client giả header), migration trên MySQL 8.4 có binary log.
