import {
  dongBoThietBiAsync,
  goThietBiAsync,
  layMaThietBi,
  type DeviceIdStorage,
  type NotificationAdapter,
} from '../src/shared/notifications';

function taoAdapter(
  quyen: { granted: boolean },
  layToken: () => Promise<{ type: string; data: unknown }>,
): NotificationAdapter {
  return {
    setNotificationHandler: jest.fn(),
    setNotificationChannelAsync: jest.fn(),
    getPermissionsAsync: jest.fn().mockResolvedValue(quyen),
    requestPermissionsAsync: jest.fn(),
    getDevicePushTokenAsync: jest.fn(layToken),
    addPushTokenListener: jest.fn(),
    addNotificationReceivedListener: jest.fn(),
    addNotificationResponseReceivedListener: jest.fn(),
  };
}

const api = () => ({ dangKy: jest.fn().mockResolvedValue(undefined), go: jest.fn().mockResolvedValue(undefined) });
const layMa = async () => 'gb-may-1';
const layMuiGio = () => 'Asia/Ho_Chi_Minh';

describe('dongBoThietBiAsync', () => {
  it('đăng ký thiết bị kèm push token khi đã có quyền', async () => {
    const server = api();
    const adapter = taoAdapter({ granted: true }, async () => ({ type: 'android', data: 'fcm-token-1' }));

    const ketQua = await dongBoThietBiAsync({ adapter, api: server, layMa, layMuiGio, os: 'android' });

    expect(ketQua).toEqual({ daDangKy: true, coToken: true });
    expect(server.dangKy).toHaveBeenCalledWith({
      ma_thiet_bi: 'gb-may-1',
      nen_tang: 'android',
      push_token: 'fcm-token-1',
      mui_gio: 'Asia/Ho_Chi_Minh',
    });
  });

  it('chưa có quyền thông báo: vẫn đăng ký thiết bị nhưng không lấy token', async () => {
    const server = api();
    const adapter = taoAdapter({ granted: false }, async () => ({ type: 'android', data: 'x' }));

    const ketQua = await dongBoThietBiAsync({ adapter, api: server, layMa, layMuiGio, os: 'ios' });

    expect(ketQua).toEqual({ daDangKy: true, coToken: false });
    expect(adapter.getDevicePushTokenAsync).not.toHaveBeenCalled();
    expect(server.dangKy).toHaveBeenCalledWith(expect.objectContaining({ nen_tang: 'ios', push_token: null }));
  });

  it('không lấy được token (Expo Go, chưa cấu hình Firebase): vẫn đăng ký, push_token null', async () => {
    const server = api();
    const adapter = taoAdapter({ granted: true }, async () => {
      throw new Error('Default FirebaseApp is not initialized');
    });

    const ketQua = await dongBoThietBiAsync({ adapter, api: server, layMa, layMuiGio, os: 'android' });

    expect(ketQua).toEqual({ daDangKy: true, coToken: false });
    expect(server.dangKy).toHaveBeenCalledWith(expect.objectContaining({ push_token: null }));
  });

  it('token không phải chuỗi thì coi như không có', async () => {
    const server = api();
    const adapter = taoAdapter({ granted: true }, async () => ({ type: 'web', data: { endpoint: 'x' } }));

    await dongBoThietBiAsync({ adapter, api: server, layMa, layMuiGio, os: 'android' });

    expect(server.dangKy).toHaveBeenCalledWith(expect.objectContaining({ push_token: null }));
  });

  it('không có adapter thì vẫn đăng ký thiết bị không token', async () => {
    const server = api();

    await dongBoThietBiAsync({ adapter: null, api: server, layMa, layMuiGio, os: 'android' });

    expect(server.dangKy).toHaveBeenCalledWith(expect.objectContaining({ push_token: null }));
  });

  it('trên web thì bỏ qua, không gọi server', async () => {
    const server = api();

    const ketQua = await dongBoThietBiAsync({ api: server, layMa, os: 'web' });

    expect(ketQua).toEqual({ daDangKy: false, coToken: false });
    expect(server.dangKy).not.toHaveBeenCalled();
  });

  it('lỗi từ server được ném ra để nơi gọi quyết định', async () => {
    const server = api();
    server.dangKy.mockRejectedValue(new Error('mạng'));

    await expect(
      dongBoThietBiAsync({ adapter: null, api: server, layMa, layMuiGio, os: 'android' }),
    ).rejects.toThrow('mạng');
  });
});

describe('goThietBiAsync', () => {
  it('gỡ thiết bị theo mã của máy này', async () => {
    const server = api();

    await goThietBiAsync({ api: server, layMa });

    expect(server.go).toHaveBeenCalledWith('gb-may-1');
  });
});

describe('layMaThietBi', () => {
  function taoStorage(giaTri: string | null = null) {
    let hienTai = giaTri;
    const storage: DeviceIdStorage = {
      get: jest.fn(async () => hienTai),
      set: jest.fn(async (v: string) => {
        hienTai = v;
      }),
    };
    return storage;
  }

  it('lần đầu tạo mã mới và lưu lại; lần sau dùng lại đúng mã đó', async () => {
    const storage = taoStorage();

    const lan1 = await layMaThietBi(storage);
    const lan2 = await layMaThietBi(storage);

    expect(lan1).toMatch(/^gb-[0-9a-f]{32}$/);
    expect(lan2).toBe(lan1);
    expect(storage.set).toHaveBeenCalledTimes(1);
  });

  it('dùng mã đã lưu sẵn', async () => {
    await expect(layMaThietBi(taoStorage('gb-da-co'))).resolves.toBe('gb-da-co');
  });

  it('không lưu được thì vẫn trả một mã để đăng ký tiếp', async () => {
    const storage: DeviceIdStorage = {
      get: async () => null,
      set: async () => {
        throw new Error('SecureStore lỗi');
      },
    };

    await expect(layMaThietBi(storage)).resolves.toMatch(/^gb-/);
  });
});
