import { Pressable, ScrollView, View } from 'react-native';
import { Text } from '@/components/ui/text';
import { TextField } from '@/shared/components/ui';
import { cn } from '@/lib/utils';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import type { BuoiTapTuan, LichTapTheoTuan } from '../../types';
import { buoiTapCuaTuan, ghiBuoiTapCuaTuan } from '../../utils/lich-tap';
import { ChonGio } from './chon-gio';
import { KhoiForm } from './thanh-phan-form';

const THU_NGAN = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'];
const THU_DAY_DU = ['Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'Chủ nhật'];

export interface KeHoachTapLuyenProps {
  lich: LichTapTheoTuan;
  soTuan: number;
  tuanDangSua: number;
  onDoiTuan(tuan: number): void;
  onDoiLich(lich: LichTapTheoTuan): void;
  /** Mở PT-02b để đặt bài tập của buổi rơi vào thứ `thu` (trong tuần đang sửa). */
  onMoChiTiet(thu: number): void;
}

/**
 * Khối "Kế hoạch tập luyện": chọn tuần, chọn các thứ có buổi tập, đặt tên buổi và giờ. Lập Tuần 1 thì các tuần sau tự
 * điền theo; sửa ở tuần khác sẽ tách tuần đó ra (adr.md, N29).
 */
export function KeHoachTapLuyen({
  lich,
  soTuan,
  tuanDangSua,
  onDoiTuan,
  onDoiLich,
  onMoChiTiet,
}: KeHoachTapLuyenProps) {
  const { colors } = useTheme();
  const danhSach = [...buoiTapCuaTuan(lich, tuanDangSua)].sort((a, b) => a.thu - b.thu);
  const daSuaRieng = tuanDangSua > 1 && lich.rieng[tuanDangSua] !== undefined;

  const ghiTuan = (moi: BuoiTapTuan[]) => onDoiLich(ghiBuoiTapCuaTuan(lich, tuanDangSua, moi));
  const batThu = (thu: number) => {
    const co = danhSach.some((b) => b.thu === thu);
    ghiTuan(
      co
        ? danhSach.filter((b) => b.thu !== thu)
        : [...danhSach, { thu, ten: 'Buổi tập', gio: '17:30', baiTap: [] }],
    );
  };
  const suaBuoi = (thu: number, phan: Partial<BuoiTapTuan>) =>
    ghiTuan(danhSach.map((b) => (b.thu === thu ? { ...b, ...phan } : b)));
  const datLai = () => {
    const { [tuanDangSua]: _bo, ...conLai } = lich.rieng;
    onDoiLich({ ...lich, rieng: conLai });
  };

  return (
    <KhoiForm
      icon="dumbbell"
      tieuDe="Kế hoạch tập luyện"
      moTa="Lập Tuần 1, các tuần sau tự điền theo và có thể sửa riêng."
    >
      <ScrollView
        horizontal
        showsHorizontalScrollIndicator={false}
        contentContainerClassName="gap-2"
        accessibilityRole="tablist"
      >
        {Array.from({ length: soTuan }, (_, i) => i + 1).map((tuan) => {
          const chon = tuan === tuanDangSua;
          const rieng = tuan > 1 && lich.rieng[tuan] !== undefined;
          return (
            <Pressable
              key={tuan}
              accessibilityRole="tab"
              accessibilityState={{ selected: chon }}
              onPress={() => onDoiTuan(tuan)}
              className={cn(
                'flex-row items-center gap-1.5 rounded-full border px-3.5 py-1.5',
                chon ? 'border-primary bg-primary' : 'border-border bg-background',
              )}
            >
              <Text
                className={cn(
                  'text-sm font-medium',
                  chon ? 'text-primary-foreground' : 'text-foreground',
                )}
              >
                Tuần {tuan}
              </Text>
              {rieng ? (
                <View
                  className={cn(
                    'h-1.5 w-1.5 rounded-full',
                    chon ? 'bg-primary-foreground' : 'bg-warning',
                  )}
                />
              ) : null}
            </Pressable>
          );
        })}
      </ScrollView>

      <View className="flex-row items-center gap-2 rounded-xl bg-secondary px-3 py-2">
        <Icon name="calendar" size={18} color={colors.muted} />
        <Text className="flex-1 text-sm text-muted-foreground">
          {tuanDangSua === 1
            ? 'Các tuần sau tự theo Tuần 1, trừ tuần bạn sửa riêng.'
            : daSuaRieng
              ? `Tuần ${tuanDangSua} đã sửa riêng, không còn theo Tuần 1.`
              : `Tuần ${tuanDangSua} đang theo Tuần 1. Sửa ở đây sẽ tách riêng tuần này.`}
        </Text>
        {daSuaRieng ? (
          <Pressable
            accessibilityRole="button"
            accessibilityLabel="Đặt lại theo Tuần 1"
            onPress={datLai}
          >
            <Text className="text-sm font-bold text-primary">Đặt lại</Text>
          </Pressable>
        ) : null}
      </View>

      <View className="flex-row justify-between">
        {THU_NGAN.map((nhan, i) => {
          const thu = i + 1;
          const chon = danhSach.some((b) => b.thu === thu);
          return (
            <Pressable
              key={nhan}
              accessibilityRole="checkbox"
              accessibilityLabel={THU_DAY_DU[i]}
              accessibilityState={{ checked: chon }}
              onPress={() => batThu(thu)}
              className={cn(
                'h-11 w-11 items-center justify-center rounded-full border',
                chon ? 'border-primary bg-primary' : 'border-border bg-background',
              )}
            >
              <Text
                className={cn(
                  'text-sm font-bold',
                  chon ? 'text-primary-foreground' : 'text-foreground',
                )}
              >
                {nhan}
              </Text>
            </Pressable>
          );
        })}
      </View>

      {danhSach.length === 0 ? (
        <Text className="text-center text-sm text-muted-foreground">
          Chưa có buổi tập nào trong tuần này. Bấm các thứ ở trên để thêm.
        </Text>
      ) : (
        <View>
          {danhSach.map((b, i) => (
            <View key={b.thu} className={cn('py-2.5', i > 0 && 'border-t border-border')}>
              <View className="flex-row items-center gap-2">
                <Text className="w-[62px] text-base font-medium text-foreground">
                  {THU_DAY_DU[b.thu - 1]}
                </Text>
                <View className="min-w-0 flex-1">
                  <TextField
                    hideLabel
                    label={`Tên buổi tập ${THU_DAY_DU[b.thu - 1]}`}
                    value={b.ten}
                    onChangeText={(ten) => suaBuoi(b.thu, { ten })}
                    maxLength={60}
                    placeholder="Tên buổi tập"
                    containerClassName="h-10 px-3"
                  />
                </View>
                <ChonGio
                  gio={b.gio}
                  tieuDe={`Giờ tập ${THU_DAY_DU[b.thu - 1]}`}
                  onDoi={(gio) => suaBuoi(b.thu, { gio })}
                />
                <Pressable
                  accessibilityRole="button"
                  accessibilityLabel={`Chi tiết buổi ${THU_DAY_DU[b.thu - 1]}`}
                  hitSlop={8}
                  onPress={() => onMoChiTiet(b.thu)}
                >
                  <Icon name="chevron-right" size={20} color={colors.muted} />
                </Pressable>
              </View>
              {b.baiTap && b.baiTap.length > 0 ? (
                <Text className="pl-[70px] pt-1 text-xs text-muted-foreground" numberOfLines={1}>
                  {b.baiTap.length} bài
                  {b.thoiLuongPhut ? ` • ${b.thoiLuongPhut} phút` : ''}
                  {b.nhomCo ? ` • ${b.nhomCo}` : ''}
                </Text>
              ) : (
                <Text className="pl-[70px] pt-1 text-xs text-warning">Chưa đặt bài tập</Text>
              )}
            </View>
          ))}
        </View>
      )}
    </KhoiForm>
  );
}
