# SQL hỗ trợ

Chỉ thêm script khi có nhu cầu setup/vận hành hoặc data fix cụ thể.
Mỗi script cần mô tả môi trường áp dụng, điều kiện trước khi chạy, cách kiểm tra
kết quả, transaction và rollback nếu áp dụng. Không đặt credentials trong SQL.
Thay đổi schema ứng dụng phải đi qua EF Core migrations của backend.

## `tao-seed-khu-vuc.py`

Sinh `Migrations/Sql/SeedKhuVuc.sql` (tỉnh/thành và phường/xã) từ file Excel danh sách đơn vị hành chính; không
kết nối database. Chạy từ gốc repo: `python database/scripts/tao-seed-khu-vuc.py <file.xlsx>`. SQL do migration
`SeedDanhMuc` chạy. Migration đã áp dụng ở đâu đó thì không sinh đè file này: danh sách đổi thì thêm migration mới.
