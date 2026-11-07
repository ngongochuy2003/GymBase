# Cấu trúc thư mục backend (mô hình Management / AMISBASE)

Tài liệu này giải thích từng thư mục trong solution mẫu `MISA.Salon` mà GymBase
đang làm theo, và cho biết mỗi thư mục tương ứng với chỗ nào trong `apps/api`.

> **Độ tin cậy của thông tin.** Phần **AMISBASE, Management, DI, Biz, Mob** và cách
> chia API / BL / DL đã được chủ repo xác nhận. Các thư mục còn lại được **suy luận từ
> tên** theo quy ước phổ biến trong solution .NET. Chúng được đánh dấu *(suy luận)* và
> cần hỏi lại người hướng dẫn trước khi dựa vào.

---

## 1. Hai khái niệm cần nắm trước

### 1.1. Solution folder không phải là thư mục thật

Trong Visual Studio, các mục có biểu tượng thư mục vàng như `3. Management` hay `BIZ`
là **solution folder**. Chúng chỉ là nhóm ảo, được khai báo trong file solution
(`.sln` / `.slnx`), và **không nhất thiết** trùng với thư mục trên ổ đĩa.

Ở GymBase, solution folder được khai báo trong `apps/api/GymBase.slnx`:

```xml
<Folder Name="/3. Management/BIZ/">
  <Project Path="3.Management/BIZ/GymBase.BIZAPI/GymBase.BIZAPI.csproj" />
  ...
</Folder>
```

Trong Visual Studio bạn sẽ thấy `3. Management > BIZ > GymBase.BIZAPI`, còn trên ổ
đĩa file nằm ở `apps/api/3.Management/BIZ/GymBase.BIZAPI/`.

**Quy tắc của GymBase: thư mục thật = tên solution folder bỏ dấu cách sau số thứ tự**
(`"3. Management"` → `3.Management`). Giữ số để thứ tự trong Explorer giống Visual
Studio; bỏ dấu cách để gõ lệnh CLI, viết CI hay Dockerfile không phải đặt trong ngoặc kép.
Thư mục con không có số (như `BIZ`, `DI`) giữ nguyên tên.

| Solution folder | Thư mục thật (trong `apps/api/`) |
| --- | --- |
| `3. Management/DI`, `/BIZ`, `/Mob`… | `3.Management/DI`, `3.Management/BIZ`, `3.Management/Mob`… |
| `4. Model` … `7. Worker` | `4.Model`, `5.Lib`, `6.Integration`, `7.Worker` |
| `8. Unitest` | `8.Unitest` |
| `99. AMISBASE` | `99.AMISBASE` |
| `1. Doc`, `2. Config` | `1.Doc`, `2.Config` (chỉ chứa file, không có project) |

### 1.2. Bộ 4 project của một nhóm chức năng

Nhóm `BIZ` trong ảnh là mẫu chuẩn. Mỗi nhóm chức năng gồm 4 project:

| Project (Salon) | Project (GymBase) | Loại | Vai trò |
| --- | --- | --- | --- |
| `MISA.Salon.BIZAPI` | `GymBase.BIZAPI` | Web | Chứa **Controller**. Nhận HTTP request, kiểm tra đầu vào cơ bản, gọi BL rồi trả response. Đây là project chạy được (host). |
| `MISA.Salon.BLBIZ` | `GymBase.BLBIZ` | Class library | **Business Logic** (tương đương tầng Application). Chứa quy tắc nghiệp vụ, tính toán và kiểm tra hợp lệ. **Không query DB.** Khi cần dữ liệu thì gọi qua interface của DL. |
| `MISA.Salon.DLBIZ.Contract` | `GymBase.DLBIZ.Contract` | Class library | **Hợp đồng** của tầng dữ liệu: các interface như `IPackageDL` và các model đầu vào/đầu ra của DL. Không chứa code SQL. |
| `MISA.Salon.DLBIZ.Mysql` | `GymBase.DLBIZ.Mysql` | Class library | **Hiện thực** các interface trên cho MySQL. Đây là nơi viết **SQL raw** và gọi DB. |

