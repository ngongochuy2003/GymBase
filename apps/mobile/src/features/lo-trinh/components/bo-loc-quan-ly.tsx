import { useState } from 'react';
import { Modal, Pressable, View } from 'react-native';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { SelectField, type SelectOption } from '@/shared/components/select-field';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import type { ThangLoc, TrangThaiLoTrinh } from '../types';
import { doiThang, nhanThang } from '../utils/ngay';

const LUA_CHON_TRANG_THAI: readonly (SelectOption & { ma: TrangThaiLoTrinh })[] = [
  { ma: 'dang_hoat_dong', ten: 'Đang hoạt động' },
  { ma: 'hoan_thanh', ten: 'Đã hoàn thành' },
  { ma: 'that_bai', ten: 'Thất bại' },
];

const THANG = Array.from({ length: 12 }, (_, i) => i + 1);

/** Bảng chọn tháng và năm: mũi tên đổi năm, lưới 12 tháng; bấm một tháng là chọn và đóng. */
function BangChonThang({
  mo,
  thang,
  onChon,
  onDong,
}: {
  mo: boolean;
  thang: ThangLoc;
  onChon(thang: ThangLoc): void;
  onDong(): void;
}) {
  const { colors } = useTheme();
  // Năm đang xem trong bảng; null là năm của tháng đang chọn. Đặt lại về null mỗi lần đóng bảng.
  const [namXem, setNamXem] = useState<number | null>(null);
  const nam = namXem ?? thang.nam;
  const dong = () => {
    setNamXem(null);
    onDong();
  };
  return (
    <Modal visible={mo} transparent animationType="slide" onRequestClose={dong}>
      <Pressable
        accessibilityLabel="Đóng"
        onPress={dong}
        className="flex-1 justify-end bg-black/40"
      >
        <Pressable className="gap-4 rounded-t-3xl bg-background px-5 pb-8 pt-5">
          <View className="flex-row items-center justify-between">
            <Pressable
              accessibilityRole="button"
              accessibilityLabel="Năm trước"
              hitSlop={8}
              onPress={() => setNamXem(nam - 1)}
            >
              <Icon name="chevron-left" size={24} color={colors.text} />
            </Pressable>
            <Text accessibilityRole="header" className="text-lg font-bold text-foreground">
              Năm {nam}
            </Text>
            <Pressable
              accessibilityRole="button"
              accessibilityLabel="Năm sau"
              hitSlop={8}
              onPress={() => setNamXem(nam + 1)}
            >
              <Icon name="chevron-right" size={24} color={colors.text} />
            </Pressable>
          </View>
          <View className="flex-row flex-wrap gap-2">
            {THANG.map((t) => {
              const chon = nam === thang.nam && t === thang.thang;
              return (
                <Pressable
                  key={t}
                  accessibilityRole="button"
                  accessibilityLabel={`Tháng ${t}/${nam}`}
                  accessibilityState={{ selected: chon }}
                  onPress={() => {
                    setNamXem(null);
                    onChon({ nam, thang: t });
                  }}
                  className={cn(
                    'w-[31%] items-center rounded-2xl border py-3',
                    chon ? 'border-primary bg-primary' : 'border-border bg-card',
                  )}
                >
                  <Text
                    className={cn(
                      'text-base font-medium',
                      chon ? 'text-primary-foreground' : 'text-foreground',
                    )}
                  >
                    Tháng {t}
                  </Text>
                </Pressable>
              );
            })}
          </View>
        </Pressable>
      </Pressable>
    </Modal>
  );
}

export interface BoLocQuanLyProps {
  trangThai: TrangThaiLoTrinh;
  thang: ThangLoc;
  onDoiTrangThai(trangThai: TrangThaiLoTrinh): void;
  onDoiThang(thang: ThangLoc): void;
}

/** Bộ lọc của PT-01 trên một dòng: ô chọn trạng thái (dropdown) và bộ chọn tháng. Mặc định: đang hoạt động, tháng hiện tại. */
export function BoLocQuanLy({ trangThai, thang, onDoiTrangThai, onDoiThang }: BoLocQuanLyProps) {
  const { colors } = useTheme();
  const [moThang, setMoThang] = useState(false);
  return (
    <View className="flex-row items-center gap-2">
      <View className="flex-[5]">
        <SelectField
          hideLabel
          singleLine
          label="Trạng thái lộ trình"
          value={trangThai}
          options={LUA_CHON_TRANG_THAI}
          onChange={(ma) => onDoiTrangThai(ma as TrangThaiLoTrinh)}
          triggerClassName="h-11 min-h-0 gap-1 rounded-full px-3"
          valueClassName="text-sm font-medium"
        />
      </View>

      <View className="h-11 flex-[5] flex-row items-center justify-between rounded-full border border-input bg-card px-2">
        <Pressable
          accessibilityRole="button"
          accessibilityLabel="Tháng trước"
          hitSlop={8}
          onPress={() => onDoiThang(doiThang(thang, -1))}
          className="h-7 w-7 items-center justify-center"
        >
          <Icon name="chevron-left" size={18} color={colors.text} />
        </Pressable>
        <Pressable
          accessibilityRole="button"
          accessibilityLabel="Chọn tháng"
          accessibilityValue={{ text: nhanThang(thang) }}
          onPress={() => setMoThang(true)}
          className="h-full flex-1 items-center justify-center"
        >
          <Text className="text-sm font-bold text-foreground" numberOfLines={1}>
            {nhanThang(thang)}
          </Text>
        </Pressable>
        <Pressable
          accessibilityRole="button"
          accessibilityLabel="Tháng sau"
          hitSlop={8}
          onPress={() => onDoiThang(doiThang(thang, 1))}
          className="h-7 w-7 items-center justify-center"
        >
          <Icon name="chevron-right" size={18} color={colors.text} />
        </Pressable>
      </View>
      <BangChonThang
        mo={moThang}
        thang={thang}
        onChon={(t) => {
          onDoiThang(t);
          setMoThang(false);
        }}
        onDong={() => setMoThang(false)}
      />
    </View>
  );
}
