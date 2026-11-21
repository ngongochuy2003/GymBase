---
status: in-progress
---

# Lộ trình: các quyết định đã chốt và còn mở

Ngày 21/11/2026. Ghi lại từng câu hỏi trong lúc chốt phạm vi và mâu thuẫn thiết kế của màn Lộ trình (xem
[luong-man-hinh.md](luong-man-hinh.md)), cùng câu trả lời. Chia hai phần: quyết định nghiệp vụ (người dùng chốt) và
quyết định kỹ thuật (hệ quả của quyết định nghiệp vụ). Mục nào do Claude suy ra, chưa được người dùng xác nhận, ghi rõ
"chờ xác nhận".

## 1. Quyết định nghiệp vụ

### N1. Phạm vi V1: làm đủ vòng PT và học viên

- **Câu hỏi:** Phạm vi màn Lộ trình ở V1 là gì? [phan-quyen.md](../../api/phan-quyen.md) đang ghi bốn bảng lộ trình không làm ở V1.
- **Các lựa chọn:** làm đủ vòng PT và học viên; chỉ học viên xem và check-in; chỉ giao diện với dữ liệu mock; không làm ở V1.
- **Chốt:** làm đủ vòng. PT tạo lộ trình, học viên check-in, PT duyệt.
- **Hệ quả:** phải làm các bảng lộ trình ở V1 và sửa ghi chú cũ trong `phan-quyen.md` mục 7 (xem K1).

### N2. Lộ trình là một thực thể riêng, check-in và tiến độ tính riêng cho từng học viên

- **Câu hỏi:** Một lộ trình có thể gán cho nhiều học viên không? (form tạo lộ trình có ô "Học viên tham gia" dạng chip.)
- **Các lựa chọn:** một lộ trình một học viên; chọn nhiều, mỗi người một bản riêng; một lộ trình dùng chung cho nhiều học viên.
- **Chốt (nguyên văn):** lộ trình là một cái riêng vì sau này có chức năng chia sẻ lộ trình, nhưng check-in và tiến độ cần tính riêng.
- **Cách hiểu ban đầu của Claude (đã xác nhận):** lộ trình là thực thể độc lập; mỗi học viên tham gia có một **lượt tham gia** riêng, check-in và tiến độ gắn vào lượt đó.
- **Sửa ngày 21/11/2026 (theo M2):** khi đối chiếu với schema 21 bảng đã có (mục 3), người dùng chọn mô hình **mỗi học viên một lộ trình riêng, chia sẻ về sau là sao chép**. Do đó:
  - Không có bảng lượt tham gia và không có "một lộ trình dùng chung nhiều học viên". Lộ trình luôn thuộc một học viên qua một kết nối PT.
  - Chia sẻ lộ trình (làm sau) là sao chép lộ trình sang học viên khác, không dùng chung bản ghi. Vì vậy check-in và tiến độ tính riêng cho từng học viên mà không cần bảng nối.
  - Ô "Học viên tham gia" ở form tạo lộ trình: ban đầu hiểu là chọn được nhiều (tạo n lộ trình độc lập); sau đó người dùng chốt chỉ chọn **một** học viên (N18).
- **Ghi chú:** lựa chọn này làm N2 gốc (lộ trình dùng chung) không còn đúng; ý "lộ trình là một cái riêng để chia sẻ sau" vẫn giữ, bằng sao chép.

### N3. Trạng thái check-in bữa ăn có 2 giá trị

- **Câu hỏi:** Trạng thái check-in bữa ăn gồm những giá trị nào? (Phần 08 có 3, phần 09 có 2.)
- **Chốt:** hai giá trị, **Đã ăn** và **Bỏ bữa**. Không có "Ăn một phần".
- **Hệ quả:** bỏ nút "Ăn một phần" ở màn HV-04. Ăn thiếu hoặc ăn khác kế hoạch thì học viên ghi trong ghi chú. Buổi tập vẫn giữ ba giá trị (Đã tập, Tập một phần, Nghỉ tập) theo thiết kế phần 10, vì chưa có quyết định ngược lại.

### N4. PT chọn chu kỳ cân nặng và ảnh cơ thể khi tạo lộ trình

- **Câu hỏi:** Chu kỳ cân nặng và ảnh cơ thể do ai quyết định, mặc định bao nhiêu? (Các ảnh ghi hằng ngày, hằng tuần, 2 lần/tuần, mỗi 2 tuần.)
- **Chốt:** PT chọn khi tạo lộ trình, theo form phần 13. Mặc định gợi ý: cân nặng 2 lần mỗi tuần, ảnh cơ thể mỗi 2 tuần.
- **Hệ quả:** các màn khác (nhiệm vụ hôm nay, tiến độ, nhắc nhở) đọc chu kỳ từ cấu hình của lộ trình, không cố định hằng ngày hay hằng tuần. Dòng "Cân nặng hằng ngày" ở phần 07 và "Chủ nhật hằng tuần" ở phần 08 chỉ là dữ liệu mẫu.

