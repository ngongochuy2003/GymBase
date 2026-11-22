import { useState } from 'react';
import { Image, Pressable, ScrollView, View } from 'react-native';
import { useRouter } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { Icon } from '@/shared/icons';
import { Rise } from '@/shared/motion';
import { useTheme } from '@/shared/theme/theme';
import { DauManCheckIn } from '../components/check-in-bua-an';
import { KhungThe } from '../components/khung-the';
import { useChiTietLoTrinh } from '../hooks/use-lo-trinh';
import type { AnhCoTheMoc, QuyenXemAnh } from '../types';
import { QUYEN_XEM } from '../utils/quyen-xem';
import { canNangTaiTuan, soThapPhan } from '../utils/tien-do';

/** Hàng chip chọn một mốc tuần cho một bên của phần so sánh. */
function ChonMoc({
  nhan,
  cacMoc,
  chon,
  onChon,
}: {
  nhan: string;
  cacMoc: readonly AnhCoTheMoc[];
  chon: number;
  onChon(tuan: number): void;
}) {
  return (
    <View accessibilityRole="radiogroup" className="flex-row flex-wrap items-center gap-2">
      <Text variant="muted" className="w-20 text-sm">
        {nhan}
      </Text>
      {cacMoc.map((m) => {
        const dangChon = m.tuan === chon;
        return (
          <Pressable
            key={m.tuan}
            accessibilityRole="radio"
            accessibilityState={{ checked: dangChon }}
            accessibilityLabel={`${nhan} Tuần ${m.tuan}`}
            onPress={() => onChon(m.tuan)}
            className={cn(
              'rounded-full border px-3 py-1.5',
              dangChon ? 'border-transparent bg-primary' : 'border-border bg-card',
            )}
          >
            <Text
              className={cn(
                'text-sm font-medium',
                dangChon ? 'text-primary-foreground' : 'text-muted-foreground',
              )}
            >
              Tuần {m.tuan}
            </Text>
          </Pressable>
        );
      })}
    </View>
  );
}

/** Một cột của phần so sánh: nhãn tuần, ảnh tỉ lệ 3:4 và cân nặng tại mốc đó (nếu biết). */
function CotSoSanh({ moc, canNang, donVi }: { moc: AnhCoTheMoc; canNang?: number; donVi: string }) {
  return (
    <View className="flex-1 gap-2">
      <Text className="text-center text-sm font-bold">Tuần {moc.tuan}</Text>
      <View className="aspect-[3/4] overflow-hidden rounded-xl bg-secondary">
        <Image
          source={{ uri: moc.uri }}
          accessibilityLabel={`Ảnh cơ thể tuần ${moc.tuan}`}
          resizeMode="cover"
          className="size-full"
        />
      </View>
      <Text variant="muted" className="text-center text-sm">
        {canNang === undefined ? '—' : `${soThapPhan(canNang)} ${donVi}`}
      </Text>
    </View>
  );
}

/** Nhãn ai xem được ảnh: biểu tượng và chữ theo N33. */
function NhanQuyenXem({ quyenXem }: { quyenXem: QuyenXemAnh }) {
  const { colors } = useTheme();
  const q = QUYEN_XEM.find((x) => x.gia === quyenXem);
  if (!q) return null;
  return (
    <View className="flex-row items-center gap-1">
      <Icon name={q.icon} size={14} color={colors.muted} />
      <Text variant="muted" className="text-xs">
        {q.nhan}
      </Text>
    </View>
  );
}

/**
 * Ảnh cơ thể theo mốc chi tiết (HV-07, tab Ảnh cơ thể): so sánh hai mốc bất kỳ cạnh nhau kèm cân nặng và khoảng cách
 * giữa hai mốc, rồi danh sách mọi mốc đã chụp (mới nhất ở trên) kèm chế độ xem của từng ảnh (N33). Mở từ "Xem tất cả"
 * cạnh ảnh theo mốc ở màn Chi tiết lộ trình.
 */
