import {
  dongBoNhacGioAsync,
  datNhacThuAsync,
  huyTatCaNhacGioAsync,
  tinhLichNhac,
  SO_NHAC_TOI_DA,
  type NotificationAdapter,
  type SuKienNhac,
} from '../src/shared/notifications';

// Giờ địa phương của máy: 24/11/2026 12:00.
const NOW = new Date(2026, 10, 24, 12, 0, 0);
const luc = (ngay: number, gio: number, phut = 0) => new Date(2026, 10, ngay, gio, phut, 0);

const tap = (maSuKien: string, ngay: string, gio: string): SuKienNhac => ({ loai: 'nhac_tap', maSuKien, ngay, gio });
const an = (maSuKien: string, ngay: string, gio: string): SuKienNhac => ({ loai: 'nhac_an', maSuKien, ngay, gio });

describe('tinhLichNhac', () => {
  it('mỗi sự kiện có hai mốc: 15 phút trước và đúng lúc, identifier cố định', () => {
    const lich = tinhLichNhac([tap('lt1:tap-1', '2026-11-25', '17:30')], NOW);

    expect(lich.map((n) => [n.identifier, n.moc, n.luc])).toEqual([
      ['nhac_tap:lt1:tap-1:2026-11-25:truoc15', 'truoc15', luc(25, 17, 15)],
      ['nhac_tap:lt1:tap-1:2026-11-25:dung_luc', 'dung_luc', luc(25, 17, 30)],
    ]);
  });

  it('mốc đã qua thì bỏ; sự kiện bắt đầu trong vòng 15 phút chỉ còn mốc đúng lúc', () => {
    const lich = tinhLichNhac(
      [
        tap('a', '2026-11-24', '11:00'), // đã qua cả hai mốc
        tap('b', '2026-11-24', '12:10'), // mốc trước 15 phút (11:55) đã qua
        an('c', '2026-11-24', '12:30'), // còn đủ hai mốc
      ],
      NOW,
    );

    expect(lich.map((n) => `${n.loai}:${n.moc}`)).toEqual(['nhac_tap:dung_luc', 'nhac_an:truoc15', 'nhac_an:dung_luc']);
  });

  it('mốc trùng đúng thời điểm đặt lịch cũng bỏ (không đặt lịch cho quá khứ)', () => {
    const lich = tinhLichNhac([tap('a', '2026-11-24', '12:00')], NOW);

    expect(lich).toEqual([]);
  });

  it('chỉ lấy các nhắc trong cửa sổ số ngày tới', () => {
    const lich = tinhLichNhac([tap('trong', '2026-12-07', '12:00'), tap('ngoai', '2026-12-09', '12:00')], NOW, {
      soNgayToiDa: 14,
    });

    expect(new Set(lich.map((n) => n.identifier.split(':')[1]))).toEqual(new Set(['trong']));
  });

  it('chỉ giữ các nhắc gần nhất khi quá số lượng tối đa, sắp theo thời gian', () => {
    const suKien = Array.from({ length: 40 }, (_, i) => an(`m${i}`, '2026-11-25', `${String(Math.floor(i / 2) + 1).padStart(2, '0')}:${i % 2 ? '30' : '00'}`));

    const lich = tinhLichNhac(suKien, NOW);

    expect(lich).toHaveLength(SO_NHAC_TOI_DA);
    const thoiGian = lich.map((n) => n.luc.getTime());
    expect(thoiGian).toEqual([...thoiGian].sort((a, b) => a - b));
  });

  it('cùng một sự kiện xuất hiện hai lần thì chỉ tính một lần', () => {
    const lich = tinhLichNhac([tap('a', '2026-11-25', '17:30'), tap('a', '2026-11-25', '17:30')], NOW);

    expect(lich).toHaveLength(2);
  });

  it('nội dung trung tính: không ghi giờ, tên bài tập hay món ăn', () => {
    const lich = tinhLichNhac([tap('a', '2026-11-25', '17:30'), an('b', '2026-11-25', '19:00')], NOW);

    expect(new Set(lich.map((n) => n.tieuDe))).toEqual(
      new Set(['Sắp đến giờ tập', 'Đến giờ tập', 'Sắp đến giờ ăn', 'Đến giờ ăn']),
    );
    for (const nhac of lich) {
      expect(nhac.noiDung).toBe('Mở GymBase để xem chi tiết.');
      expect(`${nhac.tieuDe} ${nhac.noiDung}`).not.toMatch(/\d{1,2}[:h]\d{2}/);
    }
  });

  it('bỏ qua sự kiện sai định dạng ngày hoặc giờ thay vì ném lỗi', () => {
    const lich = tinhLichNhac(
      [tap('a', '25/11/2026', '17:30'), tap('b', '2026-11-25', '25:99'), tap('c', '2026-11-25', 'abc'), tap('ok', '2026-11-25', '17:30')],
      NOW,
    );

    expect(new Set(lich.map((n) => n.identifier.split(':')[1]))).toEqual(new Set(['ok']));
  });
});