### N5. Sửa lộ trình đã có học viên tham gia: sửa chung, áp dụng từ hôm nay

- **Câu hỏi:** Khi lộ trình đã có học viên tham gia, PT sửa lộ trình (đổi bữa ăn, lịch tập, chu kỳ) thì áp dụng thế nào?
- **Các lựa chọn:** sửa chung và áp dụng từ hôm nay; tạo phiên bản mới, học viên đang tham gia giữ bản cũ; mỗi lượt tham gia có bản sao riêng khi sửa.
- **Chốt:** sửa chung, áp dụng từ hôm nay. Kế hoạch mới có hiệu lực từ ngày sửa; check-in cũ giữ nguyên.
- **Sau M2:** vì mỗi lộ trình chỉ thuộc một học viên (N2), "sửa chung" chỉ còn nghĩa là sửa lộ trình của học viên đó, áp dụng từ hôm nay. Việc sửa một lộ trình không ảnh hưởng học viên nào khác, kể cả lộ trình được sao chép từ nó.
- **Hệ quả:** không có phiên bản lộ trình ở V1. Cần bảo đảm check-in cũ không đổi nghĩa khi kế hoạch đổi (xem K5).

### N6. Ghi chú gửi PT trong check-in tối đa 300 ký tự

- **Câu hỏi:** Ghi chú gửi PT trong check-in tối đa bao nhiêu ký tự? (Phần 08 ghi 200, phần 09 ghi 300.)
- **Chốt:** 300 ký tự, thống nhất với ghi chú lộ trình của PT (cũng 300). Học viên có chỗ tả bữa ăn khác kế hoạch vì đã bỏ "Ăn một phần" (N3).

### N7. Màn đầu của "Lộ trình của tôi" là danh sách nhóm theo trạng thái

- **Câu hỏi:** Màn đầu của sub-tab "Lộ trình của tôi" (HV-01) dùng bản nào?
- **Các lựa chọn:** danh sách nhóm theo trạng thái (phần 06); tổng quan một lộ trình (phần 05).
- **Chốt:** danh sách nhóm theo trạng thái, ví dụ nhóm "Đang thực hiện (n)" bằng thẻ gọn, bên dưới là nhiệm vụ hôm nay. Phù hợp khi học viên có nhiều lộ trình.
- **Hệ quả:** thẻ lớn có 4 lối tắt của phần 05 không dùng cho màn đầu; các lối tắt đó chuyển vào màn chi tiết lộ trình (HV-02).

### N8. Tên sub-tab theo phần 12, không có tab "Lộ trình mẫu" ở V1

- **Câu hỏi:** Phía PT, lộ trình dùng chung (mẫu) hiển thị ở đâu? Thiết kế phần 14 có tab "Lộ trình mẫu" cạnh "Lộ trình tôi quản lý".
- **Trả lời của người dùng (nguyên văn):** "xem phần 12 có mà".
- **Cách hiểu của Claude, chờ xác nhận:** theo phần 12, phía PT chỉ có hai sub-tab "Lộ trình tôi quản lý" và "Lộ trình của tôi" (vì PT cũng có thể là học viên). Tab "Lộ trình mẫu" ở phần 14 là bản thiết kế cũ, bỏ.
- **Chốt:** tên hai sub-tab là **Lộ trình của tôi** và **Lộ trình tôi quản lý** cho cả hai nhóm người dùng; không có tab mẫu ở V1. Chia sẻ lộ trình làm sau (N2).

### N9. Hàng chip chỉ có ở Trang chủ

- **Câu hỏi:** Hàng chip "Dành cho bạn, Xu hướng, Lộ trình, Tập luyện, Kiến thức" xuất hiện ở một số ảnh (nhiệm vụ hôm nay, Lộ trình của PT), không có ở ảnh khác. Dùng thế nào?
- **Chốt:** chỉ ở Trang chủ. Lộ trình là tab riêng của thanh dưới, đầu màn chỉ có tiêu đề và hai sub-tab (N8).
- **Hệ quả:** các ảnh có hàng chip ở màn Lộ trình (HV-03, PT-01) bỏ hàng chip khi làm. Chip "Lộ trình" trong hàng chip của Trang chủ là lối vào bài viết cộng đồng về chủ đề lộ trình, không phải màn Lộ trình.

### N10. Thanh điều hướng dưới ẩn ở mọi màn con

