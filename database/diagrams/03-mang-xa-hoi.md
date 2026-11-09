# 03. Mạng xã hội

Bài viết, thích và bình luận, theo dõi.

```mermaid
erDiagram
    tai_khoan ||--o{ bai_viet : "tác giả"
    bai_viet ||--o{ tuong_tac_bai_viet : "có"
    tai_khoan ||--o{ tuong_tac_bai_viet : "thực hiện"
    tuong_tac_bai_viet |o--o{ tuong_tac_bai_viet : "trả lời"
    tai_khoan ||--o{ theo_doi : "người theo dõi"
    tai_khoan ||--o{ theo_doi : "được theo dõi"

    bai_viet {
        guid Id PK
        guid tac_gia_id FK
        text noi_dung
        string loai_bai_viet "van_ban, hinh_anh, video, hon_hop"
        string che_do_hien_thi "cong_khai, nguoi_theo_doi, rieng_tu"
        string trang_thai "nhap, da_dang, an, go_bo"
        datetime ngay_dang
        datetime ngay_tao
        datetime ngay_cap_nhat
    }
    tuong_tac_bai_viet {
        guid Id PK
        guid bai_viet_id FK
        guid tai_khoan_id FK
        string loai_tuong_tac "thich, binh_luan"
        text noi_dung "bắt buộc với binh_luan"
        guid tuong_tac_cha_id FK "bình luận cha"
        datetime ngay_tao
        datetime ngay_xoa "xóa mềm"
    }
    theo_doi {
        guid nguoi_theo_doi_id PK, FK
        guid nguoi_duoc_theo_doi_id PK, FK
        datetime ngay_theo_doi
    }
    tai_khoan {
        guid Id PK
    }
```

## Ghi chú

- Ảnh và video của bài viết gắn qua `tep_tin` (`loai_doi_tuong = bai_viet`), không có bảng riêng.
- `tuong_tac_bai_viet`: mỗi người chỉ thích một bài một lần. MySQL không có partial unique, nên dùng cột sinh tự động
  `IF(loai_tuong_tac = 'thich' AND ngay_xoa IS NULL, CONCAT(bai_viet_id, '|', tai_khoan_id), NULL)` kèm unique index.
- `tuong_tac_cha_id` chỉ được trỏ tới bình luận cùng bài viết, không trỏ tới lượt thích. Service kiểm tra.
- `theo_doi` có khóa chính kép và không được tự theo dõi (`CHECK nguoi_theo_doi_id <> nguoi_duoc_theo_doi_id`).
- Index đề xuất: `bai_viet(trang_thai, ngay_dang DESC)`, `bai_viet(tac_gia_id, ngay_dang DESC)`,
  `tuong_tac_bai_viet(bai_viet_id, loai_tuong_tac, ngay_tao DESC)`, `theo_doi(nguoi_duoc_theo_doi_id, ngay_theo_doi DESC)`.
