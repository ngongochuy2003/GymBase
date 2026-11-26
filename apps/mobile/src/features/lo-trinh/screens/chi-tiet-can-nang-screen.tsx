import { ScrollView, View } from 'react-native';
import { useRouter } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { Rise } from '@/shared/motion';
import { BieuDoCanNang } from '../components/bieu-do-can-nang';
import { DauManCheckIn } from '../components/check-in-bua-an';
import { KhungThe } from '../components/khung-the';
import { useChiTietLoTrinh } from '../hooks/use-lo-trinh';
import type { DiemCanNang } from '../types';
import { soThapPhan } from '../utils/tien-do';

/** Mức thay đổi có dấu, ví dụ `+0,5` hoặc `−1,0`; bằng 0 thì chỉ là `0,0`. */
function chuThayDoi(delta: number): string {
  if (delta === 0) return soThapPhan(0);
  return `${delta > 0 ? '+' : '−'}${soThapPhan(Math.abs(delta))}`;
}

/** Màu chữ của mức thay đổi: đi về phía mục tiêu là xanh, đi ngược là cam, không đổi là chữ nhạt. */
function mauThayDoi(delta: number, huong: number): string {
  if (delta === 0) return 'text-muted-foreground';
  return delta * huong > 0 ? 'text-success' : 'text-warning';
}

/** Một ô số liệu trong thẻ: nhãn nhạt phía trên, giá trị đậm phía dưới. `ngan` vẽ vạch dọc ngăn với ô bên trái. */
function OSoLieu({
  nhan,
  giaTri,
  mau,
  ngan = false,
}: {
  nhan: string;
  giaTri: string;
  mau?: string;
  ngan?: boolean;
}) {
  return (
    <View className={cn('flex-1 items-center gap-0.5 px-2', ngan && 'border-l border-border')}>
      <Text variant="muted" className="text-xs">
        {nhan}
      </Text>
      <Text className={cn('text-lg font-bold', mau)}>{giaTri}</Text>
    </View>
  );
}

/**
 * Theo dõi cân nặng chi tiết (HV-07, tab Cân nặng): cân nặng hiện tại và mức thay đổi so với lúc bắt đầu, biểu đồ đầy
 * đủ, thống kê, rồi lịch sử từng lần cân (mới nhất ở trên) kèm mức tăng giảm so với lần trước. Mở từ "Chi tiết" cạnh
 * biểu đồ ở màn Chi tiết lộ trình.
 */