type Dat = { identifier: string; content: { title: string; data?: Record<string, unknown> }; trigger: { date: Date; channelId?: string } };

function taoAdapter(daLap: { identifier: string; luc?: number }[] = []) {
  const lap = new Map<string, { identifier: string; content: { data?: Record<string, unknown> } }>(
    daLap.map((r) => [r.identifier, { identifier: r.identifier, content: { data: r.luc === undefined ? {} : { luc: r.luc } } }]),
  );
  const adapter = {
    getAllScheduledNotificationsAsync: jest.fn(async () => [...lap.values()]),
    cancelScheduledNotificationAsync: jest.fn(async (id: string) => {
      lap.delete(id);
    }),
    scheduleNotificationAsync: jest.fn(async (req: Dat) => {
      lap.set(req.identifier, { identifier: req.identifier, content: { data: req.content.data } });
      return req.identifier;
    }),
  };
  return { adapter: adapter as unknown as NotificationAdapter, mock: adapter, lap };
}

describe('dongBoNhacGioAsync', () => {
  const suKien = [tap('a', '2026-11-25', '17:30')];

  it('đặt lịch hai mốc: trigger theo ngày giờ, kênh bằng loại, kèm mốc giờ để so sánh lần sau', async () => {
    const { adapter, mock } = taoAdapter();

    const ketQua = await dongBoNhacGioAsync(suKien, { adapter, now: NOW });

    expect(ketQua).toEqual({ them: 2, doi: 0, huy: 0, giuNguyen: 0, loi: 0 });
    expect(mock.scheduleNotificationAsync).toHaveBeenCalledWith(
      expect.objectContaining({
        identifier: 'nhac_tap:a:2026-11-25:dung_luc',
        content: expect.objectContaining({
          title: 'Đến giờ tập',
          data: expect.objectContaining({ loai_thong_bao: 'nhac_tap', luc: luc(25, 17, 30).getTime() }),
        }),
        trigger: expect.objectContaining({ type: 'date', date: luc(25, 17, 30), channelId: 'nhac_tap' }),
      }),
    );
  });

  it('idempotent: chạy lại với cùng dữ liệu thì không đặt thêm, không nhân đôi', async () => {
    const { adapter, mock, lap } = taoAdapter();
    await dongBoNhacGioAsync(suKien, { adapter, now: NOW });
    mock.scheduleNotificationAsync.mockClear();

    const ketQua = await dongBoNhacGioAsync(suKien, { adapter, now: NOW });

    expect(ketQua).toEqual({ them: 0, doi: 0, huy: 0, giuNguyen: 2, loi: 0 });
    expect(mock.scheduleNotificationAsync).not.toHaveBeenCalled();
    expect(lap.size).toBe(2);
  });

  it('giờ của sự kiện đổi thì đặt lại cùng identifier (thay thế, không thêm bản mới)', async () => {
    const { adapter, mock, lap } = taoAdapter();
    await dongBoNhacGioAsync(suKien, { adapter, now: NOW });
    mock.scheduleNotificationAsync.mockClear();

    const ketQua = await dongBoNhacGioAsync([tap('a', '2026-11-25', '18:00')], { adapter, now: NOW });

    expect(ketQua.doi + ketQua.them).toBeGreaterThan(0);
    expect(lap.size).toBe(2);
    expect(lap.get('nhac_tap:a:2026-11-25:dung_luc')?.content.data?.luc).toBe(luc(25, 18, 0).getTime());
  });

  it('sự kiện bị hủy hoặc đã hoàn thành thì hủy cả hai mốc', async () => {
    const { adapter, lap } = taoAdapter();
    await dongBoNhacGioAsync(suKien, { adapter, now: NOW });

    const ketQua = await dongBoNhacGioAsync([], { adapter, now: NOW });

    expect(ketQua.huy).toBe(2);
    expect(lap.size).toBe(0);
  });

  it('chỉ chạm vào nhắc có tiền tố của mình, không đụng lịch của thứ khác', async () => {
    const { adapter, mock, lap } = taoAdapter([{ identifier: 'khac:1:thu-vien', luc: 1 }]);

    await dongBoNhacGioAsync([], { adapter, now: NOW });

    expect(mock.cancelScheduledNotificationAsync).not.toHaveBeenCalled();
    expect(lap.has('khac:1:thu-vien')).toBe(true);
  });

  it('nhắc cũ đã hết hạn (mốc đã qua) được dọn khi đồng bộ', async () => {
    const { adapter, lap } = taoAdapter([{ identifier: 'nhac_an:cu:2026-11-20:dung_luc', luc: luc(20, 7).getTime() }]);

    const ketQua = await dongBoNhacGioAsync([], { adapter, now: NOW });

    expect(ketQua.huy).toBe(1);
    expect(lap.size).toBe(0);
  });

  it('lỗi ở một nhắc không làm mất các nhắc còn lại', async () => {
    const { adapter, mock } = taoAdapter();
    mock.scheduleNotificationAsync.mockRejectedValueOnce(new Error('iOS đã đủ 64 lịch'));

    const ketQua = await dongBoNhacGioAsync(suKien, { adapter, now: NOW });

    expect(ketQua).toMatchObject({ them: 1, loi: 1 });
  });

  it('không có adapter thì bỏ qua', async () => {
    await expect(dongBoNhacGioAsync(suKien, { adapter: null, now: NOW })).resolves.toEqual({
      them: 0, doi: 0, huy: 0, giuNguyen: 0, loi: 0,
    });
  });
});

