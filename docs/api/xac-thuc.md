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
| Định danh đăng nhập | **Email** (chữ thường, bỏ khoảng trắng), duy nhất trên mọi tài khoản. `ten_dang_nhap` còn trong DB nhưng không dùng để đăng nhập | ADR-0001, chốt 13/11 |
| Tạo tài khoản | Người dùng tự đăng ký bằng mật khẩu hoặc Google (mục 8). `Admin` vẫn tạo được tài khoản mật khẩu cho người khác | Chốt 11/11 và 13/11: cả hai nằm trong V1. Đăng ký bằng mật khẩu (mục 2.1) và Google (mục 8) đã làm ở backend |
| Phân quyền | Ba lớp: `vai_tro` (`Admin`, `User`) trong token, năng lực theo hồ sơ đọc từ DB mỗi request, quyền trên từng bản ghi | Chi tiết: [phan-quyen.md](phan-quyen.md) |

Các số 15 phút, 30 ngày, 90 ngày là lựa chọn của dự án, các tiêu chuẩn không quy định con số cụ thể.

## 2. Endpoint

Có ở cả `/api/v1/web/auth` và `/api/v1/mob/auth`, dùng chung `XacThucService`.

| Request | Đăng nhập? | Kết quả |
| --- | --- | --- |
| `POST /login` | Không | Body `{ email, mat_khau }`. 200: token kèm thông tin người dùng. 401 `AUTH_INVALID_CREDENTIALS`. Email không đúng định dạng: 400 |
| `POST /register` | Không | Body `{ ho_ten, email, so_dien_thoai, mat_khau, dong_y_dieu_khoan, ma_gioi_thieu? }`. 200: như `login`, thêm `tai_khoan_moi: true`. 409 `USER_EMAIL_TAKEN`, 400 nếu dữ liệu sai hoặc chưa đồng ý điều khoản |
| `POST /google` | Không | Body `{ id_token, ten_thiet_bi? }`. 200: như `login`, `tai_khoan_moi: true` ở lần đầu. 401 `AUTH_GOOGLE_TOKEN_INVALID` hoặc `AUTH_GOOGLE_EMAIL_UNVERIFIED`, 409 `AUTH_EMAIL_USED_PASSWORD` hoặc `USER_EMAIL_TAKEN` (mục 8) |
| `POST /refresh` | Không | 200: cặp token mới. 401 `UNAUTHORIZED` |
| `POST /logout` | Có | 204: thu hồi phiên hiện tại |
| `GET /me` | Có | 200: thông tin người dùng lấy từ DB |
| `POST /change-password` | Có | 204: thu hồi các phiên khác |

### 2.1. Đăng ký bằng mật khẩu

Người mới tự tạo tài khoản và có phiên ngay, nên không phải đăng nhập lại. Response của `login`, `register`, `refresh` có cùng dạng;
`tai_khoan_moi` là `true` chỉ ở lần `register` (và lần đầu đăng nhập Google), `false` ở mọi lần khác, để app hiện màn "Đăng ký thành công"
đúng một lần.

| Trường | Quy tắc |
| --- | --- |
| `ho_ten` | Bắt buộc, tối đa 100 ký tự |
| `email` | Bắt buộc, đúng định dạng, duy nhất không phân biệt hoa thường (ADR-0001), lưu chữ thường. Trùng: 409 `USER_EMAIL_TAKEN` |
| `so_dien_thoai` | Bắt buộc, số **di động** Việt Nam (đầu 03, 05, 07, 08, 09; nhận `0…`, `+84…`, `84…`, bỏ khoảng trắng, chấm, gạch), lưu dạng `0xxxxxxxxx`. Không duy nhất, không xác minh |
| `mat_khau` | Quy tắc mục 5, và không chứa phần trước `@` của email |
| `dong_y_dieu_khoan` | Phải là `true`; lưu lúc đồng ý vào `tai_khoan.ngay_dong_y_dieu_khoan` (UTC) |
| `ma_gioi_thieu` | Nhận nhưng bỏ qua (`TODO(REFERRAL)`) |

