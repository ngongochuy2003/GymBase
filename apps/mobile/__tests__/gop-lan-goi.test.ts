import { gopLanGoi } from '../src/shared/notifications/gop-lan-goi';

function treo() {
  let xong!: () => void;
  const lời = new Promise<void>((resolve) => {
    xong = resolve;
  });
  return { lời, xong };
}

describe('gopLanGoi', () => {
  it('gọi liên tiếp khi đang chạy thì chỉ chạy thêm đúng một lần nữa sau khi xong', async () => {
    const cho = [treo(), treo()];
    let lan = 0;
    const fn = jest.fn(async () => {
      await cho[lan++]?.lời;
    });
    const chay = gopLanGoi(fn);

    const p1 = chay();
    void chay();
    void chay();
    void chay();
    expect(fn).toHaveBeenCalledTimes(1);

    cho[0]?.xong();
    await Promise.resolve();
    await Promise.resolve();
    cho[1]?.xong();
    await p1;

    expect(fn).toHaveBeenCalledTimes(2);
  });

  it('một lần gọi đơn lẻ chỉ chạy một lần', async () => {
    const fn = jest.fn(async () => undefined);

    await gopLanGoi(fn)();

    expect(fn).toHaveBeenCalledTimes(1);
  });

  it('xong rồi thì lần gọi sau chạy lại bình thường', async () => {
    const fn = jest.fn(async () => undefined);
    const chay = gopLanGoi(fn);

    await chay();
    await chay();

    expect(fn).toHaveBeenCalledTimes(2);
  });

  it('lỗi của một lần không làm kẹt các lần sau và không ném ra ngoài', async () => {
    const fn = jest.fn().mockRejectedValueOnce(new Error('mạng')).mockResolvedValue(undefined);
    const chay = gopLanGoi(fn);

    await expect(chay()).resolves.toBeUndefined();
    await chay();

    expect(fn).toHaveBeenCalledTimes(2);
  });
});
