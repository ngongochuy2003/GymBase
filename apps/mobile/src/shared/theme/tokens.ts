/**
 * Design tokens của giao diện cộng đồng (sáng, tối giản, bo góc lớn).
 * Các component UI kit chỉ đọc giá trị từ `theme`, không hard-code màu/kích thước.
 */
export const theme = {
  colors: {
    primary: '#4F6B55',
    onPrimary: '#FFFFFF',
    background: '#F3F0EC',
    card: '#FAF8F6',
    text: '#1E1E1E',
    muted: '#8B8B8B',
    border: '#E6E2DD',
    /** Nền nhạt cho chip/ô chưa chọn trên card. */
    surfaceMuted: '#F0ECE7',
    /** Nền nổi bật nhẹ cho mục đang chọn. */
    highlight: '#EDE9E3',
    /** Màu trạng thái "đã thích". */
    like: '#E5484D',
    placeholder: '#D9D5CF',
    overlay: 'rgba(30,30,30,0.6)',
  },
  radius: { card: 20, chip: 999, image: 16 },
  spacing: { xs: 4, sm: 8, md: 12, lg: 16, xl: 20 },
  font: {
    family: undefined as string | undefined, // system sans-serif (Inter có thể gắn sau)
    size: { xs: 12, sm: 13, md: 15, lg: 17, xl: 22 },
    weight: { regular: '400', medium: '500', bold: '700' } as const,
  },
  iconSize: { sm: 18, md: 22, lg: 26 },
} as const;

export type AppTheme = typeof theme;
