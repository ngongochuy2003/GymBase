# Docker

`Dockerfile.api` build backend thành hai image từ cùng một Dockerfile (build context là `apps/api`):

| Target | Image | Dùng để |
| --- | --- | --- |
| `runtime` | `gymbase-api` | Chạy `GymBase.API` bằng user không phải root, cổng 8080 trong container |
| `migrator` | `gymbase-migrator` | EF migrations bundle: áp migration lên database rồi thoát |
| `publish` | | Bản publish, dùng chung cho hai target trên |

Image được CI build và đẩy lên GHCR (`.github/workflows/api-image.yml`); cách chạy trên máy chủ nằm ở
[deploy/production/README.md](../../deploy/production/README.md). Build thử ở local:

```bash
docker build -f infrastructure/docker/Dockerfile.api --target runtime -t gymbase-api apps/api
docker build -f infrastructure/docker/Dockerfile.api --target migrator -t gymbase-migrator apps/api
```

## Ghi chú

- Cần BuildKit (mặc định trong Docker hiện nay); Dockerfile dùng `COPY --parents` nên khai báo `# syntax=docker/dockerfile:1`.
- `restore` tách riêng (chỉ chép file `.csproj`) để sửa code không làm tải lại NuGet.
- Phiên bản `dotnet-ef` (`DOTNET_EF_VERSION`) phải khớp `Microsoft.EntityFrameworkCore.Design` ở `apps/api/Directory.Packages.props`;
  nâng EF thì nâng cả hai.
- MySQL phải có `log_bin_trust_function_creators=1` nếu bật binary log, vì migration tạo trigger (cấu hình đã có trong compose production).
- `docker-compose.minio.yml` chạy MinIO cho dev, không thuộc luồng production (production dùng S3 bên ngoài).
- Expo dev server chạy ngoài Docker để debug emulator/thiết bị thật.
