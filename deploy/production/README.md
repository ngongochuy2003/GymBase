# Triển khai production (một VPS)

```text
Internet ──443──▶ nginx ──▶ api ──▶ mysql          certbot gia hạn chứng chỉ
                  (HTTPS)  (nội bộ)  (nội bộ)      file tải lên: S3 bên ngoài
```

Chỉ nginx mở cổng (80, 443). API và MySQL chỉ thấy nhau trong mạng nội bộ Docker. **Không mở cổng API hay MySQL ra ngoài**: API
tin header `X-Forwarded-For` do nginx đặt (cho rate limit và audit log), mở cổng trực tiếp thì client tự giả IP được.

| File | Việc |
| --- | --- |
| `compose.yml` | nginx, certbot, mysql, api; `migrate` là công cụ chạy tay (profile `tools`) |
| `nginx/templates/default.conf.template` | HTTP chuyển sang HTTPS, proxy tới API, đặt `X-Forwarded-*` |
| `scripts/init-letsencrypt.sh` | Xin chứng chỉ lần đầu (chạy một lần) |
| `scripts/deploy.sh <tag>` | Sao lưu, kéo image, áp migration, khởi động lại API |
| `scripts/backup.sh` | Sao lưu database ra `backups/` |
| `.env.example` | Danh sách biến cấu hình |

Image do CI build (workflow `.github/workflows/api-image.yml`) và đẩy lên GHCR; máy chủ chỉ `pull`, không build.

## Chuẩn bị máy chủ (một lần)

1. Linux có Docker Engine và Docker Compose v2; mở cổng 80 và 443 ở tường lửa.
2. Tên miền (`API_DOMAIN`) có bản ghi A/AAAA trỏ về IP máy chủ.
3. Bucket S3 (hoặc dịch vụ tương thích) và khóa truy cập chỉ có quyền trên bucket đó.
4. Đăng nhập registry nếu package GHCR để riêng tư: `docker login ghcr.io -u <user>` với PAT có quyền `read:packages`.
5. Lấy mã nguồn (chỉ cần thư mục `deploy/production`) và tạo `.env`:

```bash
cd deploy/production
cp .env.example .env && chmod 600 .env
# điền .env: API_DOMAIN, LETSENCRYPT_EMAIL, IMAGE_TAG, MYSQL_*, Jwt__SigningKey (openssl rand -base64 48), S3_*...
```

## Lần triển khai đầu tiên

```bash
# Chạy thử với LETSENCRYPT_STAGING=1 trước để khỏi chạm giới hạn của Let's Encrypt, rồi đặt về 0 và chạy lại.
./scripts/init-letsencrypt.sh                 # chứng chỉ HTTPS (script này bật nginx và các service phụ thuộc)
docker compose --env-file .env up -d mysql
docker compose --env-file .env --profile tools run --rm migrate    # tạo bảng và danh mục
docker compose --env-file .env up -d
curl -I https://<API_DOMAIN>/health           # 200
```

Tạo tài khoản Admin đầu tiên (mật khẩu lấy từ biến môi trường để không nằm trong lịch sử lệnh):

```bash
read -rsp "Mật khẩu Admin: " GYMBASE_ADMIN_PASSWORD; export GYMBASE_ADMIN_PASSWORD
docker compose --env-file .env run --rm -e GYMBASE_ADMIN_PASSWORD api create-admin admin@<domain>
unset GYMBASE_ADMIN_PASSWORD
```

Mobile: đặt `EXPO_PUBLIC_API_URL=https://<API_DOMAIN>` (không kèm `/api/v1/mob`).

## Cập nhật phiên bản

Tag image có dạng `sha-<7 ký tự đầu của commit>` (hoặc `v1.2.3` khi push tag git), xem ở trang Packages của repo.

```bash
./scripts/deploy.sh sha-1a2b3c4
```

Script sao lưu DB, ghi tag mới vào `.env`, kéo image, áp migration rồi khởi động lại API. Nếu migration lỗi thì dừng, API cũ vẫn chạy.

**Rollback code:** `./scripts/deploy.sh <tag cũ>` (script in ra tag trước đó). Migration không tự quay lui: nếu migration mới đã
đổi schema, khôi phục từ bản sao lưu trước đó, hoặc viết migration sửa tiếp.

## Sao lưu và khôi phục

```bash
./scripts/backup.sh                           # ra backups/gymbase-<UTC>.sql.gz
```

Chạy định kỳ bằng cron, ví dụ `0 2 * * * /đường/dẫn/deploy/production/scripts/backup.sh`. File nằm cùng máy chủ nên **phải sao
chép thêm ra nơi khác** (rclone, rsync, S3...). Khôi phục (ghi đè dữ liệu hiện tại, dừng API trước):

```bash
docker compose --env-file .env stop api
gunzip -c backups/gymbase-<...>.sql.gz | docker compose --env-file .env exec -T mysql sh -c 'mysql -uroot -p"$MYSQL_ROOT_PASSWORD" "$MYSQL_DATABASE"'
docker compose --env-file .env start api
```

MySQL bật binary log (giữ 7 ngày) nên có thể khôi phục theo thời điểm bằng `mysqlbinlog` khi cần; nó nằm trong volume `mysql_data`.

## Lệnh thường dùng

```bash
docker compose --env-file .env ps
docker compose --env-file .env logs -f api          # log ra console, mỗi service giữ tối đa 5 x 10 MB
docker compose --env-file .env restart api
```

## Chưa có trong cấu hình này

- **Giám sát và cảnh báo** (uptime, dung lượng đĩa, lỗi 5xx): cần dịch vụ riêng.
- **Thông báo đẩy thật:** `NOTIFICATION_SIMULATE_MODE=true` mặc định, chỉ giả lập; gửi thật cần cấu hình Firebase.
- **Đăng nhập Google:** để trống `GOOGLE_WEB_CLIENT_ID` thì `/auth/google` luôn trả 401.
- **Nhiều instance API:** bộ đếm đăng nhập và rate limit nằm trong bộ nhớ (xem `docs/api/xac-thuc.md`), nên chỉ chạy một instance.
- **Quản lý bí mật tập trung** (Vault, Docker secrets): hiện bí mật nằm ở `.env` (`chmod 600`).
- Chứng chỉ Let's Encrypt và đường gia hạn chưa được thử với tên miền thật trong repo này; hãy chạy `init-letsencrypt.sh`
  với `LETSENCRYPT_STAGING=1` trước và kiểm tra `docker compose logs certbot` sau vài ngày.
