# Audit giao diện GymBase Mobile

> **Ngày viết:** 19/11/2026 · **Trạng thái:** đã triển khai (giao diện mobile Expo: auth, thiết lập hồ sơ, bảng tin, đăng bài, cá nhân); chưa có design token mới · **Đối chiếu code:** 169fd49

Phạm vi: `apps/mobile` (Expo / React Native, NativeWind 4, Expo Router). Chưa có frontend web nên không audit.
Đường dẫn trong tài liệu tính từ `apps/mobile/` nếu không ghi khác. Tỉ lệ tương phản tính theo công thức WCAG 2.x
từ mã hex; chỗ có độ trong suốt hoặc nền là ảnh thì chỉ ước lượng.

---

## 1. Mục đích và người dùng

**App giải quyết gì.** Theo `README.md` ở gốc repo: nền tảng kết nối phòng gym, PT và học viên, để trao đổi, quản lý
tập luyện và theo dõi tiến trình. V1 tập trung vào PT và học viên; phòng gym và web để sau.

**Ai dùng.** Vai trò chọn khi thiết lập hồ sơ là `hoc_vien`, `pt`, `ca_hai` (`src/features/profile/types/vai-tro.ts`).
Bảng tin có lọc "Dành cho bạn / Xu hướng / Tập luyện / Kiến thức" (`src/features/feed/config/feed-config.ts`), bài đăng
có chủ đề "Tập luyện / Meal prep / Tiến độ / Mẹo hay" và nút "Gắn PT" (`src/features/composer/config/composer-config.ts`).
Copy màn chào mừng: "Đồng hành cùng PT trên từng buổi tập, mỗi ngày một tiến bộ" (`src/features/auth/components/welcome-hero.tsx:37`).

**Tình huống dùng.**
- Chắc chắn từ code: chỉ dọc (`app.json`, `orientation: portrait`), mở bằng điện thoại Android/iOS; hiện chỉ có luồng
  đăng nhập, thiết lập hồ sơ, xem bảng tin, thích bài, đăng bài có ảnh/video.
- *Suy đoán:* học viên mở app tại phòng tập hoặc giữa các hiệp để xem lộ trình, đăng ảnh tiến độ; PT dùng để theo dõi
  học viên. Căn cứ: tab "Lộ trình" và "Tin nhắn" đã có trên thanh tab nhưng chưa có màn
  (`src/shared/navigation/main-tabs.ts:7,10`), bảng màu mẫu minh họa "Lộ trình tăng cơ 12 tuần · Tuần 4/12 · PT …"
  (`Tài liệu dự án/GymBase_Color_Palette.html`).
- *Suy đoán:* sẽ có thông báo khẩn (tin nhắn PT, nhắc lịch): kênh `gymbase_urgent_messages` và quyền
  `USE_FULL_SCREEN_INTENT` trong `app.json`.
- *Suy đoán về hệ quả thiết kế:* dùng nơi ánh sáng mạnh (phòng tập), thao tác nhanh bằng một tay, nên tương phản chữ
  phụ và kích thước vùng bấm quan trọng hơn bình thường.

## 2. Màn hình và luồng chính

Luồng: **Chào mừng → Đăng ký / Đăng nhập → (lần đầu) Onboarding → Thiết lập hồ sơ 3 bước → Trang chủ (bảng tin)** ↔
Chi tiết bài viết, Đăng bài, Cá nhân. Route đều mỏng, chỉ re-export màn trong `src/features/*/screens`.

