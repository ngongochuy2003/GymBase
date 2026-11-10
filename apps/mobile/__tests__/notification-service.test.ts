import {
  displayUrgentMessageAsync,
  NotificationAdapter,
  requestNotificationPermissionsAsync,
  scheduleWorkoutAlarmAsync,
  setCustomNotificationAdapter,
  setupNotificationChannelsAsync,
} from '../src/shared/notifications';

describe('Notification Service (Mobile)', () => {
  let mockAdapter: jest.Mocked<NotificationAdapter>;

  beforeEach(() => {
    mockAdapter = {
      setNotificationChannelAsync: jest.fn().mockResolvedValue({}),
      scheduleNotificationAsync: jest.fn().mockResolvedValue('notif-id-999'),
      requestPermissionsAsync: jest.fn().mockResolvedValue({ status: 'granted', granted: true }),
      setNotificationHandler: jest.fn(),
    };
    setCustomNotificationAdapter(mockAdapter);
  });

  it('setupNotificationChannelsAsync đăng ký 2 kênh IMPORTANCE_MAX và lockscreenVisibility PUBLIC', async () => {
    await setupNotificationChannelsAsync(mockAdapter);

    expect(mockAdapter.setNotificationHandler).toHaveBeenCalled();
    expect(mockAdapter.setNotificationChannelAsync).toHaveBeenCalledTimes(2);

    expect(mockAdapter.setNotificationChannelAsync).toHaveBeenCalledWith(
      'gymbase_urgent_messages',
      expect.objectContaining({
        importance: 5, // AndroidImportance.MAX
        lockscreenVisibility: 1, // PUBLIC
        bypassDnd: true,
      }),
    );

    expect(mockAdapter.setNotificationChannelAsync).toHaveBeenCalledWith(
      'gymbase_alarms',
      expect.objectContaining({
        importance: 5,
        lockscreenVisibility: 1,
      }),
    );
  });

  it('requestNotificationPermissionsAsync trả về true khi được cấp quyền', async () => {
    const granted = await requestNotificationPermissionsAsync(mockAdapter);
    expect(granted).toBe(true);
    expect(mockAdapter.requestPermissionsAsync).toHaveBeenCalled();
  });

  it('displayUrgentMessageAsync tạo notification khẩn với priority max trên màn hình khóa', async () => {
    const notifId = await displayUrgentMessageAsync(
      {
        title: 'HLV Tuấn Anh',
        body: 'Hôm nay tập ngực lúc 18h nhé!',
        data: { type: 'chat_message', conversation_id: 'conv-01' },
      },
      mockAdapter,
    );

    expect(notifId).toBe('notif-id-999');
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
      }),
    );
  });

  it('scheduleWorkoutAlarmAsync lên lịch báo thức reo chuông đúng giờ', async () => {
    const triggerDate = new Date(Date.now() + 1800 * 1000);
    const notifId = await scheduleWorkoutAlarmAsync(
      {
        title: 'Báo thức giờ tập Gym',
        body: 'Buổi tập bắt đầu sau 30 phút!',
        triggerDate,
        workoutSessionId: 'session-123',
      },
      mockAdapter,
    );

    expect(notifId).toBe('notif-id-999');
    expect(mockAdapter.scheduleNotificationAsync).toHaveBeenCalledWith(
      expect.objectContaining({
        trigger: expect.objectContaining({
          channelId: 'gymbase_alarms',
          date: triggerDate,
        }),
      }),
    );
  });
});
