# Thiết kế xác thực và phân quyền

> **Trạng thái:** đã triển khai ở backend (mục 12 trong [viec-con-lai-base.md](viec-con-lai-base.md)). App mobile chưa
> nối vào các endpoint này. Cách cấu hình và tạo tài khoản Admin đầu tiên: [cach-chay-api.md](cach-chay-api.md).

## 1. Quyết định

| Vấn đề | Chọn | Lý do |
| --- | --- | --- |
| Cách xác thực | JWT access token + refresh token do `BLAuth` tự cấp | Một host, chỉ phục vụ app của mình. Không dùng Duende vì cần license |
| Mặc định | Mọi endpoint phải đăng nhập (`FallbackPolicy`), endpoint công khai ghi rõ `[AllowAnonymous]` | Quên gắn attribute thì endpoint vẫn được bảo vệ |
| Ký token | HS256, khóa ngẫu nhiên ít nhất 256 bit trong user-secrets hoặc biến môi trường | Chỉ một host vừa ký vừa kiểm tra |
| Access token | Sống 15 phút | Không thu hồi được từng token, nên giữ ngắn |
| Refresh token | Xoay vòng mỗi lần dùng, hết hạn sau 30 ngày không dùng, tối đa 90 ngày | RFC 9700 yêu cầu với client như app mobile |
| Băm mật khẩu | `PasswordHasher` của ASP.NET, 600.000 vòng | Có sẵn, tự nâng cấp hash cũ. Mặc định chỉ 100.000 vòng nên phải đặt lại |
| Mật khẩu | Tối thiểu **8 ký tự** | Chốt 08/11, xem mục 5 |
| Tạo tài khoản | `Admin` tạo và đặt mật khẩu ban đầu, không tự đăng ký | Phạm vi V1 |
| Phân quyền | Theo `vai_tro`: `Admin`, `Trainer`, `Trainee` | Quy tắc theo dữ liệu (đúng PT, đúng học viên) kiểm tra trong service |

Các số 15 phút, 30 ngày, 90 ngày là lựa chọn của dự án, các tiêu chuẩn không quy định con số cụ thể.

## 2. Endpoint

Có ở cả `/api/v1/web/auth` và `/api/v1/mob/auth`, dùng chung `AuthService`.

| Request | Đăng nhập? | Kết quả |
| --- | --- | --- |
| `POST /login` | Không | 200: token kèm thông tin người dùng. 401 `AUTH_INVALID_CREDENTIALS` |
| `POST /refresh` | Không | 200: cặp token mới. 401 `UNAUTHORIZED` |
| `POST /logout` | Có | 204: thu hồi phiên hiện tại |
| `GET /me` | Có | 200: thông tin người dùng lấy từ DB |
| `POST /change-password` | Có | 204: thu hồi các phiên khác |

Admin đặt lại mật khẩu cho người khác qua `POST /api/v1/web/users/{id}/reset-password`, mọi phiên của người đó bị thu hồi.
Không có token hợp lệ thì mọi đường dẫn đều trả 401, kể cả đường dẫn không tồn tại, trừ `/health`, `/auth/login`, `/auth/refresh`.

Response của `login` và `refresh`:

```json
{
  "access_token": "eyJ…",
  "expires_in": 900,
  "refresh_token": "…",
  "user": { "id": "…", "tai_khoan": "pt.minh", "ho_ten": "Nguyễn Văn Minh", "vai_tro": "Trainer" }
}
```

Mobile lưu token bằng SecureStore (Keychain/Keystore), đã có `AuthTransport` và `token-storage`.

## 3. JWT chứa gì

JWT chỉ được **ký, không mã hóa**: ai có token đều đọc được nội dung. Vì vậy chỉ đưa vào những gì API cần
để nhận diện và phân quyền.

```json
{
  "alg": "HS256",
  "typ": "at+jwt"
}
{
  "iss": "gymbase-api",
  "aud": "gymbase-api",
  "sub": "0193f0a0-0000-7000-8000-000000000002",
  "sid": "01a1bb8a-8cf4-721e-9083-6d18f82be80c",
  "jti": "01a1bb8a-9d10-7c11-8a02-1f0c2b7e4a55",
  "preferred_username": "pt.minh",
  "role": "Trainer",
  "iat": 1794150000,
  "nbf": 1794150000,
  "exp": 1794150900
}
```

| Claim | Ý nghĩa | Dùng để |
| --- | --- | --- |
| `iss`, `aud` | Nơi phát hành, nơi được dùng | Từ chối token không phải của hệ thống này |
| `sub` | `Id` của người dùng trong bảng `nguoi_dung` | `ICurrentUser.UserId`. Giữ nguyên dù đăng nhập bằng cách nào |
| `sid` | `Id` của phiên trong `phien_dang_nhap` | Biết token thuộc phiên nào, để sau này thu hồi tức thì |
| `jti` | Mã riêng của token | Truy vết trong log |
| `preferred_username` | `tai_khoan` | `ICurrentUser.UserName`, ghi `nguoi_tao`, `nguoi_chinh_sua` |
| `role` | `vai_tro` | `[Authorize(Roles = "...")]` |
| `iat`, `nbf`, `exp` | Thời điểm cấp, bắt đầu có hiệu lực, hết hạn | Kiểm tra thời hạn |

