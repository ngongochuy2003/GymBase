-- Danh mục phẳng (cap = 1, muc_luc = ma, muc_luc_ma = '<loại>/<mã>'). Khu vực hai cấp ở SeedKhuVuc.sql.
INSERT INTO `danh_muc` (`Id`, `ma_dinh_danh`, `ma`, `ten`, `cap`, `muc_luc`, `muc_luc_ma`, `tinh_trang`, `ngay_tao`) VALUES
('0193f0d1-0000-7000-8000-000000000001', 'muc_tieu_tap_luyen', 'tang_co', 'Tăng cơ', 1, 'tang_co', 'muc_tieu_tap_luyen/tang_co', 1, UTC_TIMESTAMP(6)),
('0193f0d1-0000-7000-8000-000000000002', 'muc_tieu_tap_luyen', 'giam_mo', 'Giảm mỡ', 1, 'giam_mo', 'muc_tieu_tap_luyen/giam_mo', 1, UTC_TIMESTAMP(6)),
('0193f0d1-0000-7000-8000-000000000003', 'muc_tieu_tap_luyen', 'duy_tri', 'Duy trì vóc dáng', 1, 'duy_tri', 'muc_tieu_tap_luyen/duy_tri', 1, UTC_TIMESTAMP(6)),
('0193f0d1-0000-7000-8000-000000000004', 'muc_tieu_tap_luyen', 'tang_suc_ben', 'Tăng sức bền', 1, 'tang_suc_ben', 'muc_tieu_tap_luyen/tang_suc_ben', 1, UTC_TIMESTAMP(6)),
('0193f0d1-0000-7000-8000-000000000005', 'muc_tieu_tap_luyen', 'linh_hoat', 'Linh hoạt, giảm đau mỏi', 1, 'linh_hoat', 'muc_tieu_tap_luyen/linh_hoat', 1, UTC_TIMESTAMP(6)),
('0193f0d3-0000-7000-8000-000000000001', 'chuyen_mon_pt', 'tang_co', 'Tăng cơ', 1, 'tang_co', 'chuyen_mon_pt/tang_co', 1, UTC_TIMESTAMP(6)),
('0193f0d3-0000-7000-8000-000000000002', 'chuyen_mon_pt', 'giam_mo', 'Giảm mỡ', 1, 'giam_mo', 'chuyen_mon_pt/giam_mo', 1, UTC_TIMESTAMP(6)),
('0193f0d3-0000-7000-8000-000000000003', 'chuyen_mon_pt', 'cai_thien_voc_dang', 'Cải thiện vóc dáng', 1, 'cai_thien_voc_dang', 'chuyen_mon_pt/cai_thien_voc_dang', 1, UTC_TIMESTAMP(6)),
('0193f0d3-0000-7000-8000-000000000004', 'chuyen_mon_pt', 'phuc_hoi_chuc_nang', 'Phục hồi chức năng', 1, 'phuc_hoi_chuc_nang', 'chuyen_mon_pt/phuc_hoi_chuc_nang', 1, UTC_TIMESTAMP(6)),
('0193f0d3-0000-7000-8000-000000000005', 'chuyen_mon_pt', 'dinh_duong', 'Dinh dưỡng', 1, 'dinh_duong', 'chuyen_mon_pt/dinh_duong', 1, UTC_TIMESTAMP(6)),
('0193f0d5-0000-7000-8000-000000000001', 'trinh_do_tap', 'moi_bat_dau', 'Mới bắt đầu', 1, 'moi_bat_dau', 'trinh_do_tap/moi_bat_dau', 1, UTC_TIMESTAMP(6)),
('0193f0d5-0000-7000-8000-000000000002', 'trinh_do_tap', 'co_nen_tang', 'Có nền tảng', 1, 'co_nen_tang', 'trinh_do_tap/co_nen_tang', 1, UTC_TIMESTAMP(6)),
('0193f0d5-0000-7000-8000-000000000003', 'trinh_do_tap', 'lau_nam', 'Tập lâu năm', 1, 'lau_nam', 'trinh_do_tap/lau_nam', 1, UTC_TIMESTAMP(6));
