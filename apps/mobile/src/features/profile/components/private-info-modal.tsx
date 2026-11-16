import { Modal, Pressable, ScrollView, Text, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { Button } from '@/shared/components/ui';
import { useTheme } from '@/shared/theme/theme';

/** Trường riêng tư ở màn thông tin cá nhân có nút (?) giải thích. */
export type PrivateField = 'ngay_sinh';

const CONTENT: Record<PrivateField, { title: string; purpose: string }> = {
  ngay_sinh: {
    title: 'Ngày sinh dùng để làm gì?',
    purpose:
      'GymBase tính tuổi của bạn để PT chọn cường độ, bài tập và lộ trình phù hợp, đồng thời ước tính nhu cầu năng lượng khi xây dựng kế hoạch dinh dưỡng.',
  },
};

/**
 * Khung giải thích mở từ nút (?) cạnh ô ngày sinh: thông tin này dùng để làm gì và ai được xem.
 * Quy tắc hiển thị khớp backend: không xuất hiện ở hồ sơ công khai, chỉ PT đã kết nối với bạn xem được.
 */
export function PrivateInfoModal({
  field,
  onClose,
}: {
  /** Trường đang mở; null thì đóng khung. */
  field: PrivateField | null;
  onClose(): void;
}) {
  const { colors } = useTheme();
  const content = field ? CONTENT[field] : null;
  return (
    <Modal transparent animationType="fade" visible={field !== null} onRequestClose={onClose}>
      <Pressable
        accessibilityLabel="Đóng"
        onPress={onClose}
        className="flex-1 items-center justify-center bg-black/50 p-6"
      >
        {/* Chạm vào thẻ không đóng khung. */}
        <Pressable className="max-h-full w-full max-w-[420px] rounded-3xl bg-card">
          <ScrollView contentContainerClassName="gap-4 p-5" bounces={false}>
            <View className="flex-row items-center gap-3">
              <Ionicons name="help-circle-outline" size={28} color={colors.primary} />
              <Text accessibilityRole="header" className="flex-1 text-xl font-bold text-foreground">
                {content?.title}
              </Text>
            </View>
            <View className="flex-row items-start gap-3">
              <View className="size-10 items-center justify-center rounded-full bg-secondary">
                <Ionicons name="lock-closed-outline" size={20} color={colors.primary} />
              </View>
              <View className="flex-1 gap-0.5">
                <Text className="text-base font-semibold text-foreground">Ai xem được?</Text>
                <Text className="text-sm leading-5 text-muted-foreground">
                  Thông tin này không hiển thị công khai. Chỉ PT đã kết nối với bạn mới xem được.
                </Text>
              </View>
            </View>
            <View className="flex-row items-start gap-3">
              <View className="size-10 items-center justify-center rounded-full bg-secondary">
                <Ionicons name="barbell-outline" size={20} color={colors.primary} />
              </View>
              <View className="flex-1 gap-0.5">
                <Text className="text-base font-semibold text-foreground">Dùng để làm gì?</Text>
                <Text className="text-sm leading-5 text-muted-foreground">{content?.purpose}</Text>
              </View>
            </View>
            <Button label="Đã hiểu" pill onPress={onClose} />
          </ScrollView>
        </Pressable>
      </Pressable>
    </Modal>
  );
}
