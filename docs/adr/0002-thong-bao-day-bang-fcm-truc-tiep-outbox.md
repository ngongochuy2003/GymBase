---
status: accepted
---

# Thông báo đẩy: FCM trực tiếp, ghi hộp thư trước rồi worker gửi, at-least-once

Ngày 21/11/2026. Thông báo (nhắc ăn, nhắc tập, tin nhắn, kết nối PT) là phần quan trọng của app. Push của Google và Apple là
best-effort: thiết bị tắt máy, mất mạng, người dùng tắt quyền hoặc hãng Android giết app nền đều làm push không tới. Vì vậy
quyết định này không tìm cách "bảo đảm push tới", mà thiết kế để push rớt không làm mất thông tin nghiệp vụ.

Các quyết định:

1. **Push chỉ là cái chuông, `thong_bao` là nguồn sự thật.** Người dùng mở app là thấy đủ thông báo, kể cả cái push không
   tới. App gọi API lấy thông báo chưa đọc mỗi lần mở app hoặc quay lại foreground.
2. **Gửi qua FCM trực tiếp, không dùng Expo Push Service.** Bỏ một bên trung gian, có mã lỗi gốc của FCM để xử lý chính xác,
   không phụ thuộc hạn mức của bên thứ ba. iOS đi qua FCM nên backend không nói chuyện với APNs; chỉ cần upload APNs Auth Key
   (.p8) lên Firebase. Mobile lấy token bằng `getDevicePushTokenAsync` của `expo-notifications` (token FCM, không phải
   `ExponentPushToken`), khớp với cột `thiet_bi.push_token`.
3. **Ghi hộp thư trước, gửi sau (outbox).** Dòng `thong_bao` được tạo trong cùng transaction với sự kiện nghiệp vụ sinh ra nó.
   Một worker nền quét `thong_bao` có `trang_thai_gui = cho_gui` và `thoi_diem_du_kien <= now` rồi mới gọi FCM. API sập giữa
   chừng thì không mất thông báo.
4. **Giao ít nhất một lần (at-least-once), chống trùng.** Thà gửi hai lần còn hơn không gửi. Phía server chống lập bằng
   `ma_chong_trung` (unique cùng `tai_khoan_id`); phía app bỏ trùng theo `thong_bao.Id`. Nhiều instance API thì worker phải
   giành dòng bằng cập nhật có điều kiện `cho_gui` sang `dang_gui` trước khi gửi, để hai worker không gửi cùng một dòng.
5. **Phân loại lỗi FCM khi retry.**
   - Lỗi tạm (429, 5xx, `UNAVAILABLE`): thử lại, backoff tăng dần, có giới hạn số lần; hết lần thì `trang_thai_gui = loi`.
   - Token chết (`UNREGISTERED`, `INVALID_ARGUMENT` do token): đặt `thiet_bi.dang_hoat_dong = false` và xóa `push_token`,
     không thử lại.
   - Lỗi cấu hình (`SENDER_ID_MISMATCH`, thiếu quyền service account): không thử lại vô hạn, ghi log mức lỗi để cảnh báo.
6. **Gửi tới mọi thiết bị đang hoạt động của tài khoản.** Đăng ký thiết bị là upsert theo `(tai_khoan_id, ma_thiet_bi)`.
   `push_token` đang unique, nên khi một token chuyển sang tài khoản khác thì gỡ khỏi bản ghi cũ trước. Đăng xuất phải gỡ
   token của thiết bị đó khỏi tài khoản, nếu không người dùng kế tiếp trên máy cũ nhận thông báo của người trước.
7. **Payload nhỏ, trung tính.** Chỉ gửi `thong_bao.Id`, `loai_thong_bao`, `loai_doi_tuong`, `doi_tuong_id` và tiêu đề ngắn; chi
   tiết app tự lấy qua API. Lý do: dữ liệu sức khỏe là nhạy cảm, nội dung hiện trên màn hình khóa và đi qua máy chủ Google và
   Apple. Dùng `notification` message kèm `data` để có deep link; không dựa vào `data-only` trên iOS.
8. **Đặt `priority` và `ttl` theo loại.** Thông báo có giờ giấc (nhắc tập) dùng `high` và TTL ngắn, quá giờ thì thôi, không
   giao muộn. Loại ít gấp dùng `normal`.
9. **Giữ token luôn mới.** App gửi lại token khi đăng nhập, khi mở app và khi `addPushTokenListener` báo token đổi.
10. **Quan sát được.** Log và đếm số gửi, thành công, token chết, retry, lỗi; cảnh báo khi tỷ lệ lỗi tăng đột biến.
11. **`IPushSender` là seam.** FCM nằm sau một interface để thay hướng và test bằng bản giả; test thật chạy trên máy thật, vì
    iOS Simulator không nhận push và Expo Go không hỗ trợ push từ SDK 53.
12. **Nhắc theo giờ cố định dùng local notification, không push.** `nhac_tap` và `nhac_an` do app tự đặt lịch trên máy, vì
    lịch ít đổi và local chạy không cần mạng, không phụ thuộc FCM hay APNs. Server không push loại này. Push dành cho thứ
    phát sinh bất ngờ: đổi lịch, tin nhắn, kết nối PT, đánh giá.
