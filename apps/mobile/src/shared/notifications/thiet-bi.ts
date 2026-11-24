import { Platform } from 'react-native';
import { getMobClient } from '@/core/api/mobile-client';
import { layMaThietBi } from './device-id';
import { layNotificationAdapter, type NotificationAdapter } from './notification-adapter';

export interface DangKyThietBiInput {
  ma_thiet_bi: string;
  nen_tang: 'android' | 'ios';
  /** Null khi chưa có quyền thông báo hoặc không lấy được token (Expo Go, chưa cấu hình Firebase). */
  push_token: string | null;
  mui_gio: string | null;
}

/** Các endpoint `/api/v1/mob/thiet-bi`. */
export const thietBiApi = {
  async dangKy(input: DangKyThietBiInput): Promise<void> {
    await getMobClient().put('/thiet-bi', input);
  },
  async go(maThietBi: string): Promise<void> {
    await getMobClient().delete(`/thiet-bi/${encodeURIComponent(maThietBi)}`);
  },
};

export interface ThietBiDeps {
  adapter?: NotificationAdapter | null;
  api?: Pick<typeof thietBiApi, 'dangKy' | 'go'>;
  layMa?: () => Promise<string>;
  os?: string;
  layMuiGio?: () => string | null;
}

/** Múi giờ IANA của máy, ví dụ `Asia/Ho_Chi_Minh`. */
export function layMuiGioCuaMay(): string | null {
  try {
    return Intl.DateTimeFormat().resolvedOptions().timeZone || null;
  } catch {
    return null;
  }
}

/**
 * Báo server thiết bị này và push token mới nhất (ADR-2, quyết định 9). Gọi khi vào app, khi quay lại foreground và khi token đổi.
 * Chưa có quyền hoặc không lấy được token thì vẫn đăng ký thiết bị, chỉ không có token.
 */
export async function dongBoThietBiAsync(
  deps: ThietBiDeps = {},
): Promise<{ daDangKy: boolean; coToken: boolean }> {
  const os = deps.os ?? Platform.OS;
  if (os !== 'android' && os !== 'ios') return { daDangKy: false, coToken: false };

  const adapter = deps.adapter === undefined ? layNotificationAdapter() : deps.adapter;
  let token: string | null = null;
  if (adapter) {
    const quyen = await adapter.getPermissionsAsync().catch(() => null);
    if (quyen?.granted) {
      try {
        const ketQua = await adapter.getDevicePushTokenAsync();
        token = typeof ketQua.data === 'string' && ketQua.data.length > 0 ? ketQua.data : null;
      } catch {
        token = null;
      }
    }
  }

  await (deps.api ?? thietBiApi).dangKy({
    ma_thiet_bi: await (deps.layMa ?? layMaThietBi)(),
    nen_tang: os,
    push_token: token,
    mui_gio: (deps.layMuiGio ?? layMuiGioCuaMay)(),
  });
  return { daDangKy: true, coToken: token !== null };
}

/** Gỡ token của thiết bị này trước khi đăng xuất, để người dùng kế tiếp trên máy không nhận thông báo của người trước. */
export async function goThietBiAsync(deps: Pick<ThietBiDeps, 'api' | 'layMa'> = {}): Promise<void> {
  await (deps.api ?? thietBiApi).go(await (deps.layMa ?? layMaThietBi)());
}