Lý do tách `Contract` ra khỏi `Mysql`:

- BL chỉ biết interface, nên muốn test BL thì chỉ cần mock DL, không cần DB thật.
- Nếu sau này đổi sang hệ quản trị CSDL khác, chỉ cần thêm `DLBIZ.SqlServer` cài
  đặt cùng Contract. BL và API giữ nguyên.
- Trình biên dịch tự chặn việc BL gọi thẳng SQL, vì BL không tham chiếu project `Mysql`.

### 1.3. Chiều phụ thuộc

```text
            ┌──────────────┐
 HTTP ───►  │   BIZAPI     │  Controller
            └──────┬───────┘
                   │ gọi
            ┌──────▼───────┐
            │   BLBIZ      │  Logic nghiệp vụ, không có SQL
            └──────┬───────┘
                   │ chỉ biết interface
            ┌──────▼───────────┐      hiện thực     ┌──────────────────┐
            │ DLBIZ.Contract   │ ◄───────────────── │ DLBIZ.Mysql      │ ──► MySQL
            └──────────────────┘                    └──────────────────┘
```

- `BIZAPI` tham chiếu `BLBIZ`. Nó tham chiếu thêm `DLBIZ.Mysql`, nhưng **chỉ để đăng
  ký Dependency Injection** (gắn `IPackageDL` → `PackageDL`) trong `Program.cs`.
  Controller không được gọi DL trực tiếp.
- `BLBIZ` chỉ tham chiếu `DLBIZ.Contract`.
- Test `LayerReferenceTests` ở `apps/api/8.Unitest/GymBase.UnitTests` sẽ báo lỗi nếu
  BL hoặc Contract tham chiếu tới `*.Mysql`.

---

## 2. Giải thích từng thư mục

### `External Sources`

Đây **không phải thư mục của solution**. Visual Studio tự tạo nút này để hiển thị mã
nguồn bên ngoài (thư viện NuGet, mã đã decompile) khi bạn debug hoặc nhấn F12 vào
code không thuộc solution. Bạn có thể bỏ qua.

### `0. AI` *(suy luận)*

Theo tên gọi, đây là nơi chứa tính năng liên quan tới AI, chẳng hạn gợi ý hoặc
chatbot, hoặc tích hợp với dịch vụ AI.
**GymBase:** không tạo. Chỉ thêm khi có yêu cầu.

### `1. Doc` *(suy luận)*

Đây là tài liệu đặt ngay trong solution để mở được từ Visual Studio, ví dụ quy ước
code, tài liệu API hay script mẫu. Thường chỉ gồm file `.md` hoặc `.txt`, không có
project biên dịch.
**GymBase:** thư mục thật `apps/api/1.Doc`, chứa tài liệu chỉ dành cho backend.
Solution folder `1. Doc` chỉ gồm các file nằm trong `1.Doc/` (hiện là
`1.Doc/README.md`), không đính kèm file ở ngoài. Tài liệu dùng chung của monorepo,
kể cả tài liệu này, vẫn đặt ở `docs/` và không hiện trong solution.

### `2. Config` *(suy luận)*

Đây là cấu hình dùng chung cho nhiều project, ví dụ `appsettings` mẫu, cấu hình
log, `Directory.Build.props` (thuộc tính MSBuild chung) hoặc file `.editorconfig`.
**GymBase:** thư mục thật `apps/api/2.Config`, chứa cấu hình mẫu dùng chung.
Solution folder `2. Config` chỉ gồm các file nằm trong `2.Config/` (hiện là
`2.Config/README.md`). Riêng `global.json` (ghim SDK), `.gitignore` và
`Directory.Build.props` (khi cần) **bắt buộc nằm ở gốc `apps/api/`**, vì
dotnet/MSBuild/Git tìm chúng theo thư mục; các file này không hiện trong solution,
mở bằng Explorer hoặc trình soạn thảo. Theo quy định của repo,
**không** để credentials ở đây; dùng biến môi trường hoặc user-secrets.

