import { useState } from 'react';
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { View } from 'react-native';
import { router, useLocalSearchParams } from 'expo-router';
import { Button } from '@/components/ui/button';
import { Text } from '@/components/ui/text';
import { ApiError } from '@/core/api/api-error';
import { useSession } from '@/core/auth/session';
import { Screen, TextField } from '@/shared/components/ui';
import { AuthHeader } from '../components/auth-header';
import { GoogleSignInButton } from '../components/google-sign-in-button';
import { OrDivider } from '../components/or-divider';
import { loginSchema, type LoginForm } from '../schemas/login-schema';

const CONNECTION_CODES = ['NETWORK', 'TIMEOUT'];
const INVALID_CREDENTIALS = 'AUTH_INVALID_CREDENTIALS';

/**
 * Sai từ lần này trở đi thì nhắc người dùng có thể đang bị khóa tạm: server khóa 1 đến 15 phút từ lần sai thứ 5 nhưng vẫn
 * trả đúng câu "sai mật khẩu" để không lộ tài khoản, nên người dùng cứ thử tiếp sẽ kéo dài thời gian khóa.
 */
const HINT_AFTER_FAILURES = 3;
const LOCK_HINT = 'Nếu bạn chắc chắn đã nhập đúng, hãy đợi vài phút rồi thử lại.';

export function LoginScreen() {
  const { signIn } = useSession();
  // Màn đăng ký chuyển sang đây kèm email vừa đăng ký.
  const { email: registeredEmail, registered } = useLocalSearchParams<{
    email?: string;
    registered?: string;
  }>();
  const [failure, setFailure] = useState<{
    message: string;
    retryable: boolean;
    hint?: string;
  } | null>(null);
  const [wrongPasswords, setWrongPasswords] = useState(0);
  const { control, handleSubmit, formState } = useForm<LoginForm>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: registeredEmail ?? '', password: '' },
  });

  const submit = handleSubmit(async ({ email, password }) => {
    setFailure(null);
    try {
      const result = await signIn(email, password);
      router.replace(result.user.is_dang_nhap_lan_dau ? '/onboarding' : '/home');
    } catch (error) {
      const apiError = error instanceof ApiError ? error : null;
      const wrongPassword = apiError?.code === INVALID_CREDENTIALS;
      const attempts = wrongPassword ? wrongPasswords + 1 : wrongPasswords;
      // Giữ nguyên mật khẩu người dùng vừa gõ: phần lớn trường hợp là gõ nhầm vài ký tự hoặc sai email, xóa đi thì phải
      // nhập lại từ đầu.
      if (wrongPassword) setWrongPasswords(attempts);
      setFailure({
        message: apiError?.message ?? 'Có lỗi xảy ra. Vui lòng thử lại.',
        retryable: apiError ? CONNECTION_CODES.includes(apiError.code) : false,
        hint: wrongPassword && attempts >= HINT_AFTER_FAILURES ? LOCK_HINT : undefined,
      });
    }
  });

  return (
    <Screen>
      <AuthHeader
        onBack={() => (router.canGoBack() ? router.back() : router.replace('/welcome'))}
      />
      <View className="my-4 items-center gap-2">
        <Text variant="h1">Đăng nhập</Text>
        <Text variant="muted" className="text-center">
          Chào mừng bạn quay trở lại!
        </Text>
      </View>
      <View className="gap-4">
        {registered ? (
          <Text accessibilityRole="alert" className="text-sm font-semibold text-primary">
            Đăng ký thành công. Hãy đăng nhập để tiếp tục.
          </Text>
        ) : null}
        <Controller
          control={control}
          name="email"
          render={({ field, fieldState }) => (
            <TextField
              inputRef={field.ref}
              label="Email"
              hideLabel
              icon="mail-outline"
              autoComplete="email"
              keyboardType="email-address"
              autoCapitalize="none"
              autoCorrect={false}
              value={field.value}
              onChangeText={field.onChange}
              onBlur={field.onBlur}
              error={fieldState.error?.message}
              returnKeyType="next"
            />
          )}
        />
        <Controller
          control={control}
          name="password"
          render={({ field, fieldState }) => (
            <TextField
              inputRef={field.ref}
              label="Mật khẩu"
              hideLabel
              icon="lock-closed-outline"
              passwordToggle
              autoComplete="current-password"
              autoCapitalize="none"
              value={field.value}
              onChangeText={field.onChange}
              onBlur={field.onBlur}
              error={fieldState.error?.message}
              returnKeyType="go"
              onSubmitEditing={() => void submit()}
            />
          )}
        />
        {failure ? (
          <View className="gap-1">
            <Text accessibilityRole="alert" variant="destructive" className="text-sm">
              {failure.message}
            </Text>
            {failure.hint ? (
              <Text variant="muted" className="text-sm">
                {failure.hint}
              </Text>
            ) : null}
          </View>
        ) : null}
        <Button
          shape="pill"
          accessibilityLabel="Đăng nhập"
          loading={formState.isSubmitting}
          onPress={() => void submit()}
        >
          <Text>Đăng nhập</Text>
        </Button>
      </View>
      <OrDivider />
      <GoogleSignInButton />
    </Screen>
  );
}
