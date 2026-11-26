import { Platform } from 'react-native';
import { layNotificationAdapter, type NotificationAdapter } from './notification-adapter';
import { LOAI_THONG_BAO, type LoaiThongBao } from './notification.types';

/** Mức quan trọng của kênh Android: 5 là MAX (hiện banner, bật sáng màn hình), 3 là mặc định. */
const MAX = 5;
const MAC_DINH = 3;

/**
 * Mỗi loại thông báo là một kênh Android cùng id, để người dùng chỉnh được từng loại trong cài đặt hệ thống (ADR-2, quyết định 19).
 * Server đặt `channel_id` của push bằng đúng `loai_thong_bao`.
 */
export const KENH_ANDROID: Record<LoaiThongBao, { name: string; importance: number; vibrationPattern?: number[] }> = {
  tin_nhan: { name: 'Tin nhắn', importance: MAX, vibrationPattern: [0, 250, 250, 250] },
  nhac_tap: { name: 'Nhắc tập', importance: MAX, vibrationPattern: [0, 500, 250, 500] },
  nhac_an: { name: 'Nhắc ăn', importance: MAX, vibrationPattern: [0, 500, 250, 500] },
  ket_noi_pt: { name: 'Kết nối PT', importance: MAC_DINH },
  danh_gia: { name: 'Đánh giá', importance: MAC_DINH },
};

/**
 * Cài cách hiện thông báo khi app đang mở và tạo kênh trên Android. Gọi một lần khi vào app.
 * Không đặt `sound`: bỏ khóa này thì Android dùng âm mặc định; chuỗi 'default' bị coi là tên file âm thanh tùy chỉnh.
 */
export async function setupNotificationChannelsAsync(
  adapter: NotificationAdapter | null = layNotificationAdapter(),
  os: string = Platform.OS,
): Promise<void> {
  if (!adapter) return;

  adapter.setNotificationHandler({
    handleNotification: async () => ({
      shouldShowBanner: true,
      shouldShowList: true,
      shouldPlaySound: true,
      shouldSetBadge: false,
    }),
  });

  if (os !== 'android') return;
  for (const loai of LOAI_THONG_BAO) {
    const kenh = KENH_ANDROID[loai];
    await adapter.setNotificationChannelAsync(loai, {
      name: kenh.name,
      importance: kenh.importance,
      lockscreenVisibility: 1, // PUBLIC: nội dung push đã trung tính (ADR-2, quyết định 7)
      enableVibrate: true,
      ...(kenh.vibrationPattern ? { vibrationPattern: kenh.vibrationPattern } : {}),
    });
  }
}

/**
 * Xin quyền hiện thông báo của hệ điều hành (Android 13+ và iOS). Đã có quyền thì không hỏi lại; người dùng đã từ chối hẳn
 * thì không hỏi nữa. Trả `true` khi đang có quyền.
 */
export async function xinQuyenThongBaoAsync(
  adapter: NotificationAdapter | null = layNotificationAdapter(),
): Promise<boolean> {
  if (!adapter) return false;

  const hienTai = await adapter.getPermissionsAsync();
  if (hienTai.granted) return true;
  if (hienTai.canAskAgain === false) return false;

  const ketQua = await adapter.requestPermissionsAsync({
    ios: { allowAlert: true, allowBadge: true, allowSound: true },
  });
  return ketQua.granted;
}