// TODO(ROUTE): HV-07 chưa có bản thiết kế (luong-man-hinh.md, HV-07); màn này tự thiết kế theo khuôn các màn tiến độ.
//   Lần cân chỉ có tuần và lần trong tuần, chưa có ngày thật; lần cân vừa ghi ở HV-08 cũng chưa vào danh sách này.
export function ChiTietCanNangScreen({ id }: { id?: string }) {
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const chiTiet = useChiTietLoTrinh(id);
  const diem: readonly DiemCanNang[] = chiTiet?.tienDo.canNang ?? [];
  const dau = diem[0];
  const cuoi = diem.at(-1);

  const content = (() => {
    if (!chiTiet || !dau || !cuoi) {
      return (
        <View className="items-center p-8">
          <Text variant="muted">
            {chiTiet ? 'Chưa có lần cân nào.' : 'Không tìm thấy lộ trình.'}
          </Text>
        </View>
      );
    }
    const { tu, den, donVi } = chiTiet.loTrinh.mucTieu;
    // Chiều đi đúng: +1 khi mục tiêu cao hơn lúc bắt đầu (tăng cân), -1 khi thấp hơn (giảm cân).
    const huong = den >= tu ? 1 : -1;
    const tong = Math.round((cuoi.kg - tu) * 10) / 10;
    const conLai = Math.round(Math.abs(den - cuoi.kg) * 10) / 10;
    const soTuan = cuoi.tuan - dau.tuan;
    const tbTuan = soTuan > 0 ? Math.round(((cuoi.kg - dau.kg) / soTuan) * 10) / 10 : 0;
    const kg = diem.map((d) => d.kg);
    // Lịch sử đọc từ mới tới cũ; mức đổi tính so với lần cân ngay trước đó theo thời gian.
    const lichSu = diem.map((d, i) => ({ d, truoc: i > 0 ? diem[i - 1] : undefined })).reverse();

    return (
      <ScrollView
        className="flex-1"
        contentContainerClassName="w-full max-w-[680px] gap-4 self-center px-4"
        contentContainerStyle={{ paddingBottom: 24 + insets.bottom }}
      >
        <Rise>
          <View className="gap-3 rounded-2xl border border-border bg-card p-4">
            <View className="items-center gap-1">
              <Text variant="muted" className="text-sm">
                Cân nặng hiện tại
              </Text>
              <Text className="text-4xl font-bold leading-[44px]">
                {soThapPhan(cuoi.kg)} {donVi}
              </Text>
              <Text className={cn('text-sm font-medium', mauThayDoi(tong, huong))}>
                {chuThayDoi(tong)} {donVi} so với lúc bắt đầu
              </Text>
            </View>
            <View className="flex-row border-t border-border pt-3">
              <OSoLieu nhan="Bắt đầu" giaTri={`${soThapPhan(tu)} ${donVi}`} />
              <OSoLieu ngan nhan="Mục tiêu" giaTri={`${soThapPhan(den)} ${donVi}`} />
              <OSoLieu ngan nhan="Còn lại" giaTri={`${soThapPhan(conLai)} ${donVi}`} />
            </View>
          </View>
        </Rise>

        <Rise delay={90}>
          <KhungThe icon="trend" tieuDe="Biểu đồ cân nặng" chuPhu={`${diem.length} lần cân`}>
            <BieuDoCanNang diem={diem} donVi={donVi} />
          </KhungThe>
        </Rise>

        <Rise delay={180}>
          <KhungThe icon="target" tieuDe="Thống kê">
            <View className="flex-row">
              <OSoLieu nhan="Cao nhất" giaTri={`${soThapPhan(Math.max(...kg))} ${donVi}`} />
              <OSoLieu ngan nhan="Thấp nhất" giaTri={`${soThapPhan(Math.min(...kg))} ${donVi}`} />
              <OSoLieu
                ngan
                nhan="Mỗi tuần"
                giaTri={`${chuThayDoi(tbTuan)} ${donVi}`}
                mau={mauThayDoi(tbTuan, huong)}
              />
            </View>
          </KhungThe>
        </Rise>

        <Rise delay={270}>
          <KhungThe icon="clock" tieuDe="Lịch sử cân nặng" chuPhu={`${diem.length} lần`}>
            <View>
              {lichSu.map(({ d, truoc }, i) => {
                const delta = truoc ? Math.round((d.kg - truoc.kg) * 10) / 10 : undefined;
                return (
                  <View
                    key={`${d.tuan}-${d.lan}`}
                    className={cn(
                      'flex-row items-center gap-3 py-3',
                      i > 0 && 'border-t border-border',
                    )}
                  >
                    <View className="flex-1">
                      <Text className="font-medium">
                        Tuần {d.tuan} • Lần {d.lan}
                      </Text>
                      {i === 0 ? (
                        <Text variant="muted" className="text-xs">
                          Mới nhất
                        </Text>
                      ) : null}
                    </View>
                    {delta === undefined ? (
                      <Text variant="muted" className="text-sm">
                        Mốc đầu
                      </Text>
                    ) : (
                      <Text className={cn('text-sm font-medium', mauThayDoi(delta, huong))}>
                        {chuThayDoi(delta)}
                      </Text>
                    )}
                    <Text className="w-20 text-right text-base font-bold">
                      {soThapPhan(d.kg)} {donVi}
                    </Text>
                  </View>
                );
              })}
            </View>
          </KhungThe>
        </Rise>
      </ScrollView>
    );
  })();

  return (
    <View className="flex-1 bg-background" style={{ paddingTop: insets.top }}>
      <DauManCheckIn
        tieuDe="Theo dõi cân nặng"
        phuDe={chiTiet ? `Lộ trình: ${chiTiet.loTrinh.ten}` : 'Không tìm thấy lộ trình'}
        onQuayLai={() => router.back()}
      />
      {content}
    </View>
  );
}
