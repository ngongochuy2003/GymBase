# 06. Huấn luyện và sức khỏe

Kế hoạch ăn hoặc tập, lịch lặp, nhật ký học viên nộp, lịch sử chỉnh sửa và chỉ số cơ thể.

```mermaid
erDiagram
    ket_noi_pt ||--o{ ke_hoach : "áp dụng"
    tai_khoan ||--o{ ke_hoach : "người tạo"
    ke_hoach ||--o{ muc_ke_hoach : "gồm"
    ket_noi_pt |o--o{ nhat_ky_hoat_dong : "phạm vi"
    muc_ke_hoach |o--o{ nhat_ky_hoat_dong : "căn cứ"
    buoi_tap |o--o{ nhat_ky_hoat_dong : "liên kết"
    tai_khoan ||--o{ nhat_ky_hoat_dong : "học viên"
    tai_khoan |o--o{ nhat_ky_hoat_dong : "người đánh giá"
    tai_khoan ||--o{ lich_su_chinh_sua : "người thực hiện"
    tai_khoan ||--o{ chi_so_co_the : "học viên"
    tai_khoan ||--o{ chi_so_co_the : "người nhập"
    ket_noi_pt |o--o{ chi_so_co_the : "chia sẻ"

    ke_hoach {
        guid Id PK
        guid ket_noi_pt_id FK
        string loai_ke_hoach "dinh_duong, tap_luyen"
        string ten_ke_hoach
        text mo_ta
        date ngay_bat_dau
        date ngay_ket_thuc
        int phien_ban
        string trang_thai "nhap, dang_ap_dung, tam_dung, ket_thuc"
        guid nguoi_tao_id FK
        datetime ngay_tao
        string nguoi_tao
        datetime ngay_chinh_sua
        string nguoi_chinh_sua
    }
    muc_ke_hoach {
        guid Id PK
        guid ke_hoach_id FK
        string loai_muc "bua_an, buoi_tap"
        string tieu_de
        time gio_du_kien
        json cac_thu_trong_tuan "mảng 1..7"
        string mui_gio "múi giờ IANA"
        json chi_tiet "khẩu phần hoặc bài tập"
        bool co_nhac
        int nhac_truoc_phut
        string trang_thai "dang_ap_dung, tam_dung, ngung"
        datetime ngay_tao
        string nguoi_tao
        datetime ngay_chinh_sua
        string nguoi_chinh_sua
    }
    nhat_ky_hoat_dong {
        guid Id PK
        guid ket_noi_pt_id FK
        guid muc_ke_hoach_id FK
        guid buoi_tap_id FK
        guid hoc_vien_id FK
        int phien_ban_ke_hoach "NULL, ke_hoach.phien_ban lúc nộp; NULL nếu hoạt động phát sinh"
        string loai_hoat_dong "an_uong, tap_luyen"
        date ngay_hoat_dong
        datetime thoi_gian_thuc_hien
        text noi_dung
        json chi_tiet "snapshot thực tế"
        string trang_thai
        guid nguoi_danh_gia_id FK
        text nhan_xet
        datetime ngay_danh_gia
        datetime ngay_tao
        string nguoi_tao
        datetime ngay_chinh_sua
        string nguoi_chinh_sua
    }
    lich_su_chinh_sua {
        guid Id PK
        string loai_doi_tuong "nhat_ky_hoat_dong, ke_hoach, muc_ke_hoach, buoi_tap, goi_pt_hoc_vien, bao_cao_vi_pham"
        guid doi_tuong_id "đa hình, không có FK"
        string hanh_dong "tao, cap_nhat, danh_gia, xac_nhan, xu_ly_khieu_nai, xu_ly_bao_cao, huy, ket_thuc"
        guid nguoi_thuc_hien_id FK
        json du_lieu_truoc
        json du_lieu_sau
        datetime ngay_thuc_hien
    }
    chi_so_co_the {
        guid Id PK
        guid hoc_vien_id FK
        guid ket_noi_pt_id FK
        datetime ngay_do
        decimal can_nang_kg
        decimal chieu_cao_cm
        decimal ty_le_mo_pct
        text ghi_chu
        guid nguoi_nhap_id FK
        datetime ngay_tao
    }
    ket_noi_pt {
        guid Id PK
    }
    buoi_tap {
        guid Id PK
    }
    tai_khoan {
        guid Id PK
    }
```

## Vòng đời nhật ký

