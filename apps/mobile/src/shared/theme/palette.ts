/**
 * Bảng màu dạng HEX cho chỗ không dùng được `className` (tab bar, ActivityIndicator, placeholder...).
 * Nguồn: Tài liệu dự án/GymBase_Color_Palette.html. Phải khớp với token HSL trong `global.css`.
 */
const light = {
  background: '#F7F5F3',
  surface: '#FFFFFF',
  text: '#202320',
  muted: '#777B77',
  border: '#E5E2DF',
  primary: '#526959',
  onPrimary: '#FFFFFF',
  // TODO(THEME): palette chưa có màu nền hero/nhấn/chữ-trên-hero (cũ là xanh lá đậm + vàng chanh);
  //   đang tạm lấy primary-dark / surface-muted. Chốt màu với thiết kế rồi thêm vào palette và global.css.
  accent: '#F0EEEC',
  hero: '#3E5545',
  heroText: '#F7F5F3',
  error: '#B85C55',
};
const dark: typeof light = {
  background: '#222D35',
  surface: '#2D3943',
  text: '#F5F6F5',
  muted: '#BAC3C8',
  border: '#43515B',
  primary: '#89A5A0',
  onPrimary: '#222D35',
  accent: '#A9C7D7',
  hero: '#36434D',
  heroText: '#F5F6F5',
  error: '#B85C55',
};

export const palette = { light, dark };
export type Colors = typeof light;
