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
}

/** Khối "Kế hoạch dinh dưỡng": các bữa ăn trong ngày, bật tắt từng bữa và đặt giờ. */
export function KeHoachDinhDuong({ buaAn, onDoi }: KeHoachDinhDuongProps) {
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
            <Text className="flex-1 text-base text-foreground">{b.ten}</Text>
            <ChonGio gio={b.gio} tieuDe={`Giờ ${b.ten}`} onDoi={(gio) => sua(b.id, { gio })} />
            <Pressable
              accessibilityRole="button"
              accessibilityLabel={`Chi tiết ${b.ten}`}
              hitSlop={8}
              // TODO(ROUTE): mở PT-02a (chi tiết bữa ăn: calo nhập tay, ghi chú món ưu tiên) khi có màn (adr.md, N21, N30).
              onPress={() => undefined}
            >
              <Icon name="chevron-right" size={20} color={colors.muted} />
            </Pressable>
          </View>
        ))}
      </View>
    </KhoiForm>
  );
}
