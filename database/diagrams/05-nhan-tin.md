# 05. Nhắn tin

Chat 1–1 ở V1, giữ chỗ cho chat nhóm. Chat không cần có kết nối PT, nên hai người nhắn được trước khi thuê.

```mermaid
erDiagram
    cuoc_tro_chuyen ||--o{ thanh_vien_cuoc_tro_chuyen : "gồm"
    tai_khoan ||--o{ thanh_vien_cuoc_tro_chuyen : "tham gia"
    cuoc_tro_chuyen ||--o{ tin_nhan : "chứa"
    tai_khoan ||--o{ tin_nhan : "gửi"
    tin_nhan |o--o{ tin_nhan : "trả lời"

    cuoc_tro_chuyen {
        guid Id PK
        string loai_cuoc_tro_chuyen "truc_tiep, nhom"
        string ma_hoi_thoai_doi UK "2 UUID đã sắp xếp, chống tạo trùng chat 1-1"
        string ten_nhom "chưa dùng ở V1"
        datetime ngay_tao
        datetime ngay_cap_nhat
    }
    thanh_vien_cuoc_tro_chuyen {
        guid cuoc_tro_chuyen_id PK, FK
        guid tai_khoan_id PK, FK
        datetime ngay_tham_gia
        datetime ngay_doc_den "mốc đã xem, dùng tính chưa đọc"
        datetime ngay_roi "dùng khi có chat nhóm"
    }
    tin_nhan {
        guid Id PK
        guid cuoc_tro_chuyen_id FK
        guid nguoi_gui_id FK
        guid ma_tin_nhan_client "idempotency key từ client"
        string loai_tin_nhan "van_ban, tep_tin, hon_hop"
        text noi_dung
        guid tra_loi_tin_nhan_id FK
        datetime ngay_gui
        datetime ngay_xoa "xóa mềm"
    }
    tai_khoan {
        guid Id PK
    }
```

## Ghi chú

- File đính kèm tin nhắn gắn qua `tep_tin` (`loai_doi_tuong = tin_nhan`).
- `UNIQUE(nguoi_gui_id, ma_tin_nhan_client)` chống tin nhắn trùng khi mạng di động gửi lại.
- `ma_hoi_thoai_doi` có thể NULL (chat nhóm). MySQL cho phép nhiều NULL trong unique index nên không cần cột phụ.
- Người gửi phải là thành viên của cuộc trò chuyện và không bị chặn (`chan_tai_khoan`), do service kiểm tra.
- `thanh_vien_cuoc_tro_chuyen` có khóa chính kép. Index đề xuất: `(tai_khoan_id, ngay_tham_gia DESC)`,
  `tin_nhan(cuoc_tro_chuyen_id, ngay_gui DESC)`.
