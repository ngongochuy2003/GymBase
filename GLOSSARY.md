# GymBase

Ứng dụng kết nối huấn luyện viên (PT) với học viên: lộ trình tập luyện và dinh dưỡng, check-in, trao đổi.

## Language

### Người dùng

**Tài khoản**:
Danh tính đăng nhập của một người trong hệ thống. Mỗi người có một tài khoản và một hồ sơ chung.
_Avoid_: User, người dùng (khi nói về danh tính đăng nhập)

**Vai trò hệ thống**:
Quyền ở mức hệ thống của một tài khoản, chỉ là `Admin` hoặc `User`. Không cho biết người đó là PT hay học viên.
_Avoid_: Vai trò (nói trơn), role

**Nhóm người dùng**:
Việc một người dùng GymBase với tư cách Học viên, PT hay Cả hai, chọn sau khi đăng ký.
_Avoid_: Vai trò, vai trò sử dụng, vai trò PT

**Hồ sơ**:
Thông tin cá nhân và nhóm người dùng của một tài khoản; tài khoản mới chưa có hồ sơ cho đến khi chọn nhóm.
_Avoid_: Profile

**PT**:
Huấn luyện viên. Chỉ được nhận học viên sau khi được Admin xác minh.
_Avoid_: Trainer, huấn luyện viên (trong giao diện dùng "PT")

**Học viên**:
Người tập luyện, được PT đồng hành theo lộ trình.
_Avoid_: Trainee, khách hàng