- **Câu hỏi:** Thanh dưới (5 mục) có còn ở các màn con như check-in, chi tiết, tạo lộ trình không?
- **Chốt:** ẩn ở mọi màn con. Chỉ hiện ở các màn tab gốc (HV-01, PT-01); màn con có nút quay lại.

### N11. Nút `+` giữa thanh dưới giữ nguyên

- **Câu hỏi:** Nút `+` ở giữa thanh dưới mở gì? (Thiết kế không nói; mobile đang có màn soạn bài `compose`.)
- **Chốt:** giữ nguyên như hiện tại (soạn bài). Không thêm hành động nào của Lộ trình vào nút này.

### N12. Lộ trình kích hoạt ngay khi PT tạo

- **Câu hỏi:** PT tạo lộ trình xong, học viên phải chấp nhận hay kích hoạt luôn? (SRS FR-PRG-02 để mở; thiết kế giao diện không có màn chấp nhận.)
- **Chốt:** kích hoạt luôn. Học viên đã đồng ý khi kết nối PT nên không thêm bước; PT tạo xong là "đang thực hiện", học viên nhận thông báo.
- **Hệ quả:** không có trạng thái "chờ học viên chấp nhận" và không có màn lời mời, từ chối. "Lưu nháp" (PT-02) vẫn giữ: nháp chỉ PT thấy, học viên không thấy. Cách lưu và mở lại: xem N22.

### N13. Sửa check-in: sửa khi chưa duyệt, sau duyệt chỉ PT mở lại

- **Câu hỏi:** Quy tắc sửa check-in sau khi gửi? (SRS FR-CHK-04 để mở: hạn sửa, ai sửa sau duyệt, trạng thái khi sửa.)
- **Các lựa chọn:** sửa khi chưa duyệt, sau duyệt chỉ PT mở lại; sửa trong 24 giờ kể cả sau duyệt; không cho sửa sau khi gửi.
- **Chốt:** học viên sửa được khi check-in đang chờ PT hoặc bị yêu cầu bổ sung; sửa xong về lại chờ duyệt. Sau khi PT duyệt thì không sửa; muốn đổi phải nhắn PT, PT dùng "Yêu cầu bổ sung" để mở lại. Mọi lần sửa ghi vào `lich_su_chinh_sua`.
- **Hệ quả:** nút "Chỉnh sửa check-in" ở HV-05 ẩn khi đã duyệt. PT đang đánh giá mà học viên vừa sửa thì phải báo phiên bản cũ (SRS mục 6.4).

### N14. Một lộ trình do một PT phụ trách cả bữa ăn và buổi tập

- **Câu hỏi:** Một học viên có thể có lộ trình tập và lộ trình ăn từ hai PT khác nhau không? (`pham_vi_phu_trach` của kết nối là `tap_luyen` và/hoặc `dinh_duong`.)
- **Các lựa chọn:** có, hai PT riêng, mỗi lộ trình thuộc một kết nối; một PT phụ trách cả hai.
- **Chốt:** một PT phụ trách cả hai. Một lộ trình có đủ bữa ăn và buổi tập như màn hình, do đúng một PT lập và đánh giá.
- **Hệ quả:**
  - Lộ trình thuộc một kết nối PT (N2 sau M2). Học viên vẫn có thể có nhiều lộ trình từ nhiều PT (SRS 3.2, đã chốt "một học viên nhiều PT"), mỗi lộ trình do một PT phụ trách trọn vẹn.
  - Một lộ trình chứa cả hai loại mục, không tách hai kế hoạch (K6).
  - Kết nối không có đủ phạm vi thì xử lý thế nào: xem M8.

### N15. Tạo lộ trình bắt buộc kết nối có đủ cả hai phạm vi

- **Câu hỏi:** Nếu kết nối PT chỉ có một phạm vi (chỉ tập luyện hoặc chỉ dinh dưỡng), PT xử lý thế nào khi tạo lộ trình?
- **Các lựa chọn:** chỉ tạo phần được giao (ẩn phần ngoài phạm vi); bắt buộc đủ cả hai phạm vi.
- **Chốt:** bắt buộc đủ cả hai. PT phải có kết nối với phạm vi `tap_luyen` và `dinh_duong` mới tạo được lộ trình.
- **Hệ quả:**
  - Lộ trình luôn có đủ bữa ăn và buổi tập như thiết kế; màn học viên và PT không cần xử lý trường hợp thiếu một phần.
  - PT-02 chỉ liệt kê học viên có kết nối `dang_hoat_dong` và `pham_vi_phu_trach` chứa cả hai mã. Kết nối thiếu phạm vi thì PT phải mở rộng phạm vi trước (luồng này thuộc module kết nối PT, chưa có thiết kế).
  - Backend kiểm tra phạm vi khi tạo lộ trình, không tin giao diện.

