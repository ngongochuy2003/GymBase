-- Dữ liệu mẫu mạng xã hội cho môi trường dev: hồ sơ người dùng, ảnh đại diện, bài viết, tệp đính kèm và tương tác.
-- Chạy sau database/seeds/dev_tai_khoan.sql; chạy lại nhiều lần không sinh bản ghi trùng.

INSERT IGNORE INTO tep_tin
    (Id, nguoi_tai_len_id, loai_doi_tuong, doi_tuong_id, muc_dich, s3_bucket, s3_key, ten_tep_goc, loai_mime, kich_thuoc_byte, che_do_truy_cap, trang_thai, thu_tu_hien_thi, ngay_tao, ngay_xac_nhan)
VALUES
    ('0193f0b0-0000-7000-8000-000000000002', '0193f0a0-0000-7000-8000-000000000002', 'ho_so_nguoi_dung', '0193f0a0-0000-7000-8000-000000000002', 'anh_dai_dien', 'external', 'https://picsum.photos/seed/gymbase-minh-khoi/160/160#tep=avatar2', 'minh-khoi.jpg', 'image/jpeg', 24500, 'cong_khai', 'da_lien_ket', 1, '2026-11-10 01:00:00', '2026-11-10 01:00:00'),
    ('0193f0b0-0000-7000-8000-000000000003', '0193f0a0-0000-7000-8000-000000000003', 'ho_so_nguoi_dung', '0193f0a0-0000-7000-8000-000000000003', 'anh_dai_dien', 'external', 'https://picsum.photos/seed/gymbase-lan-anh/160/160#tep=avatar3', 'lan-anh.jpg', 'image/jpeg', 23800, 'cong_khai', 'da_lien_ket', 1, '2026-11-10 01:05:00', '2026-11-10 01:05:00'),
    ('0193f0b0-0000-7000-8000-000000000004', '0193f0a0-0000-7000-8000-000000000004', 'ho_so_nguoi_dung', '0193f0a0-0000-7000-8000-000000000004', 'anh_dai_dien', 'external', 'https://picsum.photos/seed/gymbase-ngoc-huy/160/160#tep=avatar4', 'ngoc-huy.jpg', 'image/jpeg', 22100, 'cong_khai', 'da_lien_ket', 1, '2026-11-10 01:10:00', '2026-11-10 01:10:00');

INSERT IGNORE INTO ho_so_nguoi_dung
    (tai_khoan_id, loai_nguoi_dung, ten_hien_thi, gioi_thieu, anh_dai_dien_tep_tin_id, trang_thai_xac_minh, ngay_tao, nguoi_tao)
VALUES
    ('0193f0a0-0000-7000-8000-000000000002', 'pt', 'Minh Khôi', 'HLV cá nhân chuyên Hypertrophy & Strength', '0193f0b0-0000-7000-8000-000000000002', 'da_xac_minh', '2026-11-10 01:00:00', 'seed'),
    ('0193f0a0-0000-7000-8000-000000000003', 'pt', 'Lan Anh', 'HLV Dinh dưỡng & Body Recomposition', '0193f0b0-0000-7000-8000-000000000003', 'da_xac_minh', '2026-11-10 01:05:00', 'seed'),
    ('0193f0a0-0000-7000-8000-000000000004', 'hoc_vien', 'Ngọc Huy', 'Mục tiêu tăng cơ giảm mỡ', '0193f0b0-0000-7000-8000-000000000004', 'chua_xac_minh', '2026-11-10 01:10:00', 'seed');

INSERT IGNORE INTO bai_viet
    (Id, tac_gia_id, noi_dung, loai_bai_viet, che_do_hien_thi, trang_thai, ngay_dang, ngay_tao, nguoi_tao)
VALUES
    ('0193f0c0-0000-7000-8000-000000000001', '0193f0a0-0000-7000-8000-000000000002', 'Buổi Push hôm nay.\nTập trung ngực – vai – tay sau, cảm giác rất ổn!', 'hon_hop', 'cong_khai', 'da_dang', '2026-11-11 10:00:00', '2026-11-11 10:00:00', 'seed'),
    ('0193f0c0-0000-7000-8000-000000000002', '0193f0a0-0000-7000-8000-000000000003', 'Meal prep đơn giản cho 1 ngày siết mỡ.', 'hon_hop', 'cong_khai', 'da_dang', '2026-11-11 06:00:00', '2026-11-11 06:00:00', 'seed'),
    ('0193f0c0-0000-7000-8000-000000000003', '0193f0a0-0000-7000-8000-000000000004', 'Tuần thứ 4 theo lộ trình Upper/Lower cùng PT Minh Khôi. Squat đã chạm mốc 100kg! #tapluyen #tiendo', 'hon_hop', 'cong_khai', 'da_dang', '2026-11-11 04:00:00', '2026-11-11 04:00:00', 'seed'),
    ('0193f0c0-0000-7000-8000-000000000004', '0193f0a0-0000-7000-8000-000000000003', 'Mẹo nhỏ khi tính macro: luôn ưu tiên đủ 1.6g–2.2g protein/kg cân nặng trước khi chia carb và fat. #kienthuc #meohay', 'hon_hop', 'cong_khai', 'da_dang', '2026-11-11 02:00:00', '2026-11-11 02:00:00', 'seed');

