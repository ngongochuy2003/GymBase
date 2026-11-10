# 09. An toàn và quản trị

Báo cáo vi phạm và chặn tài khoản.

```mermaid
erDiagram
    tai_khoan ||--o{ bao_cao_vi_pham : "báo cáo"
    tai_khoan |o--o{ bao_cao_vi_pham : "xử lý"
    tai_khoan ||--o{ chan_tai_khoan : "chặn"
    tai_khoan ||--o{ chan_tai_khoan : "bị chặn"

    bao_cao_vi_pham {
        guid Id PK
        guid nguoi_bao_cao_id FK
        string loai_doi_tuong "bai_viet, tuong_tac_bai_viet, tai_khoan, tin_nhan"
        guid doi_tuong_id "đa hình, không có FK"
        string ly_do "spam, gia_mao, quay_roi, noi_dung_xau, khac"
        text mo_ta
        string trang_thai "moi, dang_xu_ly, da_giai_quyet, bo_qua"
        guid nguoi_xu_ly_id FK
        datetime ngay_tao
        datetime ngay_xu_ly
    }
    chan_tai_khoan {
        guid nguoi_chan_id PK, FK
        guid nguoi_bi_chan_id PK, FK
        datetime ngay_chan
    }
    tai_khoan {
        guid Id PK
    }
```

## Ghi chú

- `chan_tai_khoan` có khóa chính kép, không được tự chặn (`CHECK nguoi_chan_id <> nguoi_bi_chan_id`).
- Chặn ảnh hưởng nhắn tin, theo dõi và bình luận theo chính sách của ứng dụng, không tự xóa dữ liệu cũ.
- Quản trị viên xử lý báo cáo phải được kiểm tra quyền, và các quyết định nhạy cảm cần ghi lịch sử.
- Index đề xuất: `bao_cao_vi_pham(trang_thai, ngay_tao)`, `bao_cao_vi_pham(loai_doi_tuong, doi_tuong_id)`,
  `chan_tai_khoan(nguoi_bi_chan_id)`.
