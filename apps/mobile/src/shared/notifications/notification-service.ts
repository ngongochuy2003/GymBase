import {
  AlarmScheduleOptions,
  LockScreenNotificationOptions,
  NotificationChannelId,
} from './notification.types';

export interface NotificationAdapter {
  setNotificationChannelAsync(
    channelId: string,
    channel: Record<string, unknown>,
  ): Promise<unknown>;
  scheduleNotificationAsync(request: Record<string, unknown>): Promise<string>;
  requestPermissionsAsync(
    permissions?: Record<string, unknown>,
  ): Promise<{ status: string; granted: boolean }>;
  setNotificationHandler(handler: Record<string, unknown>): void;
}

let activeAdapter: NotificationAdapter | null = null;

try {
  // eslint-disable-next-line @typescript-eslint/no-require-imports
  const ExpoNotifications = require('expo-notifications');
  activeAdapter = ExpoNotifications;
} catch {
  // Môi trường Node.js / Jest
}

export function setCustomNotificationAdapter(adapter: NotificationAdapter) {
  activeAdapter = adapter;
}

/**
 * 1. Khởi tạo Notification Channels trên Android để thông báo bật sáng màn hình khóa.
 * - gymbase_urgent_messages: IMPORTANCE_MAX (5), visibility PUBLIC (1), rung mạnh.
 * - gymbase_alarms: IMPORTANCE_MAX (5), âm thanh báo thức riêng.
 */
export async function setupNotificationChannelsAsync(adapter?: NotificationAdapter): Promise<void> {
  const current = adapter || activeAdapter;
  if (!current) return;

  current.setNotificationHandler({
    handleNotification: async () => ({
      shouldShowAlert: true,
      shouldPlaySound: true,
      shouldSetBadge: true,
    }),
  });

  // 1. Channel Tin nhắn khẩn cấp (PT <-> Học viên)
  await current.setNotificationChannelAsync('gymbase_urgent_messages', {
    name: 'Tin nhắn khẩn cấp (PT & Học viên)',
    importance: 5, // AndroidImportance.MAX -> Bật sáng màn hình & thả heads-up banner
    vibrationPattern: [0, 250, 250, 250],
    lightColor: '#FF231F7C',
    lockscreenVisibility: 1, // AndroidNotificationVisibility.PUBLIC
    sound: 'default',
    enableLights: true,
    enableVibrate: true,
    bypassDnd: true,
  });

  // 2. Channel Báo thức giờ tập / Giờ ăn
  await current.setNotificationChannelAsync('gymbase_alarms', {
    name: 'Báo thức lịch tập & Dinh dưỡng',
    importance: 5,
    vibrationPattern: [0, 500, 250, 500, 250, 500],
    lockscreenVisibility: 1,
    sound: 'default',
    enableLights: true,
    enableVibrate: true,
    bypassDnd: true,
  });
}

/**
 * 2. Xin cấp quyền hiển thị thông báo (Android 13+ POST_NOTIFICATIONS & iOS)
 */
export async function requestNotificationPermissionsAsync(
  adapter?: NotificationAdapter,
): Promise<boolean> {
  const current = adapter || activeAdapter;
  if (!current) return false;

  const result = await current.requestPermissionsAsync({
    ios: {
      allowAlert: true,
      allowBadge: true,
      allowSound: true,
      allowAnnouncements: true,
    },
  });

  return result.granted || result.status === 'granted';
}

/**
 * 3. Bắn thông báo khẩn cấp lên Màn hình khóa (Lock Screen Heads-up Notification)
 */
export async function displayUrgentMessageAsync(
  options: LockScreenNotificationOptions,
  adapter?: NotificationAdapter,
): Promise<string> {
  const current = adapter || activeAdapter;
  if (!current) {
    throw new Error('Notification adapter chưa được khởi tạo.');
  }

  const channelId: NotificationChannelId = options.channelId || 'gymbase_urgent_messages';

  const notificationId = await current.scheduleNotificationAsync({
    content: {
      title: options.title,
      body: options.body,
      data: options.data || {},
      sound: true,
      vibrate: [0, 250, 250, 250],
      priority: 'max',
    },
    trigger: {
      channelId,
      seconds: 1,
    },
  });

  return notificationId;
}

/**
 * 4. Lên lịch Báo thức nhắc giờ tập (Alarm Style)
 */
export async function scheduleWorkoutAlarmAsync(
  options: AlarmScheduleOptions,
  adapter?: NotificationAdapter,
): Promise<string> {
  const current = adapter || activeAdapter;
  if (!current) {
    throw new Error('Notification adapter chưa được khởi tạo.');
  }

  const notificationId = await current.scheduleNotificationAsync({
    content: {
      title: options.title,
      body: options.body,
      sound: true,
      vibrate: [0, 500, 250, 500],
      priority: 'max',
      data: {
        type: 'alarm',
        workout_session_id: options.workoutSessionId,
      },
    },
    trigger: {
      channelId: 'gymbase_alarms',
      date: options.triggerDate,
    },
  });

  return notificationId;
}
