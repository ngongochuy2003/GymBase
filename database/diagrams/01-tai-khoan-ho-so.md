# 01. Tài khoản và hồ sơ

Gồm 4 bảng nền (`tai_khoan`, `phien_dang_nhap`, `dang_nhap_ngoai`, `system_log`) và hồ sơ người dùng.
Hỗ trợ **hai cách đăng nhập**: tên đăng nhập kèm mật khẩu, và Google.

> **Trạng thái:** entity và cấu hình EF đã đổi sang `tai_khoan` (chưa commit), nhưng các migration hiện có vẫn tạo bảng
> `nguoi_dung` và chưa có `dang_nhap_ngoai`. Sơ đồ này vẽ theo thiết kế, sẽ khớp code sau khi tạo lại migration
> `InitialBase`. Cột `phien_dang_nhap.thiet_bi_id` thêm ở migration `AddBizTables`, cùng lúc tạo `thiet_bi`.

```mermaid
erDiagram
    tai_khoan ||--o{ phien_dang_nhap : "đăng nhập trên"
    tai_khoan ||--o{ dang_nhap_ngoai : "liên kết Google"
    tai_khoan ||--o| ho_so_nguoi_dung : "có hồ sơ"
    tep_tin |o--o{ ho_so_nguoi_dung : "ảnh đại diện"
    thiet_bi |o--o{ phien_dang_nhap : "đăng nhập từ"
    tai_khoan |o--o{ ho_so_nguoi_dung : "xác minh PT"

    tai_khoan {
        guid Id PK
        string ten_dang_nhap UK "NULL với tài khoản chỉ dùng Google, unique trong số tài khoản còn hiệu lực"
        string mat_khau_hash "NULL với tài khoản chỉ dùng Google"
        string ho_ten
        string email "lấy từ Google, chỉ để hiển thị, không dùng làm định danh"
        string so_dien_thoai
        string vai_tro "Admin hoặc User"
        bool dang_hoat_dong
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
        guid thiet_bi_id FK "NULL, thiết bị của phiên, dùng để tắt push khi đăng xuất"
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
        datetime ngay_xac_minh "NULL, lần Admin duyệt hoặc từ chối gần nhất"
        guid nguoi_xac_minh_id FK "NULL, Admin xử lý"
        text ly_do_tu_choi "NULL, chỉ có khi tu_choi"
        datetime ngay_tao
        string nguoi_tao
        datetime ngay_chinh_sua
        string nguoi_chinh_sua
    }
    tep_tin {
        guid Id PK
    }
    thiet_bi {
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
- `ten_dang_nhap` cho phép NULL (tài khoản chỉ dùng Google) và là unique index thường; MySQL cho phép nhiều NULL trong unique index.
- Cột `nguoi_tao` và `nguoi_chinh_sua` ghi `ten_dang_nhap` nếu có, không thì ghi `email`.
- `ho_so_nguoi_dung` có khóa chính là `tai_khoan_id`, nên không kế thừa `BaseEntity` và không dùng CRUD chung. Bốn cột
  audit (`ngay_tao`, `nguoi_tao`, `ngay_chinh_sua`, `nguoi_chinh_sua`) giống `BaseAuditableEntity` nhưng phải khai báo và ghi
  tay. Cột `ngay_cap_nhat` của tài liệu thiết kế được thay bằng `ngay_chinh_sua`.
- Chưa chốt: người dùng có đổi nhóm sau khi đã chọn được không. Đề xuất: chọn một lần, đổi từ học viên sang PT
  được nhưng phải qua xác minh lại.
- `ngay_xac_minh`, `nguoi_xac_minh_id`, `ly_do_tu_choi` là **đề xuất thêm so với tài liệu thiết kế**: không có chúng thì
  không biết ai duyệt PT, khi nào, vì sao từ chối. Cột `ly_do_tu_choi` chỉ có giá trị khi `tu_choi`.
- Chưa chốt: V1 xác minh PT bằng gì (chứng chỉ, ảnh giấy tờ). Nếu có file thì gắn qua `tep_tin` với
  `loai_doi_tuong = ho_so_nguoi_dung` và `muc_dich` khác ảnh đại diện, xem [02-tep-tin.md](02-tep-tin.md).
- `phien_dang_nhap.thiet_bi_id` (đề xuất, cho phép NULL, `ON DELETE SET NULL`): khi đăng xuất hoặc thu hồi phiên, tắt
  `dang_hoat_dong` của `thiet_bi` tương ứng. Phiên đăng nhập từ web không có thiết bị nên để NULL.
- **Đã chốt 11/11:** người lạ tự đăng ký bằng Google ngay V1. [xac-thuc.md](../../docs/api/xac-thuc.md) mục 8 đã sửa theo.
- Index đề xuất: `ho_so_nguoi_dung(loai_nguoi_dung, trang_thai_xac_minh)`.
- `phien_dang_nhap` và `system_log` được mô tả trong [logging.md](../../docs/api/logging.md) và
  [xac-thuc.md](../../docs/api/xac-thuc.md).