INSERT IGNORE INTO tep_tin
    (Id, nguoi_tai_len_id, loai_doi_tuong, doi_tuong_id, muc_dich, s3_bucket, s3_key, ten_tep_goc, loai_mime, kich_thuoc_byte, che_do_truy_cap, trang_thai, thu_tu_hien_thi, ngay_tao, ngay_xac_nhan)
VALUES
    ('0193f0b0-0000-7000-8000-000000000011', '0193f0a0-0000-7000-8000-000000000002', 'bai_viet', '0193f0c0-0000-7000-8000-000000000001', 'workout', 'external', 'https://picsum.photos/seed/gymbase-push/800/500#tep=post1', 'push-day.jpg', 'image/jpeg', 142000, 'cong_khai', 'da_lien_ket', 1, '2026-11-11 10:00:00', '2026-11-11 10:00:00'),
    ('0193f0b0-0000-7000-8000-000000000012', '0193f0a0-0000-7000-8000-000000000003', 'bai_viet', '0193f0c0-0000-7000-8000-000000000002', 'meal-prep', 'external', 'https://picsum.photos/seed/gymbase-mealprep/800/500#tep=post2', 'meal-prep.jpg', 'image/jpeg', 135000, 'cong_khai', 'da_lien_ket', 1, '2026-11-11 06:00:00', '2026-11-11 06:00:00'),
    ('0193f0b0-0000-7000-8000-000000000013', '0193f0a0-0000-7000-8000-000000000004', 'bai_viet', '0193f0c0-0000-7000-8000-000000000003', 'workout', 'external', 'https://picsum.photos/seed/gymbase-squat/800/500#tep=post3', 'squat-pr.jpg', 'image/jpeg', 128000, 'cong_khai', 'da_lien_ket', 1, '2026-11-11 04:00:00', '2026-11-11 04:00:00'),
    ('0193f0b0-0000-7000-8000-000000000014', '0193f0a0-0000-7000-8000-000000000003', 'bai_viet', '0193f0c0-0000-7000-8000-000000000004', 'knowledge', 'external', 'https://picsum.photos/seed/gymbase-macro/800/500#tep=post4', 'macro-tips.jpg', 'image/jpeg', 119000, 'cong_khai', 'da_lien_ket', 1, '2026-11-11 02:00:00', '2026-11-11 02:00:00');

INSERT IGNORE INTO tuong_tac_bai_viet
    (Id, bai_viet_id, tai_khoan_id, loai_tuong_tac, noi_dung, tuong_tac_cha_id, ngay_tao, ngay_xoa)
VALUES
    ('0193f0d0-0000-7000-8000-000000000001', '0193f0c0-0000-7000-8000-000000000001', '0193f0a0-0000-7000-8000-000000000003', 'thich', NULL, NULL, '2026-11-11 10:15:00', NULL),
    ('0193f0d0-0000-7000-8000-000000000002', '0193f0c0-0000-7000-8000-000000000001', '0193f0a0-0000-7000-8000-000000000004', 'thich', NULL, NULL, '2026-11-11 10:20:00', NULL),
    ('0193f0d0-0000-7000-8000-000000000003', '0193f0c0-0000-7000-8000-000000000001', '0193f0a0-0000-7000-8000-000000000004', 'binh_luan', 'Form chuẩn quá coach ơi, chiều nay em cũng tới lịch Push!', NULL, '2026-11-11 10:25:00', NULL),
    ('0193f0d0-0000-7000-8000-000000000004', '0193f0c0-0000-7000-8000-000000000002', '0193f0a0-0000-7000-8000-000000000002', 'thich', NULL, NULL, '2026-11-11 06:30:00', NULL),
    ('0193f0d0-0000-7000-8000-000000000005', '0193f0c0-0000-7000-8000-000000000002', '0193f0a0-0000-7000-8000-000000000004', 'binh_luan', 'Nhìn hộp ức gà bông cải ngon mắt thật sự!', NULL, '2026-11-11 06:45:00', NULL),
    ('0193f0d0-0000-7000-8000-000000000006', '0193f0c0-0000-7000-8000-000000000003', '0193f0a0-0000-7000-8000-000000000002', 'thich', NULL, NULL, '2026-11-11 04:15:00', NULL);
