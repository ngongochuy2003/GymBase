import { layNotificationAdapter, type NotificationAdapter } from './notification-adapter';

/**
 * Nhắc ăn và nhắc tập theo giờ cố định, đặt lịch ngay trên máy (ADR-2, quyết định 12 đến 16): chạy không cần mạng, không
 * phụ thuộc FCM hay APNs. Lịch trên máy là bản sao có hạn dùng của kế hoạch; `dongBoNhacGioAsync` tính lại toàn bộ rồi thêm,
 * sửa, hủy cho khớp, nên chạy bao nhiêu lần cũng không nhân đôi.
 */

export type LoaiNhac = 'nhac_an' | 'nhac_tap';
export type MocNhac = 'truoc15' | 'dung_luc';

/** Một sự kiện có giờ giấc cần nhắc, theo giờ của máy. */
export interface SuKienNhac {
  loai: LoaiNhac;
  /** Mã ổn định của sự kiện (ví dụ `{loTrinhId}:tap-1`), cùng ngày thì hai lần đồng bộ ra cùng identifier. */
  maSuKien: string;
  /** `yyyy-MM-dd`. */
  ngay: string;
  /** `HH:mm`. */
  gio: string;
}

export interface NhacCanDat {
  identifier: string;
  loai: LoaiNhac;
  moc: MocNhac;
  luc: Date;
  tieuDe: string;
  noiDung: string;
}

/** Quyết định 13: hai mốc cố định, người dùng không chỉnh. */
export const PHUT_NHAC_TRUOC = 15;
/** Quyết định 15: iOS giới hạn khoảng 64 lịch đang chờ, mỗi sự kiện tốn hai; đặt tối đa khoảng 50 nhắc gần nhất. */
export const SO_NHAC_TOI_DA = 50;
export const SO_NGAY_TOI_DA = 14;

const TIEN_TO = ['nhac_an:', 'nhac_tap:'];
const NOI_DUNG = 'Mở GymBase để xem chi tiết.';
const TIEU_DE: Record<LoaiNhac, Record<MocNhac, string>> = {
  nhac_tap: { truoc15: 'Sắp đến giờ tập', dung_luc: 'Đến giờ tập' },
  nhac_an: { truoc15: 'Sắp đến giờ ăn', dung_luc: 'Đến giờ ăn' },
};

const laNhacCuaMinh = (identifier: string) => TIEN_TO.some((t) => identifier.startsWith(t));

/** Đọc `yyyy-MM-dd` và `HH:mm` thành giờ địa phương; sai định dạng hoặc ngày giờ không tồn tại thì `null`. */
function docThoiDiem(ngay: string, gio: string): Date | null {
  const n = /^(\d{4})-(\d{2})-(\d{2})$/.exec(ngay);
  const g = /^(\d{2}):(\d{2})$/.exec(gio);
  if (!n || !g) return null;
  const [nam, thang, ngayTrongThang, gioSo, phut] = [n[1], n[2], n[3], g[1], g[2]].map(Number) as [number, number, number, number, number];
  const ketQua = new Date(nam, thang - 1, ngayTrongThang, gioSo, phut, 0, 0);
  const dung =
    ketQua.getFullYear() === nam &&
    ketQua.getMonth() === thang - 1 &&
    ketQua.getDate() === ngayTrongThang &&
    gioSo < 24 &&
    phut < 60;
  return dung ? ketQua : null;
}

/**
 * Các nhắc cần có trên máy: hai mốc cho mỗi sự kiện, bỏ mốc đã qua, chỉ trong `soNgayToiDa` ngày tới, và tối đa
 * `soNhacToiDa` nhắc gần nhất. Nội dung trung tính (không ghi giờ hay bài tập): nhắc hiện trên màn hình khóa.
 */
export function tinhLichNhac(
  suKien: readonly SuKienNhac[],
  now: Date,
  { soNgayToiDa = SO_NGAY_TOI_DA, soNhacToiDa = SO_NHAC_TOI_DA } = {},
): NhacCanDat[] {
  const gioiHan = now.getTime() + soNgayToiDa * 24 * 60 * 60 * 1000;
  const theoIdentifier = new Map<string, NhacCanDat>();

  for (const sk of suKien) {
    const batDau = docThoiDiem(sk.ngay, sk.gio);
    if (!batDau) continue;
    const moc: [MocNhac, Date][] = [
      ['truoc15', new Date(batDau.getTime() - PHUT_NHAC_TRUOC * 60 * 1000)],
      ['dung_luc', batDau],
    ];
    for (const [ten, luc] of moc) {
      if (luc.getTime() <= now.getTime() || luc.getTime() > gioiHan) continue;
      const identifier = `${sk.loai}:${sk.maSuKien}:${sk.ngay}:${ten}`;
      theoIdentifier.set(identifier, {
        identifier,
        loai: sk.loai,
        moc: ten,
        luc,
        tieuDe: TIEU_DE[sk.loai][ten],
        noiDung: NOI_DUNG,
      });
    }
  }

  return [...theoIdentifier.values()]
    .sort((a, b) => a.luc.getTime() - b.luc.getTime() || a.identifier.localeCompare(b.identifier))
    .slice(0, soNhacToiDa);
}

