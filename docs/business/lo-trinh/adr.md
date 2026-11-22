---
status: in-progress
---

# Lộ trình: các quyết định đã chốt và còn mở

Ngày 21/11/2026, cập nhật 22/11/2026 (thêm thông tin từ sếp, mục 1b). Ghi lại từng câu hỏi trong lúc chốt phạm vi và mâu thuẫn thiết kế của màn Lộ trình (xem
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
- **Sửa ngày 22/11/2026 (theo N24):** sếp yêu cầu **lộ trình nhóm** (một lộ trình, nhiều học viên). Phần "không có bảng lượt tham gia, mỗi học viên một lộ trình riêng" **không còn đúng** cho lộ trình nhóm; cần có lượt tham gia, tức quay lại cách hiểu ban đầu của N2. Xem N24 và K9.

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
- **Bị thay thế ngày 22/11/2026 (theo N23):** sếp yêu cầu học viên phải chấp nhận lời mời mới tham gia. Quyết định N12 (kích hoạt ngay) **bỏ**; "Lưu nháp" vẫn giữ.

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
  - Bổ sung 22/11/2026 (N33): ảnh cơ thể có chế độ riêng tư, học viên chọn "chỉ mình tôi" hoặc cho PT xem.

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
- **Bị thay thế ngày 22/11/2026 (theo N24):** có lộ trình nhóm nên chọn được nhiều học viên; lộ trình đơn vẫn một học viên. Giới hạn số lộ trình đang hoạt động: xem N36 và M12.

### N19. "Đánh giá từ PT" ở tiến độ là nhận xét mới nhất

- **Câu hỏi:** "Đánh giá từ PT" ở HV-07 là nhận xét của check-in mới nhất hay một đánh giá tổng quan riêng do PT viết theo tuần?
- **Chốt (nguyên văn):** "hiện nhận xét mới nhất".
- **Hệ quả:** HV-07 hiển thị nhận xét PT gần nhất trên một check-in (bữa ăn hoặc buổi tập), kèm tên PT và thời điểm. Không cần bảng hay cột mới; đọc từ `nhat_ky_hoat_dong.nhan_xet` và `ngay_danh_gia` của check-in được đánh giá gần nhất của lộ trình.

### N20. PT-02 thêm phần đặt lịch tập (buổi tập và bài tập)

- **Đề xuất của người dùng (nguyên văn):** "nên có thêm 1 phần set là tập những cái gì nữa".
- **Cách hiểu của Claude, chờ xác nhận:** form tạo lộ trình có thêm một khối **Lịch tập**, nơi PT đặt học viên tập những gì: danh sách buổi tập, mỗi buổi có tên, nhóm cơ, giờ, thời lượng và danh sách bài tập (tên bài, số hiệp x số lần), như nội dung học viên thấy ở HV-06. Khối này thay cho hàng "Buổi tập" cũ trong "Kế hoạch hằng ngày".
- **Hệ quả:** mỗi buổi mở một màn con (PT-02b, `luong-man-hinh.md`). Buổi tập không lặp mỗi ngày như bữa ăn, nên PT phải chỉ ra buổi rơi vào thứ nào trong tuần (xem M14). Lưu vào `muc_ke_hoach` loại `buoi_tap`, danh sách bài tập trong `chi_tiet`.
- **Bổ sung 22/11/2026 (N29):** sếp nói rõ lịch tập gồm khung thời gian trong tuần, giờ, bài tập, video hoặc ảnh minh họa, và lặp theo số tuần. M14 đã trả lời phần lớn; xem N29.

### N21. Mỗi bữa ăn mở một màn con đặt món

- **Đề xuất của người dùng (nguyên văn):** "Với mỗi bữa dấu > mở ra 1 tab để set các món cho mỗi bữa, bao gồm thời gian ăn, danh sách món, tổng dinh dưỡng".
- **Chốt:** dấu `>` ở từng bữa (sáng, trưa, tối) mở màn con PT-02a gồm: thời gian ăn, danh sách món, tổng dinh dưỡng.
- **Hệ quả:**
  - Thời gian ăn ở PT-02a và giờ ở hàng bữa ăn của PT-02 là một giá trị, đổi một nơi thì cả hai cập nhật.
  - Dữ liệu lưu vào `muc_ke_hoach` loại `bua_an`: giờ ở `gio_du_kien`, món và tổng dinh dưỡng ở `chi_tiet`. HV-04 đọc lại để hiển thị kế hoạch bữa, kcal, P, C, F.
  - Chưa chốt: tổng dinh dưỡng là cộng từ dinh dưỡng từng món hay PT nhập tay (M13).
  - Cập nhật 22/11/2026 (N30): sếp nói calo bữa ăn do PT **nhập tay**, không cần công thức tính; có thêm ghi chú món ưu tiên. M13 đã chốt.

### N22. Lưu nháp ở lại màn, nháp được nạp lại khi bấm "Tạo lộ trình"

- **Đề xuất của người dùng (nguyên văn):** "Nút lưu nháp sẽ lưu thôi chứ chưa về đâu cả, sau này nếu thoát thì nếu ấn Tạo lộ trình ở PT-01 thì sẽ load nháp nếu có".
- **Chốt:**
  - "Lưu nháp" chỉ lưu, **ở lại PT-02**, không thoát về PT-01.
  - Khi PT thoát và bấm "Tạo lộ trình" ở PT-01 thì PT-02 nạp sẵn bản nháp nếu có.
  - Nháp chỉ PT thấy, học viên không thấy (N12).
- **Cách hiểu của Claude, chờ xác nhận:** mỗi PT có tối đa một bản nháp. Chưa nói rõ làm sao bỏ nháp để làm lộ trình mới (M15).
- **Hệ quả:** thay cho mô tả cũ "Lưu nháp thoát về PT-01, chỗ mở lại nháp chưa có thiết kế". Về nơi lưu nháp có vướng schema (K8).

## 1b. Bổ sung từ sếp (22/11/2026)

Nguồn: hai đoạn yêu cầu người dùng chuyển lại từ sếp ngày 22/11/2026. Đây là yêu cầu của người ra quyết định nên ghi là đã
chốt về nghiệp vụ; những chỗ mâu thuẫn với N1 đến N22 hoặc với schema hiện có ghi rõ ở từng mục và đưa vào mục 4.
Chi tiết giao diện chưa viết vào `luong-man-hinh.md`.

### N23. Học viên phải chấp nhận lời mời mới tham gia lộ trình

- **Yêu cầu:** PT tạo lộ trình rồi thêm học viên; học viên chấp nhận thì mới tham gia.
- **Hệ quả:** thay N12. Cần trạng thái lời mời (chờ chấp nhận, đã chấp nhận, từ chối) và màn lời mời cho học viên; học viên
  nhận thông báo khi được mời. Lộ trình chỉ tính nhiệm vụ, check-in cho học viên đã chấp nhận.
- **Mâu thuẫn:** N12 ("kích hoạt luôn, học viên đã đồng ý khi kết nối PT") bị bỏ; trạng thái `ke_hoach` (K6) phải xét lại vì
  lời mời thuộc từng học viên, không thuộc cả lộ trình (M16).

### N24. Có hai loại lộ trình: đơn và nhóm

- **Yêu cầu:** khi bấm tạo lộ trình chọn **lộ trình đơn** (quản lý một người) hoặc **lộ trình nhóm** (nhiều người chung một
  lộ trình).
- **Hệ quả:**
  - Lộ trình nhóm là một thực thể chung, nhiều học viên tham gia; check-in và tiến độ vẫn tính riêng từng học viên (khớp N2
    gốc). Vì vậy cần bảng lượt tham gia (lộ trình, học viên, trạng thái lời mời, ngày vào, ngày ra).
  - Lộ trình đơn có thể dùng cùng cấu trúc với một lượt tham gia, để màn hình và API không tách hai đường (M17).
  - PT-02 cho chọn nhiều học viên khi là lộ trình nhóm, một học viên khi là lộ trình đơn.
- **Mâu thuẫn:** N2 sau M2, N18 và K6 (không có bảng nối, mỗi học viên một lộ trình, chia sẻ là sao chép) không còn đúng.
  Xem K9.

### N25. Chỉ người đã kết bạn mới hiện trong danh sách để PT mời

- **Yêu cầu:** danh sách chọn học viên chỉ gồm người kết bạn với PT.
- **Mâu thuẫn:** N15 và `phan-quyen.md` đang dùng **kết nối PT** (`ket_noi_pt` đang hoạt động, đủ phạm vi `tap_luyen` và
  `dinh_duong`) làm điều kiện. "Kết bạn" và "kết nối PT" có phải cùng một thứ không, và N15 (bắt buộc đủ hai phạm vi) còn áp
  dụng không: chưa rõ (M18).

### N26. Danh sách chọn người dùng phải tải dần và phân trang

- **Yêu cầu:** trang chọn người dùng dùng lazy load kèm phân trang.
- **Hệ quả:** API danh sách bạn của PT nhận `PagingRequest` có sẵn trong `apps/api/src/Base`, có ô tìm theo tên; mobile cuộn
  vô hạn. Áp dụng cho cả lộ trình đơn và nhóm.

### N27. Màn "Lộ trình tôi quản lý": lọc, mặc định tháng này và đang hoạt động

- **Yêu cầu:** một PT quản lý nhiều lộ trình. Có bộ lọc theo trạng thái (đang hoạt động, đã hoàn thành, thất bại) và theo
  tháng; mặc định là tháng hiện tại và đang hoạt động.
- **Mục đích:** PT thấy tháng này đang dạy bao nhiêu người, bao nhiêu lộ trình, lộ trình nào sắp hết hạn.
- **Hệ quả:**
  - Cần ngày bắt đầu và ngày kết thúc của lộ trình để lọc và để biết "sắp hết".
  - Màn hiển thị tổng số lộ trình đang hoạt động và (với lộ trình nhóm) số người; cần ngưỡng "sắp hết" (M20).
  - API danh sách có tham số trạng thái, tháng và phân trang.

### N28. Đóng lộ trình trước thời hạn

- **Yêu cầu:** PT đóng lộ trình trước hạn; trạng thái chuyển sang hoàn thành hoặc thất bại; có hộp xác nhận và bắt buộc
  nhập lý do.
- **Hệ quả:** lưu lý do, người đóng và thời điểm đóng; thông báo cho học viên. Sau khi đóng, lộ trình chỉ xem. PT chọn hoàn
  thành hay thất bại khi đóng, hay hệ thống tự suy ra: chưa rõ (M19).
- **Mâu thuẫn:** `ke_hoach.trang_thai` hiện có `nhap`, `dang_ap_dung`, `tam_dung`, `ket_thuc`; chưa có "thất bại" (K9).

### N29. Lịch tập: số tuần, khung giờ trong tuần, tuần đầu tự điền cho các tuần sau

- **Yêu cầu của sếp, tổng hợp từ hai đoạn:**
  - Chọn khung thời gian trong tuần; mỗi buổi có ca tập mấy giờ, bài tập gì, có video hướng dẫn hoặc ảnh minh họa hay không.
  - Số tuần của lịch lấy từ độ dài lộ trình. PT lập tuần 1 thì các tuần sau tự điền giống tuần 1; PT có thể sang từng tuần
    để sửa riêng; gắn được video.
  - Học viên phải xem được trước khi check-in: tuần này tập những buổi nào, ngày đó mấy giờ, tập bài gì, video hoặc ảnh.
- **Hệ quả:** trả lời phần lớn M14 (lặp theo thứ trong tuần, PT chọn từng buổi). Cần lưu **theo từng tuần** vì tuần sau sửa
  riêng được, không chỉ "mẫu tuần". Video và ảnh dùng `tep_tin` (cần giá trị `loai_doi_tuong` cho bài tập, K7). Quy tắc khi PT
  sửa tuần 1 sau khi đã sửa riêng tuần khác chưa rõ (M21).
- **Mâu thuẫn:** `muc_ke_hoach` hiện lặp theo `cac_thu_trong_tuan` mỗi tuần, không có chỗ cho tuần khác nhau; xem K10.

### N30. Lịch ăn: ca, giờ, calo nhập tay, ghi chú món ưu tiên

- **Yêu cầu:** PT đặt ăn những ca nào, mấy giờ, số calo của bữa đó (nhập tay, không cần công thức), kèm ghi chú món ưu tiên.
- **Hệ quả:** chốt M13: calo là số PT nhập, không cộng từ món; không cần bảng món và dinh dưỡng từng món. Cột P, C, F ở màn
  HV-04 hiện chưa rõ còn dùng không (M22). Ghi chú gắn mỗi bữa; giới hạn ký tự thống nhất với N6.

### N31. Học viên được xem trước chi tiết khi đến giờ check-in

- **Yêu cầu:** đến giờ có thông báo; vào check-in thì xem được những gì PT thiết lập ở mục cần check-in (bữa ăn: giờ, calo,
  ghi chú; buổi tập: bài, video, ảnh).
- **Hệ quả:** màn check-in đọc chi tiết từ mục kế hoạch của đúng ngày; snapshot khi gửi vẫn theo K5.

### N32. Quyền sửa: lộ trình tôi quản lý chỉ PT sửa; lộ trình của tôi thì chủ lộ trình sửa

- **Yêu cầu:** lộ trình trong "Lộ trình tôi quản lý" chỉ PT tạo ra được sửa và thao tác. Lộ trình trong "Lộ trình của tôi"
  thì ai là chủ lộ trình đều được sửa.
- **Hệ quả:** học viên tham gia lộ trình của PT chỉ xem và check-in, không sửa kế hoạch. Cần xác định "chủ lộ trình" là một
  cột của lộ trình. Câu "ai là chủ lộ trình" gợi ý có lộ trình học viên tự tạo cho mình, điều N1 chưa nói (M23).

### N33. Ảnh cơ thể có chế độ riêng tư

- **Yêu cầu:** ảnh cơ thể theo dõi định kỳ chọn được "chỉ mình tôi" hoặc cho cả PT xem.
- **Hệ quả:** thêm cờ hiển thị cho ảnh; kiểm quyền đọc ảnh theo cờ này ngoài kết nối PT (K7 nói ảnh là riêng tư, đọc qua URL
  ký). Mặc định nên là "chỉ mình tôi" (chờ xác nhận, M24). Đổi cờ sau khi đã đăng có hiệu lực ngay.

### N34. Chọn ngày có hiển thị thứ

- **Yêu cầu:** bộ chọn ngày (DatePicker) hiện thứ trong tuần.
- **Hệ quả:** dùng cho ngày bắt đầu và kết thúc của lộ trình; thuộc phần giao diện, không ảnh hưởng schema.

### N35. Màn tạo lộ trình theo yêu cầu sếp

- **Yêu cầu:** ngày bắt đầu và ngày kết thúc bắt buộc (N27); loại lộ trình đơn hoặc nhóm (N24); lịch tập (N29); lịch ăn (N30);
  chọn học viên (N25, N26).
- **Hệ quả:** thứ tự và bố cục màn chưa viết lại; làm trong `luong-man-hinh.md` sau khi chốt các câu mở.

### N36. Chuẩn bị cho V2: gói Free giới hạn 5 lộ trình đang hoạt động mỗi PT

- **Yêu cầu:** gói Free chỉ cho một PT tạo tối đa 5 lộ trình đang hoạt động; V2 mới làm.
- **Hệ quả:** V1 chưa giới hạn, nhưng nên gom việc đếm lộ trình đang hoạt động theo PT vào một hàm để V2 bổ sung mà không sửa
  nhiều nơi. Schema chưa có khái niệm gói. Đề xuất cho M12 ở V1: không chặn.

### N37. PT-02a, PT-02b: bài tập theo buổi và theo tuần, bữa ăn lặp mỗi ngày

- **Nguồn:** người dùng hỏi cách PT đặt món và bài tập (22/11/2026), Claude đề xuất, người dùng đồng ý ("ok"). Chưa có bản thiết kế
  chính thức; app đã dựng theo đề xuất này.
- **Chốt:**
  - Bài tập gắn vào **buổi tập của từng tuần**. Sửa Tuần 1 thì các tuần **chưa sửa riêng** đổi theo, tuần đã sửa riêng giữ nguyên
    (đề xuất cho M21). Sửa ở tuần khác thì tách riêng tuần đó (N29).
  - Bữa ăn **lặp mỗi ngày**; V1 không có thực đơn khác nhau theo ngày.
  - Mỗi bữa gồm: giờ ăn, một ô "mô tả món ăn" kèm ghi chú món ưu tiên (tối đa 300 ký tự, N6), năng lượng nhập tay (kcal, N30),
    một ảnh tùy chọn. **Không có P, C, F** (đề xuất cho M22; màn học viên đã bỏ các cột này).
  - Mỗi bài tập gồm: tên (PT tự gõ, trả lời một phần M14), số set, số lần (chữ tự do như "8–10 rep" hoặc "45 giây"), một ảnh
    minh họa tùy chọn. Đổi thứ tự và xóa được. **Video chưa làm** (N29 nhắc tới video).
- **Hệ quả:** lưu trong `muc_ke_hoach.chi_tiet` (JSON) theo từng tuần (K10); ảnh dùng `tep_tin` (K7). Học viên đọc lại đúng các giá
  trị này ở HV-04 và HV-06 (N31).

### N38. PT-03: công việc hôm nay là của cả lộ trình, trạng thái là của cả nhóm

- **Nguồn:** người dùng chốt ngày 22/11/2026 khi làm màn chi tiết lộ trình của PT: "phần công việc hôm nay là của chung cả nhóm,
  không phải của cá nhân; trạng thái là của cả nhóm; thành viên nào check-in rồi thì ở task đó có avatar nhỏ".
- **Chốt:**
  - Danh sách "Công việc hôm nay" **chung** cho cả lộ trình; lộ trình nhóm không có danh sách riêng từng người.
  - Trạng thái nhiệm vụ là của cả nhóm: **Đã hoàn thành** khi mọi thành viên *đang tham gia* đã check-in (lộ trình đơn là một
    người); chưa thì theo mốc giờ: Chờ check-in (đến giờ), Sắp tới giờ, Ngày mai, Quá hạn.
  - Thành viên đã check-in hiện bằng ảnh đại diện nhỏ xếp chồng ở dòng nhiệm vụ (tối đa 3, nhiều hơn thì "+n"), chỉ ở lộ trình
    nhóm. Dòng phụ ghi "n/m đã check-in" khi mới có một phần.
  - Người chưa chấp nhận lời mời (N23), đã từ chối hoặc đã rời **không chặn** việc hoàn thành và không được tính vào "m".
  - Check-in chờ duyệt vẫn **từng người một** vì PT duyệt riêng từng bản (ghi tên học viên ở mỗi dòng). Tab Tiến độ có chọn thành
    viên vì cân nặng và ảnh là của từng người.
  - Đóng lộ trình: PT chọn Hoàn thành hoặc Thất bại và bắt buộc nhập lý do (N28; đề xuất cho M19, người dùng đồng ý).
    Lộ trình đã đóng chỉ xem.
- **Hệ quả:** hoàn thành của nhiệm vụ nhóm phải suy ra từ tập thành viên đang tham gia (bảng lượt tham gia, K9), không lưu
  một cột trạng thái riêng cho nhiệm vụ nhóm. Ảnh cơ thể chỉ hiện cho PT khi học viên để chế độ "Cho PT xem" (N33).

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

### K9. Lộ trình nhóm và lời mời cần bảng lượt tham gia, kéo theo sửa K6 (từ N23, N24, N28, chờ xác nhận)

- Thêm bảng lượt tham gia: `ke_hoach_id`, học viên (qua `ket_noi_pt_id` hoặc `tai_khoan_id` tùy M18), `trang_thai` lời mời
  (`cho_chap_nhan`, `tham_gia`, `tu_choi`), thời điểm mời, trả lời, rời. Lộ trình đơn là lộ trình có đúng một lượt tham gia.
- `ke_hoach.ket_noi_pt_id` (NOT NULL, một học viên) không còn phù hợp cho lộ trình nhóm; chuyển sang gắn PT (người tạo) ở
  `ke_hoach`, còn kết nối PT nằm ở lượt tham gia. `nhat_ky_hoat_dong.ket_noi_pt_id` vẫn giữ để dùng lại kiểm quyền có sẵn.
- `trang_thai` của `ke_hoach` thêm giá trị thất bại, cộng cột lý do đóng, người đóng, thời điểm đóng (N28), ngày bắt đầu,
  ngày kết thúc.
- Đảo K6 phần "không thêm bảng nối" và N5 phần "sửa lộ trình không ảnh hưởng học viên khác": sửa lộ trình nhóm áp dụng cho
  mọi người tham gia từ hôm sửa.

### K10. Lịch tập theo từng tuần (từ N29, chờ xác nhận)

- `muc_ke_hoach` lặp theo thứ nên không đủ cho tuần khác nhau. Hướng: thêm số thứ tự tuần vào mục buổi tập (một dòng mỗi
  buổi mỗi tuần, tạo bằng cách sao chép tuần 1 khi lập), hoặc cột tuần cho mục; chốt khi viết schema.
- Video và ảnh bài tập dùng `tep_tin`; ảnh cơ thể có cờ riêng tư (N33) và không dùng chung quy tắc với ảnh bài tập.

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
| M12 | Học viên đã có một lộ trình đang chạy với cùng PT thì PT có được tạo lộ trình thứ hai không? Sếp chỉ nói giới hạn 5 lộ trình đang hoạt động ở gói Free (N36); đề xuất V1 không chặn. | N36 |
| M14 | PT-02b còn lại: tên bài lấy từ danh sách có sẵn hay PT tự gõ? (Lặp theo thứ và theo tuần đã trả lời ở N29.) V1 đang cho PT tự gõ (N37), chưa có danh sách bài có sẵn. | N20, N29, N37 |
| M15 | Nháp: mỗi PT một hay nhiều bản; muốn tạo lộ trình mới khi đang có nháp thì có nút "Bắt đầu lại / xóa nháp" không; nháp lưu ở server hay thiết bị (K8). | N22, K8 |
| M16 | Lộ trình nhóm: một người từ chối lời mời thì lộ trình vẫn chạy với những người còn lại? Có mời thêm người khi lộ trình đã chạy không, người vào muộn bắt đầu từ tuần nào? Học viên rời giữa chừng thì sao? | N23, N24 |
| M17 | Lộ trình đơn và nhóm dùng chung một cấu trúc (đơn là nhóm một người) hay tách hai loại? | N24, K9 |
| M18 | "Kết bạn" và "kết nối PT" có phải cùng một thứ không? Nếu khác thì kết bạn là tính năng nào (chưa có trong schema); N15 (đủ hai phạm vi) còn áp dụng không? | N25, N15 |
| M19 | Đóng sớm: PT tự chọn "hoàn thành" hay "thất bại", hay hệ thống suy ra? Hết hạn mà không đóng thì tự chuyển trạng thái gì? | N28 |
| M20 | Lộ trình "sắp hết" là còn bao nhiêu ngày? | N27 |
| M21 | Sửa tuần 1 sau khi đã sửa riêng tuần khác: ghi đè các tuần sau hay chỉ tuần chưa sửa riêng? Đang làm theo "chỉ tuần chưa sửa riêng" (N37), chờ xác nhận. | N29, N37 |
| M22 | Calo bữa ăn nhập tay: còn hiển thị P, C, F (đạm, tinh bột, béo) không, hay chỉ calo? Đang làm chỉ calo (N37), chờ xác nhận. | N30, N37 |
| M23 | "Lộ trình của tôi, ai là chủ đều được sửa": học viên tự tạo lộ trình cho mình được không (N1 chỉ nói PT tạo)? Nếu được thì khác gì lộ trình PT tạo? | N32 |
| M24 | Ảnh cơ thể mặc định "chỉ mình tôi"; chế độ chọn theo từng ảnh hay một cài đặt chung cho lộ trình? | N33 |

Đã chốt từ các câu hỏi trước: M13 (N30, ngày 22/11/2026), M1 (N2 và N8 xác nhận ngày 21/11/2026), M2 (N2 sau sửa), M3 (N12), M4 (N13), M5 (N14), M7 (N16, N17, K7), M8 (N15), M10 (N19), M11 (N18).

Mâu thuẫn thiết kế số 1 đến 9 ở `luong-man-hinh.md` mục 7 đã chốt hết (N2 đến N11, thêm N14 đến N17); số 10 (số liệu mẫu) không cần chốt.
