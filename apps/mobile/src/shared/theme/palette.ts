/**
 * Bảng màu dạng HEX cho chỗ không dùng được `className` (tab bar, ActivityIndicator, placeholder...).
 * Nguồn: Tài liệu dự án/GymBase_Color_Palette.html. Phải khớp với token HSL trong `global.css`.
 */
const light = {
  background: '#F7F5F3',
  surface: '#FFFFFF',
  text: '#202320',
  muted: '#686C68',
  border: '#E5E2DF',
  inputBorder: '#85827D',
  primary: '#526959',
  onPrimary: '#FFFFFF',
  error: '#A6504A',
  warning: '#B58A51',
};
const dark: typeof light = {
  background: '#222D35',
  surface: '#2D3943',
  text: '#F5F6F5',
  muted: '#BAC3C8',
  border: '#43515B',
  inputBorder: '#73838E',
  primary: '#89A5A0',
  onPrimary: '#222D35',
  error: '#E08E87',
  warning: '#D4A96A',
};

export const palette = { light, dark };
export type Colors = typeof light;
