# Shared TypeScript packages

Chưa có package dùng chung ở REPO-01. Chỉ tạo package khi có consumer thực tế:

| Package dự kiến | Trách nhiệm |
| --- | --- |
| `api-client/` | Client sinh từ OpenAPI; cấu hình transport/base URL |
| `contracts/` | Request/response types sinh từ OpenAPI, không sửa tay |
| `shared-utils/` | Helpers thuần TypeScript, không phụ thuộc Expo/Next.js/UI |

Không đặt C# projects, EF entities hoặc nghiệp vụ riêng của một feature ở đây.
Khi tạo package, thêm `package.json`, public exports và validation tương ứng.