| Màn | Route | Mục đích | Component chính | File |
| --- | --- | --- | --- | --- |
| Chào mừng | `/welcome` | Giới thiệu, dẫn tới đăng ký/đăng nhập | `FixedScreen`, `AuthHeader`, `WelcomeHero` (carousel ảnh + `WaveEdge`), `Button pill`, `OrDivider`, `GoogleSignInButton`, `LegalNotice` | `src/features/auth/screens/welcome-screen.tsx` |
| Đăng nhập | `/login` | Email + mật khẩu, Google | `Screen`, `AuthHeader`, `AppText title`, `TextField`, `Button` | `src/features/auth/screens/login-screen.tsx` |
| Đăng ký | `/register` | Tạo tài khoản | `TextField` ×6, `Checkbox`, `ReferralInfoModal`, `Button` | `src/features/auth/screens/register-screen.tsx` |
| Onboarding | `/onboarding` | Báo đăng nhập lần đầu, mời thiết lập hồ sơ | `SuccessBadge`, khối thông tin `bg-secondary`, `Button` ×2 | `src/features/auth/screens/onboarding-screen.tsx` |
| Thiết lập hồ sơ: vai trò | `/profile-setup` | Chọn học viên / PT / cả hai | `AuthHeader`, thẻ chọn vai trò, `Button` | `src/features/profile/screens/profile-setup-screen.tsx` |
| Thiết lập hồ sơ: cá nhân | `/profile-setup/ca-nhan` | Thông tin cá nhân | `TextField`, `SelectField`, `PrivateInfoModal` | `src/features/profile/screens/personal-info-screen.tsx` |
| Thiết lập hồ sơ: chi tiết | `/profile-setup/chi-tiet` | Thông tin theo vai trò | thanh tab dạng pill, `MultiSelectChips`, `SelectField`, `TextField` | `src/features/profile/screens/profile-detail-screen.tsx` |
| Trang chủ (bảng tin) | `/home` | Xem, lọc, thích bài | `ScreenRenderer`, `AppHeader`, `FilterChips`, `PostList` → `PostCard`, `BottomTabBar` | `src/features/feed/screens/feed-screen.tsx` |
| Chi tiết bài viết | `/post-detail` | Xem một bài, xem ảnh toàn màn | `SubHeader`, `PostCard`, `PostImageViewer` | `src/features/feed/screens/post-detail-screen.tsx` |
| Đăng bài | `/compose` | Soạn bài, chọn chủ đề, quyền xem, ảnh | `ScreenRenderer`, `ComposerCard`, `TopicSelector`, `VisibilitySelector`, `MediaGrid`, `ToolbarActions` | `src/features/composer/screens/composer-screen.tsx` |
| Cá nhân | `/profile` | Đổi sáng/tối, đăng xuất | `Screen`, `Card`, `Button` | `src/features/profile/screens/profile-screen.tsx` |
| Không tìm thấy | `+not-found` | Route sai | | `src/app/+not-found.tsx` |

Tab chưa có màn: Lộ trình, Tin nhắn (bấm không làm gì), Khám phá (khai báo nhưng không dùng).

**Hai "hệ" giao diện song song** (gốc của phần lớn vấn đề bên dưới):

| | Hệ A: auth + hồ sơ | Hệ B: bảng tin + đăng bài |
| --- | --- | --- |
| Màn | Chào mừng, Đăng nhập, Đăng ký, Onboarding, Thiết lập hồ sơ, Cá nhân | Trang chủ, Chi tiết bài, Đăng bài |
| Cách viết style | `className` NativeWind | `StyleSheet.create` |
| Nguồn màu | token HSL `global.css` + `useTheme().colors` (`palette.ts`) | `theme` tĩnh trong `src/shared/theme/tokens.ts` |
| Theo dark mode | Có | **Không** |
| Component dùng chung | `src/shared/components/ui.tsx` (`Button`, `TextField`, `Card`…) | `src/shared/ui-kit/*` (`Surface`, `Chip`, `AppHeader`…) |
| Icon | `Ionicons` gọi trực tiếp | `Icon` qua registry (`src/shared/icons`) trộn Ionicons, Feather, MaterialCommunityIcons |

## 3. Kiểm kê màu

### 3.1 Bốn nguồn định nghĩa màu/kích thước