**Không đưa vào token:** mật khẩu hay hash, email, số điện thoại, họ tên, `is_deleted`, hay dữ liệu nghiệp vụ.
Màn hình cần thông tin người dùng thì đọc từ `user` lúc đăng nhập hoặc gọi `GET /me`.

**Khi kiểm tra token phải:** chỉ chấp nhận đúng HS256; kiểm tra chữ ký, `iss`, `aud`, `exp`, `typ`; tắt
`MapInboundClaims` để giữ nguyên tên claim; đặt `RoleClaimType = "role"`. Không đọc claim từ token chưa xác minh.

**Giới hạn đã chấp nhận:** access token còn dùng được tối đa 15 phút sau khi đăng xuất, đổi quyền hoặc bị khóa.
Đổi `vai_tro` cũng chỉ có hiệu lực ở lần refresh kế tiếp. Nếu cần tức thì, kiểm tra `sid` trong DB ở mỗi request.

## 4. Refresh token và phiên

Refresh token là chuỗi ngẫu nhiên, không phải JWT. Bảng `phien_dang_nhap` thuộc nhóm Auth:

| Cột | Ý nghĩa |
| --- | --- |
| `Id` | Mã phiên, là `sid` trong JWT |
| `nguoi_dung_id` | Chủ phiên |
| `token_hash` | SHA-256 của refresh token hiện tại, **không lưu bản gốc** |
| `token_hash_truoc`, `ngay_xoay_vong` | Token vừa thay thế và lúc đổi, để nhận biết dùng lại |
| `ngay_het_han` | 30 ngày kể từ lần dùng gần nhất |
| `ngay_het_han_toi_da` | 90 ngày kể từ lúc đăng nhập |
| `ngay_thu_hoi` | Khác null là đã thu hồi |
| `ten_thiet_bi` | Để người dùng biết phiên nào đang mở |

Khi `POST /refresh`:

1. Khớp `token_hash`, phiên còn hạn, tài khoản còn hoạt động: cấp cặp token mới, đổi `token_hash`.
2. Khớp `token_hash_truoc` trong vài chục giây sau khi xoay vòng: coi là gửi lại do mất mạng, không thu hồi.
3. Khớp `token_hash_truoc` sau khoảng đó: token cũ bị dùng lại, coi như đã lộ, **thu hồi cả phiên**.
4. Còn lại: 401.

Token được đổi bằng một lệnh `UPDATE` có điều kiện trên `token_hash`, nên hai request refresh cùng lúc chỉ một request
thắng, request kia nhận 401 và phiên không bị thu hồi. Client phải gộp các lần refresh đồng thời thành một (`createApiClient`
của mobile đã làm vậy).

## 5. Mật khẩu

| Quy tắc | Giá trị |
| --- | --- |
| Tối thiểu | 8 ký tự |
| Tối đa | 128 ký tự, không cắt bớt, không trim |
| Loại ký tự | Không ép chữ hoa, số, ký tự đặc biệt; cho phép mọi ký tự |
| Danh sách chặn | Từ chối mật khẩu phổ biến và mật khẩu chứa tên đăng nhập |
| Đổi định kỳ | Không bắt buộc; chỉ bắt đổi khi có dấu hiệu lộ |

**Về mức 8 ký tự (chốt 08/11):** NIST và OWASP đặt tối thiểu 15 ký tự khi không có xác thực nhiều lớp, và
8 ký tự khi có. GymBase chọn 8 để học viên dễ dùng, tức là thấp hơn khuyến nghị cho đến khi có xác thực nhiều
lớp. Danh sách chặn và giới hạn đăng nhập sai vì vậy là **bắt buộc**. Đăng nhập Google (mục 8) giúp giảm số người dùng mật khẩu. Nên
xem lại mức này cho tài khoản `Admin`.

## 6. Chống dò mật khẩu

- Sai tên đăng nhập, sai mật khẩu, tài khoản bị khóa hoặc ngừng hoạt động đều trả cùng
  `AUTH_INVALID_CREDENTIALS`, cùng một câu thông báo.
- Tên đăng nhập không tồn tại vẫn chạy hàm băm một lần với hash giả, để thời gian phản hồi tương đương.
- Sai 4 lần đầu không khóa. Từ lần thứ 5: khóa 1 phút, mỗi lần sai tiếp theo gấp đôi, tối đa 15 phút; đếm cả tên
  đăng nhập không tồn tại (`LoginThrottle`). Không khóa vĩnh viễn, vì kẻ xấu có thể cố tình khóa người khác.
- Bộ đếm nằm trong bộ nhớ vì V1 chỉ có một host; khi chạy nhiều instance phải chuyển sang kho dùng chung.
- Giới hạn 20 lần gọi mỗi phút theo IP cho `/auth/login`, `/auth/refresh`, `/auth/change-password`, vượt quá trả 429.
  Sau Nginx phải bật ForwardedHeaders (mục 14), nếu không mọi client bị tính chung một IP.