// TODO(ROUTE): HV-07 chưa có bản thiết kế (luong-man-hinh.md, HV-07); màn này tự thiết kế theo khuôn các màn tiến độ.
//   Chưa có xem ảnh lớn toàn màn hình, chưa đổi chế độ xem của ảnh đã lưu; ảnh vừa ghi ở HV-08 chưa vào danh sách này.
export function AnhTheoMocScreen({ id }: { id?: string }) {
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const chiTiet = useChiTietLoTrinh(id);
  const cacMoc = chiTiet?.tienDo.anhMoc ?? [];
  // Mặc định so sánh mốc đầu với mốc mới nhất.
  const [truocChon, setTruoc] = useState<number>();
  const [sauChon, setSau] = useState<number>();

  const content = (() => {
    if (!chiTiet) {
      return (
        <View className="items-center p-8">
          <Text variant="muted">Không tìm thấy lộ trình.</Text>
        </View>
      );
    }
    const { donVi } = chiTiet.loTrinh.mucTieu;
    const canNang = chiTiet.tienDo.canNang;
    const truoc = cacMoc.find((m) => m.tuan === (truocChon ?? cacMoc[0]?.tuan));
    const sau = cacMoc.find((m) => m.tuan === (sauChon ?? cacMoc.at(-1)?.tuan));
    const kgTruoc = truoc ? canNangTaiTuan(canNang, truoc.tuan) : undefined;
    const kgSau = sau ? canNangTaiTuan(canNang, sau.tuan) : undefined;
    const doi =
      kgTruoc !== undefined && kgSau !== undefined
        ? Math.round((kgSau - kgTruoc) * 10) / 10
        : undefined;
    const khoangTuan = truoc && sau ? Math.abs(sau.tuan - truoc.tuan) : 0;

    return (
      <ScrollView
        className="flex-1"
        contentContainerClassName="w-full max-w-[680px] gap-4 self-center px-4"
        contentContainerStyle={{ paddingBottom: 24 + insets.bottom }}
      >
        {cacMoc.length === 0 ? (
          <KhungThe icon="images" tieuDe="Ảnh cơ thể theo mốc">
            <Text variant="muted">
              Chưa có ảnh nào. Ảnh bạn ghi ở nhiệm vụ Ảnh cơ thể sẽ xuất hiện ở đây.
            </Text>
          </KhungThe>
        ) : (
          <>
            <Rise>
              <KhungThe icon="images" tieuDe="So sánh mốc">
                {cacMoc.length >= 2 && truoc && sau ? (
                  <>
                    <View className="gap-2">
                      <ChonMoc
                        nhan="Mốc trước"
                        cacMoc={cacMoc}
                        chon={truoc.tuan}
                        onChon={setTruoc}
                      />
                      <ChonMoc nhan="Mốc sau" cacMoc={cacMoc} chon={sau.tuan} onChon={setSau} />
                    </View>
                    <View className="flex-row gap-3">
                      <CotSoSanh moc={truoc} canNang={kgTruoc} donVi={donVi} />
                      <CotSoSanh moc={sau} canNang={kgSau} donVi={donVi} />
                    </View>
                    <Text variant="muted" className="text-center text-sm">
                      {khoangTuan === 0 ? 'Cùng một mốc' : `Cách nhau ${khoangTuan} tuần`}
                      {doi === undefined || khoangTuan === 0
                        ? ''
                        : ` • ${doi > 0 ? '+' : doi < 0 ? '−' : ''}${soThapPhan(Math.abs(doi))} ${donVi}`}
                    </Text>
                  </>
                ) : (
                  <Text variant="muted">Cần ít nhất 2 ảnh mốc để so sánh.</Text>
                )}
              </KhungThe>
            </Rise>

            <Rise delay={120}>
              <KhungThe icon="clock" tieuDe="Các mốc đã chụp" chuPhu={`${cacMoc.length} ảnh`}>
                <View>
                  {[...cacMoc].reverse().map((m, i) => {
                    const kg = canNangTaiTuan(canNang, m.tuan);
                    return (
                      <View
                        key={m.tuan}
                        className={cn(
                          'flex-row items-center gap-3 py-3',
                          i > 0 && 'border-t border-border',
                        )}
                      >
                        <View className="aspect-[3/4] w-16 overflow-hidden rounded-lg bg-secondary">
                          <Image
                            source={{ uri: m.uri }}
                            accessibilityLabel={`Ảnh mốc tuần ${m.tuan}`}
                            resizeMode="cover"
                            className="size-full"
                          />
                        </View>
                        <View className="flex-1 gap-1">
                          <Text className="font-bold">Tuần {m.tuan}</Text>
                          <Text variant="muted" className="text-sm">
                            {kg === undefined ? 'Chưa ghi cân nặng' : `${soThapPhan(kg)} ${donVi}`}
                          </Text>
                          <NhanQuyenXem quyenXem={m.quyenXem} />
                        </View>
                      </View>
                    );
                  })}
                </View>
              </KhungThe>
            </Rise>
          </>
        )}
      </ScrollView>
    );
  })();

  return (
    <View className="flex-1 bg-background" style={{ paddingTop: insets.top }}>
      <DauManCheckIn
        tieuDe="Ảnh cơ thể theo mốc"
        phuDe={chiTiet ? `Lộ trình: ${chiTiet.loTrinh.ten}` : 'Không tìm thấy lộ trình'}
        onQuayLai={() => router.back()}
      />
      {content}
    </View>
  );
}
