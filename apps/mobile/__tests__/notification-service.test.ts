import {
  KENH_ANDROID,
  LOAI_THONG_BAO,
  setupNotificationChannelsAsync,
  xinQuyenThongBaoAsync,
  type NotificationAdapter,
} from '../src/shared/notifications';

function taoAdapter(quyen: { granted: boolean; canAskAgain?: boolean } = { granted: true }) {
  const adapter: jest.Mocked<NotificationAdapter> = {
    setNotificationHandler: jest.fn(),
    setNotificationChannelAsync: jest.fn().mockResolvedValue({}),
    getPermissionsAsync: jest.fn().mockResolvedValue(quyen),
    requestPermissionsAsync: jest.fn().mockResolvedValue({ granted: true }),
    getDevicePushTokenAsync: jest.fn(),
    addPushTokenListener: jest.fn(),
    addNotificationReceivedListener: jest.fn(),
    addNotificationResponseReceivedListener: jest.fn(),
    scheduleNotificationAsync: jest.fn(),
    getAllScheduledNotificationsAsync: jest.fn(),
    cancelScheduledNotificationAsync: jest.fn(),
  };
  return adapter;
}

describe('setupNotificationChannelsAsync', () => {
  it('tạo một kênh Android cho mỗi loại thông báo, id trùng tên loại', async () => {
    const adapter = taoAdapter();

    await setupNotificationChannelsAsync(adapter, 'android');

    expect(adapter.setNotificationChannelAsync.mock.calls.map(([id]) => id).sort()).toEqual(
      [...LOAI_THONG_BAO].sort(),
    );
    expect(adapter.setNotificationChannelAsync).toHaveBeenCalledWith(
      'tin_nhan',
      expect.objectContaining({ name: KENH_ANDROID.tin_nhan.name, importance: 5, lockscreenVisibility: 1 }),
    );
    expect(adapter.setNotificationChannelAsync).toHaveBeenCalledWith(
      'danh_gia',
      expect.objectContaining({ importance: 3 }),
    );
  });

  it('không đặt sound (chuỗi "default" bị Android coi là tên file âm thanh)', async () => {
    const adapter = taoAdapter();

    await setupNotificationChannelsAsync(adapter, 'android');

    for (const [, kenh] of adapter.setNotificationChannelAsync.mock.calls) {
      expect(kenh).not.toHaveProperty('sound');
    }
  });

  it('trên iOS chỉ cài cách hiện thông báo, không tạo kênh', async () => {
    const adapter = taoAdapter();

    await setupNotificationChannelsAsync(adapter, 'ios');

    expect(adapter.setNotificationHandler).toHaveBeenCalledTimes(1);
    expect(adapter.setNotificationChannelAsync).not.toHaveBeenCalled();
  });

  it('khi app đang mở vẫn hiện banner và vào danh sách thông báo', async () => {
    const adapter = taoAdapter();
    await setupNotificationChannelsAsync(adapter, 'ios');

    const handler = adapter.setNotificationHandler.mock.calls[0]?.[0] as {
      handleNotification(): Promise<Record<string, boolean>>;
    };

    await expect(handler.handleNotification()).resolves.toMatchObject({
      shouldShowBanner: true,
      shouldShowList: true,
    });
  });

  it('không có adapter (Jest, web) thì bỏ qua, không ném lỗi', async () => {
    await expect(setupNotificationChannelsAsync(null, 'android')).resolves.toBeUndefined();
  });
});

describe('xinQuyenThongBaoAsync', () => {
  it('đã có quyền thì không hỏi lại', async () => {
    const adapter = taoAdapter({ granted: true });

    await expect(xinQuyenThongBaoAsync(adapter)).resolves.toBe(true);
    expect(adapter.requestPermissionsAsync).not.toHaveBeenCalled();
  });

  it('chưa có quyền thì xin và trả kết quả', async () => {
    const adapter = taoAdapter({ granted: false, canAskAgain: true });

    await expect(xinQuyenThongBaoAsync(adapter)).resolves.toBe(true);
    expect(adapter.requestPermissionsAsync).toHaveBeenCalledTimes(1);
  });

  it('người dùng đã từ chối hẳn thì không hỏi nữa', async () => {
    const adapter = taoAdapter({ granted: false, canAskAgain: false });

    await expect(xinQuyenThongBaoAsync(adapter)).resolves.toBe(false);
    expect(adapter.requestPermissionsAsync).not.toHaveBeenCalled();
  });

  it('không có adapter thì trả false', async () => {
    await expect(xinQuyenThongBaoAsync(null)).resolves.toBe(false);
  });
});