Tài khoản tạo ra có `vai_tro = User`, chưa có hồ sơ (app đưa người dùng tới bước chọn nhóm). Email và số điện thoại chưa xác minh (`TODO(VERIFY)`).
`/register` dùng chung giới hạn 20 lần mỗi phút theo IP với `/login`.

Admin đặt lại mật khẩu cho người khác qua `POST /api/v1/web/users/{id}/reset-password`, mọi phiên của người đó bị thu hồi.
Không có token hợp lệ thì mọi đường dẫn đều trả 401, kể cả đường dẫn không tồn tại, trừ `/health`, `/auth/login`, `/auth/register`, `/auth/refresh`.

Response của `login` và `refresh`:

```json
{
  "access_token": "eyJ…",
  "expires_in": 900,
  "refresh_token": "…",
  "user": { "id": "…", "ten_dang_nhap": "pt.minh", "email": "minh@gymbase.local", "ho_ten": "Nguyễn Văn Minh", "vai_tro": "User" }
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
  "role": "User",
  "iat": 1794150000,
  "nbf": 1794150000,
  "exp": 1794150900
}
```

| Claim | Ý nghĩa | Dùng để |
| --- | --- | --- |
| `iss`, `aud` | Nơi phát hành, nơi được dùng | Từ chối token không phải của hệ thống này |
| `sub` | `Id` của người dùng trong bảng `tai_khoan` | `ICurrentUser.UserId`. Giữ nguyên dù đăng nhập bằng cách nào |
| `sid` | `Id` của phiên trong `phien_dang_nhap` | Biết token thuộc phiên nào, để sau này thu hồi tức thì |
| `jti` | Mã riêng của token | Truy vết trong log |
| `preferred_username` | `ten_dang_nhap`, không có thì `email` | `ICurrentUser.UserName`, ghi `nguoi_tao`, `nguoi_chinh_sua` |
| `role` | `vai_tro` | `[Authorize(Roles = "...")]` |
| `iat`, `nbf`, `exp` | Thời điểm cấp, bắt đầu có hiệu lực, hết hạn | Kiểm tra thời hạn |

**Không đưa vào token:** mật khẩu hay hash, email, số điện thoại, họ tên, hay dữ liệu nghiệp vụ.
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
| `tai_khoan_id` | Chủ phiên |
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
| Danh sách chặn | Từ chối mật khẩu phổ biến và mật khẩu chứa tên đăng nhập (với tài khoản không có tên đăng nhập thì chưa kiểm tra phần trước `@` của email) |
| Đổi định kỳ | Không bắt buộc; chỉ bắt đổi khi có dấu hiệu lộ |

**Về mức 8 ký tự (chốt 08/11):** NIST và OWASP đặt tối thiểu 15 ký tự khi không có xác thực nhiều lớp, và
8 ký tự khi có. GymBase chọn 8 để học viên dễ dùng, tức là thấp hơn khuyến nghị cho đến khi có xác thực nhiều
lớp. Danh sách chặn và giới hạn đăng nhập sai vì vậy là **bắt buộc**. Đăng nhập Google (mục 8) giúp giảm số người dùng mật khẩu. Nên
xem lại mức này cho tài khoản `Admin`.

## 6. Chống dò mật khẩu

- Sai email, sai mật khẩu, tài khoản bị khóa hoặc ngừng hoạt động đều trả cùng
  `AUTH_INVALID_CREDENTIALS`, cùng một câu thông báo.
- Email không tồn tại vẫn chạy hàm băm một lần với hash giả, để thời gian phản hồi tương đương.
- Sai 4 lần đầu không khóa. Từ lần thứ 5: khóa 1 phút, mỗi lần sai tiếp theo gấp đôi, tối đa 15 phút; đếm theo email,
  cả email không tồn tại (`LoginThrottle`). Không khóa vĩnh viễn, vì kẻ xấu có thể cố tình khóa người khác.