describe('datNhacThuAsync', () => {
  it('đặt một nhắc tập sau số giây cho trước, trên kênh nhac_tap', async () => {
    const { adapter, mock } = taoAdapter();

    const id = await datNhacThuAsync(30, { adapter, now: NOW });

    expect(id).toMatch(/^nhac_thu:/);
    expect(mock.scheduleNotificationAsync).toHaveBeenCalledWith(
      expect.objectContaining({
        identifier: id,
        trigger: expect.objectContaining({ type: 'date', channelId: 'nhac_tap', date: new Date(NOW.getTime() + 30_000) }),
      }),
    );
  });

  it('không bị đồng bộ dọn đi (không mang tiền tố nhắc của kế hoạch)', async () => {
    const { adapter, lap } = taoAdapter();
    const id = await datNhacThuAsync(30, { adapter, now: NOW });

    await dongBoNhacGioAsync([], { adapter, now: NOW });

    expect(lap.has(id as string)).toBe(true);
  });

  it('không có adapter thì trả null', async () => {
    await expect(datNhacThuAsync(30, { adapter: null, now: NOW })).resolves.toBeNull();
  });
});

describe('huyTatCaNhacGioAsync', () => {
  it('hủy mọi nhắc của mình (khi đăng xuất), giữ lịch của thứ khác', async () => {
    const { adapter, lap } = taoAdapter([
      { identifier: 'nhac_tap:a:2026-11-25:dung_luc' },
      { identifier: 'nhac_an:b:2026-11-25:truoc15' },
      { identifier: 'khac:1' },
    ]);

    await huyTatCaNhacGioAsync(adapter);

    expect([...lap.keys()]).toEqual(['khac:1']);
  });

  it('không có adapter thì bỏ qua', async () => {
    await expect(huyTatCaNhacGioAsync(null)).resolves.toBeUndefined();
  });
});