13. **Hai mốc nhắc cố định cho mỗi sự kiện: 15 phút trước và đúng lúc bắt đầu.** Người dùng không chỉnh được giờ nhắc.
    Mốc đã qua thì bỏ qua (sự kiện bắt đầu trong vòng 15 phút kể từ lúc đặt lịch chỉ có mốc đúng lúc).
14. **Lịch local là bản sao có hạn dùng của lộ trình trên server, đồng bộ bằng cách tính lại toàn bộ (idempotent).** Hàm
    đồng bộ lấy lộ trình, tính tập nhắc cần có, rồi thêm, sửa, xóa cho khớp với những gì đã đặt trên máy. Mỗi nhắc có
    identifier cố định, ví dụ `nhac_tap:{buoi_tap_id}:{yyyy-MM-dd}:truoc15` và `...:dung_luc`, nên đặt lại thì thay thế chứ
    không nhân đôi; chỉ chạm vào nhắc có tiền tố của mình. Sự kiện bị hủy hoặc đã hoàn thành thì hủy cả hai mốc. Server trả kèm
    số phiên bản của lộ trình; app lưu phiên bản đã đồng bộ và chỉ tính lại khi khác. Chạy đồng bộ khi đăng nhập, mỗi lần mở
    app hoặc quay lại foreground, khi nhận push "lịch đã đổi", ngay sau khi người dùng sửa lịch, và khi quyền thông báo đổi.
15. **Chỉ đặt local trong một cửa sổ ngắn, tính theo số lượng chứ không chỉ theo ngày.** iOS giới hạn khoảng 64 lịch đang chờ
    và mỗi sự kiện tốn hai lịch (nhắc ăn nhiều bữa mỗi ngày dễ chạm giới hạn). Đặt các nhắc gần nhất, tối đa khoảng 50, trong
    khoảng 7 đến 14 ngày tới, nạp lại mỗi lần mở app. Cửa sổ ngắn cũng giới hạn thời gian lịch cũ còn bắn sai khi push
    "lịch đã đổi" bị rớt. Nội dung nhắc trung tính ("Sắp đến giờ tập, mở app để xem chi tiết"), không ghi cứng giờ hay bài tập.
16. **`nhac_tap` và `nhac_an` là loại không được phép rớt.** Chúng chạy local (quyết định 12), nên "không rớt" nghĩa là lịch
    local phải đúng và còn hiệu lực, và khi không đảm bảo được thì phải có phương án dự phòng (xem điểm còn mở). Push "lịch
    đã đổi" từ PT cũng dùng ưu tiên cao vì nó giữ lịch local khỏi lệch. Người dùng tự tắt một loại là ý của họ, không tính là rớt.
17. **Hộp thư trong app vẫn có nhắc tập, nhắc ăn.** Server vẫn tạo dòng `thong_bao` cho hai loại này nhưng đánh dấu kênh
    local, không gửi push (cần thêm một cột kênh hoặc một giá trị `TrangThaiGui`). Dòng chỉ hiện trong hộp thư từ
    `thoi_diem_du_kien` trở đi, không hiện sớm.
18. **Múi giờ là múi giờ của máy học viên.** Nhắc local dùng giờ của máy. Giờ trong lịch tập lưu theo giờ địa phương, không
    gắn múi giờ. Khi server cần đổi sang UTC (tính `thoi_diem_du_kien` cho hộp thư, hoặc push dự phòng) thì dùng múi giờ IANA
    do app gửi lên: thêm cột `mui_gio` ở `thiet_bi`, app gửi lại mỗi lần mở app, đổi múi giờ thì tính lại các dòng chưa tới giờ.
19. **Người dùng có danh sách kênh để bật hoặc tắt, mỗi kênh là một `LoaiThongBao`** (`nhac_an`, `nhac_tap`, `tin_nhan`,
    `danh_gia`, `ket_noi_pt`). Cài đặt lưu ở server theo tài khoản (đề xuất bảng `cai_dat_thong_bao`: `tai_khoan_id`,
    `loai_thong_bao`, `bat`, unique theo cặp đầu), để đổi máy vẫn giữ. Kênh tắt thì không đặt lịch local và server không push
    loại đó; dòng hộp thư vẫn ghi, chỉ không có chuông. Cài đặt đổi thì tăng số phiên bản dùng cho đồng bộ (quyết định 14). Trên
    Android tạo thêm notification channel theo từng loại để người dùng chỉnh được cả ở cài đặt hệ thống. Quyền thông báo của hệ
    điều hành bị tắt là chuyện khác với tắt kênh: app phát hiện và hiện banner dẫn vào cài đặt.