| Nguồn | Dạng | Ai dùng | Ghi chú |
| --- | --- | --- | --- |
| `global.css:7-62` | Biến HSL `--background`, `--primary`… light/dark | Lớp Tailwind qua `tailwind.config.js:12-34` | Khớp 100% với `Tài liệu dự án/GymBase_Color_Palette.html` (đã quy đổi HSL→hex) |
| `src/shared/theme/palette.ts` | Hex light/dark | `useTheme().colors` cho prop không nhận class (icon, placeholder, nền navigator) | Bản sao tay của `global.css`; 4 khóa không ai dùng: `accent`, `hero`, `heroText`, `error` |
| `src/shared/theme/tokens.ts` | Hex **chỉ một bộ** + radius/spacing/font/iconSize | 20 file hệ B | Giá trị lệch nhẹ so với palette (mục 3.3); `src/features/README.md:13` lại ghi đây là "design tokens" chính thức |
| `src/shared/theme/theme.tsx:5` | `spacing` {4, 8, 16, 24, 32} | Không file nào | Trùng tên với `tokens.ts` nhưng khác giá trị (`md` = 16 so với 12) |

### 3.2 Bảng màu đang dùng

Số lần = số chỗ tham chiếu trong `src/` (lớp Tailwind + `theme.colors.*` + `colors.*` + hex cứng).

| Hex | Vai trò | Nơi dùng chính | Số lần | Token hay hardcode |
| --- | --- | --- | --- | --- |
| `#F7F5F3` | Nền trang (light) | `bg-background` (`ui.tsx` `Screen`), `colors.background` (layout navigator, `wave-edge.tsx`) | ~14 | Token (`global.css`, `palette.ts`); thêm 1 lần trong `app.json` |
| `#F3F0EC` | Nền trang hệ B | `screen-renderer.tsx:110,117`, `app-header` badge | 5 | Token tĩnh `tokens.ts` |
| `#FFFFFF` | Card, ô nhập (light); chữ trên nền primary | `bg-card` (`Card`, `TextField`, `Button` phụ), `colors.surface`, `onPrimary` | ~25 | Token |
| `#FFFFFF` | Chữ/icon trên ảnh và trình xem ảnh | `welcome-hero.tsx:69`, `post-card.tsx:540`, `post-image-viewer.tsx:128,146,158,204,214,249` | 8 | **Hardcode** |
| `#FAF8F6` | Card hệ B | `Surface`, `Chip`, `BottomTabBar`, `PostCard` | 6 | Token tĩnh |
| `#202320` | Chữ chính (light) | `text-foreground`, `colors.text` | ~25 | Token |
| `#1E1E1E` | Chữ chính hệ B, màu icon mặc định | `theme.colors.text` (`PostCard`, `SubHeader`, `Icon` mặc định `icon.tsx:12`) | 17 | Token tĩnh |
| `#777B77` | Chữ phụ, placeholder, icon trong ô nhập | `text-muted-foreground`, `colors.muted` | 32 | Token |
| `#8B8B8B` | Chữ phụ hệ B, tab chưa chọn | `theme.colors.muted` (`PostCard` thời gian, `BottomTabBar`, `ComposerCard`) | 21 | Token tĩnh |
| `#526959` | Primary: nút chính, liên kết, checkbox | `bg-primary`, `text-primary`, `border-primary`, `colors.primary` | ~25 | Token |
| `#4F6B55` | Primary hệ B: chip chọn, tab đang chọn, nút "Đăng" | `theme.colors.primary` | 17 | Token tĩnh |
| `#3E5545` | Primary đậm | `bg-primary-dark` (1 chỗ) | 1 | Token |
| `#F0EEEC` | Nền phụ (khối thông tin, ô chọn vai trò, tab pill) | `bg-secondary` (`onboarding-screen.tsx:551`, `profile-setup-screen.tsx:64,75`, `profile-detail-screen.tsx:53`, `success-badge.tsx:36`) | 8 | Token |
| `#F0ECE7` | Nền chip "soft" hệ B | `theme.colors.surfaceMuted` (`chip.tsx:58`, `topic-selector`) | 3 | Token tĩnh |
| `#EDE9E3` | Nền mục đang chọn | `theme.colors.highlight` | 1 | Token tĩnh |
| `#D9D5CF` | Nền ảnh đang tải | `theme.colors.placeholder` (`remote-image.tsx`) | 2 | Token tĩnh |
| `#E5E2DF` | Viền, đường kẻ | `border-border`, `bg-border` (`or-divider.tsx`) | ~12 | Token |
| `#E6E2DD` | Viền hệ B, vạch ngăn bài viết 2px | `theme.colors.border` | 11 | Token tĩnh |
| `#B85C55` | Lỗi (chữ, viền ô lỗi) | `text-destructive` 13, `border-destructive` 3 | 16 | Token |
| `#E5484D` | Tim "đã thích" | `theme.colors.like` (`post-card`) | 1 | Token tĩnh, không có bản dark |
| `#0A0A0A` | Nền khung ảnh bài viết | `post-card.tsx:453,469,483,509` | 4 | **Hardcode** |
| `#000` / `#000000` | Nền trình xem ảnh; màu bóng | `post-image-viewer.tsx:227`; `shadowColor` ở `surface.tsx:16`, `bottom-tab-bar.tsx:82`, `post-list.tsx:346` | 4 | **Hardcode** |
| `rgba(0,0,0,.5–.55)` | Lớp phủ ảnh, bóng chữ | `welcome-hero.tsx:58,86`, `post-card.tsx:535`, `post-image-viewer.tsx:290` | 5 | **Hardcode** |
| `bg-black/10, /40, /50` | Lớp phủ modal, nút trên ảnh | `referral-info-modal`, `private-info-modal`, `media-grid` | 4 | Lớp Tailwind mặc định, ngoài palette |
| `rgba(30,30,30,.6)` | Nền toast | `theme.colors.overlay` (`toast.tsx`) | 2 | Token tĩnh |
| `#231F7C` (ARGB `#FF231F7C`) | Màu icon thông báo Android | `app.json`, `notification-service.ts:55` | 2 | **Hardcode**, tím than, không thuộc palette (giống giá trị mẫu trong tài liệu Expo) |
| `#527A5D`, `#B58A51`, `#66899A`, `#8A8F8C` | success / warning / info / pending | Có trong `global.css`, `tailwind.config.js` | 0 | Token chưa ai dùng |

