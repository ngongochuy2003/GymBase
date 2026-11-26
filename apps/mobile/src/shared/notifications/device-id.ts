import * as SecureStore from 'expo-secure-store';

const KEY = 'gymbase.device.id';

export interface DeviceIdStorage {
  get(): Promise<string | null>;
  set(value: string): Promise<void>;
}

const secureStorage: DeviceIdStorage = {
  get: () => SecureStore.getItemAsync(KEY),
  set: (value) => SecureStore.setItemAsync(KEY, value),
};

/** 32 ký tự hex ngẫu nhiên. Chỉ để phân biệt các lần cài đặt, không phải bí mật. */
export function taoMaThietBiMoi(): string {
  const hex = () => Math.floor(Math.random() * 0x10000).toString(16).padStart(4, '0');
  return `gb-${Array.from({ length: 8 }, hex).join('')}`;
}

/**
 * Mã thiết bị do app cấp và nhớ lại (định danh cài đặt, không dùng định danh quảng cáo). Server upsert thiết bị theo mã này
 * cùng tài khoản.
 */
export async function layMaThietBi(storage: DeviceIdStorage = secureStorage): Promise<string> {
  const daCo = await storage.get().catch(() => null);
  if (daCo) return daCo;
  const moi = taoMaThietBiMoi();
  await storage.set(moi).catch(() => undefined);
  return moi;
}
