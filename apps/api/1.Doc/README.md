# 1. Doc

Tài liệu chỉ dành cho backend, mở được ngay trong Visual Studio (solution folder
`1. Doc`). Ví dụ: quy ước code C#, hướng dẫn debug, script mẫu.

Thêm file mới vào thư mục này rồi thêm vào solution folder `1. Doc`. Solution folder
này chỉ chứa file nằm trong `1.Doc/`, không đính kèm file ở ngoài.

| Trong `1.Doc/` | Nội dung |
| --- | --- |
| `cach-chay-api.md` | Chạy host nào cho web/mobile, task VS Code, điện thoại thật |
| `bruno/` | Collection [Bruno](https://www.usebruno.com/) để gọi thử API (thay cho file `.http`) |

Tài liệu dùng chung của monorepo nằm ở `docs/` ở gốc repo, ví dụ:

| File | Nội dung |
| --- | --- |
| `apps/api/README.md` | Cấu trúc, lệnh build/test/run, việc còn mở |
| `docs/api/README.md` | Quy ước API contract |
| `docs/api/cau-truc-thu-muc.md` | Giải thích chi tiết từng thư mục của solution |