```mermaid
stateDiagram-v2
    [*] --> nhap
    nhap --> cho_kiem_tra : học viên nộp
    cho_kiem_tra --> dat : PT đánh giá đạt
    cho_kiem_tra --> can_dieu_chinh : PT yêu cầu sửa
    can_dieu_chinh --> cho_kiem_tra : học viên sửa và gửi lại
    dat --> hoan_thanh
```

Tài liệu chưa mô tả khi nào nhật ký chuyển từ `dat` sang `hoan_thanh`, cần chốt trước khi viết nghiệp vụ.
Mỗi lần sửa hoặc đánh giá lại đều ghi vào `lich_su_chinh_sua`.

## Luồng báo cáo bữa ăn

```mermaid
sequenceDiagram
    participant W as Worker nhắc giờ
    participant HV as Học viên
    participant B as Backend
    participant PT as PT
    W->>B: Tạo thong_bao từ muc_ke_hoach (có ma_chong_trung)
    B-->>HV: Push qua FCM
    HV->>B: Nộp nhat_ky_hoat_dong (an_uong) kèm ảnh tep_tin
    B->>B: trang_thai = cho_kiem_tra
    PT->>B: Đánh giá dat hoặc can_dieu_chinh
    B->>B: Ghi lich_su_chinh_sua
    B-->>HV: Thông báo kết quả
```

## Ghi chú

- Ảnh bữa ăn gắn qua `tep_tin` (`loai_doi_tuong = nhat_ky_hoat_dong`), không có bảng `anh_bua_an`.
- `ke_hoach.phien_ban` tăng khi sửa nội dung đã áp dụng. Nhật ký lưu snapshot nên kế hoạch đổi không làm thay đổi nhật ký cũ.
- `nhat_ky_hoat_dong.phien_ban_ke_hoach` là **đề xuất thêm so với tài liệu thiết kế**. `muc_ke_hoach` được sửa tại chỗ, còn
  `chi_tiet` của nhật ký chỉ lưu phần thực tế, nên không có cách biết nhật ký cũ làm theo phiên bản kế hoạch nào. Cách
  khác: không thêm cột, mà quy định JSON `chi_tiet` có thêm khối kế hoạch lúc nộp. Cần chọn một cách.
- `lich_su_chinh_sua`: tài liệu để danh sách `loai_doi_tuong` và `hanh_dong` mở ("..."). Đề xuất danh sách đầy đủ ở sơ đồ,
  thêm `muc_ke_hoach` (sửa lịch làm tăng `phien_ban`), `goi_pt_hoc_vien` (đổi trạng thái gói) và `bao_cao_vi_pham` (tài liệu
  yêu cầu ghi lịch sử quyết định của quản trị). Khai báo ở một chỗ và dùng chung cho enum, `CHECK` và validator.
- `ke_hoach`, `muc_ke_hoach`, `nhat_ky_hoat_dong` kế thừa `BaseAuditableEntity`: bốn cột audit thay cho `ngay_cap_nhat` của tài
  liệu thiết kế, xem [README.md](README.md#quy-ước-chung-khi-làm-migration). `nguoi_tao` là chuỗi tên đăng nhập, còn
  `ke_hoach.nguoi_tao_id` vẫn giữ làm khóa ngoại nghiệp vụ.
- `lich_su_chinh_sua` và `chi_so_co_the` là bản ghi chỉ thêm, không sửa, nên chỉ kế thừa `BaseEntity` và giữ cột thời gian riêng.
- Có thể thêm sau: khóa chống gửi trùng cho `nhat_ky_hoat_dong` khi mạng di động gửi lại (như `tin_nhan.ma_tin_nhan_client`).
- `loai_muc` của mục phải khớp `loai_ke_hoach` của kế hoạch cha. Service kiểm tra, JSON `chi_tiet` kiểm tra theo schema của từng loại.
- `chi_so_co_the` không chia sẻ mặc định cho mọi PT của học viên, chỉ chia sẻ khi gắn `ket_noi_pt_id`.
- Index đề xuất: `ke_hoach(ket_noi_pt_id, loai_ke_hoach, trang_thai)`, `muc_ke_hoach(ke_hoach_id, trang_thai)`,
  `nhat_ky_hoat_dong(hoc_vien_id, ngay_hoat_dong DESC)`, `nhat_ky_hoat_dong(ket_noi_pt_id, trang_thai)`,
  `lich_su_chinh_sua(loai_doi_tuong, doi_tuong_id, ngay_thuc_hien DESC)`, `chi_so_co_the(hoc_vien_id, ngay_do DESC)`.