### `3. Management`

Đây là **phần chính của ứng dụng**, nơi chứa toàn bộ tính năng quản lý. Mỗi thư mục
con là một nhóm chức năng, và đều theo bộ 4 project API / BL / DL.Contract /
DL.Mysql đã mô tả ở mục 1.2 (trừ `Mob` chỉ có API). Các nhóm *(suy luận)* đã có đủ
project nhưng **chưa có code**: API host chỉ có `/health`, các class library rỗng.

| Thư mục con | Ý nghĩa | Ghi chú |
| --- | --- | --- |
| `Auth` | Đăng nhập, đăng xuất, cấp và làm mới token, phân quyền *(suy luận)* | `GymBase.AuthAPI` (:5152) + BL/DL. Sẽ dùng khi làm xác thực cho mobile. |
| `Base` | Phần nền **riêng của Salon** dùng chung cho các nhóm trong Management, ví dụ controller hay service cơ sở đã gắn thêm cấu hình của sản phẩm *(suy luận)* | Khác `99. AMISBASE`: AMISBASE dùng chung cho **nhiều sản phẩm**, còn `Base` chỉ dùng chung **trong một sản phẩm**. GymBase: `GymBase.BaseAPI`, `BLBase`, `DLBase.*`, **đều là class library** (không chạy được), refer AMISBASE. Các nhóm khác **chưa** refer Base; khi Base có code thì chuyển chúng sang refer Base. |
| `BIZ` | **Business**: chức năng nghiệp vụ chính của sản phẩm (đã xác nhận) | `GymBase.BIZAPI` (:5084) + BL/DL. |
| `DI` | **Dictionary, tức danh mục**: dữ liệu nền ít thay đổi và được nghiệp vụ tham chiếu tới (đã xác nhận) | `GymBase.DIAPI` (:5055) + BL/DL. |
| `ImportExport` | Nhập và xuất dữ liệu bằng Excel hoặc CSV *(suy luận)* | `GymBase.ImportExportAPI` (:5100) + BL/DL. |
| `Mob` | **Mobile**: chỉ có một project API dành riêng cho app mobile (đã xác nhận) | `GymBase.MobAPI` (:5106). |
| `Register` | Đăng ký tài khoản hoặc tenant mới, khởi tạo dữ liệu ban đầu cho khách hàng mới *(suy luận)* | `GymBase.RegisterAPI` (:5072) + BL/DL. |
| `Report` | Báo cáo và thống kê, thường là các truy vấn tổng hợp nặng *(suy luận)* | `GymBase.ReportAPI` (:5150) + BL/DL. |
| `System` | Thiết lập hệ thống: tham số, cấu hình chung, người dùng hoặc vai trò, nhật ký *(suy luận)* | `GymBase.SystemAPI` (:5020) + BL/DL. |

#### DI và Biz khác nhau thế nào?

- **DI (danh mục)** giống như các "từ điển" dữ liệu: thêm, sửa, xóa, xem danh sách
  là chính, và logic thường đơn giản. Ví dụ trong GymBase có thể là: gói tập, loại
  bài tập, nhóm cơ, chi nhánh hoặc phòng tập.
- **Biz (nghiệp vụ)** là các luồng có quy tắc và trạng thái. Ví dụ trong GymBase
  có thể là: đặt và hủy buổi PT, trừ buổi trong gói khi điểm danh, ghi nhận buổi
  tập, khiếu nại.

Cách phân biệt nhanh: nếu bản ghi chủ yếu được **chọn từ một danh sách** trong
nghiệp vụ khác thì đó là danh mục (DI). Nếu nó **diễn ra theo thời gian và có quy
tắc** thì đó là nghiệp vụ (Biz). Việc xếp Trainers, Trainees, Workouts… vào DI hay
Biz **chưa được chốt**.

