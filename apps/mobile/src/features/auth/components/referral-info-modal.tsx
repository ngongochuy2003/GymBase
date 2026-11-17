import { Modal, Pressable, ScrollView, Text, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { Button } from '@/shared/components/ui';
import { useTheme } from '@/shared/theme/theme';

const NOTES = [
  'Mã giới thiệu là tùy chọn.',
  'Mỗi tài khoản chỉ nhập 1 mã khi đăng ký.',
  'Điểm thưởng sẽ được dùng để đổi quà hoặc quy đổi trong các phiên bản sau.',
];

function Row({
  icon,
  title,
  children,
}: {
  icon: 'people-outline' | 'gift-outline';
  title: string;
  children: string;
}) {
  const { colors } = useTheme();
  return (
    <View className="flex-row items-start gap-3">
      <View className="size-10 items-center justify-center rounded-full bg-secondary">
        <Ionicons name={icon} size={20} color={colors.primary} />
      </View>
      <View className="flex-1 gap-0.5">
        <Text className="text-base font-semibold text-foreground">{title}</Text>
        <Text className="text-sm leading-5 text-muted-foreground">{children}</Text>
      </View>
    </View>
  );
}

/** Khung giải thích "Mã giới thiệu là gì?" mở từ biểu tượng (i); chỉ có nội dung giải thích, chưa tính điểm. */
export function ReferralInfoModal({ visible, onClose }: { visible: boolean; onClose(): void }) {
  const { colors } = useTheme();
  return (
    <Modal transparent animationType="fade" visible={visible} onRequestClose={onClose}>
      <Pressable
        accessibilityLabel="Đóng"
        onPress={onClose}
        className="flex-1 items-center justify-center bg-black/50 p-6"
      >
        {/* Chạm vào thẻ không đóng khung. */}
        <Pressable className="max-h-full w-full max-w-[420px] rounded-3xl bg-card">
          <ScrollView contentContainerClassName="gap-4 p-5" bounces={false}>
            <View className="flex-row items-center gap-3">
              <Ionicons name="gift-outline" size={28} color={colors.primary} />
              <Text accessibilityRole="header" className="text-xl font-bold text-foreground">
                Mã giới thiệu là gì?
              </Text>
            </View>
            <Text className="text-sm leading-5 text-muted-foreground">
              Là mã do người dùng khác chia sẻ cho bạn khi đăng ký tài khoản. Khi bạn đăng ký thành
              công, người giới thiệu sẽ được cộng điểm thưởng.
            </Text>
            <Row icon="people-outline" title="Bạn nhận được">
              Hỗ trợ từ người đã giới thiệu bạn, đồng thời có thể nhận ưu đãi trong tương lai.
            </Row>
            <Row icon="gift-outline" title="Người giới thiệu nhận được">
              Điểm thưởng vào tài khoản khi bạn đăng ký thành công.
            </Row>
            <View className="gap-1">
              <Text className="text-base font-semibold text-foreground">Lưu ý</Text>
              {NOTES.map((note) => (
                <Text key={note} className="text-sm leading-5 text-muted-foreground">
                  {'•'} {note}
                </Text>
              ))}
            </View>
            <Button label="Đã hiểu" pill onPress={onClose} />
          </ScrollView>
        </Pressable>
      </Pressable>
    </Modal>
  );
}
