import { useState } from 'react';
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Linking, Pressable, Text, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { router } from 'expo-router';
import { ApiError } from '@/core/api/api-error';
import { useSession } from '@/core/auth/session';
import { env } from '@/core/config/env';
import { AppText, Button, Checkbox, Screen, TextField } from '@/shared/components/ui';
import { useTheme } from '@/shared/theme/theme';
import { AuthHeader } from '../components/auth-header';
import { ReferralInfoModal } from '../components/referral-info-modal';
import { registerSchema, type RegisterForm } from '../schemas/register-schema';

const CONNECTION_CODES = ['NETWORK', 'TIMEOUT'];
const EMAIL_TAKEN_MESSAGE = 'Email này đã được dùng. Hãy đăng nhập hoặc dùng email khác.';

/** Tên trường JSON của API đăng ký và trường tương ứng trong form. */
const SERVER_FIELDS: Record<string, keyof RegisterForm> = {
  ho_ten: 'name',
  email: 'email',
  so_dien_thoai: 'phone',
  mat_khau: 'password',
  dong_y_dieu_khoan: 'agreed',
  ma_gioi_thieu: 'referral',
};

export function RegisterScreen() {
  const { register } = useSession();
  const { colors } = useTheme();
  const [referralInfoOpen, setReferralInfoOpen] = useState(false);
  const [emailTaken, setEmailTaken] = useState(false);
  const [failure, setFailure] = useState<{ message: string; retryable: boolean } | null>(null);
  const { control, handleSubmit, setError, formState } = useForm<RegisterForm>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      name: '',
      email: '',
      phone: '',
      password: '',
      confirm: '',
      referral: '',
      agreed: false,
    },
  });

  const submit = handleSubmit(async (form) => {
    setFailure(null);
    try {
      // TODO(REFERRAL): backend nhận ma_gioi_thieu nhưng chưa xử lý; vẫn gửi để khớp thiết kế.
      const account = await register({
        ho_ten: form.name,
        email: form.email,
        so_dien_thoai: form.phone,
        mat_khau: form.password,
        dong_y_dieu_khoan: form.agreed,
        ...(form.referral ? { ma_gioi_thieu: form.referral } : {}),
      });
<<<<<<< HEAD
      router.replace('/register-success');
=======
      // Đăng ký không có phiên: về màn đăng nhập, điền sẵn email và báo đã đăng ký xong.
      router.replace({
        pathname: '/login',
        params: { email: account.email ?? form.email, registered: '1' },
      });
>>>>>>> develop
    } catch (error) {
      const apiError = error instanceof ApiError ? error : null;
      if (apiError?.code === 'USER_EMAIL_TAKEN') {
        setError('email', { message: EMAIL_TAKEN_MESSAGE });
        setEmailTaken(true);
        return;
      }
      const fieldMessages = Object.entries(apiError?.fieldErrors ?? {}).flatMap(
        ([field, messages]) => {
          const target = SERVER_FIELDS[field];
          return target && messages[0] ? [{ target, message: messages[0] }] : [];
        },
      );
      if (fieldMessages.length > 0) {
        fieldMessages.forEach(({ target, message }) => setError(target, { message }));
        return;
      }
      setFailure({
        message: apiError?.message ?? 'Có lỗi xảy ra. Vui lòng thử lại.',
        retryable: apiError ? CONNECTION_CODES.includes(apiError.code) : false,
      });
    }
  });

  return (
    <Screen>
      <AuthHeader
        onBack={() => (router.canGoBack() ? router.back() : router.replace('/welcome'))}
      />
      <View className="items-center gap-1">
        <AppText title>Đăng ký tài khoản</AppText>
        <AppText muted className="text-center">
          Tạo tài khoản GymBase để kết nối PT, theo dõi lộ trình tập luyện và dinh dưỡng của bạn.
        </AppText>
      </View>
      <View className="gap-3">
        <Controller
          control={control}
          name="name"
          render={({ field, fieldState }) => (
            <TextField
              inputRef={field.ref}
              label="Họ và tên"
              hideLabel
              icon="person-outline"
              autoComplete="name"
              maxLength={100}
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
              onChangeText={(text) => {
                // Sửa email thì câu "email đã được dùng" không còn đúng nữa.
                setEmailTaken(false);
                field.onChange(text);
              }}
              onBlur={field.onBlur}
              error={fieldState.error?.message}
              errorAction={
                emailTaken ? (
                  <Text
                    accessibilityRole="link"
                    className="font-bold underline"
                    onPress={() => router.replace('/login')}
                  >
                    Đăng nhập ngay
                  </Text>
                ) : undefined
              }
              returnKeyType="next"
            />
          )}
        />
        <Controller
          control={control}
          name="phone"
          render={({ field, fieldState }) => (
            <TextField
              inputRef={field.ref}
              label="Số điện thoại"
              hideLabel
              icon="call-outline"
              autoComplete="tel"
              keyboardType="phone-pad"
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
              autoComplete="new-password"
              autoCapitalize="none"
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
          name="confirm"
          render={({ field, fieldState }) => (
            <TextField
              inputRef={field.ref}
              label="Xác nhận mật khẩu"
              hideLabel
              icon="lock-closed-outline"
              passwordToggle
              autoComplete="new-password"
              autoCapitalize="none"
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
          name="referral"
          render={({ field, fieldState }) => (
            <TextField
              inputRef={field.ref}
              label="Mã giới thiệu (nếu có)"
              hideLabel
              icon="gift-outline"
              trailing={
                <Pressable
                  accessibilityRole="button"
                  accessibilityLabel="Mã giới thiệu là gì?"
                  hitSlop={8}
                  onPress={() => setReferralInfoOpen(true)}
                >
                  <Ionicons name="information-circle-outline" size={22} color={colors.muted} />
                </Pressable>
              }
              autoCapitalize="characters"
              autoCorrect={false}
              maxLength={50}
              value={field.value}
              onChangeText={field.onChange}
              onBlur={field.onBlur}
              error={fieldState.error?.message}
              returnKeyType="done"
            />
          )}
        />
        <Controller
          control={control}
          name="agreed"
          render={({ field, fieldState }) => (
            <Checkbox
              checked={field.value}
              onChange={field.onChange}
              label="Tôi đồng ý với Điều khoản sử dụng và Chính sách bảo mật của GymBase"
              error={fieldState.error?.message}
            >
              Tôi đồng ý với{' '}
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
            </Checkbox>
          )}
        />
        {failure ? (
          <Text accessibilityRole="alert" className="text-destructive">
            {failure.message}
          </Text>
        ) : null}
        <Button
          pill
          label="Đăng ký"
          loading={formState.isSubmitting}
          onPress={() => void submit()}
        />
        {failure?.retryable ? (
          <Button pill label="Thử lại" secondary onPress={() => void submit()} />
        ) : null}
      </View>
      <Text className="text-center text-[15px] text-muted-foreground">
        Đã có tài khoản?{' '}
        <Text
          accessibilityRole="link"
          className="font-bold text-foreground"
          onPress={() => router.replace('/login')}
        >
          Đăng nhập
        </Text>
      </Text>
      <ReferralInfoModal visible={referralInfoOpen} onClose={() => setReferralInfoOpen(false)} />
    </Screen>
  );
}