### N16. Cân nặng và ảnh cơ thể không qua PT duyệt

- **Câu hỏi:** Check-in cân nặng và ảnh cơ thể có đi qua PT duyệt như bữa ăn không? (Thiết kế PT-03 có dòng "Cập nhật cân nặng, Chờ duyệt" trong hàng chờ duyệt.)
- **Các lựa chọn:** có, dùng `nhat_ky_hoat_dong`; không, ghi thẳng `chi_so_co_the`.
- **Chốt:** không qua duyệt. Học viên nhập là xong; PT chỉ xem trong tiến độ.
- **Hệ quả:**
  - Bỏ dòng "Cập nhật cân nặng, Chờ duyệt" khỏi khối "Check-in chờ duyệt" ở PT-03 và khỏi số "check-in cần duyệt" ở PT-01. Hàng chờ duyệt chỉ có bữa ăn và buổi tập.
  - Nhiệm vụ cân nặng và ảnh ở HV-03 có trạng thái "đã ghi" thay cho "đã check-in, chờ PT".
  - Không có "Yêu cầu bổ sung" cho cân nặng và ảnh. PT muốn nhận xét thì nhắn tin.
  - Ảnh cơ thể gắn vào `chi_so_co_the` (xem K7).

### N17. PT không chọn thứ trong tuần cho cân nặng và ảnh

- **Câu hỏi:** "2 lần/tuần" cân nặng rơi vào những thứ nào? (Form phần 13 chỉ chọn số lần.)
- **Chốt:** hệ thống tự chia đều các ngày, PT không chọn thứ; ví dụ 2 lần mỗi tuần là Thứ 2 và Thứ 5. Form tạo lộ trình giữ nguyên, không thêm ô chọn thứ.
- **Hệ quả:** cần một quy tắc chia ngày cố định cho từng số lần mỗi tuần (viết ở tài liệu vòng đời hoặc quy tắc).

### N18. Tạo lộ trình chỉ chọn một học viên

- **Câu hỏi:** Chọn nhiều học viên ở PT-02 (N2) thì tạo xong về đâu, và học viên đã có lộ trình đang chạy với cùng PT thì có chặn không? (Đề xuất ban đầu: về PT-01 nếu tạo nhiều, vào PT-03 nếu một.)
- **Chốt (nguyên văn):** "chỉ chọn 1 thôi".
- **Hệ quả:**
  - Ô "Học viên tham gia" chỉ chứa một học viên; không còn trường hợp tạo nhiều lộ trình cùng lúc, nên tạo xong luôn vào PT-03 của lộ trình vừa tạo.
  - Muốn cùng nội dung cho học viên khác thì dùng chức năng chia sẻ (sao chép), làm sau.
  - Việc có chặn học viên đã có lộ trình đang chạy với cùng PT hay không chưa được trả lời (M12).

### N19. "Đánh giá từ PT" ở tiến độ là nhận xét mới nhất

- **Câu hỏi:** "Đánh giá từ PT" ở HV-07 là nhận xét của check-in mới nhất hay một đánh giá tổng quan riêng do PT viết theo tuần?
- **Chốt (nguyên văn):** "hiện nhận xét mới nhất".
- **Hệ quả:** HV-07 hiển thị nhận xét PT gần nhất trên một check-in (bữa ăn hoặc buổi tập), kèm tên PT và thời điểm. Không cần bảng hay cột mới; đọc từ `nhat_ky_hoat_dong.nhan_xet` và `ngay_danh_gia` của check-in được đánh giá gần nhất của lộ trình.

### N20. PT-02 thêm phần đặt lịch tập (buổi tập và bài tập)

- **Đề xuất của người dùng (nguyên văn):** "nên có thêm 1 phần set là tập những cái gì nữa".
- **Cách hiểu của Claude, chờ xác nhận:** form tạo lộ trình có thêm một khối **Lịch tập**, nơi PT đặt học viên tập những gì: danh sách buổi tập, mỗi buổi có tên, nhóm cơ, giờ, thời lượng và danh sách bài tập (tên bài, số hiệp x số lần), như nội dung học viên thấy ở HV-06. Khối này thay cho hàng "Buổi tập" cũ trong "Kế hoạch hằng ngày".
- **Hệ quả:** mỗi buổi mở một màn con (PT-02b, `luong-man-hinh.md`). Buổi tập không lặp mỗi ngày như bữa ăn, nên PT phải chỉ ra buổi rơi vào thứ nào trong tuần (xem M14). Lưu vào `muc_ke_hoach` loại `buoi_tap`, danh sách bài tập trong `chi_tiet`.

### N21. Mỗi bữa ăn mở một màn con đặt món