- Không ghi mật khẩu, token vào log.

## 7. Phân quyền

| Vai trò | Được làm |
| --- | --- |
| `Admin` | Quản lý người dùng, danh mục |
| `Trainer` | Nghiệp vụ của chính PT đó |
| `Trainee` | Dữ liệu của chính học viên đó |

- API quản lý người dùng (`/users`) chỉ cho `Admin`, không mở cho học viên trên mobile.
- `[Authorize(Roles = ...)]` chỉ kiểm tra vai trò. Quyền trên từng bản ghi kiểm tra trong service, dùng
  `ICurrentUser`.
- Tiền tố `/mob` không phải ranh giới bảo mật.
- Lỗi 401, 403 trả ProblemDetails như các lỗi khác.

## 8. Đăng nhập bằng Google (sau V1)

Chưa làm, nhưng thiết kế trên đã chừa chỗ. App lấy ID token từ Google rồi gửi cho API. API xác minh rồi cấp
**JWT và refresh token của GymBase**, nên phần còn lại của hệ thống không phân biệt cách đăng nhập.

```text
App ──► Google: đăng nhập, nhận ID token
App ──► POST /auth/google { id_token } (HTTPS)
API: xác minh ID token → tìm tài khoản đã liên kết → cấp token GymBase như /login
```

**API phải kiểm tra** ID token: chữ ký bằng khóa công khai của Google, `aud` là một trong các client ID của app,
`iss` là `accounts.google.com` hoặc `https://accounts.google.com`, `exp` chưa qua. Dùng thư viện, không tự viết.

**Dùng `sub` của Google làm định danh, không dùng email**, vì email của tài khoản Google có thể thay đổi. Thêm bảng:

| Cột | Ý nghĩa |
| --- | --- |
| `nguoi_dung_id` | Tài khoản GymBase |
| `nha_cung_cap` | `google`; sau này có thể thêm `apple` |
| `ma_ngoai` | `sub` của Google; unique cùng `nha_cung_cap` |

**Liên kết tài khoản:** vì `Admin` tạo tài khoản và không có tự đăng ký, Google chỉ đăng nhập được vào tài khoản đã
liên kết. Người dùng đăng nhập bằng mật khẩu rồi chọn "Liên kết Google" trong hồ sơ. **Không tự liên kết theo email
trùng**: Google cho biết `email_verified` có thể chỉ phản ánh lần xác minh lúc tạo tài khoản, còn quyền sở hữu email đó
có thể đã đổi.

**Cần sửa khi làm:** `mat_khau_hash` đổi thành cho phép null nếu muốn có tài khoản chỉ dùng Google; thêm client ID cho
từng nền tảng; app chạy trên iOS thường phải có thêm Sign in with Apple theo quy định của App Store (kiểm tra lại khi làm).

## 9. Chưa làm trong V1

| Việc | Ghi chú |
| --- | --- |
| Quên mật khẩu qua email | Trước mắt `Admin` đặt lại |
| Xác thực nhiều lớp (TOTP) | Nên ưu tiên cho `Admin` |
| Gắn token với thiết bị (DPoP) | RFC 9700 khuyến nghị, quá nặng cho V1 |
| Cách lưu token cho web | Xem khi làm web (V2) |
| Đăng nhập bằng mật khẩu cho bên thứ ba | Không hỗ trợ. Có ứng dụng bên ngoài thì dùng Authorization Code + PKCE, vì RFC 9700 cấm Password Grant |

## 10. Khác với dự án tham khảo (dev-HuyNgo)

| Dự án tham khảo | GymBase |
| --- | --- |
| Duende IdentityServer, Password Grant | JWT tự cấp, không cần license |
| MD5 kèm salt | PBKDF2 600.000 vòng |
| Tắt kiểm tra issuer, audience, độ hợp lệ của khóa | Bật đủ |
| Refresh token dùng lại nhiều lần | Xoay vòng, phát hiện dùng lại |
| Đọc claim từ token chưa xác minh | Chỉ đọc request đã xác thực |

Giữ lại từ dự án đó: response đăng nhập có kèm thông tin người dùng, `[Authorize(Roles = ...)]`, claim `tai_khoan`.

## Nguồn

- [OWASP Password Storage](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html),
  [Authentication](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html),
  [JWT](https://cheatsheetseries.owasp.org/cheatsheets/JSON_Web_Token_Cheat_Sheet.html),
  [Mobile Application Security](https://cheatsheetseries.owasp.org/cheatsheets/Mobile_Application_Security_Cheat_Sheet.html)
- [NIST SP 800-63B-4](https://pages.nist.gov/800-63-4/sp800-63b/authenticators)
- [RFC 9700: OAuth 2.0 Security BCP](https://www.rfc-editor.org/rfc/rfc9700.html),
  [RFC 8725: JWT BCP](https://www.rfc-editor.org/rfc/rfc8725.html)
- [Microsoft: yêu cầu người dùng đã xác thực](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/secure-data)
- [Google: xác minh ID token](https://developers.google.com/identity/gsi/web/guides/verify-google-id-token),
  [OpenID Connect](https://developers.google.com/identity/openid-connect/openid-connect)
