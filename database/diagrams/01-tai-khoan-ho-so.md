# 01. Tài khoản và hồ sơ

Gồm 4 bảng nền (`tai_khoan`, `phien_dang_nhap`, `dang_nhap_ngoai`, `system_log`) và hồ sơ người dùng.
Hỗ trợ **hai cách đăng nhập**: tên đăng nhập kèm mật khẩu, và Google.

> **Trạng thái:** trong code, bảng tài khoản hiện vẫn tên `nguoi_dung` và chưa có `dang_nhap_ngoai`. Sơ đồ này vẽ theo
> thiết kế đã chốt, sẽ khớp code sau bước đổi tên và migration `InitialBase`.

```mermaid
erDiagram
    tai_khoan ||--o{ phien_dang_nhap : "đăng nhập trên"
    tai_khoan ||--o{ dang_nhap_ngoai : "liên kết Google"
    tai_khoan ||--o| ho_so_nguoi_dung : "có hồ sơ"
    tep_tin |o--o{ ho_so_nguoi_dung : "ảnh đại diện"

    tai_khoan {
        guid Id PK
        string ten_dang_nhap UK "NULL với tài khoản chỉ dùng Google, unique trong số tài khoản còn hiệu lực"
        string mat_khau_hash "NULL với tài khoản chỉ dùng Google"
        string ho_ten
        string email "lấy từ Google, chỉ để hiển thị, không dùng làm định danh"
        string so_dien_thoai
        string vai_tro "Admin hoặc User"
        bool dang_hoat_dong
        bool is_deleted
        datetime ngay_tao
        string nguoi_tao
        datetime ngay_chinh_sua
        string nguoi_chinh_sua
    }
    phien_dang_nhap {
        guid Id PK
        guid tai_khoan_id FK
        string token_hash UK "SHA-256 của refresh token"
        string token_hash_truoc
        datetime ngay_xoay_vong
        datetime ngay_het_han
        datetime ngay_het_han_toi_da
        datetime ngay_thu_hoi
        string ten_thiet_bi
        datetime ngay_tao
    }
    dang_nhap_ngoai {
        guid Id PK
        guid tai_khoan_id FK
        string nha_cung_cap "google, sau này có thể thêm apple"
        string ma_ngoai "sub của Google, không dùng email"
        datetime ngay_tao
    }
    ho_so_nguoi_dung {
        guid tai_khoan_id PK, FK "PK kiêm FK 1-1"
        string loai_nguoi_dung "hoc_vien, pt, ca_hai"
        string ten_hien_thi
        text gioi_thieu
        guid anh_dai_dien_tep_tin_id FK
        text muc_tieu_tap_luyen
        json chuyen_mon "mảng mã chuyên môn"
        smallint so_nam_kinh_nghiem
        string khu_vuc_hoat_dong
        decimal vi_do
        decimal kinh_do
        smallint ban_kinh_phuc_vu_km
        string trang_thai_xac_minh "chua_xac_minh, dang_xem_xet, da_xac_minh, tu_choi"
        datetime ngay_cap_nhat
    }
    tep_tin {
        guid Id PK
    }
    system_log {
        guid Id PK
        string nguoi_dung "ten_dang_nhap hoặc email, không phải FK"
        string phuong_thuc
        string duong_dan
        int ma_trang_thai
        bigint thoi_gian_ms
        string trace_id
        string ip
        datetime thoi_diem
    }
```

## Đăng ký bằng Google và chọn nhóm

Người lạ tự đăng ký được. Sau khi đăng nhập lần đầu, tài khoản chưa có `ho_so_nguoi_dung`, và app đưa người dùng
tới bước chọn nhóm.

```mermaid
sequenceDiagram
    participant A as App
    participant G as Google
    participant B as Backend
    participant D as Database
    A->>G: Đăng nhập, nhận ID token
    A->>B: POST /auth/google { id_token }
    B->>B: Xác minh chữ ký, aud, iss, exp
    B->>D: Tìm dang_nhap_ngoai theo (google, sub)
    alt Chưa có
        B->>D: Tạo tai_khoan (vai_tro = User, không mật khẩu)
        B->>D: Tạo dang_nhap_ngoai
    end
    B->>D: Tạo phien_dang_nhap
    B-->>A: Access token, refresh token, cờ chưa chọn nhóm
    A->>B: Chọn nhóm hoc_vien, pt hoặc ca_hai
    B->>D: Tạo ho_so_nguoi_dung
    Note over B,D: pt hoặc ca_hai thì trang_thai_xac_minh = chua_xac_minh
```

## Ai được làm PT

`vai_tro` chỉ là vai trò hệ thống. Quyền làm PT dựa vào hồ sơ, không dựa vào `vai_tro`.

```mermaid
flowchart TD
    A[Tài khoản đã chọn nhóm] --> B{loai_nguoi_dung}
    B -- hoc_vien --> C[Chỉ là học viên]
    B -- pt hoặc ca_hai --> D{trang_thai_xac_minh}
    D -- da_xac_minh --> E[Được nhận học viên và lập kế hoạch qua ket_noi_pt]
    D -- chua_xac_minh, dang_xem_xet, tu_choi --> F[Chưa được nhận học viên]
```

## Ghi chú

- `vai_tro` chỉ có `Admin` hoặc `User`. `ca_hai` (vừa là PT vừa là học viên) không còn vướng với cột `vai_tro`.
- `dang_nhap_ngoai` có hai unique: `(nha_cung_cap, ma_ngoai)` để một tài khoản Google không gắn vào hai tài khoản
  GymBase, và `(tai_khoan_id, nha_cung_cap)` để mỗi tài khoản chỉ liên kết một Google.
- `ten_dang_nhap` cho phép NULL. MySQL không có partial unique, nên dùng cột sinh tự động
  `IF(is_deleted = 0, ten_dang_nhap, NULL)` kèm unique index, cùng cách với cột `tai_khoan_hieu_luc` hiện tại.
- Cột `nguoi_tao` và `nguoi_chinh_sua` ghi `ten_dang_nhap` nếu có, không thì ghi `email`.
- `ho_so_nguoi_dung` có khóa chính là `tai_khoan_id`, nên không kế thừa `BaseEntity` và không dùng CRUD chung.
- Chưa chốt: người dùng có đổi nhóm sau khi đã chọn được không. Đề xuất: chọn một lần, đổi từ học viên sang PT
  được nhưng phải qua xác minh lại.
- Index đề xuất: `ho_so_nguoi_dung(loai_nguoi_dung, trang_thai_xac_minh)`.
- `phien_dang_nhap` và `system_log` được mô tả trong [logging.md](../../docs/api/logging.md) và
  [xac-thuc.md](../../docs/api/xac-thuc.md).
