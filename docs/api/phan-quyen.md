# Phân quyền

> **Trạng thái:** phần nền đã làm: policy theo hồ sơ, hàm kiểm tra quyền trên dữ liệu học viên, endpoint `GET /auth/quyen-han`.
> Chưa có endpoint nghiệp vụ nào dùng chúng (chưa có CRUD cho bảng Biz). Đăng nhập và token: [xac-thuc.md](xac-thuc.md).

## 1. Ba lớp, mỗi lớp trả lời một câu hỏi

| Lớp | Câu hỏi | Nguồn dữ liệu | Cách dùng |
| --- | --- | --- | --- |
| 1. Vai trò hệ thống | Có phải quản trị không? | `vai_tro` (`Admin` hoặc `User`) trong token | `[Authorize(Policy = AppPolicies.RequireAdmin)]` |
| 2. Năng lực theo hồ sơ | Được dùng chức năng này không? | `ho_so_nguoi_dung` và `tai_khoan.dang_hoat_dong`, **đọc DB mỗi request** | `[Authorize(Policy = AppPolicies.RequirePtProfile)]`... |
| 3. Quyền trên bản ghi | Có quyền với đúng bản ghi này không? | `ket_noi_pt` | Gọi `TruyCapService` trong hàm BL |

Không lớp nào thay được lớp khác. Policy chỉ cho biết "là PT đã xác minh", không cho biết PT đó có phải PT của học viên này.
Ẩn nút trên app không phải bảo mật: backend luôn kiểm tra lại.

## 2. Ai làm được gì

| Trạng thái của tài khoản | Có hồ sơ | Là PT | Quản lý học viên |
| --- | --- | --- | --- |
| Chưa chọn nhóm (chưa có `ho_so_nguoi_dung`) | Không | Không | Không |
| `hoc_vien` | Có | Không | Không, kể cả khi `trang_thai_xac_minh` đã là `da_xac_minh` |
| `pt` hoặc `ca_hai`, chưa `da_xac_minh` | Có | Có | Không |
| `pt` hoặc `ca_hai`, `da_xac_minh`, tài khoản đang hoạt động | Có | Có | **Có**, trong phạm vi kết nối |
| `pt` hoặc `ca_hai`, `da_xac_minh`, tài khoản bị khóa | Có | Có | Không |
| `Admin` | Theo hồ sơ như người khác | Theo hồ sơ | Không tự có |

`loai_nguoi_dung` chỉ cho biết nhóm, không tự cấp quyền. Khóa PT bằng `tai_khoan.dang_hoat_dong = false`, không thêm trạng thái mới
vào `trang_thai_xac_minh`. `Admin` không tự xem được ảnh bữa ăn, cân nặng hay tin nhắn của học viên; truy cập dữ liệu riêng cần
quyền riêng có ghi lịch sử (chưa làm).

## 3. Policy (lớp 2)

Tên ở `GymBase.Base.Auth.AppPolicies`. Thỏa điều kiện thì qua, không thì 403 (chưa đăng nhập là 401).

| Policy | Điều kiện |
| --- | --- |
| `RequireAdmin` | `vai_tro` trong token là `Admin` |
| `RequireProfile` | Đã chọn nhóm |
| `RequirePtProfile` | Có hồ sơ PT (`pt` hoặc `ca_hai`), kể cả chưa xác minh |
| `RequireVerifiedPt` | PT `da_xac_minh` và tài khoản không bị khóa |

Ba policy sau đọc DB mỗi request nên đổi hồ sơ hoặc khóa tài khoản có hiệu lực ở request kế tiếp, không phải đợi token hết hạn
(15 phút). `vai_tro` thì vẫn lấy từ token, nên đổi `vai_tro` của một người chỉ có hiệu lực ở lần refresh kế tiếp. Mỗi request dùng
policy hồ sơ tốn một đến hai truy vấn nhẹ theo khóa chính.

## 4. Quyền trên bản ghi (lớp 3)

`TruyCapService` (`GymBase.BLBiz`). Mọi hàm nhận id người gọi và id học viên từ nơi đáng tin (token, bản ghi trong DB), **không
bao giờ lấy từ body**, vì client sửa được. Không đủ quyền thì ném `ForbiddenException` (403) kèm mã riêng.

| Hàm | Dùng khi | Điều kiện để qua |
| --- | --- | --- |
| `BLKiemTraPtDaXacMinhAsync(ptId)` | Việc chỉ PT đã xác minh làm | `la_pt_da_xac_minh` |
| `BLKiemTraQuanLyHocVienAsync(ptId, hocVienId, phamVi)` | **Ghi** hay sửa kế hoạch, duyệt nhật ký của học viên | PT đã xác minh, có kết nối `dang_hoat_dong` với học viên, kết nối phụ trách `phamVi` (`tap_luyen` hoặc `dinh_duong`) |
| `BLKiemTraDocDuLieuHocVienAsync(nguoiGoiId, hocVienId, ketNoiPtId)` | **Đọc** dữ liệu của học viên | Chủ dữ liệu luôn qua. PT chỉ qua khi `ketNoiPtId` (cột `ket_noi_pt_id` của bản ghi) là kết nối của chính PT với học viên đó |