- **Đề xuất của người dùng (nguyên văn):** "Với mỗi bữa dấu > mở ra 1 tab để set các món cho mỗi bữa, bao gồm thời gian ăn, danh sách món, tổng dinh dưỡng".
- **Chốt:** dấu `>` ở từng bữa (sáng, trưa, tối) mở màn con PT-02a gồm: thời gian ăn, danh sách món, tổng dinh dưỡng.
- **Hệ quả:**
  - Thời gian ăn ở PT-02a và giờ ở hàng bữa ăn của PT-02 là một giá trị, đổi một nơi thì cả hai cập nhật.
  - Dữ liệu lưu vào `muc_ke_hoach` loại `bua_an`: giờ ở `gio_du_kien`, món và tổng dinh dưỡng ở `chi_tiet`. HV-04 đọc lại để hiển thị kế hoạch bữa, kcal, P, C, F.
  - Chưa chốt: tổng dinh dưỡng là cộng từ dinh dưỡng từng món hay PT nhập tay (M13).

### N22. Lưu nháp ở lại màn, nháp được nạp lại khi bấm "Tạo lộ trình"

- **Đề xuất của người dùng (nguyên văn):** "Nút lưu nháp sẽ lưu thôi chứ chưa về đâu cả, sau này nếu thoát thì nếu ấn Tạo lộ trình ở PT-01 thì sẽ load nháp nếu có".
- **Chốt:**
  - "Lưu nháp" chỉ lưu, **ở lại PT-02**, không thoát về PT-01.
  - Khi PT thoát và bấm "Tạo lộ trình" ở PT-01 thì PT-02 nạp sẵn bản nháp nếu có.
  - Nháp chỉ PT thấy, học viên không thấy (N12).
- **Cách hiểu của Claude, chờ xác nhận:** mỗi PT có tối đa một bản nháp. Chưa nói rõ làm sao bỏ nháp để làm lộ trình mới (M15).
- **Hệ quả:** thay cho mô tả cũ "Lưu nháp thoát về PT-01, chỗ mở lại nháp chưa có thiết kế". Về nơi lưu nháp có vướng schema (K8).

## 2. Quyết định kỹ thuật

Các mục này suy ra từ quyết định nghiệp vụ ở trên, chưa có câu hỏi riêng. Cần người dùng xác nhận trước khi thiết kế schema.

### K1. Sửa ghi chú cũ trong phan-quyen.md (từ N1)

- `phan-quyen.md` mục 7 đang ghi "Bốn bảng lộ trình và điểm giới thiệu không làm ở V1, nên quyền gắn với `ket_noi_pt`, không có lượt tham gia lộ trình".
- Sau N1, câu đó sai: lộ trình làm ở V1. Lượt tham gia thì không có (N2 sau M2, xem K6). Điểm giới thiệu vẫn không làm.
- Việc cần làm: sửa mục 7 và ghi lại cách kiểm quyền mới (K2).

### K2. Lượt tham gia gắn với kết nối PT và học viên (từ N2, đã bỏ, thay bằng K6)

Không còn áp dụng sau M2: không có lượt tham gia. Quyền vẫn gắn với `ket_noi_pt` như `phan-quyen.md`: dữ liệu lộ trình, check-in và chỉ số mang `ket_noi_pt_id` nên dùng được `BLKiemTraDocDuLieuHocVienAsync` có sẵn (PT chỉ đọc khi là kết nối của chính PT với học viên đó). Việc PT khác xem được lộ trình của PT khác hay không thuộc chức năng chia sẻ làm sau.

### K3. Chu kỳ theo dõi lưu trong lịch của lộ trình (từ N4, chờ xác nhận)

- Cân nặng và ảnh cơ thể mỗi cái là một mục của lịch với chu kỳ riêng (xem K7).
- Nhiệm vụ trong ngày của học viên do hệ thống tính từ lịch này, không lưu cố định từng ngày.

### K4. Giá trị trạng thái bữa ăn (từ N3)

- Cột trạng thái check-in bữa ăn chỉ có hai giá trị. Nếu dùng danh mục thì tạo mục trong `danh-muc.md`, nếu dùng enum thì ghi rõ nơi định nghĩa.
- Cách lưu cụ thể chốt khi thiết kế schema.

### K5. Check-in cũ không đổi nghĩa khi kế hoạch bị sửa (từ N5, chờ xác nhận)