20. **Server push dự phòng khi lịch local không phủ được một mốc của `nhac_tap` hoặc `nhac_an`.** Đây là phần làm cho
    "không được phép rớt" (quyết định 16) thành thật.
    - Sau mỗi lần đồng bộ (quyết định 14), app báo lên server: phiên bản đã đồng bộ, `phu_den` (UTC, mốc cuối cùng đã đặt
      lịch local thành công), quyền thông báo của hệ điều hành, và trên Android là quyền báo thức chính xác. Server lưu ở
      `thiet_bi` (cột mới, ví dụ `lich_local_phu_den`, `lich_local_phien_ban`, `cho_phep_thong_bao`).
    - Mốc coi là **được phủ** khi một thiết bị có `phu_den` không nhỏ hơn mốc, phiên bản báo lên bằng phiên bản hiện tại của
      lộ trình và cài đặt kênh, và hệ điều hành cho phép thông báo. Lệch phiên bản nghĩa là lịch local có thể sai nên không tin.
    - Worker quyết định ngay trước mốc (sớm hơn mốc vài phút, đủ để gửi): mốc **chưa được phủ** thì đổi kênh của dòng
      `thong_bao` từ local sang push và gửi qua luồng chung (quyết định 3 đến 6). Mốc đã được phủ thì không push, để không bắn
      hai lần. Xét riêng từng thiết bị; push chỉ tới thiết bị chưa được phủ.
    - Thiết bị báo hệ điều hành đã tắt thông báo thì bỏ qua, vì push cũng bị chặn; chỉ còn hộp thư và banner trong app.
    - Push dự phòng dùng ưu tiên cao, TTL ngắn (quyết định 8) và `ma_chong_trung` làm collapse key.
    - Chấp nhận gửi trùng ở khe hẹp (app báo đã phủ nhưng local không bắn, hoặc app đồng bộ đúng lúc push đang đi), theo
      quyết định 4: thà trùng còn hơn mất. Lịch local đã hết hạn thì mốc nằm ngoài `phu_den` và được push thay, nên người
      chỉ làm theo nhắc mà không mở app lâu vẫn được nhắc.

Điều kiện ngoài code: tài khoản Apple Developer Program (APNs), project Firebase riêng cho dev và prod (kèm bundle ID tương
ứng), service account của backend để ngoài git và nạp qua biến môi trường hoặc secret.

## Điểm còn mở (chưa chốt)

- **Lịch local bị mất mà app không biết** (Android giết app, khởi động lại máy, người dùng xóa dữ liệu): server vẫn tin
  `phu_den` đã báo nên không push dự phòng (quyết định 20). Cần thử trên máy thật xem mức này xảy ra thường không, và nếu có
  thì thêm cách kiểm tra lại (ví dụ app báo lại khi mở, hoặc coi báo cáo quá cũ là không chắc).
- **Một dòng hộp thư cho mỗi sự kiện hay mỗi mốc nhắc** (15 phút trước và đúng lúc): đề xuất một dòng cho mỗi sự kiện, hiện
  tại mốc đúng lúc.
- **Kênh tắt có ẩn dòng hộp thư không**: mặc định ghi và vẫn hiện, chỉ tắt chuông (quyết định 19). Cần xác nhận.
- **Quyền báo thức chính xác trên Android 12 trở lên** (`SCHEDULE_EXACT_ALARM`, mặc định bị từ chối từ Android 14): thiếu
  quyền thì nhắc có thể lệch vài phút. Cần thử trên máy thật, kể cả việc `expo-notifications` có đặt lại lịch sau khi khởi động
  lại máy hay không.
- **Giờ yên tĩnh** của người dùng: chưa có chỗ lưu và chưa quyết có làm không. Giờ nhắc (15 phút trước, đúng lúc) đã chốt cố
  định.
- **Dọn dòng `thong_bao` cũ** và chính sách giữ lại bao lâu.

## Considered Options

- Expo Push Service: cấu hình nhanh nhất (upload key lên EAS, backend gọi HTTP), nhưng thêm một bên trung gian vào đường
  gửi, ít kiểm soát lỗi và hạn mức. Loại vì thông báo là phần quan trọng.
- Gửi push ngay trong request nghiệp vụ, không qua hộp thư: đơn giản hơn nhưng API sập hoặc FCM lỗi giữa chừng thì mất
  thông báo, không retry được. Loại.
- Giao đúng một lần (exactly-once): không đạt được trên push; dựng nó phức tạp hơn lợi ích. Chọn at-least-once kèm chống trùng.
- Chỉ push cho cả nhắc hẹn giờ: một cơ chế duy nhất, nhưng nhắc đúng giờ phụ thuộc FCM, APNs và mạng, trong khi lịch ít
  đổi nên lên lịch trên máy chắc hơn. Loại; push chỉ giữ cho thứ phát sinh bất ngờ.
- Luôn gửi cả local lẫn push rồi bỏ trùng: khi app đóng, hai thông báo từ hai đường khác nhau không bỏ trùng được, người dùng
  nhận hai cái mỗi mốc. Loại; chỉ push khi local không phủ được (quyết định 20).
- Cho người dùng chỉnh giờ nhắc (30 phút, 1 giờ trước): thêm cột lưu và đồng bộ, không có nhu cầu. Chọn hai mốc cố định.
- Gọi APNs trực tiếp cho iOS: thêm một đường tích hợp và quản lý khóa thứ hai. Loại, để FCM lo APNs.
