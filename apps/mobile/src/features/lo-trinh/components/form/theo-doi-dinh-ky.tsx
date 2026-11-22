import { View } from 'react-native';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import { SelectField, type SelectOption } from '@/shared/components/select-field';
import { HopKiem, KhoiForm } from './thanh-phan-form';

export const TAN_SUAT_CAN_NANG: readonly SelectOption[] = [
  { ma: '1', ten: '1 lần/tuần' },
  { ma: '2', ten: '2 lần/tuần' },
  { ma: '3', ten: '3 lần/tuần' },
];

export const TAN_SUAT_ANH: readonly SelectOption[] = [
  { ma: '1', ten: 'mỗi tuần' },
  { ma: '2', ten: 'mỗi 2 tuần' },
  { ma: '4', ten: 'mỗi 4 tuần' },
];

export interface MucTheoDoi {
  bat: boolean;
  tanSuat: string;
}

export interface TheoDoiDinhKyProps {
  canNang: MucTheoDoi;
  anhCoThe: MucTheoDoi;
  onDoiCanNang(muc: MucTheoDoi): void;
  onDoiAnhCoThe(muc: MucTheoDoi): void;
}

function Dong({
  icon,
  ten,
  muc,
  luaChon,
  onDoi,
  coKe,
}: {
  icon: string;
  ten: string;
  muc: MucTheoDoi;
  luaChon: readonly SelectOption[];
  onDoi(muc: MucTheoDoi): void;
  coKe: boolean;
}) {
  const { colors } = useTheme();
  return (
    <View className={cn('flex-row items-center gap-3 py-2.5', coKe && 'border-t border-border')}>
      <HopKiem chon={muc.bat} onDoi={(bat) => onDoi({ ...muc, bat })} nhan={`Bật ${ten}`} />
      <Icon name={icon} size={22} color={colors.text} />
      <Text className="flex-1 text-base text-foreground">{ten}</Text>
      <View
        className={cn('w-[148px]', !muc.bat && 'opacity-50')}
        pointerEvents={muc.bat ? 'auto' : 'none'}
      >
        <SelectField
          hideLabel
          singleLine
          label={`Tần suất ${ten}`}
          value={muc.tanSuat}
          options={luaChon}
          onChange={(tanSuat) => onDoi({ ...muc, tanSuat })}
          triggerClassName="min-h-0 h-10 gap-2 px-3"
          valueClassName="text-sm"
        />
      </View>
    </View>
  );
}

/** Khối "Theo dõi định kỳ": cân nặng và ảnh cơ thể, mỗi mục có tần suất riêng (adr.md, N4). */
export function TheoDoiDinhKy({
  canNang,
  anhCoThe,
  onDoiCanNang,
  onDoiAnhCoThe,
}: TheoDoiDinhKyProps) {
  return (
    <KhoiForm
      icon="progress"
      tieuDe="Theo dõi định kỳ"
      moTa="Theo dõi tiến trình để đánh giá và điều chỉnh kịp thời."
    >
      <View>
        <Dong
          icon="scale"
          ten="Cân nặng"
          muc={canNang}
          luaChon={TAN_SUAT_CAN_NANG}
          onDoi={onDoiCanNang}
          coKe={false}
        />
        <Dong
          icon="camera"
          ten="Ảnh cơ thể"
          muc={anhCoThe}
          luaChon={TAN_SUAT_ANH}
          onDoi={onDoiAnhCoThe}
          coKe
        />
      </View>
    </KhoiForm>
  );
}