**Dark mode** (chỉ hệ A dùng): nền `#222D35`, card `#2D3943`, phụ `#36434D`, viền `#43515B`, chữ `#F5F6F5`, chữ phụ
`#BAC3C8`, primary `#89A5A0`, active `#A9C7D7`, lỗi giữ nguyên `#B85C55`.

### 3.3 Màu gần giống nhau nhưng khác mã

| Vai trò | Hệ A (`global.css` / `palette.ts`) | Hệ B (`tokens.ts`) | Khác biệt thấy được? |
| --- | --- | --- | --- |
| Nền trang | `#F7F5F3` | `#F3F0EC` | Có, khi chuyển Cá nhân ↔ Trang chủ nền đổi tông |
| Card | `#FFFFFF` | `#FAF8F6` | Nhẹ |
| Chữ chính | `#202320` | `#1E1E1E` | Không, nhưng là hai token |
| Chữ phụ | `#777B77` (ánh xanh) | `#8B8B8B` (xám trung tính, nhạt hơn) | Có |
| Primary | `#526959` | `#4F6B55` | Rất nhẹ |
| Viền | `#E5E2DF` | `#E6E2DD` | Không |
| Nền phụ | `#F0EEEC` | `#F0ECE7`, `#EDE9E3` | Nhẹ; ba mã cho cùng ý "nền nhạt" |
| Đen của ảnh | | `#0A0A0A` vs `#000000` | Không |
| Lớp phủ tối | | `.5`, `.55`, `/40`, `/50`, `rgba(30,30,30,.6)` | Năm mức cho một vai trò |
| Đỏ | `#B85C55` (lỗi) | `#E5484D` (tim) | Cố ý khác vai trò, nhưng hai sắc đỏ không cùng họ |

