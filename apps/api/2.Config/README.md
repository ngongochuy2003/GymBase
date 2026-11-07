# 2. Config

Cấu hình dùng chung cho nhiều project (solution folder `2. Config`), ví dụ file
`appsettings` mẫu, cấu hình log hoặc file mẫu cho môi trường. Solution folder này
chỉ chứa file nằm trong `2.Config/`.

## Các file phải nằm ở gốc `apps/api/`

Một số file cấu hình được .NET/MSBuild/Git tìm theo thư mục, nên **không đặt
được** trong thư mục này. Chúng nằm ở gốc `apps/api/` và không hiện trong solution;
mở bằng Explorer hoặc trình soạn thảo:

| File | Lý do phải ở gốc |
| --- | --- |
| `global.json` | `dotnet` tìm file này từ thư mục hiện tại đi ngược lên để chọn SDK |
| `.gitignore` | Chỉ áp dụng cho thư mục chứa nó và các thư mục con |
| `Directory.Build.props` (khi cần) | MSBuild tự nạp file này từ thư mục project đi ngược lên |

## Quy định

Không đặt credentials (connection string thật, mật khẩu, token) ở đây hay trong
`appsettings.json`. Dùng biến môi trường hoặc `dotnet user-secrets`.
