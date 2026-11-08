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
| `MISA.Salon.DLBIZ.Mysql` | `GymBase.DLBIZ.Mysql` | Class library | **Hiện thực** các interface trên cho MySQL. Đây là nơi duy nhất dùng **EF Core** (`DbContext`, cấu hình entity) và gọi DB. |

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
`README.md`, `cach-chay-api.md`, `crud-dung-chung.md` và collection Bruno `bruno/`), không đính kèm file ở ngoài. Tài liệu dùng chung của monorepo,
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

Route của hai kênh được phân biệt bằng tiền tố do `Program.cs` gắn: các host web dùng
`/api/web/v1/...`, MobAPI dùng `/api/mobile/v1/...`. Controller chỉ khai báo phần
`v1/[controller]` (kế thừa từ `BaseApiController`), nên chép controller từ web sang
MobAPI không phải sửa route. Cách chạy: `apps/api/1.Doc/cach-chay-api.md`.

### `4. Model` *(suy luận)*

Đây là các lớp dữ liệu dùng chung giữa nhiều nhóm, ví dụ entity ánh xạ tới bảng,
DTO, enum hoặc hằng số. Nếu đặt chúng trong từng nhóm thì DI và Biz sẽ phải tham
chiếu chéo nhau.
**GymBase:** `apps/api/4.Model/GymBase.Model` chứa **entity của mọi bảng**:
`Entities/<phân hệ>/<tên_bảng>.cs`, ví dụ `Entities/System/nguoi_dung.cs`. Tên class trùng
tên bảng, property trùng tên cột. Namespace chung là `GymBase.Model.Entities` (không theo
thư mục con, để tránh `...Entities.System` che namespace `System` của .NET). Model chỉ tham
chiếu `AMISBase.DL.Contract` (lấy `BaseEntity`), không chứa logic hay SQL. `DL*.Contract`
tham chiếu Model, nên DI và Biz cùng dùng được một entity mà không phải tham chiếu chéo nhau.

**DTO không để ở Model** mà để trong BL của phân hệ, ví dụ
`GymBase.BLSystem/NguoiDung/NguoiDungDtos.cs`. Chỉ kiểu dữ liệu mà nhiều phân hệ cùng cần
mới đưa lên Model. Không trả entity ra API; luôn dùng DTO. Chi tiết:
`apps/api/1.Doc/crud-dung-chung.md`, mục 2.

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
| `GymBase.AMISBase.API` | `BaseApiController`; `BaseCrudController<TDto, TCreateDto, TUpdateDto>` (GET danh sách, GET `{id}`, POST, PUT `{id}`, DELETE `{id}`); tiền tố route theo kênh | Các host `*API`, `MobAPI` |
| `GymBase.AMISBase.BL` | `ICrudBL<...>` và `BaseBL<TEntity, TDto, TCreateDto, TUpdateDto>`: chuẩn hóa phân trang, map entity ↔ DTO, tự điền ngày/người tạo, sửa (UTC), xóa mềm nếu entity có `is_deleted` | `BL*` |
| `GymBase.AMISBase.DL.Contract` | Entity gốc, `IHasId`, phân trang, `IBaseDL<TEntity>`: `GetPaging`, `GetById`, `Insert`, `Update`, `Delete` | `DL*.Contract`, `GymBase.Model` |
| `GymBase.AMISBase.DL.Mysql` | **Hiện là mock:** `InMemoryStore<T>` (đăng ký singleton) + `InMemoryBaseDL<TEntity>`. Khi nối DB sẽ thêm lớp nền `DbContext` và `BaseDL<TEntity>` dùng EF Core | `DL*.Mysql` |

HTTP trả về của controller dùng chung: `GET` 200 / 404, `POST` 201 kèm header `Location`,
`PUT` 200 / 404, `DELETE` 204 / 404. Body sai hoặc vi phạm DataAnnotations bị `[ApiController]`
trả 400 trước khi vào BL.

DL hiện lưu trong bộ nhớ, mất dữ liệu khi tắt host. Nối MySQL (mục 3 trong
[viec-con-lai-base.md](viec-con-lai-base.md)) chỉ phải đổi lớp cha của DL từng bảng từ
`InMemoryBaseDL` sang `BaseDL`, BL và API giữ nguyên.

