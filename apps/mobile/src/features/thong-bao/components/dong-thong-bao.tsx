import { Pressable, View } from 'react-native';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { formatRelativeTimeVi } from '@/features/feed';
import { Icon } from '@/shared/icons';
import type { ThongBaoDto } from '@/shared/notifications';
import { useTheme } from '@/shared/theme/theme';

/** Icon theo loại thông báo (tên đã đăng ký trong icon-registry); loại lạ dùng chuông. */
const ICON_THEO_LOAI: Record<string, string> = {
  tin_nhan: 'message',
  nhac_tap: 'dumbbell',
  nhac_an: 'meal',
  danh_gia: 'thumbs-up',
  ket_noi_pt: 'user',
};

export interface DongThongBaoProps {
  thongBao: ThongBaoDto;
  onPress(thongBao: ThongBaoDto): void;
}

/** Một dòng hộp thư: icon theo loại, tiêu đề, nội dung ngắn, thời gian; chấm màu nếu chưa đọc. */
export function DongThongBao({ thongBao, onPress }: DongThongBaoProps) {
  const { colors } = useTheme();
  const chuaDoc = !thongBao.da_doc;
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={`${thongBao.tieu_de}${chuaDoc ? ', chưa đọc' : ''}`}
      onPress={() => onPress(thongBao)}
      className={cn(
        'flex-row items-start gap-3 border-b border-border px-4 py-3 active:bg-accent',
        chuaDoc && 'bg-card',
      )}
    >
      <View className="h-10 w-10 items-center justify-center rounded-full bg-secondary">
        <Icon name={ICON_THEO_LOAI[thongBao.loai_thong_bao] ?? 'bell'} size={20} color={colors.primary} />
      </View>
      <View className="flex-1 gap-0.5">
        <Text className={cn('text-base', chuaDoc && 'font-bold')} numberOfLines={2}>
          {thongBao.tieu_de}
        </Text>
        {thongBao.noi_dung ? (
          <Text variant="muted" className="text-sm" numberOfLines={2}>
            {thongBao.noi_dung}
          </Text>
        ) : null}
        <Text variant="muted" className="text-xs">
          {formatRelativeTimeVi(thongBao.thoi_diem)}
        </Text>
      </View>
      {chuaDoc ? (
        <View testID="cham-chua-doc" className="mt-2 h-2.5 w-2.5 rounded-full bg-primary" />
      ) : null}
    </Pressable>
  );
}
