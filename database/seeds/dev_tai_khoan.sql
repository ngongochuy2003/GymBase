-- Dữ liệu mẫu cho môi trường dev: 5 người dùng.
-- Chạy có chủ đích sau khi áp dụng migration; chạy lại nhiều lần không sinh bản ghi trùng.
-- KHÔNG chạy trên production. mat_khau_hash là giá trị giả, tài khoản mẫu không đăng nhập được.
-- Id lưu dạng chuỗi UUID trong cột varchar(36).

INSERT IGNORE INTO tai_khoan
    (Id, ten_dang_nhap, ho_ten, email, so_dien_thoai, vai_tro, dang_hoat_dong, mat_khau_hash, ngay_tao, nguoi_tao)
VALUES
    ('0193f0a0-0000-7000-8000-000000000001', 'admin', 'Quản trị viên', 'admin@gymbase.local', '0900000001', 'Admin', 1, 'mock-not-a-real-hash', '2026-11-01 00:00:00', 'seed'),
    ('0193f0a0-0000-7000-8000-000000000002', 'pt.minh', 'Nguyễn Văn Minh', 'minh@gymbase.local', '0900000002', 'User', 1, 'mock-not-a-real-hash', '2026-11-02 00:00:00', 'seed'),
    ('0193f0a0-0000-7000-8000-000000000003', 'pt.lan', 'Trần Thị Lan', 'lan@gymbase.local', '0900000003', 'User', 1, 'mock-not-a-real-hash', '2026-11-03 00:00:00', 'seed'),
    ('0193f0a0-0000-7000-8000-000000000004', 'hv.huy', 'Ngô Ngọc Huy', 'huy@gymbase.local', '0900000004', 'User', 1, 'mock-not-a-real-hash', '2026-11-04 00:00:00', 'seed'),
    ('0193f0a0-0000-7000-8000-000000000005', 'hv.phuong', 'Lê Thu Phương', NULL, '0900000005', 'User', 0, 'mock-not-a-real-hash', '2026-11-05 00:00:00', 'seed');
