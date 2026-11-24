/** Kết quả của `getPermissionsAsync` và `requestPermissionsAsync`; chỉ phần app dùng. */
export interface NotificationPermission {
  granted: boolean;
  canAskAgain?: boolean;
}

export interface NotificationSubscription {
  remove(): void;
}

/** Phần của `expo-notifications` mà app dùng. Tách ra để test bằng bản giả và để web, Jest không cần module native. */
export interface NotificationAdapter {
  setNotificationHandler(handler: Record<string, unknown> | null): void;
  setNotificationChannelAsync(channelId: string, channel: Record<string, unknown>): Promise<unknown>;
  getPermissionsAsync(): Promise<NotificationPermission>;
  requestPermissionsAsync(permissions?: Record<string, unknown>): Promise<NotificationPermission>;
  getDevicePushTokenAsync(): Promise<{ type: string; data: unknown }>;
  addPushTokenListener(listener: (token: { data: unknown }) => void): NotificationSubscription;
  addNotificationReceivedListener(listener: () => void): NotificationSubscription;
  addNotificationResponseReceivedListener(listener: () => void): NotificationSubscription;
}

let adapter: NotificationAdapter | null | undefined;

/** Adapter thật (`expo-notifications`); `null` khi module không nạp được (Jest, môi trường không có native). */
export function layNotificationAdapter(): NotificationAdapter | null {
  if (adapter === undefined) {
    try {
      // eslint-disable-next-line @typescript-eslint/no-require-imports
      adapter = require('expo-notifications') as NotificationAdapter;
    } catch {
      adapter = null;
    }
  }
  return adapter;
}

/** Chỉ cho test: thay adapter, hoặc `undefined` để nạp lại adapter thật. */
export function datNotificationAdapterChoTest(value: NotificationAdapter | null | undefined): void {
  adapter = value;
}
