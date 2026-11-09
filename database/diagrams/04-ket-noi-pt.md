# 04. Kết nối PT

`ket_noi_pt` gộp yêu cầu thuê PT và quan hệ huấn luyện. Mỗi lần hợp tác là một dòng, nên giữ được lịch sử.
Bảng này là gốc phân quyền: PT chỉ quản lý dữ liệu thuộc kết nối `dang_hoat_dong` của mình.

```mermaid
erDiagram
    tai_khoan ||--o{ ket_noi_pt : "pt"
    tai_khoan ||--o{ ket_noi_pt : "học viên"
    tai_khoan ||--o{ ket_noi_pt : "người khởi tạo"
    ket_noi_pt ||--o{ ke_hoach : "áp dụng"
    ket_noi_pt |o--o{ nhat_ky_hoat_dong : "phạm vi"
    ket_noi_pt |o--o{ chi_so_co_the : "chia sẻ"
    ket_noi_pt ||--o{ goi_pt_hoc_vien : "sở hữu"
    ket_noi_pt |o--o{ buoi_tap : "thuộc"

    ket_noi_pt {
        guid Id PK
        guid pt_id FK
        guid hoc_vien_id FK
        guid nguoi_khoi_tao_id FK
        json pham_vi_phu_trach "tap_luyen, dinh_duong"
        text loi_nhan
        string trang_thai
        datetime ngay_de_nghi
        datetime ngay_chap_nhan
        datetime ngay_ket_thuc
        text ly_do_ket_thuc
    }
    tai_khoan {
        guid Id PK
    }
    ke_hoach {
        guid Id PK
    }
    nhat_ky_hoat_dong {
        guid Id PK
    }
    chi_so_co_the {
        guid Id PK
    }
    goi_pt_hoc_vien {
        guid Id PK
    }
    buoi_tap {
        guid Id PK
    }
```

## Vòng đời

```mermaid
stateDiagram-v2
    [*] --> cho_phan_hoi : gửi đề nghị
    cho_phan_hoi --> dang_hoat_dong : chấp nhận
    cho_phan_hoi --> tu_choi
    cho_phan_hoi --> da_huy
    dang_hoat_dong --> da_ket_thuc
    tu_choi --> [*]
    da_huy --> [*]
    da_ket_thuc --> [*]
```

Chỉ kết nối `dang_hoat_dong` mới cho PT lập hoặc cập nhật kế hoạch mới. Sau khi kết thúc vẫn xem được lịch sử theo
chính sách sau, và cặp PT với học viên có thể nối lại bằng một dòng mới.

## Ghi chú

- `pt_id` khác `hoc_vien_id` (`CHECK`).
- Mỗi cặp PT với học viên chỉ có tối đa một kết nối đang `cho_phan_hoi` hoặc `dang_hoat_dong`. Trên MySQL dùng cột sinh tự động
  `IF(trang_thai IN ('cho_phan_hoi','dang_hoat_dong'), CONCAT(pt_id, '|', hoc_vien_id), NULL)` kèm unique index.
- Index đề xuất: `(pt_id, trang_thai)`, `(hoc_vien_id, trang_thai)`.
