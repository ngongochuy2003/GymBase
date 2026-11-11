# 08. Thiết bị và thông báo

`thiet_bi` giữ push token theo từng điện thoại. `thong_bao` vừa là hộp thông báo vừa là hàng đợi nhắc giờ, nên không
cần bảng lịch nhắc riêng.

```mermaid
erDiagram
    tai_khoan ||--o{ thiet_bi : "dùng"
    tai_khoan ||--o{ thong_bao : "nhận"
    thiet_bi |o--o{ phien_dang_nhap : "đăng nhập từ"

    thiet_bi {
        guid Id PK
        guid tai_khoan_id FK
        string ma_thiet_bi "định danh cài đặt do app cấp"
        string nen_tang "android, ios"
        string push_token UK "varchar(512), NULL được, FCM token mới nhất"
        bool dang_hoat_dong
        datetime ngay_hoat_dong_cuoi
        datetime ngay_tao
        string nguoi_tao
        datetime ngay_chinh_sua "lần đổi push token gần nhất"
        string nguoi_chinh_sua
    }
    thong_bao {
        guid Id PK
        guid tai_khoan_id FK
        string loai_thong_bao "nhac_an, nhac_tap, tin_nhan, danh_gia, ket_noi_pt"
        string tieu_de
        text noi_dung
        string loai_doi_tuong "nguồn phát sinh"
        guid doi_tuong_id "đa hình, không có FK"
        datetime thoi_diem_du_kien
        datetime thoi_diem_da_gui
        string trang_thai_gui
        datetime ngay_da_doc
        string ma_chong_trung "UNIQUE cùng tai_khoan_id"
        datetime ngay_tao
    }
    phien_dang_nhap {
        guid Id PK
        guid thiet_bi_id FK "NULL"
    }
    tai_khoan {
        guid Id PK
    }
```

## Trạng thái gửi

```mermaid
stateDiagram-v2
    [*] --> cho_gui : worker tạo theo lịch
    cho_gui --> dang_gui
    dang_gui --> da_gui : đã đẩy qua FCM
    dang_gui --> loi
    cho_gui --> huy : kế hoạch bị sửa hoặc tạm dừng
    da_gui --> [*]
    loi --> [*]
    huy --> [*]
```

`da_gui` chỉ cho biết hệ thống đã gửi yêu cầu tới dịch vụ push, không chứng minh thiết bị đã nhận hay người dùng đã đọc.
Việc người dùng đã đọc nằm ở `ngay_da_doc`.

## Luồng nhắc giờ

```mermaid
flowchart LR
    A[muc_ke_hoach<br/>giờ, thứ, múi giờ, nhắc trước] --> B[Worker<br/>sinh trong cửa sổ 24 giờ tới]
    B --> C[thong_bao<br/>ma_chong_trung chống trùng]
    C --> D[Đẩy FCM]
    D --> E[thiet_bi<br/>chỉ dang_hoat_dong]
    A -. sửa lịch .-> F[Hủy hoặc lập lại<br/>thong_bao chưa gửi]
    F --> C
```

## Ghi chú

- `UNIQUE(tai_khoan_id, ma_thiet_bi)`; khi đăng xuất thì tắt `dang_hoat_dong` của thiết bị đó.
- `UNIQUE(tai_khoan_id, ma_chong_trung)` giúp worker và retry không tạo thông báo trùng.
- Index đề xuất: `thong_bao(trang_thai_gui, thoi_diem_du_kien)`, `thong_bao(tai_khoan_id, ngay_tao DESC)`,
  `thiet_bi(tai_khoan_id, dang_hoat_dong)`.
- `phien_dang_nhap.thiet_bi_id` (đề xuất) nối phiên với thiết bị. Đăng xuất hoặc thu hồi phiên thì tắt `dang_hoat_dong` của
  thiết bị đó. Xem [01-tai-khoan-ho-so.md](01-tai-khoan-ho-so.md).
- `push_token` **đổi từ text sang `varchar(512)` kèm `UNIQUE`** (đề xuất so với tài liệu thiết kế). MySQL không đặt unique
  được trên text, và nếu không unique thì khi A đăng xuất, B đăng nhập cùng máy, cùng một token nằm ở hai tài khoản,
  thông báo của A có thể hiện trên máy B. Unique cho phép nhiều NULL.
- Khi app đăng ký token đã có ở dòng `thiet_bi` của tài khoản khác, service phải xóa token khỏi dòng cũ (đặt NULL và
  `dang_hoat_dong = false`) trước khi gán cho dòng mới, trong cùng transaction.
