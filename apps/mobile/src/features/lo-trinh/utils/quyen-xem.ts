import type { QuyenXemAnh } from '../types';

/** Hai chế độ xem ảnh cơ thể (N33). Mặc định "Chỉ mình tôi" vì ảnh cơ thể là dữ liệu riêng tư. */
export const QUYEN_XEM: readonly {
  gia: QuyenXemAnh;
  nhan: string;
  icon: string;
  moTa: string;
}[] = [
  {
    gia: 'rieng_tu',
    nhan: 'Chỉ mình tôi',
    icon: 'lock',
    moTa: 'Chỉ mình bạn xem được ảnh này. Bạn có thể cho PT xem bất cứ lúc nào.',
  },
  {
    gia: 'cho_pt',
    nhan: 'Cho PT xem',
    icon: 'person-check',
    moTa: 'PT xem được ảnh này trong tiến độ của bạn để so sánh các mốc.',
  },
];
