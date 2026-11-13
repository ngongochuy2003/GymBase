# Quy ước comment TODO

Code tạm (mock, bỏ qua một kiểm tra, chờ phần base khác) luôn kèm một comment TODO ngay tại chỗ đó,
viết cùng lúc với code. Extension Todo Tree (cấu hình ở `.vscode/settings.json`) gom các TODO theo chủ đề.

```csharp
// TODO(DB): đổi lớp cha sang BaseRepository<tai_khoan>; dữ liệu mẫu chuyển sang script seed. Mục 3, 13.
// TODO(ERROR): lỗi nghiệp vụ ném exception và trả ProblemDetails qua global exception handler,
//   không try/catch trong từng action. Mục 7.
```

- Comment thường `//`, đặt ngay trên khối `///` / attribute của lớp hoặc thành viên liên quan.
  Dòng tiếp theo bắt đầu bằng `//   `. TypeScript dùng cùng dạng.
- `(CHỦ_ĐỀ)` là sub-tag, chọn trong bảng dưới. Cần chủ đề mới thì thêm một dòng vào bảng.
- Cuối comment ghi số mục trong `docs/api/viec-con-lai-base.md` khi việc đó có mục ở đó.
- `FIXME` cho lỗi đã biết nhưng chưa sửa; `TODO` cho phần chưa làm.
- Khi làm xong một chủ đề, tìm `TODO(CHỦ_ĐỀ)` trong toàn repo, xử lý và xóa từng comment.

| Chủ đề | Việc chờ |
| --- | --- |
| `DB` | Nối MySQL bằng EF Core: `DbContext`, `BaseRepository` dùng EF Core, migrations, bỏ DL mock `InMemory*`, seed, health check DB |
| `ERROR` | Exception nghiệp vụ + ProblemDetails, chặn dữ liệu trùng |
| `PAGING` | Sort, filter theo danh sách cột cho phép |
| `CURRENT-USER` | `ICurrentUser` để ghi `nguoi_tao`, `nguoi_chinh_sua` |
| `HOST` | `AddAppApi()` / `UseAppApi()` gom cấu hình của `GymBase.API`: CORS, LowercaseUrls… |
| `AUTH` | Xác thực, phân quyền, mật khẩu |
| `REFERRAL` | Mã giới thiệu và điểm thưởng khi đăng ký (V1 chỉ nhận ô nhập, chưa xử lý) |
| `VERIFY` | Xác minh email, số điện thoại (OTP hoặc link) và quên mật khẩu qua email |
| `PROFILE` | Màn thiết lập hồ sơ và chọn nhóm người dùng (hiện là màn giữ chỗ) |
| `ASSET` | Logo, ảnh hero, carousel của màn chào mừng (hiện là khối màu giữ chỗ) |
| `LINK-GOOGLE` | Liên kết Google vào tài khoản mật khẩu có sẵn từ tab Cá nhân (backend và mobile) |
| `IOS` | Đăng nhập Google trên iOS (plugin `iosUrlScheme`, client ID iOS) và Sign in with Apple |
| `LEGAL` | Điều khoản sử dụng và Chính sách bảo mật thật (hiện là URL giữ chỗ) |
