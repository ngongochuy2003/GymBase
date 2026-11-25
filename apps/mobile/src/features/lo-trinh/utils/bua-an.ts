import type { BuaAnKeHoach } from '../types';

/** Số bữa tối đa mỗi ngày (adr.md, N40): mỗi bữa tốn lượt nhắc trên máy. */
export const TOI_DA_BUA = 6;

/** Bản sao xếp theo giờ ăn; `HH:mm` so chuỗi là đúng thứ tự. */
export function xepTheoGio(buaAn: readonly BuaAnKeHoach[]): BuaAnKeHoach[] {
  return [...buaAn].sort((a, b) => a.gio.localeCompare(b.gio));
}

export function tenBua(bua: Pick<BuaAnKeHoach, 'ten'>): string {
  return bua.ten.trim() || 'Bữa chưa đặt tên';
}