Lệch khác trong cùng hệ A: `--accent` dark trong `global.css:42` là `#36434D` nhưng `palette.ts:28` đặt `accent` dark
là `#A9C7D7` (màu `active`); `--primary-dark` dark (`global.css:37`) bằng đúng `--primary`, còn file bảng màu gốc đặt
primary-dark dark là `#A9C7D7`. `palette.ts:13-14` có `TODO(THEME)` ghi màu hero/nhấn chưa chốt.

## 4. Typography, spacing, bo góc, đổ bóng

### 4.1 Font và cỡ chữ

- **Font:** font hệ thống, không nạp font riêng (`tokens.ts:25` để `family: undefined`, ghi chú "Inter có thể gắn sau").
  File bảng màu gốc thiết kế bằng Inter.
- **Hai thang cỡ chữ không khớp:**
  - `tokens.ts:27`: 12 / 14 / 16 / 18 / **22**.
  - `tailwind.config.js:35-43`: 12 / 14 / 16 / 18 / **20** / 24 / 30.
  - Ngoài thang: 26 (`app-header.tsx` chữ "G", `welcome-hero.tsx:66`), 28 (`text-[28px]` ×5, `post-card.tsx:541`),
    20 = `font.size.xl - 2` (`sub-header.tsx:31`).
- **Tiêu đề trang 28px viết lại ở 5 chỗ** thay vì dùng `AppText title`: `onboarding-screen.tsx:25`,
  `profile-setup-screen.tsx:42`, `personal-info-screen.tsx:75`, `profile-detail-screen.tsx:200`, và `ui.tsx:80`.
  Chỉ bản trong `AppText` có `tracking-[-0.8px]`, nên cùng cỡ mà tiêu đề trông khác nhau. Màn Cá nhân dùng
  `AppText title` (28px) cho cả tiêu đề trang lẫn tiêu đề card, không có cấp bậc.
- **Độ đậm:** bold 700 chiếm ưu thế (`font-bold` 15, `weight.bold` 12, thêm `'700'`, `'bold'`), semibold 600
  (`font-semibold` 10, `'600'` 1), medium 500 (4). `tokens.ts` không có semibold nên hệ B không dùng được mức này.
- **Line-height:** Tailwind có thang (16…40px); `tokens.ts` không có, hệ B hardcode `22` (`post-card.tsx:427`),
  `34` (`welcome-hero.tsx:68`), còn lại để mặc định.

### 4.2 Khoảng cách

- **Hai thang spacing cùng tên khác giá trị:** `tokens.ts:24` {xs 4, sm 8, **md 12**, lg 16, xl 20} và
  `theme.tsx:5` {xs 4, sm 8, **md 16**, lg 24, xl 32} (không ai dùng).
- **Cộng trừ trên token (21 chỗ)** tạo ra số lẻ ngoài lưới: 2, 5, 6, 7, 9, 11, 13, 14, 15, 20. Ví dụ
  `chip.tsx:51-53` (gap 7, padding 9×5), `app-header.tsx:80-82` (14×5), `sub-header.tsx:70-72`,
  `post-card.tsx:404,446,549,554`, `bottom-tab-bar.tsx:80,97`. Thực chất là chỉnh pixel theo mockup rồi gắn lên token.
- **Lưới 3,5px và 4px trộn nhau (suy ra, chưa đo trên máy):** `tailwind.config.js:33-34` ghi NativeWind trên native quy
  1rem = 14px. Lớp spacing/bo góc của Tailwind tính bằng rem nên `p-6` ≈ 21px, `gap-3` ≈ 10,5px, `rounded-xl` ≈ 10,5px,
  trong khi hệ B dùng số px theo bội 4. Cỡ chữ đã được sửa về px, spacing thì chưa.
