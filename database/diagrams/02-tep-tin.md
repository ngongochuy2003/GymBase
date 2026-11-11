# 02. File (Amazon S3)

Một bảng `tep_tin` lưu metadata cho mọi file. Database không chứa binary, chỉ giữ khóa S3.

```mermaid
erDiagram
    tai_khoan ||--o{ tep_tin : "tải lên"
    tep_tin |o--o{ ho_so_nguoi_dung : "ảnh đại diện"

    tep_tin {
        guid Id PK
        guid nguoi_tai_len_id FK
        string loai_doi_tuong "bai_viet, tin_nhan, nhat_ky_hoat_dong, ho_so_nguoi_dung"
        guid doi_tuong_id "đa hình, không có FK"
        string muc_dich "NULL, phân biệt file cùng đối tượng, ví dụ anh_dai_dien, chung_chi_pt"
        string s3_bucket
        string s3_key UK "object key, không phải URL"
        string ten_tep_goc
        string loai_mime
        bigint kich_thuoc_byte
        string che_do_truy_cap "cong_khai, rieng_tu"
        string trang_thai
        int thu_tu_hien_thi
        datetime ngay_tao
        datetime ngay_xac_nhan
    }
    tai_khoan {
        guid Id PK
    }
    ho_so_nguoi_dung {
        guid tai_khoan_id PK
        guid anh_dai_dien_tep_tin_id FK
    }
```

## Vòng đời của file

```mermaid
stateDiagram-v2
    [*] --> cho_tai_len : xin quyền upload
    cho_tai_len --> da_tai_len : backend kiểm tra object trên S3
    cho_tai_len --> loi : upload hỏng hoặc quá hạn
    da_tai_len --> da_lien_ket : gắn vào bài viết, tin nhắn, nhật ký, avatar
    da_lien_ket --> da_xoa
    da_tai_len --> da_xoa : job dọn file mồ côi
    loi --> da_xoa
    da_xoa --> [*]
```

## Luồng upload

```mermaid
sequenceDiagram
    participant M as Mobile
    participant B as Backend
    participant S as Amazon S3
    participant D as Database
    M->>B: 1. Xin upload (loại file, kích thước, đích)
    B->>B: Kiểm tra đăng nhập, quyền, MIME, dung lượng
    B->>D: Tạo tep_tin (cho_tai_len) với s3_key duy nhất
    B-->>M: Presigned PUT hoặc POST ngắn hạn
    M->>S: 2. Upload trực tiếp
    M->>B: 3. Báo upload xong
    B->>S: HEAD object, đối chiếu kích thước và kiểu file
    B->>D: tep_tin = da_tai_len
    M->>B: 4. Tạo bài viết, tin nhắn, nhật ký hoặc avatar
    B->>D: tep_tin = da_lien_ket (loai_doi_tuong, doi_tuong_id)
    M->>B: 5. Đọc file riêng tư
    B->>B: Kiểm tra quyền trên đối tượng đích
    B-->>M: Presigned GET ngắn hạn
```

## Ghi chú

- `(loai_doi_tuong, doi_tuong_id)` không có khóa ngoại, service phải kiểm tra loại đích, chủ sở hữu và trạng thái.
- Index đề xuất: `UNIQUE(s3_key)`, `(loai_doi_tuong, doi_tuong_id)`, `(nguoi_tai_len_id, ngay_tao DESC)`.
- `s3_key` cần collation phân biệt hoa thường trên MySQL, vì S3 phân biệt còn collation mặc định thì không.
- Job nền dọn file `cho_tai_len` quá hạn và file mồ côi.
- `muc_dich` là **đề xuất thêm so với tài liệu thiết kế**. Hồ sơ có thể có nhiều loại file (ảnh đại diện, chứng chỉ PT) cùng
  gắn vào một `doi_tuong_id`, cặp `(loai_doi_tuong, doi_tuong_id)` không đủ để phân biệt. Danh sách giá trị chưa chốt.
- Ảnh đại diện có hai liên kết: FK `ho_so_nguoi_dung.anh_dai_dien_tep_tin_id` để đọc nhanh, và
  `loai_doi_tuong = ho_so_nguoi_dung`, `doi_tuong_id = tai_khoan_id`, `muc_dich = anh_dai_dien` để job dọn file nhận ra file
  đang được dùng. Service cập nhật cả hai cùng lúc.
- Có thể thêm sau (cột cho phép NULL, không cần làm ngay): `thoi_luong_giay`, `chieu_rong`, `chieu_cao` và file thu nhỏ cho
  video trên feed. Tài liệu coi xử lý video là cấu hình của Media module.
