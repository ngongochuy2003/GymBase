# 07. Gói PT và buổi tập

Gói buổi giữa một PT với học viên, từng buổi tập, và sổ cái biến động số buổi. Số buổi còn lại **không lưu**,
luôn tính bằng `SUM(bien_dong_so_buoi.so_buoi_thay_doi)` của gói.

```mermaid
erDiagram
    ket_noi_pt ||--o{ goi_pt_hoc_vien : "sở hữu"
    tai_khoan ||--o{ goi_pt_hoc_vien : "người tạo"
    ket_noi_pt |o--o{ buoi_tap : "thuộc"
    goi_pt_hoc_vien |o--o{ buoi_tap : "dự kiến trừ"
    muc_ke_hoach |o--o{ buoi_tap : "lịch gốc"
    tai_khoan ||--o{ buoi_tap : "học viên"
    tai_khoan |o--o{ buoi_tap : "pt phụ trách"
    tai_khoan |o--o{ buoi_tap : "người xác nhận"
    goi_pt_hoc_vien ||--o{ bien_dong_so_buoi : "biến động"
    buoi_tap |o--o{ bien_dong_so_buoi : "phát sinh"
    tai_khoan ||--o{ bien_dong_so_buoi : "người tạo"

    goi_pt_hoc_vien {
        guid Id PK
        guid ket_noi_pt_id FK
        string ten_goi
        int so_buoi_mua
        decimal gia_thoa_thuan
        date ngay_bat_dau
        date ngay_het_han
        string trang_thai "cho_kich_hoat, dang_su_dung, tam_dung, hoan_tat, huy"
        guid nguoi_tao_id FK
        datetime ngay_tao
    }
    buoi_tap {
        guid Id PK
        guid ket_noi_pt_id FK
        guid goi_pt_hoc_vien_id FK
        guid muc_ke_hoach_id FK
        guid hoc_vien_id FK
        guid pt_id FK
        string loai_buoi "pt_truc_tiep, tu_tap"
        datetime bat_dau_du_kien
        datetime bat_dau_thuc_te
        datetime ket_thuc_thuc_te
        string trang_thai
        guid nguoi_xac_nhan_id FK
        datetime ngay_xac_nhan
        text noi_dung_khieu_nai
        string trang_thai_khieu_nai "cho_xu_ly, chap_nhan, tu_choi"
        datetime ngay_cap_nhat
    }
    bien_dong_so_buoi {
        guid Id PK
        guid goi_pt_hoc_vien_id FK
        guid buoi_tap_id FK
        string loai_bien_dong "cap_buoi, tru_buoi, hoan_buoi, dieu_chinh"
        int so_buoi_thay_doi "có dấu, khác 0"
        string ma_chong_trung UK "idempotency key"
        guid nguoi_tao_id FK
        text ly_do
        datetime ngay_tao
    }
    ket_noi_pt {
        guid Id PK
    }
    muc_ke_hoach {
        guid Id PK
    }
    tai_khoan {
        guid Id PK
    }
```

## Vòng đời buổi tập

```mermaid
stateDiagram-v2
    [*] --> da_len_lich
    da_len_lich --> da_hoan_thanh : báo đã tập
    da_len_lich --> da_huy
    da_hoan_thanh --> da_xac_nhan : PT xác nhận, trừ 1 buổi
    da_xac_nhan --> dang_khieu_nai : học viên khiếu nại
    dang_khieu_nai --> da_xac_nhan : khiếu nại bị bác
    dang_khieu_nai --> da_huy : chấp nhận, hoàn 1 buổi
```

Tài liệu mô tả kết quả xử lý khiếu nại ở `trang_thai_khieu_nai` (`chap_nhan`, `tu_choi`) nhưng không nói
`trang_thai` của buổi sau đó là gì. Hai mũi tên cuối ở trên là gợi ý, **cần chốt** trước khi viết nghiệp vụ.

## Xác nhận buổi và trừ gói

```mermaid
sequenceDiagram
    participant PT
    participant B as Backend
    participant D as Database
    PT->>B: Xác nhận buổi tập
    B->>D: BEGIN
    B->>D: Khóa dòng goi_pt_hoc_vien
    B->>B: Kiểm tra đúng PT, loại pt_truc_tiep, còn số dư
    B->>D: INSERT bien_dong_so_buoi (tru_buoi, -1, ma_chong_trung)
    B->>D: buoi_tap = da_xac_nhan
    B->>D: INSERT lich_su_chinh_sua
    B->>D: COMMIT
    B-->>PT: Thành công
```

Ví dụ số dư: mua 12 buổi (+12), PT xác nhận 3 buổi (−1 × 3) thì còn 9. Một khiếu nại được chấp nhận hoàn 1 buổi (+1) thì còn 10.

## Ghi chú

- `buoi_tap` loại `tu_tap` không bao giờ bị trừ gói.
- Sổ cái không được `UPDATE` hay `DELETE`, điều chỉnh bằng giao dịch bù. Trên MySQL chặn bằng trigger.
- Chặn hai lần trừ cho một buổi: cột sinh tự động `IF(loai_bien_dong = 'tru_buoi', buoi_tap_id, NULL)` kèm unique index.
- Chỉ một khiếu nại đang xử lý cho mỗi buổi (khiếu nại nằm ngay trong `buoi_tap`). Muốn nhiều vòng khiếu nại thì sau này tách bảng `phieu_khieu_nai`.
- Index đề xuất: `goi_pt_hoc_vien(ket_noi_pt_id, trang_thai)`, `buoi_tap(hoc_vien_id, bat_dau_du_kien DESC)`,
  `buoi_tap(pt_id, trang_thai)`, `buoi_tap(goi_pt_hoc_vien_id, trang_thai)`.
