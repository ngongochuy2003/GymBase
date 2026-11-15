import { Linking, Text } from 'react-native';
import { env } from '@/core/config/env';

/** Dòng nhỏ cuối màn chào mừng; hai cụm có link mở Điều khoản sử dụng và Chính sách bảo mật. */
export function LegalNotice() {
  return (
    <Text className="px-4 text-center text-[13px] leading-5 text-muted-foreground">
      Bằng việc tiếp tục, bạn đồng ý với{' '}
      <Text
        accessibilityRole="link"
        className="font-semibold text-primary underline"
        onPress={() => void Linking.openURL(env.termsUrl)}
      >
        Điều khoản sử dụng
      </Text>{' '}
      và{' '}
      <Text
        accessibilityRole="link"
        className="font-semibold text-primary underline"
        onPress={() => void Linking.openURL(env.privacyUrl)}
      >
        Chính sách bảo mật
      </Text>{' '}
      của GymBase
    </Text>
  );
}
