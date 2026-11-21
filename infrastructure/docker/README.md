# Docker

`Dockerfile.api` build backend thành hai image từ cùng một Dockerfile (build context là `apps/api`):

| Target | Image | Dùng để |
| --- | --- | --- |
| `runtime` | `gymbase-api` | Chạy `GymBase.API` bằng user không phải root, cổng 8080 trong container |
| `migrator` | `gymbase-migrator` | EF migrations bundle: áp migration lên database rồi thoát |
| `publish` | | Bản publish, dùng chung cho hai target trên |

Dockerfile này dùng chung cho cả hai môi trường: dev build từ mã nguồn ([deploy/development](../../deploy/development/README.md)),
production cũng build từ mã nguồn ngay trên máy chủ
([deploy/production](../../deploy/production/README.md)). Build thử ở local:

```bash
docker build -f infrastructure/docker/Dockerfile.api --target runtime -t gymbase-api apps/api
docker build -f infrastructure/docker/Dockerfile.api --target migrator -t gymbase-migrator apps/api
```

## Ghi chú

- Cần BuildKit (mặc định trong Docker hiện nay); Dockerfile dùng `COPY --parents` nên khai báo `# syntax=docker/dockerfile:1`.
- `restore` tách riêng (chỉ chép file `.csproj`) để sửa code không làm tải lại NuGet.
- Phiên bản `dotnet-ef` (`DOTNET_EF_VERSION`) phải khớp `Microsoft.EntityFrameworkCore.Design` ở `apps/api/Directory.Packages.props`;
  nâng EF thì nâng cả hai.
- Migration tạo trigger nên MySQL cần `log_bin_trust_function_creators=1` khi bật binary log (compose production), hoặc tắt binary log
  (`--skip-log-bin`, compose dev).
- `docker-compose.minio.yml` chạy MinIO riêng (cổng 9000, 9001) cho lúc chạy API bằng tay, khớp `appsettings.json`. Compose dev
  ([deploy/development](../../deploy/development/README.md)) có MinIO riêng ở cổng 9100, 9101; production dùng S3 bên ngoài.
- Expo dev server chạy ngoài Docker để debug emulator/thiết bị thật.
