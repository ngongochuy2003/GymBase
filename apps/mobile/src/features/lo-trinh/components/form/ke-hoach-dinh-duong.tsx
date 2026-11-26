import { Pressable, View } from 'react-native';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import type { BuaAnKeHoach } from '../../types';
import { ChonGio } from './chon-gio';
import { HopKiem, KhoiForm } from './thanh-phan-form';

export interface KeHoachDinhDuongProps {
  buaAn: readonly BuaAnKeHoach[];
  onDoi(buaAn: BuaAnKeHoach[]): void;
  /** Mở PT-02a để đặt món, năng lượng, ảnh của một bữa. */
  onMoChiTiet(id: string): void;
}

/**
 * Khối "Kế hoạch dinh dưỡng": các bữa ăn trong ngày, bật tắt từng bữa và đặt giờ. Dưới tên bữa là tóm tắt món đã đặt
 * (hoặc nhắc chưa đặt); mũi tên mở PT-02a.
 */
export function KeHoachDinhDuong({ buaAn, onDoi, onMoChiTiet }: KeHoachDinhDuongProps) {
  const { colors } = useTheme();
  const sua = (id: string, phan: Partial<BuaAnKeHoach>) =>
    onDoi(buaAn.map((b) => (b.id === id ? { ...b, ...phan } : b)));

  return (
    <KhoiForm
      icon="meal"
      tieuDe="Kế hoạch dinh dưỡng"
      moTa="Thiết lập các bữa ăn cần check-in trong ngày."
    >
      <View>
        {buaAn.map((b, i) => (
          <View
            key={b.id}
            className={cn('flex-row items-center gap-3 py-2.5', i > 0 && 'border-t border-border')}
          >
            <HopKiem chon={b.bat} onDoi={(bat) => sua(b.id, { bat })} nhan={`Bật ${b.ten}`} />
            <Icon name="meal" size={22} color={colors.text} />
            <View className="min-w-0 flex-1">
              <Text className="text-base text-foreground">{b.ten}</Text>
              {b.kcal ? (
                <Text className="text-xs text-muted-foreground" numberOfLines={1}>
                  ~{b.kcal} kcal{b.moTa ? ` • ${b.moTa}` : ''}
                </Text>
              ) : (
                <Text className="text-xs text-warning">Chưa đặt món</Text>
              )}
            </View>
            <ChonGio gio={b.gio} tieuDe={`Giờ ${b.ten}`} onDoi={(gio) => sua(b.id, { gio })} />
            <Pressable
              accessibilityRole="button"
              accessibilityLabel={`Chi tiết ${b.ten}`}
              hitSlop={8}
              onPress={() => onMoChiTiet(b.id)}
            >
              <Icon name="chevron-right" size={20} color={colors.muted} />
            </Pressable>
          </View>
        ))}
      </View>
    </KhoiForm>
  );
}