- Bộ đếm nằm trong bộ nhớ vì V1 chỉ có một host; khi chạy nhiều instance phải chuyển sang kho dùng chung.
- Giới hạn 20 lần gọi mỗi phút theo IP cho `/auth/login`, `/auth/register`, `/auth/refresh`, `/auth/change-password`, vượt quá trả 429.
  Sau Nginx phải bật ForwardedHeaders (mục 14), nếu không mọi client bị tính chung một IP.
- Không ghi mật khẩu, token vào log.

## 7. Phân quyền

`vai_tro` chỉ có hai giá trị, là vai trò **hệ thống**. Việc một người là PT hay học viên, và PT đã xác minh chưa, lấy từ hồ sơ
(`ho_so_nguoi_dung`), không nằm trong `vai_tro`. Cách phân quyền đầy đủ (policy, quyền trên bản ghi, mã lỗi): [phan-quyen.md](phan-quyen.md).

| Vai trò | Được làm |
| --- | --- |
| `Admin` | Quản lý người dùng, duyệt PT. Không tự xem được dữ liệu riêng của học viên |
| `User` | Dùng app; quyền cụ thể theo hồ sơ và theo kết nối PT |

- API quản lý người dùng (`/users`) chỉ cho `Admin`, không mở cho học viên trên mobile.
- `[Authorize(Roles = ...)]` hoặc policy chỉ cho biết có được dùng chức năng không. Quyền trên từng bản ghi kiểm tra trong BL, dùng
  `ICurrentUser` và `TruyCapService`.
- Tiền tố `/mob` không phải ranh giới bảo mật.
- Lỗi 401, 403 trả ProblemDetails như các lỗi khác.

## 8. Đăng nhập và đăng ký bằng Google (V1)

**Chốt 11/11:** Google nằm trong V1, người lạ tự đăng ký được. **Backend đã làm** (`POST /google`, 13/11); app mobile đã nối trên Android (dev build). App lấy ID token từ Google rồi gửi cho API. API xác minh rồi cấp
**JWT và refresh token của GymBase**, nên phần còn lại của hệ thống không phân biệt cách đăng nhập.

```text
App ──► Google: đăng nhập, nhận ID token
App ──► POST /auth/google { id_token } (HTTPS)
API: xác minh ID token → tìm dang_nhap_ngoai theo (google, sub)
     ├─ có    → cấp token GymBase như /login
     └─ chưa  → tạo tai_khoan mới + dang_nhap_ngoai, rồi cấp token
```

Luồng đầy đủ kèm bước chọn nhóm (học viên, PT, cả hai) nằm ở
[database/diagrams/01-tai-khoan-ho-so.md](../../database/diagrams/01-tai-khoan-ho-so.md).

**API phải kiểm tra** ID token: chữ ký bằng khóa công khai của Google, `aud` là một trong các client ID của app,
`iss` là `accounts.google.com` hoặc `https://accounts.google.com`, `exp` chưa qua. Dùng thư viện, không tự viết.

**Dùng `sub` của Google làm định danh, không dùng email**, vì email của tài khoản Google có thể thay đổi. Thêm bảng:

| Cột | Ý nghĩa |
| --- | --- |
| `tai_khoan_id` | Tài khoản GymBase |
| `nha_cung_cap` | `google`; sau này có thể thêm `apple` |
| `ma_ngoai` | `sub` của Google; unique cùng `nha_cung_cap` |
| `ngay_tao` | Lúc liên kết |

Hai unique: `(nha_cung_cap, ma_ngoai)` và `(tai_khoan_id, nha_cung_cap)`.

