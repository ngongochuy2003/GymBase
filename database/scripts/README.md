# SQL hỗ trợ

Chỉ thêm script khi có nhu cầu setup/vận hành hoặc data fix cụ thể.
Mỗi script cần mô tả môi trường áp dụng, điều kiện trước khi chạy, cách kiểm tra
kết quả, transaction và rollback nếu áp dụng. Không đặt credentials trong SQL.
Thay đổi schema ứng dụng phải đi qua EF Core migrations của backend.