- Sửa lộ trình thì kế hoạch ăn và lịch tập đổi ngay từ hôm sửa. Check-in đã gửi phải còn hiểu được: PT xem lại phải thấy đúng kế hoạch lúc check-in (món, kcal, bài tập), không phải kế hoạch mới.
- Schema đã có sẵn chỗ cho việc này: `nhat_ky_hoat_dong.chi_tiet` là snapshot khẩu phần hoặc bài tập thực tế, `ke_hoach.phien_ban` tăng khi sửa nội dung đã áp dụng (xem mục 3). Khi viết API, check-in phải ghi snapshot mục kế hoạch lúc gửi.
- Sửa lộ trình chỉ ảnh hưởng nhiệm vụ từ hôm sửa trở đi (K3).

### K6. Lộ trình dùng lại `ke_hoach` và `muc_ke_hoach`, không thêm bảng lượt tham gia (từ N2 sau M2, chờ xác nhận)

- Người dùng chọn: mỗi học viên một lộ trình riêng, chia sẻ là sao chép. Vậy giữ `ke_hoach.ket_noi_pt_id` (một học viên, một PT) và không thêm bảng nối.
- **Chỗ còn lệch giữa màn hình và schema, cần sửa migration hoặc chốt cách ánh xạ (M5):**
  - `ke_hoach` chỉ có một `loai_ke_hoach` (`dinh_duong` hoặc `tap_luyen`), nhưng một lộ trình trên màn hình có cả bữa ăn và buổi tập. `muc_ke_hoach.loai_muc` đã có `bua_an` và `buoi_tap`, nên nếu cho một `ke_hoach` chứa cả hai loại mục thì không cần tách hai kế hoạch.
  - `ke_hoach` chưa có mục tiêu (loại mục tiêu, cân nặng đầu và đích), chu kỳ theo dõi cân nặng và ảnh cơ thể (N4), ghi chú cho học viên. Cần thêm cột hoặc một cột JSON có schema.
  - `trang_thai` của `ke_hoach` (`nhap`, `dang_ap_dung`, `tam_dung`, `ket_thuc`) đã đủ cho N12 (kích hoạt ngay) và vòng đời SRS mục 6.1 (bỏ "chờ học viên chấp nhận" và "hủy" nếu không dùng).
  - Cân nặng dùng `chi_so_co_the`; ảnh cơ thể dùng `tep_tin` (cần quy ước loại đối tượng); ghi nhận hoàn thành cân nặng và ảnh theo chu kỳ cần quyết định lưu ở đâu.
- **Sau M5 (N14):** một `ke_hoach` chứa cả mục `bua_an` lẫn `buoi_tap`. Vì vậy `loai_ke_hoach` không còn phân biệt ăn hay tập: bỏ cột, hoặc thêm giá trị kết hợp. Thêm vào `ke_hoach` các cột còn thiếu (mục tiêu, chu kỳ theo dõi, ghi chú cho học viên), chi tiết cột chốt khi viết schema. Không thêm bảng nhóm.
- Chưa sửa migration hay entity; việc đó làm khi bắt đầu viết schema. Các câu M5, M7, M8 đã chốt (N14 đến N17, K7).

### K7. Cân nặng và ảnh cơ thể là hai loại mục của lịch (từ N4 và M7, chờ xác nhận chi tiết)

- Thêm `loai_muc` là `can_nang` và `anh_co_the` vào `muc_ke_hoach`. Mọi nhiệm vụ trong ngày là một mục, đồng nhất với màn "Nhiệm vụ hôm nay".
- **Chu kỳ theo tuần (đã chốt):** thêm cột `chu_ky_tuan` vào `muc_ke_hoach` (số tuần giữa hai lần lặp, mặc định 1, tính từ ngày bắt đầu của kế hoạch, có CHECK lớn hơn 0). "2 lần mỗi tuần" biểu diễn bằng `cac_thu_trong_tuan` do hệ thống sinh theo N17; "mỗi 2 tuần" là `chu_ky_tuan = 2`.
- **Ghi nhận (đã chốt theo N16):** cân nặng và ảnh ghi thẳng vào `chi_so_co_the`, không qua `nhat_ky_hoat_dong`.
  - `chi_so_co_the` đã có `can_nang_kg`; ghi `ket_noi_pt_id` của lộ trình để PT xem được.
  - Ảnh cơ thể cần đích trong `tep_tin.loai_doi_tuong`: thêm giá trị `chi_so_co_the`, `doi_tuong_id` là dòng chỉ số. Ảnh là riêng tư, đọc qua URL ký ngắn hạn sau khi kiểm quyền (thiết kế mục 6).
  - Nhãn "Tuần 1", "Tuần 5" của ảnh theo mốc tính từ `ngay_do` so với ngày bắt đầu lộ trình.
- **Thông báo:** thêm loại `thong_bao` cho nhắc cân nặng và ảnh cơ thể.

### K8. Nháp và ràng buộc `ket_noi_pt_id NOT NULL` của `ke_hoach` (từ N22, chờ xác nhận)

