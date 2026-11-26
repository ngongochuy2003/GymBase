import { useState } from 'react';
import { View } from 'react-native';
import { Button } from '@/components/ui/button';
import { Text } from '@/components/ui/text';
import { ApiError, toApiError } from '@/core/api/api-error';
import {
  chanDoanNhacGioAsync,
  datNhacThuAsync,
  useGuiThongBaoThu,
  xinBaoThucChinhXacAsync,
} from '@/shared/notifications';

const coKhong = (v: boolean | null) => (v === null ? 'không áp dụng' : v ? 'có' : 'không');
const loiRaChu = (e: unknown) => (e instanceof Error ? e.message : String(e));

/**
 * Khu vực chỉ có ở bản dev: gửi thông báo thử (qua server và FCM), đặt nhắc thử trên máy, và xem tình trạng nhắc để biết vì sao
 * nhắc không tới. Kết quả và lỗi hiện ngay bên dưới, không nuốt.
 */
// TODO(THONGBAO): bỏ khu vực này (và endpoint thong-bao/thu-nghiem) khi đã thử xong trên máy thật và tin nhắn thật đã tạo thông báo.
export function KhuVucThu({ paddingBottom }: { paddingBottom: number }) {
  const guiThu = useGuiThongBaoThu();
  const [ketQua, setKetQua] = useState<string | null>(null);

  const chay = async (viec: () => Promise<string>) => {
    try {
      setKetQua(await viec());
    } catch (e) {
      setKetQua(`Lỗi: ${loiRaChu(e)}`);
    }
  };

  const datNhacThu = () =>
    chay(async () => {
      const id = await datNhacThuAsync(30);
      const luc = new Date(Date.now() + 30_000).toLocaleTimeString('vi-VN');
      return id ? `Đã đặt nhắc, sẽ hiện lúc ${luc}` : 'Không có module thông báo (web hoặc môi trường test).';
    });

  const kiemTra = () =>
    chay(async () => {
      const cd = await chanDoanNhacGioAsync();
      if (!cd) return 'Không có module thông báo (web hoặc môi trường test).';
      return [
        `Quyền thông báo: ${coKhong(cd.quyenThongBao)}`,
        `Báo thức chính xác: ${coKhong(cd.baoThucChinhXac)}`,
        `Đang chờ trên máy: ${cd.soDangCho} (theo kế hoạch: ${cd.soNhacCuaKeHoach})`,
      ].join('\n');
    });

  const capQuyenChinhXac = () =>
    chay(async () => {
      const ok = await xinBaoThucChinhXacAsync();
      return ok === null ? 'Máy này không có quyền báo thức chính xác.' : `Báo thức chính xác: ${coKhong(ok)}`;
    });

  return (
    <View className="border-t border-border p-4" style={{ paddingBottom }}>
      <Button
        variant="outline"
        loading={guiThu.isPending}
        onPress={() => guiThu.mutate(undefined)}
        accessibilityLabel="Gửi thông báo thử"
      >
        <Text>Gửi thông báo thử</Text>
      </Button>
      {/* Hẹn giờ để kịp tắt app rồi xem push tới khi app đã đóng. */}
      <Button
        variant="outline"
        className="mt-2"
        disabled={guiThu.isPending}
        onPress={() => guiThu.mutate(15)}
        accessibilityLabel="Gửi thông báo thử sau 15 giây"
      >
        <Text>Gửi thử sau 15 giây</Text>
      </Button>
      {/* Nhắc local (đặt lịch ngay trên máy, không qua server): thử khi đã tắt app và tắt mạng. */}
      <Button
        variant="outline"
        className="mt-2"
        onPress={() => void datNhacThu()}
        accessibilityLabel="Nhắc thử trên máy sau 30 giây"
      >
        <Text>Nhắc thử trên máy sau 30 giây</Text>
      </Button>
      <Button
        variant="outline"
        className="mt-2"
        onPress={() => void kiemTra()}
        accessibilityLabel="Kiểm tra nhắc trên máy"
      >
        <Text>Kiểm tra nhắc trên máy</Text>
      </Button>
      <Button
        variant="outline"
        className="mt-2"
        onPress={() => void capQuyenChinhXac()}
        accessibilityLabel="Cấp quyền báo thức chính xác"
      >
        <Text>Cấp quyền báo thức chính xác</Text>
      </Button>
      {guiThu.isError && !(guiThu.error instanceof ApiError && guiThu.error.status === 404) ? (
        <Text variant="destructive" className="mt-2 text-sm">
          {toApiError(guiThu.error).message}
        </Text>
      ) : null}
      {ketQua ? (
        <Text testID="ket-qua-thu" className="mt-2 text-sm">
          {ketQua}
        </Text>
      ) : null}
    </View>
  );
}
