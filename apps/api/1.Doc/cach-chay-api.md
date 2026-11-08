# Cách chạy API cho web và mobile

Mỗi nhóm chức năng là một API host riêng. Mọi route của host có **tiền tố theo
kênh client**, do `Program.cs` của host gắn (xem `RoutePrefixConvention` trong
`99.AMISBASE/GymBase.AMISBase.API`):

| Kênh | Tiền tố | Host | Ví dụ |
| --- | --- | --- | --- |
| Web (trang quản trị) | `/web` | SystemAPI, DIAPI, BIZAPI, AuthAPI, ReportAPI… | `GET http://localhost:5020/api/web/v1/users` |
| Mobile | `/mobile` | Chỉ MobAPI | `GET http://localhost:5106/api/mobile/v1/users` |

Gọi sai tiền tố (ví dụ `/mobile/...` vào SystemAPI, hoặc thiếu tiền tố) sẽ nhận **404**.
`/health` và `/openapi/v1.json` không có tiền tố.

## 1. Cần chạy project nào

Chỉ chạy những host mà client đang gọi tới.

| Muốn dùng | Startup project | Base URL |
| --- | --- | --- |
| API cho **mobile** | `GymBase.MobAPI` | `http://localhost:5106/api/mobile/v1/` |
| API cho **web** | `GymBase.SystemAPI` | `http://localhost:5020/api/web/v1/` |
| Cả hai | `GymBase.SystemAPI` + `GymBase.MobAPI` | Như trên |

Sau này, khi nhóm khác có endpoint, web phải bật thêm host của nhóm đó:

| Host | Cổng http | Base URL |
| --- | --- | --- |
| `GymBase.SystemAPI` | 5020 | `http://localhost:5020/api/web/v1/` |
| `GymBase.DIAPI` | 5055 | `http://localhost:5055/api/web/v1/` |
| `GymBase.RegisterAPI` | 5072 | `http://localhost:5072/api/web/v1/` |
| `GymBase.BIZAPI` | 5084 | `http://localhost:5084/api/web/v1/` |
| `GymBase.ImportExportAPI` | 5100 | `http://localhost:5100/api/web/v1/` |
| `GymBase.MobAPI` | 5106 | `http://localhost:5106/api/mobile/v1/` |
| `GymBase.ReportAPI` | 5150 | `http://localhost:5150/api/web/v1/` |
| `GymBase.AuthAPI` | 5152 | `http://localhost:5152/api/web/v1/` |

Cổng lấy từ `Properties/launchSettings.json` của từng host, profile `http`.

## 2a. Chạy bằng Visual Studio

1. Mở `apps/api/GymBase.slnx`.
2. Chuột phải vào Solution, chọn **Configure Startup Projects…**, chọn **Multiple startup projects**.
3. Đặt **Action = Start** cho `GymBase.SystemAPI` và `GymBase.MobAPI`. Ở cột **Debug Target**,
   chọn profile **http**.
4. Nhấn F5. Mỗi host mở một cửa sổ console riêng.

Chỉ cần một host thì chuột phải vào project đó, chọn **Set as Startup Project**.

## 2b. Chạy bằng VS Code (task)

Mở **thư mục gốc repo** (`GymBase`) trong VS Code. Các task khai báo ở `.vscode/tasks.json`.
Nhấn `Ctrl+Shift+P`, chọn **Tasks: Run Task**, rồi chọn task:

| Task | Việc làm |
| --- | --- |
| `API: chạy web + mobile` | Build một lần, rồi chạy song song SystemAPI (5020) và MobAPI (5106) |
| `API: chạy web (SystemAPI)` | Build rồi chạy SystemAPI |
| `API: chạy mobile (MobAPI)` | Build rồi chạy MobAPI |
| `API: chạy mobile cho điện thoại thật (LAN)` | Như trên nhưng nghe trên mọi địa chỉ mạng (xem mục 5) |
| `API: build` | Build solution. Phím tắt `Ctrl+Shift+B` |
| `API: test` | Chạy toàn bộ unit test |
| **`Dev: MobAPI + Expo`** | Build API, rồi chạy song song MobAPI (5106) và Expo (8081). Dùng khi làm app mobile |
| `Mobile: chạy Expo` | `pnpm mobile`. Trong terminal của task nhấn `a` (Android), `w` (web), `r` (reload) |
| `Mobile: chạy Expo (xóa cache)` | Như trên kèm `-c`, dùng khi app hiển thị code cũ hoặc lỗi bundle lạ |
| `Mobile: kiểm tra (lint + typecheck + test)` | `pnpm mobile:check` |

App mobile mặc định chạy chế độ demo (không gọi API). Muốn app gọi MobAPI thì cấu hình
`apps/mobile/.env` theo mục 5.

Mỗi API chạy trong một terminal riêng. Muốn dừng thì bấm vào terminal đó rồi nhấn
Ctrl+C, hoặc chạy **Tasks: Terminate Task**. Task chạy API luôn build trước một lần
rồi mới chạy với `--no-build`, để hai API không build chồng lên nhau cùng lúc.

