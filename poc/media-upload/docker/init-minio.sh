#!/usr/bin/env bash
set -e

MINIO_ENDPOINT=${1:-"http://localhost:9000"}
MINIO_ACCESS_KEY=${2:-"minioadmin"}
MINIO_SECRET_KEY=${3:-"minioadmin"}
BUCKET_NAME=${4:-"gymbase-media"}

echo "Cấu hình MinIO Client (mc)..."
mc alias set local-minio "$MINIO_ENDPOINT" "$MINIO_ACCESS_KEY" "$MINIO_SECRET_KEY"

echo "Tạo bucket: $BUCKET_NAME (nếu chưa có)..."
mc mb --ignore-existing "local-minio/$BUCKET_NAME"

echo "Cấu hình vòng đời (Lifecycle Rule): xóa staging/ sau 1 ngày..."
mc ilm rule add --prefix "staging/" --expire-days 1 "local-minio/$BUCKET_NAME"

echo "Đã khởi tạo MinIO cho GymBase Media POC thành công!"
