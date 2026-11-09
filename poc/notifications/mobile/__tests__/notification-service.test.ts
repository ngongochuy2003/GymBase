import {
  displayUrgentMessageAsync,
  NotificationAdapter,
  requestNotificationPermissionsAsync,
  scheduleWorkoutAlarmAsync,
  setCustomNotificationAdapter,
  setupNotificationChannelsAsync,
} from '../src/services/notification-service';

describe('Notification Service (Lock Screen & Alarm)', () => {
  let mockAdapter: jest.Mocked<NotificationAdapter>;

  beforeEach(() => {
    mockAdapter = {
      setNotificationChannelAsync: jest.fn().mockResolvedValue({}),
      scheduleNotificationAsync: jest.fn().mockResolvedValue('notif-id-12345'),
      requestPermissionsAsync: jest.fn().mockResolvedValue({ status: 'granted', granted: true }),
      setNotificationHandler: jest.fn(),
    };
    setCustomNotificationAdapter(mockAdapter);
  });

  it('setupNotificationChannelsAsync đăng ký đúng 2 kênh Android với IMPORTANCE_MAX và lockscreenVisibility PUBLIC', async () => {
    await setupNotificationChannelsAsync(mockAdapter);

    expect(mockAdapter.setNotificationHandler).toHaveBeenCalled();
    expect(mockAdapter.setNotificationChannelAsync).toHaveBeenCalledTimes(2);

    // Kênh tin nhắn khẩn cấp
    expect(mockAdapter.setNotificationChannelAsync).toHaveBeenCalledWith(
      'gymbase_urgent_messages',
      expect.objectContaining({
        importance: 5, // AndroidImportance.MAX
        lockscreenVisibility: 1, // AndroidNotificationVisibility.PUBLIC
        bypassDnd: true,
      })
    );

    // Kênh báo thức
    expect(mockAdapter.setNotificationChannelAsync).toHaveBeenCalledWith(
      'gymbase_alarms',
      expect.objectContaining({
        importance: 5,
        lockscreenVisibility: 1,
      })
    );
  });

  it('requestNotificationPermissionsAsync trả về true khi người dùng cho phép', async () => {
    const granted = await requestNotificationPermissionsAsync(mockAdapter);

    expect(granted).toBe(true);
    expect(mockAdapter.requestPermissionsAsync).toHaveBeenCalled();
  });

  it('displayUrgentMessageAsync tạo thông báo với priority max và channel gymbase_urgent_messages', async () => {
    const notifId = await displayUrgentMessageAsync(
      {
        title: 'HLV Tuấn Anh',
        body: 'Hôm nay tập ngực lúc 18h nhé!',
        data: { type: 'chat_message', conversation_id: 'conv-01' },
      },
      mockAdapter
    );

    expect(notifId).toBe('notif-id-12345');
    expect(mockAdapter.scheduleNotificationAsync).toHaveBeenCalledWith(
      expect.objectContaining({
        content: expect.objectContaining({
          title: 'HLV Tuấn Anh',
          body: 'Hôm nay tập ngực lúc 18h nhé!',
          priority: 'max',
        }),
        trigger: expect.objectContaining({
          channelId: 'gymbase_urgent_messages',
        }),
      })
    );
  });

  it('scheduleWorkoutAlarmAsync lên lịch báo thức với channel gymbase_alarms', async () => {
    const triggerDate = new Date(Date.now() + 3600 * 1000);
    const notifId = await scheduleWorkoutAlarmAsync(
      {
        title: 'Báo thức giờ tập Gym',
        body: 'Chuẩn bị quần áo và bình nước, buổi tập bắt đầu sau 30 phút!',
        triggerDate,
        workoutSessionId: 'sess-999',
      },
      mockAdapter
    );

    expect(notifId).toBe('notif-id-12345');
    expect(mockAdapter.scheduleNotificationAsync).toHaveBeenCalledWith(
      expect.objectContaining({
        trigger: expect.objectContaining({
          channelId: 'gymbase_alarms',
          date: triggerDate,
        }),
      })
    );
  });
});