export interface KetQuaDongBoNhac {
  them: number;
  doi: number;
  huy: number;
  giuNguyen: number;
  loi: number;
}

/**
 * Đồng bộ lịch nhắc trên máy với `suKien`: thêm nhắc còn thiếu, đặt lại nhắc đổi giờ (cùng identifier nên thay thế), hủy nhắc
 * không còn cần (sự kiện bị hủy, đã xong, hoặc đã hết hạn). Chỉ chạm vào nhắc có tiền tố của mình. Lỗi ở một nhắc được đếm và
 * bỏ qua để các nhắc còn lại vẫn được đặt.
 */
export async function dongBoNhacGioAsync(
  suKien: readonly SuKienNhac[],
  { adapter = layNotificationAdapter(), now = new Date() }: { adapter?: NotificationAdapter | null; now?: Date } = {},
): Promise<KetQuaDongBoNhac> {
  const ketQua: KetQuaDongBoNhac = { them: 0, doi: 0, huy: 0, giuNguyen: 0, loi: 0 };
  if (!adapter) return ketQua;

  const canCo = tinhLichNhac(suKien, now);
  const canCoTheoId = new Map(canCo.map((n) => [n.identifier, n]));
  const daLap = (await adapter.getAllScheduledNotificationsAsync()).filter((r) => laNhacCuaMinh(r.identifier));
  const daLapTheoId = new Map(daLap.map((r) => [r.identifier, r]));

  for (const r of daLap) {
    if (canCoTheoId.has(r.identifier)) continue;
    try {
      await adapter.cancelScheduledNotificationAsync(r.identifier);
      ketQua.huy++;
    } catch {
      ketQua.loi++;
    }
  }

  for (const nhac of canCo) {
    const hienCo = daLapTheoId.get(nhac.identifier);
    if (hienCo && hienCo.content.data?.luc === nhac.luc.getTime()) {
      ketQua.giuNguyen++;
      continue;
    }
    try {
      await adapter.scheduleNotificationAsync({
        identifier: nhac.identifier,
        content: {
          title: nhac.tieuDe,
          body: nhac.noiDung,
          data: { loai_thong_bao: nhac.loai, luc: nhac.luc.getTime() },
        },
        trigger: { type: 'date', date: nhac.luc, channelId: nhac.loai },
      });
      if (hienCo) ketQua.doi++;
      else ketQua.them++;
    } catch {
      ketQua.loi++;
    }
  }

  return ketQua;
}

/**
 * Chỉ để thử: đặt ngay một nhắc tập trên máy sau `giay` giây, đi qua đúng đường (kênh, quyền, lịch hệ thống) như nhắc thật. Mang tiền tố
 * riêng nên đồng bộ không dọn nó. Trả identifier, hoặc `null` khi không có adapter.
 */
// TODO(THONGBAO): bỏ cùng nút thử ở màn thông báo khi đã thử xong nhắc giờ trên máy thật.
export async function datNhacThuAsync(
  giay: number,
  { adapter = layNotificationAdapter(), now = new Date() }: { adapter?: NotificationAdapter | null; now?: Date } = {},
): Promise<string | null> {
  if (!adapter) return null;
  const identifier = `nhac_thu:${now.getTime()}`;
  await adapter.scheduleNotificationAsync({
    identifier,
    content: { title: TIEU_DE.nhac_tap.dung_luc, body: NOI_DUNG, data: { loai_thong_bao: 'nhac_tap' } },
    trigger: { type: 'date', date: new Date(now.getTime() + giay * 1000), channelId: 'nhac_tap' },
  });
  return identifier;
}

/** Hủy mọi nhắc của mình, dùng khi đăng xuất để máy dùng chung không nhắc theo lịch của người trước. */
export async function huyTatCaNhacGioAsync(adapter: NotificationAdapter | null = layNotificationAdapter()): Promise<void> {
  if (!adapter) return;
  for (const r of await adapter.getAllScheduledNotificationsAsync()) {
    if (laNhacCuaMinh(r.identifier)) await adapter.cancelScheduledNotificationAsync(r.identifier);
  }
}
