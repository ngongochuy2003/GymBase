---
status: accepted
---

# Đăng nhập bằng email, email unique cho cả mật khẩu và Google, không tự liên kết

Ngày 13/11/2026. Thiết kế đăng ký (form họ tên, email, số điện thoại, mật khẩu) không có tên đăng nhập, nên đăng nhập
chỉ bằng email, chuẩn hóa chữ thường. Email là unique trên mọi tài khoản, kể cả tài khoản Google, thay cho quyết định
trước trong `xac-thuc.md` (email Google không ràng buộc, trùng email thì tạo tài khoản riêng).

Hệ quả: người đăng nhập Google mà email đã thuộc một tài khoản mật khẩu bị từ chối (`AUTH_EMAIL_USED_PASSWORD`), và ngược lại
đăng ký mật khẩu bằng email đã có tài khoản Google báo trùng. Không tự liên kết theo email, vì Google không đảm bảo email còn thuộc về
chủ cũ nên tự liên kết là đường chiếm tài khoản. Người dùng liên kết Google chủ động trong Hồ sơ (làm sau). V1 không xác minh email
nên email chỉ là định danh đăng nhập, chưa chứng minh quyền sở hữu.

Cột `ten_dang_nhap` giữ lại (còn ghi `nguoi_tao`, `nguoi_chinh_sua`) nhưng không dùng để đăng nhập.

## Considered Options

- Tự liên kết Google vào tài khoản mật khẩu cùng email: tiện nhưng cho phép chiếm tài khoản.
- Bắt nhập mật khẩu cũ rồi liên kết: an toàn hơn, thêm một màn hình; để sau.
- Email không unique với Google (cách cũ): hai tài khoản cùng email, dễ nhầm và khó hỗ trợ.
