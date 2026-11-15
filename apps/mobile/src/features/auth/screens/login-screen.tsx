import { useState } from 'react';
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Text, View } from 'react-native';
import { router, useLocalSearchParams } from 'expo-router';
import { ApiError } from '@/core/api/api-error';
import { useSession } from '@/core/auth/session';
import { AppText, Button, Screen, TextField } from '@/shared/components/ui';
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
  const { control, handleSubmit, formState, resetField, setFocus } = useForm<LoginForm>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: registeredEmail ?? '', password: '' },
  });

  const submit = handleSubmit(async ({ email, password }) => {
    setFailure(null);
    try {
      await signIn(email, password);
    } catch (error) {
      const apiError = error instanceof ApiError ? error : null;
      const wrongPassword = apiError?.code === INVALID_CREDENTIALS;
      const attempts = wrongPassword ? wrongPasswords + 1 : wrongPasswords;
      if (wrongPassword) {
        setWrongPasswords(attempts);
        // Xóa mật khẩu vừa gõ sai và đưa con trỏ về ô đó để nhập lại ngay.
        resetField('password');
        setFocus('password');
      }
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
      <View className="items-center gap-1">
        <AppText title>Đăng nhập</AppText>
        <AppText muted className="text-center">
          Dùng email và mật khẩu bạn đã đăng ký.
        </AppText>
      </View>
      <View className="gap-3">
        {registered ? (
          <Text accessibilityRole="alert" className="font-semibold text-primary">
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
            <Text accessibilityRole="alert" className="text-destructive">
              {failure.message}
            </Text>
            {failure.hint ? (
              <Text className="text-sm text-muted-foreground">{failure.hint}</Text>
            ) : null}
          </View>
        ) : null}
        <Button
          pill
          label="Đăng nhập"
          loading={formState.isSubmitting}
          onPress={() => void submit()}
        />
      </View>
      <OrDivider />
      <GoogleSignInButton />
    </Screen>
  );
}
