import { useState } from 'react';
import { Modal, Pressable, ScrollView, View } from 'react-native';
import { Button } from '@/components/ui/button';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import type { NguoiLoTrinh } from '../../types';
import { AnhDaiDien } from '../anh-nguoi';
import { HopKiem } from './thanh-phan-form';

export interface ChonHocVienProps {
  /** Danh sách bạn của PT để chọn. */
  banBe: readonly NguoiLoTrinh[];
  daChon: readonly NguoiLoTrinh[];
  onDoi(daChon: NguoiLoTrinh[]): void;
  coLoi?: boolean;
}

/** Ô "Học viên tham gia": hiện người đã chọn dưới dạng chip có nút xóa; bấm ô để mở danh sách chọn thêm. */
export function ChonHocVien({ banBe, daChon, onDoi, coLoi = false }: ChonHocVienProps) {
  const { colors } = useTheme();
  const [mo, setMo] = useState(false);

  const bo = (id: string) => onDoi(daChon.filter((n) => n.id !== id));
  const bat = (nguoi: NguoiLoTrinh) => {
    const co = daChon.some((n) => n.id === nguoi.id);
    onDoi(co ? daChon.filter((n) => n.id !== nguoi.id) : [...daChon, nguoi]);
  };

  return (
    <>
      <View
        className={cn(
          'min-h-[52px] flex-row items-center gap-2 rounded-xl border bg-background px-2 py-1.5',
          coLoi ? 'border-destructive' : 'border-border',
        )}
      >
        <View className="flex-1 flex-row flex-wrap gap-1.5">
          {daChon.length === 0 ? (
            <Pressable
              accessibilityRole="button"
              accessibilityLabel="Chọn học viên tham gia"
              onPress={() => setMo(true)}
            >
              <Text className="px-1 py-1.5 text-base text-muted-foreground">Chọn học viên</Text>
            </Pressable>
          ) : (
            daChon.map((n) => (
              <View
                key={n.id}
                className="flex-row items-center gap-1.5 rounded-full bg-secondary py-1 pl-1 pr-2"
              >
                <AnhDaiDien uri={n.anh} ten={n.ten} className="h-7 w-7" />
                <Text className="text-sm font-bold text-foreground">{n.ten}</Text>
                <Pressable
                  accessibilityRole="button"
                  accessibilityLabel={`Bỏ ${n.ten}`}
                  hitSlop={8}
                  onPress={() => bo(n.id)}
                >
                  <Icon name="close" size={16} color={colors.muted} />
                </Pressable>
              </View>
            ))
          )}
        </View>
        <Pressable
          accessibilityRole="button"
          accessibilityLabel="Chọn học viên tham gia"
          hitSlop={8}
          onPress={() => setMo(true)}
        >
          <Icon name="chevron-down" size={18} color={colors.muted} />
        </Pressable>
      </View>

      <Modal visible={mo} transparent animationType="slide" onRequestClose={() => setMo(false)}>
        <Pressable
          accessibilityLabel="Đóng"
          onPress={() => setMo(false)}
          className="flex-1 justify-end bg-black/40"
        >
          <Pressable className="max-h-[75%] rounded-t-3xl bg-background px-5 pb-8 pt-5">
            <Text className="text-lg font-bold text-foreground">Học viên tham gia</Text>
            <Text className="pb-2 text-sm text-muted-foreground">
              Chỉ hiện người đã kết bạn với bạn. Chọn một người cho lộ trình đơn, nhiều người cho lộ
              trình nhóm.
            </Text>
            <ScrollView>
              {banBe.map((n) => {
                const chon = daChon.some((d) => d.id === n.id);
                return (
                  <Pressable
                    key={n.id}
                    accessibilityRole="checkbox"
                    accessibilityLabel={n.ten}
                    accessibilityState={{ checked: chon }}
                    onPress={() => bat(n)}
                    className="min-h-14 flex-row items-center gap-3 border-b border-border py-2"
                  >
                    <AnhDaiDien uri={n.anh} ten={n.ten} className="h-10 w-10" />
                    <Text className="flex-1 text-base text-foreground">{n.ten}</Text>
                    <HopKiem chon={chon} onDoi={() => bat(n)} nhan={`Chọn ${n.ten}`} />
                  </Pressable>
                );
              })}
            </ScrollView>
            <Button
              shape="pill"
              accessibilityLabel="Xong"
              onPress={() => setMo(false)}
              className="mt-4 min-h-0 border-0 px-0 py-3"
            >
              <Text>Xong{daChon.length > 0 ? ` (${daChon.length})` : ''}</Text>
            </Button>
          </Pressable>
        </Pressable>
      </Modal>
    </>
  );
}