- **Lề ngang màn không thống nhất:** hệ A `p-6` (`ui.tsx:47,64`); `AppHeader`/`SubHeader` 14px; nội dung `PostCard` 8px
  (`post-card.tsx:413,429`), hàng nút 11px (`post-card.tsx:549`).
- Lớp spacing dùng nhiều nhất: `*-3` (25), `*-4` (22), `*-2` (15), `*-5` (8), `*-6` (6).

### 4.3 Bo góc

| Giá trị | Nơi dùng |
| --- | --- |
| 999 / `rounded-full` | Chip, nút pill auth, pill `SubHeader`, avatar, toast (`radius.chip` 6 + `rounded-full` 10) |
| 20 | `Card` (`ui.tsx:91`, `rounded-[20px]`), `radius.card` (`screen-renderer`) |
| 16 | `radius.image`, `rounded-2xl` (khối thông tin onboarding/hồ sơ) |
| 14 | `Button` thường (`rounded-[14px]`) |
| 12 / `rounded-xl` | `TextField` (`ui.tsx:189`) |
| 24 / `rounded-3xl` | modal (`referral-info-modal`, `private-info-modal`) |
| 6, 5, 4, 3, 1 | `rounded-md` checkbox, nút "+" giữa tab bar **5px** (`bottom-tab-bar.tsx:93`), badge, chấm carousel |
| 0 | `PostCard`, ảnh bài viết |

Bảy mức bo góc khác nhau cho các khối cùng cấp (card 20, nút 14, ô nhập 12, modal 24, khối thông tin 16). Nút đăng bài
ở giữa tab bar bo 5px lệch hẳn với ngôn ngữ "bo góc lớn" mà `tokens.ts:2` mô tả.

### 4.4 Đổ bóng

Không có token bóng. Ba bộ khác nhau: `surface.tsx:16-20` (opacity .04, radius 10, y 3, elevation 1),
`bottom-tab-bar.tsx:82-86` (.06, 12, y −3, elevation 8), `post-list.tsx:346-350` (.1, radius 4, y 2, elevation 3).
`PostCard` tắt bóng của `Surface` (`post-card.tsx:407-408`); `Card` hệ A không có bóng. File bảng màu gốc dùng
`0 8px 26px rgba(29,38,33,.05)` (light) và `rgba(0,0,0,.10)` (dark).

### 4.5 Icon

Thang `tokens.ts:30` {10, 12, 14, 18, 22, 26} kèm cộng trừ (`iconSize.xs-1`, `md - 3`, `lg - 4`…). Hệ A ghi số trực
tiếp: 20 (×8), 22 (×6), 24 (×4), 16, 18, 26, 28, 32, 56. Hai cách gọi icon (mục 2) trộn ba bộ icon khác nét vẽ.

## 5. Khả năng đọc

### 5.1 Cặp chữ/nền tương phản thấp

Ngưỡng WCAG AA: chữ thường 4.5:1, chữ lớn (≥18,66px đậm hoặc ≥24px) 3:1, viền ô nhập và thành phần điều khiển 3:1.

