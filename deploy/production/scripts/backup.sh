#!/usr/bin/env bash
# Sao lưu database ra deploy/production/backups/gymbase-<UTC>.sql.gz (nhất quán, không khóa bảng) rồi xóa bản cũ.
#   ./scripts/backup.sh
# Chạy định kỳ bằng cron, ví dụ mỗi ngày 02:00:  0 2 * * * /đường/dẫn/deploy/production/scripts/backup.sh
# File sao lưu nằm cùng máy chủ nên cần sao chép thêm ra nơi khác (rclone, rsync, S3...).
set -euo pipefail
cd "$(dirname "$0")/.."

[ -f .env ] || { echo "Chưa có .env." >&2; exit 1; }
set -a; . ./.env; set +a

mkdir -p backups
file="backups/gymbase-$(date -u +%Y%m%dT%H%M%SZ).sql.gz"

docker compose --env-file .env exec -T mysql sh -c \
  'exec mysqldump --single-transaction --routines --triggers --events -uroot -p"$MYSQL_ROOT_PASSWORD" "$MYSQL_DATABASE"' \
  | gzip > "${file}.partial"

# Chỉ đổi tên khi mysqldump thành công và file không rỗng, để không có bản sao lưu hỏng.
[ -s "${file}.partial" ] || { rm -f "${file}.partial"; echo "Sao lưu rỗng." >&2; exit 1; }
mv "${file}.partial" "${file}"
echo "Đã sao lưu: ${file}"

find backups -name 'gymbase-*.sql.gz' -mtime "+${BACKUP_KEEP_DAYS:-14}" -delete
