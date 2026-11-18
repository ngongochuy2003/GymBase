#!/usr/bin/env bash
# Triển khai mã nguồn đang checkout: sao lưu DB, build image, áp migration, khởi động lại API.
#   git pull && ./scripts/deploy.sh
# Rollback code: git checkout <commit cũ> rồi chạy lại (script in ra commit đang chạy). Rollback schema không tự động:
# khôi phục từ bản sao lưu.
set -euo pipefail
cd "$(dirname "$0")/.."

[ -f .env ] || { echo "Chưa có .env." >&2; exit 1; }

compose() { docker compose --env-file .env "$@"; }
commit="$(git rev-parse --short HEAD)"
dirty=""; [ -z "$(git status --porcelain -- ../../apps/api ../../infrastructure/docker)" ] || dirty=" (có thay đổi chưa commit)"
echo "Triển khai commit: ${commit}${dirty}"

echo "==> Sao lưu database"
./scripts/backup.sh

echo "==> Build image (api và migrator)"
compose --profile tools build

echo "==> Áp migration"
compose --profile tools run --rm migrate

echo "==> Khởi động lại API"
compose up -d --remove-orphans

echo "Xong. Commit đang chạy: ${commit}. Để rollback: git checkout <commit cũ> rồi chạy lại script này."
