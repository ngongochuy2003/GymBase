/** Kết quả của `getPermissionsAsync` và `requestPermissionsAsync`; chỉ phần app dùng. */
export interface NotificationPermission {
  granted: boolean;
  canAskAgain?: boolean;
}

export interface NotificationSubscription {
  remove(): void;
}

/** Một thông báo đã lên lịch trên máy; chỉ phần app đọc lại. */
export interface ScheduledNotification {
  identifier: string;
  content: { data?: Record<string, unknown> | null };
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
  scheduleNotificationAsync(request: Record<string, unknown>): Promise<string>;
  getAllScheduledNotificationsAsync(): Promise<ScheduledNotification[]>;
  cancelScheduledNotificationAsync(identifier: string): Promise<void>;
  /** Chỉ có trên Android 12 trở lên; nền tảng khác thiếu hàm hoặc ném lỗi. */
  getExactAlarmPermissionsAsync?(): Promise<{ granted: boolean }>;
  requestExactAlarmPermissionsAsync?(): Promise<{ granted: boolean }>;
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