- `ke_hoach.trang_thai` đã có giá trị `nhap`, nhưng `ket_noi_pt_id` bắt buộc. PT chưa chọn học viên thì chưa có kết nối, **nên nháp chưa chọn học viên không lưu được vào `ke_hoach`** như hiện tại.
- Các hướng (chốt khi viết schema, cần người dùng chọn): (a) bắt buộc chọn học viên trước khi "Lưu nháp"; (b) cho `ket_noi_pt_id` NULL khi `trang_thai = nhap`, kèm `nguoi_tao_id` để biết nháp của PT nào; (c) lưu nháp ở thiết bị, không lên server.
- Hướng (b) hợp với "mỗi PT một nháp": ràng buộc duy nhất theo `nguoi_tao_id` với `trang_thai = nhap`.
- Danh sách món và bài tập (N20, N21) nằm trong `muc_ke_hoach.chi_tiet` (JSON); backend phải kiểm tra cấu trúc JSON theo loại mục (thiết kế 21 bảng, mục 4).

## 3. Đối chiếu với tài liệu và schema đã có

Đọc thêm `GymBase_V1_Dac_Ta_Chuc_Nang_SRS.docx` và `GymBase_V1_Thiet_ke_Database_21_bang.docx` (ngày 21/11/2026):

- **SRS khớp N2.** SRS mục 2.2 liệt kê `Program` (Lộ trình), `ProgramParticipant`, `ProgramAssignment` và mục 1.5 nói lộ trình có "người phụ trách và người tham gia". Chưa kiểm chứng được "bốn bảng lộ trình" trong `phan-quyen.md` chính là bốn đối tượng này (ở SRS dòng đó có cả `CoachingRelationship`). Thiết kế 21 bảng không dùng từ "lộ trình" ở chỗ nào, chỉ có `ke_hoach`.
- **SRS khác N3.** FR-CHK-01 ghi ba trạng thái bữa ăn (Đã ăn, Bỏ bữa, Ăn một phần). N3 chốt hai; cần cập nhật SRS hoặc ghi chú lệch.
- **SRS khớp N5.** FR-PRG-04: sửa kế hoạch thì lưu phiên bản thay đổi, nhiệm vụ tương lai đổi, check-in quá khứ không sai lệch, thông báo học viên.
- **Schema 21 bảng đã có trong code và đã áp dụng trên MySQL local** (migration `AddBizTables`), nhưng mô hình kế hoạch khác N2:
  - `ke_hoach` có `ket_noi_pt_id NOT NULL`, nghĩa là mỗi kế hoạch thuộc đúng một học viên qua kết nối PT, và mỗi kế hoạch chỉ một loại (`dinh_duong` hoặc `tap_luyen`), trong khi một lộ trình trên màn hình gồm cả bữa ăn lẫn buổi tập.
  - `muc_ke_hoach` (bữa ăn, buổi tập; thứ trong tuần, giờ, nhắc) thuộc `ke_hoach`.
  - `nhat_ky_hoat_dong` là check-in (`nhap`, `cho_kiem_tra`, `dat`, `can_dieu_chinh`, `hoan_thanh`), có `ket_noi_pt_id`, `muc_ke_hoach_id`, `chi_tiet` (snapshot), `nhan_xet`.
  - `chi_so_co_the` đã dùng cho cân nặng; `lich_su_chinh_sua` là audit log chung; `tep_tin` giữ ảnh.
  - Không có bảng nào chứa mục tiêu, chu kỳ theo dõi hay lượt tham gia.
- **Kết luận:** N2 không đặt được lên schema hiện tại mà không đổi. Cần chốt mô hình dữ liệu (M2).

Đọc lại toàn bộ file thiết kế 21 bảng (21/11/2026), thêm các điểm ảnh hưởng đến lộ trình:

1. **Phạm vi phụ trách của kết nối.** `ket_noi_pt.pham_vi_phu_trach` là mảng `tap_luyen` và/hoặc `dinh_duong`. Một học viên có nhiều PT, mỗi PT chỉ quản lý phạm vi được giao (mục 5.1 và 10). Nghĩa là một PT chỉ phụ trách tập luyện thì không được tạo mục bữa ăn; và học viên có thể có lộ trình tập từ PT này, lộ trình ăn từ PT khác. Khớp SRS 3.2 ("nhiều lộ trình từ nhiều PT"), và ảnh hưởng trực tiếp đến M5.
2. **Chỉ kết nối đang hoạt động mới được lập và sửa kế hoạch** (mục 5.1). PT-02 chỉ cho chọn học viên có kết nối `dang_hoat_dong`. Kết nối kết thúc thì lộ trình chỉ xem lịch sử.
3. **Cân nặng và ảnh cơ thể chưa có chỗ trong lịch.**
   - `muc_ke_hoach.loai_muc` chỉ có `bua_an` và `buoi_tap`, nên chu kỳ theo dõi (N4) không có nơi lưu trong `muc_ke_hoach`.
   - `nhat_ky_hoat_dong.loai_hoat_dong` chỉ có `an_uong` và `tap_luyen`; `thong_bao.loai_thong_bao` chưa có loại nhắc cân nặng hay ảnh.
   - `chi_so_co_the` lưu cân nặng nhưng không có ảnh. `tep_tin.loai_doi_tuong` (`bai_viet`, `tin_nhan`, `nhat_ky_hoat_dong`, `ho_so_nguoi_dung`) chưa có đích cho ảnh cơ thể.
