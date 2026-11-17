#!/usr/bin/env bash
# Xin chứng chỉ Let's Encrypt lần đầu và khởi động nginx. Chạy một lần sau khi điền .env:
#   ./scripts/init-letsencrypt.sh
#
# nginx cần chứng chỉ mới khởi động được, còn Let's Encrypt cần nginx đang chạy để xác thực qua cổng 80. Vì vậy: tạo
# chứng chỉ tạm, bật nginx, xin chứng chỉ thật, xóa chứng chỉ tạm, nạp lại nginx.
set -euo pipefail
cd "$(dirname "$0")/.."

[ -f .env ] || { echo "Chưa có .env (copy từ .env.example)." >&2; exit 1; }
set -a; . ./.env; set +a
: "${API_DOMAIN:?đặt API_DOMAIN trong .env}"
: "${LETSENCRYPT_EMAIL:?đặt LETSENCRYPT_EMAIL trong .env}"

compose() { docker compose --env-file .env "$@"; }
cert_dir="/etc/letsencrypt/live/${API_DOMAIN}"

echo "==> Tạo chứng chỉ tạm cho ${API_DOMAIN}"
compose run --rm --no-deps --entrypoint sh certbot -c "
  rm -rf '${cert_dir}' '/etc/letsencrypt/archive/${API_DOMAIN}' '/etc/letsencrypt/renewal/${API_DOMAIN}.conf' &&
  mkdir -p '${cert_dir}' &&
  openssl req -x509 -nodes -newkey rsa:2048 -days 1 \
    -keyout '${cert_dir}/privkey.pem' -out '${cert_dir}/fullchain.pem' -subj '/CN=localhost'"

echo "==> Khởi động nginx (và các service nó phụ thuộc)"
compose up -d nginx

echo "==> Xin chứng chỉ thật từ Let's Encrypt"
compose run --rm --no-deps --entrypoint sh certbot -c "
  rm -rf '${cert_dir}' '/etc/letsencrypt/archive/${API_DOMAIN}' '/etc/letsencrypt/renewal/${API_DOMAIN}.conf'"

staging=()
[ "${LETSENCRYPT_STAGING:-0}" = "1" ] && staging=(--staging)
compose run --rm --no-deps --entrypoint certbot certbot certonly --webroot -w /var/www/certbot \
  -d "${API_DOMAIN}" --email "${LETSENCRYPT_EMAIL}" --agree-tos --no-eff-email "${staging[@]}"

echo "==> Nạp lại nginx với chứng chỉ thật"
compose exec nginx nginx -s reload
echo "Xong. Kiểm tra: curl -I https://${API_DOMAIN}/health"