| Cặp | Tỉ lệ | Chỗ dùng | Kết quả |
| --- | --- | --- | --- |
| Chữ phụ `#777B77` / nền `#F7F5F3` | 3.95 | `text-muted-foreground` trên nền trang: phụ đề đăng nhập (`login-screen.tsx:69-71`), `OrDivider`, `LegalNotice` (14px), gợi ý khóa tài khoản (`login-screen.tsx:127`), placeholder ô nhập | Trượt |
| Chữ phụ `#777B77` / card `#FFFFFF` | 4.30 | placeholder và icon trong `TextField`, `EmptyState` | Trượt sát ngưỡng |
| Chữ phụ `#777B77` / nền phụ `#F0EEEC` | 3.72 | khối "Bạn có thể thiết lập sau…" (`onboarding-screen.tsx:551-555`), `profile-setup-screen.tsx:75` | Trượt |
| Chữ phụ hệ B `#8B8B8B` / card `#FAF8F6` | 3.22 | thời gian đăng bài 12px, nhãn tab chưa chọn 12px (`bottom-tab-bar.tsx`), quyền xem ở `ComposerCard` | Trượt, lại là chữ 12px |
| "Xem thêm" `#8B8B8B` × opacity .65 / `#FAF8F6` | ≈2.0 | `post-card.tsx:353,435-440` | Trượt nặng |
| Lỗi `#B85C55` / nền `#F7F5F3` | 4.11 | `text-destructive` dưới ô nhập, lỗi đăng nhập | Trượt sát ngưỡng |
| Lỗi `#B85C55` / card `#FFFFFF` | 4.47 | `ErrorState` trong `Card` | Trượt sát ngưỡng |
| Lỗi `#B85C55` / nền dark `#222D35` | 3.14 | như trên, ở dark mode | Trượt |
| Lỗi `#B85C55` / card dark `#2D3943` | 2.64 | `ErrorState` dark | Trượt |
| Viền `#E5E2DF` / card `#FFFFFF` | 1.29 | khung `TextField`, `Checkbox` chưa tick, `Button` phụ | Trượt 1.4.11 (khó thấy ô nhập) |
| Viền dark `#43515B` / `#222D35` | 1.72 | như trên, dark | Trượt 1.4.11 |
| Chữ trắng / ảnh hero | phụ thuộc ảnh | `welcome-hero.tsx`: có lớp tối 50% → 0% và bóng chữ | Có xử lý; phần chữ lấn sang phải (lớp phủ còn ~0) có thể thấp |
| Status warning `#B58A51` / info `#66899A` / pending `#8A8F8C` trên nền sáng | 2.87 / 3.44 / 3.02 | Chưa dùng | Sẽ trượt nếu dùng làm chữ |

Các cặp **đạt**: chữ chính light 14.6, dark 13.0; chữ phụ dark `#BAC3C8` 7.85 trên nền, 6.60 trên card; trắng trên
primary 5.96; primary làm chữ/liên kết trên nền 5.48; chữ tối trên primary dark 5.32.

Khác: tim đã thích `#E5484D` trên card 3.69 (đạt cho icon 3:1, không đạt nếu dùng cho số đếm cạnh tim). Trạng thái
disabled dùng `opacity-50` (`ui.tsx:131`, `sub-header.tsx:56`): WCAG không bắt buộc, nhưng nút primary mờ 50% trên nền
kem gần như mất. Hệ A đặt `border-border` sáng cho cả nút primary (`ui.tsx:128`), tạo viền nhạt quanh nền xanh.

### 5.2 Dark mode

- **Có**, do NativeWind giữ (`darkMode: 'class'`, `useColorScheme`), mặc định theo hệ thống (`app.json`
  `userInterfaceStyle: automatic`), đổi tay ở màn Cá nhân (`profile-screen.tsx:19-23`, `theme.tsx:13-23`).
  Chưa thấy code lưu lựa chọn sáng/tối qua lần mở app sau (chưa chạy thử để xác nhận).
- Logo có bản tối riêng (`auth-header.tsx:5-6,28`). Theme điều hướng, nền cửa sổ native đồng bộ theo `colors`
  (`src/app/_layout.tsx:27-45`).
- **Hệ B không theo dark mode.** `AppHeader`, `BottomTabBar`, `SubHeader`, `Chip`, `FilterChips`, `Surface`, `Toast`,
  `Icon` (màu mặc định), `ScreenRenderer`, `PostCard`, `PostList`, màn Chi tiết bài và Đăng bài đọc `theme` tĩnh của
  `tokens.ts`. Ở dark mode:
  - Trang chủ, Chi tiết bài, Đăng bài vẫn sáng; Cá nhân và các màn auth tối. Chuyển tab là đổi hẳn tông.
  - Navigator đặt nền tối (`_layout.tsx:62`) sau màn sáng, nên khi trượt/mờ chuyển màn sẽ lộ nền tối.
  - `StatusBar` đổi sang chữ trắng (`_layout.tsx:55`) trong khi header Trang chủ nền `#F3F0EC`: giờ, pin gần như không
    đọc được (trắng trên kem ≈1.1:1).
