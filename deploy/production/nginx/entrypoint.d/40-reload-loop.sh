#!/bin/sh
# Chạy nền trong container nginx: nạp lại cấu hình mỗi 6 giờ để nhận chứng chỉ vừa được certbot gia hạn.
set -eu

(
  while :; do
    sleep 6h
    nginx -s reload || true
  done
) &