Quy tắc đọc:

- Dữ liệu không gắn kết nối (`ket_noi_pt_id` null) là dữ liệu riêng, chỉ chủ đọc được.
- Kết nối `dang_hoat_dong`: PT còn phải là PT đã xác minh.
- Kết nối `da_ket_thuc`: PT chỉ **đọc lại** dữ liệu gắn với kết nối đó (lịch sử), không ghi mới.
- Kết nối `cho_phan_hoi`, `tu_choi`, `da_huy`: không chia sẻ gì.
- `ketNoiPtId` của kết nối khác, của PT khác hay của học viên khác đều bị từ chối (chống IDOR: dùng một id hợp lệ để đọc dữ liệu
  của người khác).

Cần giấu việc bản ghi có tồn tại với người không có quyền thì endpoint đổi `ForbiddenException` thành `NotFoundException`.

Mã lỗi (đều 403, bảng đầy đủ ở [quy-uoc-response.md](quy-uoc-response.md) mục 4.2):

| `code` | Nghĩa |
| --- | --- |
| `PT_NOT_VERIFIED` | Chưa phải PT đã xác minh, hoặc tài khoản bị khóa |
| `CONNECTION_NOT_ACTIVE` | Không có kết nối đang hoạt động với học viên này |
| `SCOPE_NOT_ALLOWED` | Kết nối không phụ trách lĩnh vực này |
| `DATA_NOT_SHARED` | Dữ liệu chưa được chia sẻ cho người gọi |

## 5. Thêm một endpoint có quyền

1. Policy chung cho chức năng, đặt trên action:

   ```csharp
   [HttpPut("{hocVienId:guid}/ke-hoach")]
   [Authorize(Policy = AppPolicies.RequireVerifiedPt)]
   public Task<...> CapNhat(Guid hocVienId, ...) => ...;
   ```

2. Trong hàm BL, kiểm tra quyền trên đúng bản ghi **trước khi** đọc hay ghi dữ liệu nhạy cảm:

   ```csharp
   var ptId = currentUser.UserId ?? throw new UnauthorizedException();
   await truyCap.BLKiemTraQuanLyHocVienAsync(ptId, hocVienId, PhamViPhuTrach.dinh_duong, cancellationToken);
   ```

3. Viết test cho từng dòng của bảng ở mục 4: đúng PT, PT khác, sai lĩnh vực, kết nối đã kết thúc, PT chưa xác minh. Mẫu:
   `TruyCapServiceTests` (BL) và `PhanQuyenTests` (HTTP).

Nghiệp vụ nhiều bước: xem [huong-dan-them-api-nghiep-vu.md](huong-dan-them-api-nghiep-vu.md).

## 6. Endpoint cho client

`GET /api/v1/{web|mob}/auth/quyen-han` (cần đăng nhập) trả quyền theo hồ sơ ở thời điểm gọi, để app hiện hoặc ẩn chức năng:

```json
{
  "da_thiet_lap_ho_so": true,
  "loai_nguoi_dung": "ca_hai",
  "trang_thai_xac_minh": "da_xac_minh",
  "la_pt": true,
  "la_pt_da_xac_minh": true,
  "nang_luc": ["quan_ly_hoc_vien"]
}
```

`vai_tro` lấy ở `GET /auth/me`. App nên gọi lại endpoint này khi PT được duyệt, bị khóa hay kết nối kết thúc.

## 7. Đã chốt (12/11) và còn thiếu

Đã chốt:

- Không có bảng role hay permission động trong V1: Base đã có `Admin` và `User`.
- Chưa thiết lập hồ sơ nghĩa là chưa có dòng `ho_so_nguoi_dung`, không thêm giá trị `chua_thiet_lap`.
- Bốn bảng lộ trình và điểm giới thiệu không làm ở V1, nên quyền gắn với `ket_noi_pt`, không có "lượt tham gia lộ trình".
- PT chưa xác minh không có kết nối nên không tạo được kế hoạch nháp.

Chưa làm:

- API Admin duyệt hoặc từ chối PT (`RequireAdmin` đã có, chưa có endpoint).
- Quyền đọc file riêng tư trên S3 (`tep_tin`) theo đối tượng đích.
- Chặn tài khoản (`chan_tai_khoan`) trong chat, theo dõi, bình luận.
- Truy cập dữ liệu riêng của học viên bởi Admin với ghi lịch sử.
