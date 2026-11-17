#!/usr/bin/env bash
# Triển khai một tag image: sao lưu DB, kéo image, áp migration, khởi động lại API.
#   ./scripts/deploy.sh <tag>        ví dụ: ./scripts/deploy.sh sha-1a2b3c4
# Rollback code: chạy lại với tag cũ (script in ra tag đang chạy). Rollback schema không tự động: khôi phục từ bản sao lưu.
set -euo pipefail
cd "$(dirname "$0")/.."

tag="${1:?Cách dùng: $0 <tag>}"
[ -f .env ] || { echo "Chưa có .env." >&2; exit 1; }

compose() { docker compose --env-file .env "$@"; }
current="$(grep -E '^IMAGE_TAG=' .env | cut -d= -f2- || true)"
echo "Tag đang chạy: ${current:-<chưa có>}  ->  tag mới: ${tag}"

echo "==> Sao lưu database"
./scripts/backup.sh

sed -i "s|^IMAGE_TAG=.*|IMAGE_TAG=${tag}|" .env

echo "==> Kéo image"
compose pull api
compose --profile tools pull migrate

echo "==> Áp migration"
compose --profile tools run --rm migrate

echo "==> Khởi động lại API"
compose up -d --remove-orphans

echo "Xong. Tag trước đó để rollback: ${current:-<không có>}"
