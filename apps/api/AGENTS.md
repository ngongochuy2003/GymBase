# Backend (`apps/api`): quy tắc phải giữ

**Tên trong DB và code:** bảng, cột, entity và property của DTO dùng snake_case tiếng Việt không dấu (`tai_khoan`,
`ten_dang_nhap`); class entity trùng tên bảng. Khóa chính là `Id` (UUIDv7, `varchar(36)`), trừ bảng nối và
`ho_so_nguoi_dung` có khóa riêng. Giờ lưu UTC.

**Ngôn ngữ trong tên:** danh từ và việc nghiệp vụ (chủ phòng gym hoặc PT sẽ nói từ đó) dùng tiếng Việt không dấu
(`TaiKhoan`, `BuoiTap`, `DangNhap`); từ kỹ thuật dùng tiếng Anh (`Service`, `Repository`, `Get`, `Find`, `Async`).

**Tên lớp và file** của Service, Repository, DTO, Validator, Configuration: `<Danh từ nghiệp vụ><Vai trò>`, danh từ tiếng
Việt, vai trò tiếng Anh (`TaiKhoanService`, `ITaiKhoanRepository`, `TaiKhoanDtos.cs`). Service và Repository nằm trong file
cùng tên. Controller giữ tên theo URL.

**Tiền tố tầng:** thư mục chức năng và hàm public/protected của Service trong BL (`GymBase.BL<Nhóm>`) bắt đầu bằng `BL`
(`BLXacThuc/`, `BLLamMoiTokenAsync`); trong DL (`GymBase.DL<Nhóm>.Contract`, `.MySql`) bắt đầu bằng `DL`. Hàm private tên thường.

**Tên hàm:** `BL` hoặc `DL` + tên + `Async` khi trả `Task`. BL đặt theo việc nghiệp vụ bằng tiếng Việt, khớp tên DTO
(`BLDangNhapAsync`, `BLXacNhanBuoiTapAsync`); hàm nền dùng chung (`BLGetByIdAsync`) giữ tiếng Anh. DL đặt theo thao tác dữ
liệu bằng động từ tiếng Anh: `DLGetByIdAsync`, `DLInsertAsync`, `DLFindBy…Async` (trả null nếu không có), `DLExistsBy…Async`
(trả bool); tên bảng hoặc cột trong tên giữ tiếng Việt (`DLFindByTenDangNhapAsync`).

**Route mới:** `[Route("buoi-tap")]` tường minh (chữ thường, gạch nối, danh từ tiếng Việt), action là việc (`XacNhan` ứng với
`{id}/xac-nhan`). URL đã công bố (`/users`, `/auth`) không đổi.

**Phân tầng:** Controller gọi BL. BL chỉ chứa luật nghiệp vụ, DL chỉ truy vấn và ghi dữ liệu. Thư mục BL phụ thuộc nhau
một chiều. Entity ở `GymBase.Entities/<Nhóm>/`; đổi schema chỉ bằng EF migration ở `GymBase.Persistence`.

Kiểm tra: `dotnet test GymBase.slnx` tại `apps/api`; test kiến trúc giữ quy tắc đặt tên và ma trận tham chiếu project.

## Tài liệu: mở khi

| Khi bạn | Mở |
| --- | --- |
| Thêm hoặc đổi tên project, thư mục, file, hàm | [cau-truc-thu-muc.md](../../docs/api/cau-truc-thu-muc.md) |
| Thêm bảng hoặc CRUD mới | [huong-dan-them-crud.md](../../docs/api/huong-dan-them-crud.md), [crud-dung-chung.md](../../docs/api/crud-dung-chung.md) |
| Viết API không phải CRUD, có nhiều quy tắc hoặc nhiều bảng | [huong-dan-them-api-nghiep-vu.md](../../docs/api/huong-dan-them-api-nghiep-vu.md) |
| Đổi schema, thêm cột | [database/diagrams/README.md](../../database/diagrams/README.md) |
| Trả response hoặc lỗi, thêm mã lỗi | [quy-uoc-response.md](../../docs/api/quy-uoc-response.md) |
| Thêm sort hoặc filter | [sort-va-filter.md](../../docs/api/sort-va-filter.md) |
| Đụng xác thực, token, phân quyền | [xac-thuc.md](../../docs/api/xac-thuc.md) |
| Chạy API hoặc dựng MySQL local | [cach-chay-api.md](../../docs/api/cach-chay-api.md) |
