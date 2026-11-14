import { useState } from 'react';
import { Text, View } from 'react-native';
import { router } from 'expo-router';
import { ApiError } from '@/core/api/api-error';
import { GoogleSignInUnavailableError, googleSignIn } from '@/core/auth/google-sign-in';
import { useSession } from '@/core/auth/session';
import { Button } from '@/shared/components/ui';

const UNAVAILABLE_MESSAGES = {
  NOT_CONFIGURED: 'Đăng nhập Google chưa được cấu hình trên bản này.',
  NATIVE_MODULE_MISSING: 'Đăng nhập Google cần bản dev build, không chạy trong Expo Go.',
  PLAY_SERVICES_UNAVAILABLE: 'Thiết bị này chưa có Google Play Services.',
  NO_ID_TOKEN: 'Không lấy được thông tin từ Google. Vui lòng thử lại.',
} as const;

// TODO(LINK-GOOGLE): email đã đăng ký bằng mật khẩu thì hiện chỉ bảo đăng nhập bằng mật khẩu; làm thêm "Liên kết Google"
//   trong tab Cá nhân để người dùng tự liên kết Google vào tài khoản có sẵn.
function errorMessage(error: unknown) {
  if (error instanceof GoogleSignInUnavailableError) return UNAVAILABLE_MESSAGES[error.reason];
  if (error instanceof ApiError) {
    if (error.code === 'AUTH_EMAIL_USED_PASSWORD') {
      return 'Email này đã đăng ký bằng mật khẩu. Hãy đăng nhập bằng email và mật khẩu.';
    }
    return error.message;
  }
  return 'Không đăng nhập được bằng Google. Vui lòng thử lại.';
}

/**
 * Nút "Đăng nhập với Google" dùng ở màn chào mừng và màn đăng nhập. Hủy hộp chọn tài khoản thì không báo lỗi; lỗi mạng
 * thì bấm lại nút để thử lại. Đăng nhập xong màn này bị thay thế nên không cần tự tắt trạng thái chờ.
 */
export function GoogleSignInButton() {
  const { signInWithGoogle } = useSession();
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState<string | null>(null);

  const press = async () => {
    setMessage(null);
    setBusy(true);
    try {
      const idToken = await googleSignIn.getIdToken();
      if (!idToken) {
        setBusy(false);
        return;
      }
      const res = await signInWithGoogle(idToken);
      router.replace(res.tai_khoan_moi ? '/register-success' : '/home');
    } catch (error) {
      setMessage(errorMessage(error));
      setBusy(false);
    }
  };

  return (
    <View className="gap-3">
      <Button label="Đăng nhập với Google" secondary loading={busy} onPress={() => void press()} />
      {message ? (
        <Text accessibilityRole="alert" className="text-destructive">
          {message}
        </Text>
      ) : null}
    </View>
  );
}