---

## 3. Ví dụ: thêm danh mục "Gói tập" từ đầu đến cuối

Làm theo đúng mẫu người dùng (`nguoi_dung`) đang có. Code đầy đủ xem ở
`GymBase.Model/Entities/System`, `GymBase.DLSystem.*`, `GymBase.BLSystem/NguoiDung`,
`GymBase.SystemAPI` và `GymBase.MobAPI`. Hướng dẫn kèm code: `apps/api/1.Doc/crud-dung-chung.md`.

| Bước | Project | File | Nội dung |
| --- | --- | --- | --- |
| 1 | `GymBase.Model` | `Entities/DI/goi_tap.cs` | `class goi_tap : BaseAuditableEntity, ISoftDelete`, namespace `GymBase.Model.Entities`, property trùng tên cột. |
| 2 | `GymBase.DLDI.Contract` | `IGoiTapDL.cs` | `interface IGoiTapDL : IBaseDL<goi_tap>`, thêm hàm riêng nếu cần, ví dụ `ExistsByCode(code)`. |
| 3 | `GymBase.DLDI.Mysql` | `GoiTapDL.cs` | `class GoiTapDL : InMemoryBaseDL<goi_tap>, IGoiTapDL` (sau này là `BaseDL<goi_tap>`). Override `GetSearchValues` để chọn cột tìm kiếm. |
| 4 | `GymBase.BLDI` | `GoiTap/GoiTapDtos.cs`, `GoiTap/GoiTapBL.cs` | `GoiTapDto : IHasId`, `GoiTapCreateDto`, `GoiTapUpdateDto` kèm DataAnnotations. `class GoiTapBL : BaseBL<goi_tap, GoiTapDto, GoiTapCreateDto, GoiTapUpdateDto>`: viết 3 hàm map. Quy tắc riêng (mã không trùng, giá > 0) thì override `CreateAsync` / `UpdateAsync`. **Không** viết SQL. |
| 5 | `GymBase.DIAPI` | `Controllers/GoiTapController.cs` | `class GoiTapController(GoiTapBL bl) : BaseCrudController<GoiTapDto, GoiTapCreateDto, GoiTapUpdateDto>(bl);` là đủ 5 endpoint. |
| 6 | `GymBase.DIAPI` | `Program.cs` | Đăng ký DI: `TimeProvider.System`, `InMemoryStore<>` (singleton, khi còn mock), `IGoiTapDL → GoiTapDL`, `GoiTapBL`. |
| 7 | `GymBase.MobAPI` | `Controllers/GoiTapController.cs` + `Program.cs` | Kế thừa `BaseCrudController<GoiTapDto, GoiTapCreateDto, GoiTapUpdateDto>` như web, gọi lại `GoiTapBL`; đăng ký DI tương tự. |
| 8 | `GymBase.UnitTests` | `GoiTapBLTests.cs` | Tạo `GoiTapBL` trên `GoiTapDL` với `InMemoryStore` riêng cho từng test. |

Luồng chạy khi app mobile lấy danh sách gói tập:

```text
GET /api/mobile/v1/goitap  ──► MobAPI.GoiTapController
                          ──► BLDI.GoiTapBL.GetPagingAsync()
                              ──► IGoiTapDL (DLDI.Contract)
                                  ──► GoiTapDL (DLDI.Mysql) ── SQL ──► MySQL
```

---

## 4. Thêm một nhóm mới (ví dụ `Payment`)

1. Tạo 4 project trong `apps/api/3.Management/Payment/`: `GymBase.PaymentAPI`
   (webapi, `--use-controllers`), `GymBase.BLPayment`, `GymBase.DLPayment.Contract` và
   `GymBase.DLPayment.Mysql` (classlib). Tất cả dùng `-f net10.0`.
2. Nối tham chiếu theo đúng chiều ở mục 1.3, và tham chiếu tầng tương ứng của AMISBASE.
3. Thêm vào solution: `dotnet sln GymBase.slnx add --solution-folder "3. Management/Payment" <csproj...>`.
4. Chạy `dotnet test GymBase.slnx`. `LayerReferenceTests` tự nhận hai project BL và
   Contract mới, không cần sửa test.

Lệnh scaffold và build xem thêm tại `apps/api/README.md`.