## 2c. Chạy bằng dòng lệnh

Mở hai terminal tại `apps/api`, mỗi terminal chạy một host:

```bash
dotnet run --project 3.Management/System/GymBase.SystemAPI --launch-profile http
```

```bash
dotnet run --project 3.Management/Mob/GymBase.MobAPI --launch-profile http
```

Dừng bằng Ctrl+C.

## 3. Kiểm tra đã chạy

- Mở `http://localhost:5020/health` và `http://localhost:5106/health`, thấy `Healthy` là được.
- **Bruno:** mở [Bruno](https://www.usebruno.com/), chọn **Open Collection**, trỏ tới thư mục
  `apps/api/1.Doc/bruno`, chọn environment **local** ở góc trên bên phải. Collection có hai thư mục
  `Web (SystemAPI)` và `Mobile (MobAPI)`, mỗi request đã có sẵn assert mã trả về. Chạy cả
  collection bằng **Run** để kiểm tra nhanh.
- **Postman:** **Import** URL `http://localhost:5020/openapi/v1.json` (web) hoặc
  `http://localhost:5106/openapi/v1.json` (mobile) để tạo collection tự động. Trang
  OpenAPI chỉ có ở môi trường Development.

Id người dùng mẫu (dữ liệu mock): `0193f0a0-0000-7000-8000-000000000001` (admin).

## 5. Cho app mobile gọi được API

Cấu hình `apps/mobile/.env` (xem `.env.example`). Base URL **gồm cả tiền tố `/api/mobile/v1`**:

```text
EXPO_PUBLIC_DATA_MODE=api
EXPO_PUBLIC_API_URL=<địa chỉ ở bảng dưới>
```

| App chạy trên | `EXPO_PUBLIC_API_URL` | Cách chạy MobAPI |
| --- | --- | --- |
| Trình duyệt (Expo web), iOS Simulator | `http://localhost:5106/api/mobile/v1`  | Như mục 2 |
| Android Emulator | `http://10.0.2.2:5106/api/mobile/v1` | Như mục 2 |
| Điện thoại thật | `http://<IP-máy-tính>:5106/api/mobile/v1` | Phải lắng nghe trên mọi địa chỉ mạng, xem bên dưới |

`10.0.2.2` là địa chỉ mà Android Emulator dùng để trỏ về máy tính chạy nó.

**Điện thoại thật:** mặc định MobAPI chỉ nghe ở `localhost`, nên điện thoại không gọi tới được.
1. Chạy MobAPI cho nghe trên mọi địa chỉ mạng:

   ```bash
   dotnet run --project 3.Management/Mob/GymBase.MobAPI --launch-profile http --urls "http://0.0.0.0:5106"
   ```

2. Lấy IP của máy tính bằng lệnh `ipconfig`. Dùng dòng **IPv4 Address** của card Wi-Fi
   hoặc Ethernet đang dùng. Đừng dùng IP của card `vEthernet (WSL)` hay `Hyper-V`.
3. Điện thoại và máy tính phải dùng **chung một mạng Wi-Fi**.
4. Lần đầu chạy, nếu Windows Firewall hỏi thì cho phép `dotnet` truy cập mạng **Private**.
5. Thử mở `http://<IP-máy-tính>:5106/health` bằng trình duyệt trên điện thoại.

Đang phát triển với Expo Go thì gọi `http://` được. Bản build phát hành cần API chạy HTTPS.

## 6. Cho trang web gọi được API

- Gọi qua Bruno, Postman hoặc curl không cần cấu hình gì thêm.
- Trang web chạy trong trình duyệt ở origin khác (ví dụ `http://localhost:3000`) sẽ bị
  trình duyệt chặn vì **CORS chưa được cấu hình**. Việc này nằm trong mục 11 của
  `docs/api/viec-con-lai-base.md`. Khi làm, CORS phải đọc danh sách origin từ cấu hình,
  không mở cho mọi origin.

## 7. Lỗi thường gặp

| Hiện tượng | Nguyên nhân / cách xử lý |
| --- | --- |
| 404 khi gọi API | Thiếu tiền tố `/web` hoặc `/mobile`, gọi nhầm host, hoặc host của nhóm đó chưa chạy |
| `address already in use` / cổng bị chiếm | Host đó đang chạy ở cửa sổ khác. Tắt cửa sổ đó, hoặc tìm và dừng tiến trình `GymBase.*API` |
| Điện thoại không kết nối được | Chưa chạy với `--urls http://0.0.0.0:5106`, sai IP, khác mạng Wi-Fi, hoặc bị firewall chặn |
| Lỗi chứng chỉ khi gọi `https://localhost:7xxx` | Chạy `dotnet dev-certs https --trust` một lần, hoặc dùng profile `http` |
| `A project with an Output Type of Class Library cannot be started directly` | Startup project đang là class library. Chọn một project `*API` (trừ `BaseAPI`, `AMISBase.API`) |