#### Mob: vì sao chỉ có một project?

Mobile không có logic riêng. Quy trình là:

1. Viết controller ở web (`DIAPI` hoặc `BIZAPI`), gọi BL rồi tới DL.
2. Thêm controller tương ứng ở `MobAPI`, gọi **lại đúng BL đó**.

Như vậy một quy tắc nghiệp vụ chỉ được viết một lần, trong BL. `MobAPI` tách riêng
để có thể dùng route, response gọn hơn cho mobile, xác thực và phiên bản API độc
lập, cũng như deploy và scale riêng. Vì thế `GymBase.MobAPI` tham chiếu BL của cả
DI và Biz.

### `4. Model` *(suy luận)*

Đây là các lớp dữ liệu dùng chung giữa nhiều nhóm, ví dụ entity ánh xạ tới bảng,
DTO, enum hoặc hằng số. Nếu đặt chúng trong từng nhóm thì DI và Biz sẽ phải tham
chiếu chéo nhau.
**GymBase:** `apps/api/4.Model/GymBase.Model` (class library rỗng, chưa project nào
refer). Khi Biz cần dùng model của DI thì đặt model vào đây thay vì để Biz tham
chiếu thẳng vào DI. Không trả entity trực tiếp
cho mobile; dùng DTO.

### `5. Lib` *(suy luận)*

Đây là thư viện tiện ích không gắn với nghiệp vụ, ví dụ xử lý chuỗi hoặc ngày giờ,
mã hóa, đọc ghi Excel, gửi email, hoặc wrapper cho thư viện bên thứ ba.
**GymBase:** `apps/api/5.Lib/GymBase.Lib` (class library rỗng).

### `6. Integration` *(suy luận)*

Đây là nơi tích hợp với hệ thống bên ngoài, ví dụ cổng thanh toán, SMS hoặc Zalo,
hóa đơn điện tử, hay API của đối tác. Mỗi tích hợp thường là một project riêng.
**GymBase:** `apps/api/6.Integration/GymBase.Integration` (class library rỗng). Khi có
nhiều tích hợp thì tách mỗi tích hợp một project trong thư mục này.

### `7. Worker` *(suy luận)*

Đây là các tiến trình chạy nền, không phục vụ request trực tiếp, ví dụ job theo
lịch (nhắc lịch tập, báo gói tập sắp hết hạn) hoặc consumer đọc message queue.
Trong .NET thường là project kiểu `worker` (`BackgroundService`). Worker cũng gọi
BL như API.
**GymBase:** `apps/api/7.Worker/GymBase.Worker` (project `worker`, mới chỉ khởi động host,
chưa có job nào).

### `8. Unitest`

Đây là các project kiểm thử. Tên "Unitest" là cách viết của solution gốc.
**GymBase:** `apps/api/8.Unitest/GymBase.UnitTests`, nằm trong solution folder `8. Unitest`.
Hiện có `LayerReferenceTests`, dùng để giữ quy tắc phân tầng. Test tự tìm mọi project
`GymBase.BL*` và `*.Contract`, nên nhóm mới không cần khai báo thêm. Test cho BL sẽ mock
interface trong `DL*.Contract`.

### `99. AMISBASE`

Đây là **nền thêm, sửa, xóa dùng chung**, được các nhóm khác tham chiếu tới (đã
xác nhận). Trong MISA, AMISBASE là bộ khung chung cho nhiều sản phẩm AMIS: viết
CRUD một lần, sau đó mỗi danh mục hay nghiệp vụ chỉ cần kế thừa.

GymBase chia AMISBASE thành đúng 4 tầng để các tầng của DI và Biz kế thừa tương ứng:

| Project | Chứa gì | Ai tham chiếu |
| --- | --- | --- |
| `GymBase.AMISBase.API` | `BaseApiController` (đã có), sau này có `BaseCrudController<T>` | `DIAPI`, `BIZAPI`, `MobAPI` |
| `GymBase.AMISBase.BL` | `BaseBL<T>`: kiểm tra hợp lệ chung, rồi gọi DL | `BLDI`, `BLBIZ` |
| `GymBase.AMISBase.DL.Contract` | `IBaseDL<T>`: `GetById`, `GetPaging`, `Insert`, `Update`, `Delete` | `DLDI.Contract`, `DLBIZ.Contract` |
| `GymBase.AMISBase.DL.Mysql` | `BaseDL<T>` hiện thực bằng SQL raw, kèm connection factory | `DLDI.Mysql`, `DLBIZ.Mysql` |

Hiện mới có `BaseApiController`. Phần CRUD dùng chung **chờ DB-01**, vì cần chọn
trước thư viện chạy SQL raw (Dapper hoặc ADO.NET) và MySQL driver.

---

## 3. Ví dụ: thêm danh mục "Gói tập" từ đầu đến cuối

Đoạn code dưới đây **chỉ minh họa** vị trí đặt code. Tên lớp và chữ ký hàm sẽ chốt
khi làm AMISBASE ở DB-01.

| Bước | Project | File | Nội dung |
| --- | --- | --- | --- |
| 1 | `GymBase.DLDI.Contract` | `IPackageDL.cs` | `interface IPackageDL : IBaseDL<Package>` và thêm hàm riêng nếu cần, ví dụ `ExistsByCode(code)`. |
| 2 | `GymBase.DLDI.Mysql` | `PackageDL.cs` | `class PackageDL : BaseDL<Package>, IPackageDL`, viết SQL raw cho các hàm riêng. |
| 3 | `GymBase.BLDI` | `PackageBL.cs` | Quy tắc nghiệp vụ, ví dụ mã gói không trùng, giá lớn hơn 0. Gọi `IPackageDL`, **không** viết SQL. |
| 4 | `GymBase.DIAPI` | `Controllers/PackagesController.cs` | `class PackagesController : BaseApiController`, gọi `PackageBL`. |
| 5 | `GymBase.DIAPI` | `Program.cs` | Đăng ký DI: `IPackageDL → PackageDL`, `PackageBL`. |
| 6 | `GymBase.MobAPI` | `Controllers/PackagesController.cs` + `Program.cs` | Chỉ mở những endpoint mobile cần, gọi lại `PackageBL` và đăng ký DI tương tự. |
| 7 | `GymBase.UnitTests` | `PackageBLTests.cs` | Mock `IPackageDL` để test quy tắc của `PackageBL`. |

Luồng chạy khi app mobile lấy danh sách gói tập:

```text
GET /api/v1/packages  ──► MobAPI.PackagesController
                          ──► BLDI.PackageBL.GetPaging()
                              ──► IPackageDL (DLDI.Contract)
                                  ──► PackageDL (DLDI.Mysql) ── SQL ──► MySQL
```

---

## 4. Thêm một nhóm mới (ví dụ `Payment`)

1. Tạo 4 project trong `apps/api/3.Management/Payment/`: `GymBase.PaymentAPI`
   (webapi, `--use-controllers`), `GymBase.BLPayment`, `GymBase.DLPayment.Contract` và
   `GymBase.DLPayment.Mysql` (classlib). Tất cả dùng `-f net9.0`.
2. Nối tham chiếu theo đúng chiều ở mục 1.3, và tham chiếu tầng tương ứng của AMISBASE.
3. Thêm vào solution: `dotnet sln GymBase.slnx add --solution-folder "3. Management/Payment" <csproj...>`.
4. Chạy `dotnet test GymBase.slnx`. `LayerReferenceTests` tự nhận hai project BL và
   Contract mới, không cần sửa test.

Lệnh scaffold và build xem thêm tại `apps/api/README.md`.
