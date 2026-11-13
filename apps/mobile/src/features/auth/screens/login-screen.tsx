import { useState } from 'react';
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Text, View } from 'react-native';
import { router } from 'expo-router';
import { ApiError } from '@/core/api/api-error';
import { useSession } from '@/core/auth/session';
import { AppText, Button, Card, Screen, TextField } from '@/shared/components/ui';
import { GoogleSignInButton } from '../components/google-sign-in-button';
import { loginSchema, type LoginForm } from '../schemas/login-schema';

const CONNECTION_CODES = ['NETWORK', 'TIMEOUT'];

export function LoginScreen() {
  const { signIn } = useSession();
  const [failure, setFailure] = useState<{ message: string; retryable: boolean } | null>(null);
  const { control, handleSubmit, formState } = useForm<LoginForm>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: '', password: '' },
  });

  const submit = handleSubmit(async ({ email, password }) => {
    setFailure(null);
    try {
      await signIn(email, password);
    } catch (error) {
      const apiError = error instanceof ApiError ? error : null;
      setFailure({
        message: apiError?.message ?? 'Có lỗi xảy ra. Vui lòng thử lại.',
        retryable: apiError ? CONNECTION_CODES.includes(apiError.code) : false,
      });
    }
  });

  return (
    <Screen>
      <View className="flex-row items-center gap-3 py-3">
        <View className="size-11 items-center justify-center rounded-[14px] bg-primary">
          <Text className="text-[22px] font-black text-primary-foreground">G</Text>
        </View>
        <AppText title>GymBase</AppText>
      </View>
      <Card>
        <AppText title>Đăng nhập</AppText>
        <AppText muted>Dùng email và mật khẩu bạn đã đăng ký.</AppText>
        <Controller
          control={control}
          name="email"
          render={({ field, fieldState }) => (
            <TextField
              label="Email"
              placeholder="ban@email.com"
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
              label="Mật khẩu"
              autoComplete="current-password"
              secureTextEntry
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
          <Text accessibilityRole="alert" className="text-destructive">
            {failure.message}
          </Text>
        ) : null}
        <Button
          label="Đăng nhập tài khoản"
          loading={formState.isSubmitting}
          onPress={() => void submit()}
        />
        {failure?.retryable ? (
          <Button label="Thử lại" secondary onPress={() => void submit()} />
        ) : null}
      </Card>
      <View className="gap-3">
        <AppText muted>Hoặc tiếp tục với</AppText>
        <GoogleSignInButton />
      </View>
      <Button
        label="Quay lại"
        secondary
        onPress={() => (router.canGoBack() ? router.back() : router.replace('/welcome'))}
      />
    </Screen>
  );
}
