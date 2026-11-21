import { Text, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { router, useLocalSearchParams } from 'expo-router';
import { useSession } from '@/core/auth/session';
import { SuccessBadge } from '@/features/auth';
import { Button, FixedScreen } from '@/shared/components/ui';
import { Rise } from '@/shared/motion';
import { useTheme } from '@/shared/theme/theme';
import { coKhoiPt } from '../schemas/profile-detail-schema';
import { parseVaiTro } from '../types/vai-tro';

type IconName = keyof typeof Ionicons.glyphMap;

/** Gợi ý ở màn chào mừng học viên: chỉ nêu chức năng đã có, phần chưa làm ghi rõ là sắp ra mắt. */
const GOI_Y: readonly { icon: IconName; title: string; description: string }[] = [
  {
    icon: 'newspaper-outline',
    title: 'Bảng tin cộng đồng',
    description: 'Xem bài tập, thực đơn và tiến độ của mọi người.',
  },
  {
    icon: 'camera-outline',
    title: 'Chia sẻ tiến độ',
    description: 'Đăng ảnh, video để ghi lại từng bước tiến bộ của bạn.',
  },
  {
    icon: 'sparkles-outline',
    title: 'Lộ trình và tin nhắn',
    description: 'Sắp ra mắt. GymBase sẽ báo ngay khi sẵn sàng.',
  },
];

/** Tên gọi: từ cuối của họ tên ("Nguyễn Minh Anh" thành "Anh"); không có họ tên thì để trống. */
function tenGoi(hoTen: string | undefined) {
  return hoTen?.trim().split(/\s+/).pop() ?? '';
}

/**
 * Màn sau khi lưu hồ sơ ở bước cuối. Vai trò có khối PT (`pt`, `ca_hai`): báo đã gửi hồ sơ PT và đang chờ xác minh
 * (Storyboard 3, bước 6). Học viên: chào mừng đến GymBase và gợi ý việc làm tiếp. Cả hai kết thúc ở Trang chủ.
 */
export function ProfileDoneScreen() {
  const { colors } = useTheme();
  const { user } = useSession();
  const { loai } = useLocalSearchParams<{ loai?: string }>();
  const vaiTro = parseVaiTro(loai);
  const laPt = coKhoiPt(vaiTro);
  const ten = tenGoi(user?.ho_ten);
  const tieuDe = laPt
    ? 'Đã gửi hồ sơ PT!'
    : ten
      ? `Chào mừng ${ten} đến với GymBase!`
      : 'Chào mừng đến với GymBase!';
  const veTrangChu = () => router.replace('/home');

  return (
    <FixedScreen>
      <View className="flex-1 items-center justify-center gap-4">
        <SuccessBadge />

        <Rise delay={220}>
          <Text
            accessibilityRole="header"
            className="text-center text-[28px] font-bold leading-9 text-foreground"
          >
            {tieuDe}
          </Text>
        </Rise>

        {laPt ? (
          <>
            <Rise delay={300}>
              <View
                accessibilityLabel="Trạng thái: Chờ xác minh"
                className="flex-row items-center gap-2 rounded-full bg-secondary px-4 py-2"
              >
                <Ionicons name="time-outline" size={18} color={colors.primary} />
                <Text className="text-sm font-bold text-foreground">Chờ xác minh</Text>
              </View>
            </Rise>
            <Rise delay={380}>
              <Text className="text-center text-base leading-6 text-muted-foreground">
                Hồ sơ PT của bạn đã được ghi nhận và đang chờ đội ngũ GymBase xác minh.
                {vaiTro === 'ca_hai' ? ' Hồ sơ học viên của bạn đã sẵn sàng dùng ngay.' : ''}
              </Text>
            </Rise>
            <Rise delay={460} style={{ alignSelf: 'stretch' }}>
              <View className="flex-row gap-3 rounded-2xl bg-secondary p-4">
                <Ionicons name="information-circle-outline" size={22} color={colors.primary} />
                <Text className="flex-1 text-sm leading-5 text-muted-foreground">
                  Sau khi được xác minh, badge PT sẽ hiển thị trên hồ sơ và bài viết của bạn, đồng
                  thời bạn có thể quản lý học viên và tạo lộ trình.
                </Text>
              </View>
            </Rise>
          </>
        ) : (
          <>
            <Rise delay={300}>
              <Text className="text-center text-base leading-6 text-muted-foreground">
                Hồ sơ của bạn đã sẵn sàng. Cùng bắt đầu hành trình tập luyện nào.
              </Text>
            </Rise>
            <View className="mt-2 w-full gap-3">
              {GOI_Y.map((goiY, index) => (
                <Rise key={goiY.title} delay={380 + index * 90} style={{ alignSelf: 'stretch' }}>
                  <View className="flex-row items-center gap-3 rounded-2xl bg-secondary p-4">
                    <View className="size-11 items-center justify-center rounded-full bg-card">
                      <Ionicons name={goiY.icon} size={22} color={colors.primary} />
                    </View>
                    <View className="flex-1">
                      <Text className="text-base font-bold text-foreground">{goiY.title}</Text>
                      <Text className="text-sm leading-5 text-muted-foreground">
                        {goiY.description}
                      </Text>
                    </View>
                  </View>
                </Rise>
              ))}
            </View>
          </>
        )}
      </View>

      <Button pill label={laPt ? 'Về Trang chủ' : 'Bắt đầu khám phá'} onPress={veTrangChu} />
    </FixedScreen>
  );
}
