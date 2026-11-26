/**
 * Bọc một việc async để các lần gọi chồng nhau không chạy song song: đang chạy mà có thêm lời gọi thì chạy đúng một lần nữa
 * sau khi xong (để thấy trạng thái mới nhất), dù có bao nhiêu lời gọi đến giữa chừng. Lỗi được nuốt: nơi gọi là việc nền.
 *
 * Dùng cho đồng bộ thiết bị: vào app, quay lại foreground và token đổi có thể đến cùng lúc, nếu để chạy song song thì server
 * nhận nhiều request đăng ký cùng một thiết bị mới.
 */
export function gopLanGoi(viec: () => Promise<unknown>): () => Promise<void> {
  let dangChay: Promise<void> | null = null;
  let canChayLai = false;

  const chay = async () => {
    do {
      canChayLai = false;
      try {
        await viec();
      } catch {
        /* Việc nền: lần gọi sau sẽ thử lại. */
      }
    } while (canChayLai);
  };

  return () => {
    if (dangChay) {
      canChayLai = true;
      return dangChay;
    }
    dangChay = chay().finally(() => {
      dangChay = null;
    });
    return dangChay;
  };
}
