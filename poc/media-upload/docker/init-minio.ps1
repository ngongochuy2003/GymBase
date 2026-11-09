param (
    [string]$MinioEndpoint = "http://localhost:9000",
    [string]$AccessKey = "minioadmin",
    [string]$SecretKey = "minioadmin",
    [string]$BucketName = "gymbase-media"
)

Write-Host "Cấu hình MinIO Client (mc)..." -ForegroundColor Cyan
mc alias set local-minio $MinioEndpoint $AccessKey $SecretKey

Write-Host "Tạo bucket $BucketName..." -ForegroundColor Cyan
mc mb --ignore-existing "local-minio/$BucketName"

Write-Host "Cấu hình Lifecycle Rule (tự động xóa file staging/ sau 1 ngày)..." -ForegroundColor Cyan
mc ilm rule add --prefix "staging/" --expire-days 1 "local-minio/$BucketName"

Write-Host "Khởi tạo MinIO hoàn tất!" -ForegroundColor Green