- Bản dark chưa tách màu cho: lỗi (dùng chung `#B85C55`, trượt tương phản), tim đã thích, lớp phủ, status colors
  (success `#527A5D` trên nền dark chỉ 2.88).

## 6. Kết luận

**Vấn đề chủ yếu là "mỗi chỗ một kiểu" và thiếu quy tắc dùng, không phải bản thân palette.**

- Palette gốc (kem + xanh rêu, dark xanh than) mạch lạc, có tài liệu nguồn, và `global.css` đã khớp nó 100%. Cặp chính
  (chữ/nền, trắng/primary) đạt AA ở cả hai chế độ.
- Cái làm giao diện trông "xấu, rời rạc":
  1. **Ba nguồn token song song** (`global.css`, `palette.ts`, `tokens.ts`) với giá trị trôi dần, cộng một `spacing` chết
     trong `theme.tsx`. Hai hệ component (`shared/components/ui.tsx` và `shared/ui-kit`) mỗi bên đọc một nguồn.
  2. **Hệ B không có dark mode**, làm app nửa sáng nửa tối.
  3. **Không có thang chữ và thang khoảng cách bắt buộc**: tiêu đề viết lại 5 lần, 21 chỗ cộng trừ token, lưới 3,5px
     (rem 14) trộn với 4px, lề ngang 24 / 14 / 8 / 11.
  4. **Bảy mức bo góc, ba kiểu bóng không token**, nút giữa tab bar bo 5px.
  5. **Thiếu token cho vai trò có thật**: lớp phủ trên ảnh, nền media, chữ trên ảnh, tim đã thích, hero, chữ phụ cỡ nhỏ.
- Palette chỉ có vấn đề ở vài vai trò phụ: chữ phụ light, màu lỗi (cả hai chế độ), viền ô nhập, và các màu status.

**Gợi ý: giữ palette cũ làm gốc, không thiết kế lại từ đầu.** Lý do:

- Màu nền, primary và chữ chính đã ổn về tương phản và có bản dark tương ứng; tài liệu bảng màu đã được thống nhất
  (`GymBase_Color_Palette.html` ghi "đã thống nhất").
- Phần lớn vấn đề sẽ hết khi gom về **một nguồn token** (ví dụ `global.css` + một file TS sinh ra từ cùng dữ liệu,
  có cả light/dark) và bắt hệ B đọc theo theme. Thiết kế màu mới không sửa được những chỗ này.
- Việc cần làm với palette chỉ là chỉnh và bổ sung, ví dụ (đã kiểm tỉ lệ):
  - Chữ phụ light `#777B77` → khoảng `#686C68` (4.91 trên nền, 4.61 trên nền phụ, 5.34 trên trắng). Bỏ `#8B8B8B`.
  - Lỗi light `#B85C55` → khoảng `#A6504A` (4.99 / 5.43); lỗi dark tách riêng, khoảng `#D9837C` (5.01 trên nền; trên
    card dark còn 4.21, cần sáng thêm chút nếu đặt lỗi trong card).
  - Viền ô nhập: tách token `input` đậm hơn `border` (đạt 3:1 với nền), giữ `border` nhạt cho đường kẻ trang trí.
  - Thêm token: `scrim` (một mức), `media-bg`, `on-media`, `like`, `hero` (đang `TODO(THEME)`), và bản dark cho
    status colors.
- Song song với màu, cần chốt: một thang chữ có tên vai trò (title / heading / body / caption, kèm line-height và
  semibold), một lưới spacing (4px, xử lý rem 14 của NativeWind), 3–4 mức bo góc, 1–2 mức bóng, 1 thang icon và
  một bộ icon.

Ngoài phạm vi UI nhưng thấy khi đọc: `feed-screen.tsx:60-77` gắn ảnh đại diện cứng theo một tài khoản cụ thể, không
có `TODO`.