4. **Trạng thái check-in có sẵn:** `nhat_ky_hoat_dong.trang_thai` là `nhap`, `cho_kiem_tra`, `dat`, `can_dieu_chinh`, `hoan_thanh`. Ánh xạ gợi ý với màn hình: Chờ duyệt là `cho_kiem_tra`, Duyệt là `dat`, Yêu cầu bổ sung là `can_dieu_chinh`; `hoan_thanh` chưa rõ dùng khi nào (SRS không có tương ứng).
5. **Kết quả "Đã ăn / Bỏ bữa" và "Đã tập / Tập một phần / Nghỉ tập" chưa có cột.** Phải lưu trong `nhat_ky_hoat_dong.chi_tiet` (JSON) hoặc thêm cột. Chỉ có một `nhan_xet` mới nhất; lịch sử nhận xét nằm ở `lich_su_chinh_sua`.
6. **Nhắc giờ đã có sẵn:** `muc_ke_hoach.co_nhac` và `nhac_truoc_phut` trả lời câu SRS "thời điểm nhắc trước giờ"; worker sinh `thong_bao` trong cửa sổ 24 giờ và phải hủy hoặc lập lại thông báo chưa gửi khi kế hoạch đổi (mục 5.4).
7. **`ke_hoach.phien_ban`** tăng khi sửa nội dung đã áp dụng, nhưng `muc_ke_hoach` không có phiên bản và sửa tại chỗ; lịch sử được bảo vệ bằng snapshot trong `nhat_ky_hoat_dong.chi_tiet` (xem K5).
8. **Thiết kế viết cho PostgreSQL** (`jsonb`, partial index); dự án dùng MySQL 8.4 và đã chuyển đổi trong migration `AddBizTables`.

## 4. Còn mở

Các câu hỏi chưa hỏi hoặc chưa trả lời:

| # | Câu hỏi | Liên quan |
| --- | --- | --- |
| M6 | Xác nhận K3, K5, K6, K7 và cập nhật SRS: N3 (hai trạng thái bữa ăn) khác FR-CHK-01 (ba trạng thái); N16 (cân nặng, ảnh không qua duyệt) khác SRS FR-PRG-06 nếu SRS coi chúng là check-in. | K3, K5, K6, K7 |
| M9 | Luồng mở rộng phạm vi kết nối PT sang đủ cả hai phạm vi (N15): thuộc module kết nối PT, cần thiết kế riêng. | N15 |
| M12 | Học viên đã có một lộ trình đang chạy với cùng PT thì PT có được tạo lộ trình thứ hai không (hay phải kết thúc lộ trình cũ trước)? | N18 |
| M13 | PT-02a: tổng dinh dưỡng là cộng từ từng món (mỗi món có kcal, P, C, F) hay PT nhập tay? Món có ảnh và mô tả không (HV-04 hiển thị ảnh món)? | N21 |
| M14 | PT-02b: buổi tập lặp vào những thứ nào trong tuần (PT chọn từng buổi), bao nhiêu buổi mỗi tuần, một buổi có nhiều bài tập thế nào (tên bài lấy từ danh sách có sẵn hay PT tự gõ)? | N20 |
| M15 | Nháp: mỗi PT một hay nhiều bản; muốn tạo lộ trình mới khi đang có nháp thì có nút "Bắt đầu lại / xóa nháp" không; nháp lưu ở server hay thiết bị (K8). | N22, K8 |

Đã chốt từ các câu hỏi trước: M1 (N2 và N8 xác nhận ngày 21/11/2026), M2 (N2 sau sửa), M3 (N12), M4 (N13), M5 (N14), M7 (N16, N17, K7), M8 (N15), M10 (N19), M11 (N18).

Mâu thuẫn thiết kế số 1 đến 9 ở `luong-man-hinh.md` mục 7 đã chốt hết (N2 đến N11, thêm N14 đến N17); số 10 (số liệu mẫu) không cần chốt.