**Tài khoản mới từ Google:** `vai_tro = User`, `mat_khau_hash` và `ten_dang_nhap` để NULL, `ho_ten` và `email` lấy từ Google
(email là định danh đăng nhập và duy nhất như mọi tài khoản, xem ADR-0001). Sau lần đăng nhập đầu, tài khoản chưa có hồ sơ và app đưa người dùng tới bước chọn nhóm.

**Liên kết với tài khoản mật khẩu có sẵn:** người dùng đăng nhập bằng mật khẩu rồi chọn "Liên kết Google" trong hồ sơ.
**Không tự liên kết theo email trùng**: Google cho biết `email_verified` có thể chỉ phản ánh lần xác minh lúc tạo tài khoản,
còn quyền sở hữu email đó có thể đã đổi. Vì email là duy nhất (ADR-0001), người đăng nhập Google mà email đã thuộc một
tài khoản mật khẩu bị từ chối với `AUTH_EMAIL_USED_PASSWORD` và được hướng dẫn đăng nhập bằng mật khẩu; ngược lại, đăng ký
mật khẩu bằng email đã có tài khoản Google báo email đã dùng.

**Cần làm:** `mat_khau_hash` và `ten_dang_nhap` đã cho phép NULL trong entity; thêm client ID cho từng nền tảng; app chạy
trên iOS thường phải có thêm Sign in with Apple theo quy định của App Store (kiểm tra lại khi làm). Các mục 1, 2, 7 còn
nói về `Trainer`/`Trainee` và `[Authorize(Roles = ...)]`, trong khi thiết kế mới chỉ có `Admin`/`User`: cần cập nhật khi
làm phân quyền.

### 8.1. Cấu hình và hành vi đã làm

- Client ID khai báo ở `Google:ClientIds` (mảng; user-secrets `Google:ClientIds:0` khi dev, biến môi trường `Google__ClientIds__0`
  khi chạy thật). Dùng client ID **Web** của dự án làm `aud` khi app lấy ID token qua `webClientId`; thêm client ID Android vào
  mảng nếu app lấy token bằng chính client ID Android. Chưa cấu hình thì mọi token bị từ chối (`AUTH_GOOGLE_TOKEN_INVALID`) và API ghi log lỗi. App mobile (Android, dev build) lấy ID token
  bằng `webClientId` này, xem [chay-mobile.md](../onboarding/chay-mobile.md) mục 6.
- Xác minh bằng thư viện `Google.Apis.Auth` (`GoogleIdTokenVerifier`): chữ ký, `iss`, `exp`, `aud`. Lỗi mạng khi tải khóa của Google
  thành 500, không báo nhầm là token sai.
- Tài khoản tìm theo `sub` (bảng `dang_nhap_ngoai`), không theo email: đổi email Google không tạo tài khoản mới.
- Lần đầu: cần email đã xác minh (`AUTH_GOOGLE_EMAIL_UNVERIFIED` nếu không có); tạo `tai_khoan` không mật khẩu, `ho_ten` lấy từ Google
  (không có thì lấy phần trước `@`), `ngay_dong_y_dieu_khoan` ghi lúc tạo vì màn đăng nhập Google có dòng "bằng việc tiếp tục bạn
  đồng ý…". Không tạo được liên kết thì xóa tài khoản vừa tạo để không để lại tài khoản mồ côi.
- Email đã thuộc tài khoản có mật khẩu: 409 `AUTH_EMAIL_USED_PASSWORD`; đã thuộc tài khoản khác không có mật khẩu: 409 `USER_EMAIL_TAKEN`.
- Tài khoản đã liên kết bị khóa: 401 chung. Tài khoản chỉ dùng Google không đăng nhập được bằng mật khẩu.
- Dùng chung giới hạn 20 lần mỗi phút theo IP với `/login`.
- Chưa làm: liên kết Google vào tài khoản mật khẩu từ Hồ sơ; Sign in with Apple.

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

Giữ lại từ dự án đó: response đăng nhập có kèm thông tin người dùng, `[Authorize(Roles = ...)]`, claim `ten_dang_nhap`.

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
