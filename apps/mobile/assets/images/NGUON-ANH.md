# Nguồn ảnh

Ba ảnh hero của màn chào mừng lấy từ [Pexels](https://www.pexels.com), dùng theo [Giấy phép Pexels](https://www.pexels.com/license/)
(miễn phí, dùng cho dự án thương mại, không bắt buộc ghi nguồn). Đây là ảnh tạm để dựng giao diện.
<!-- TODO(ASSET): thay bằng ảnh thật của GymBase (cắt từ file thiết kế) khi chủ dự án cung cấp, rồi xóa ảnh và tệp này. -->

| Tệp                           | Ảnh gốc                                                                                                                                    | Ghi chú                                |
| ----------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------ | -------------------------------------- |
| `welcome-tap-luyen.jpg`       | [Man Exercising at a Gym (4164756)](https://www.pexels.com/photo/man-exercising-at-a-gym-4164756/)                                         | Ivan S. Đã thu nhỏ còn 800x1000        |
| `welcome-dinh-duong.jpg`      | [Healthy Lunch Box with Rice and Vegetables (32287751)](https://www.pexels.com/photo/healthy-lunch-box-with-rice-and-vegetables-32287751/) | David Balbino. Đã thu nhỏ còn 800x1000 |
| `welcome-huan-luyen-vien.jpg` | [A Woman Guiding an Exercising Man (6111580)](https://www.pexels.com/photo/a-woman-guiding-an-exercising-man-6111580/)                     | Đã thu nhỏ còn 800x1000                |

## Logo

`logo.png` và `logo-dark.png` làm từ file logo chủ dự án cung cấp (chữ G xanh và chữ GymBase): đã cắt sát, thu nhỏ còn rộng 480
và bỏ nền kem để đặt được trên nền sáng lẫn tối. Bản `-dark` đổi chữ sang gần trắng và chữ G sang màu primary của chế độ tối.

## Logo Google

`google-g.png` là logo "G" nhiều màu của Google, lấy từ [Wikimedia Commons](https://commons.wikimedia.org/wiki/File:Google_%22G%22_logo.svg)
(bản PNG 250x250 nền trong suốt) và dùng trên nút "Đăng nhập với Google". Logo là nhãn hiệu của Google; dùng theo hướng dẫn
thương hiệu của Google cho nút đăng nhập (giữ nguyên màu, không biến dạng, không đổi màu theo giao diện sáng tối).

## Biểu tượng ứng dụng

`logo-mark.svg` là chữ G của logo, dò lại từ ảnh raster chủ dự án cung cấp thành đường Bézier (độ khớp với ảnh gốc khoảng 0,93; các cạnh chéo được làm thẳng).
Từ SVG này dựng ra `icon.png` (1024, nền kem `#F7F5F3`), `adaptive-icon.png` và `adaptive-icon-monochrome.png` (1024, nền trong suốt, chữ G chiếm 50% khung để nằm trong vùng an toàn của Android) và `favicon.png` (64). Khai báo ở `app.json`.
<!-- TODO(ASSET): thay bằng vector gốc của nhà thiết kế khi có; bản dò này chỉ chính xác ở mức ảnh raster ban đầu (266x266). -->
