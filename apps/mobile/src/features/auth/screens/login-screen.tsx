import { useState } from 'react';
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { StyleSheet, Text, View } from 'react-native';
import { useSession } from '@/core/auth/session';
import { ApiError } from '@/core/api/api-error';
import { AppText, Button, Card, Screen, TextField } from '@/shared/components/ui';
import { useTheme } from '@/shared/theme/theme';
import { loginSchema, type LoginForm } from '../schemas/login-schema';

export function LoginScreen() {
  const { login } = useSession();
  const { colors } = useTheme();
  const [submitError, setSubmitError] = useState('');
  const { control, handleSubmit, formState: { isSubmitting } } = useForm<LoginForm>({
    resolver: zodResolver(loginSchema), defaultValues: { userName: '', password: '' },
  });
  const submit = handleSubmit(async ({ userName, password }) => {
    setSubmitError('');
    try { await login(userName, password); }
    catch (error) { setSubmitError(error instanceof ApiError ? error.message : 'Không thể đăng nhập. Kiểm tra kết nối API.'); }
  });
  return (
    <Screen>
      <View style={styles.brandRow}><View style={[styles.mark, { backgroundColor: colors.primary }]}><Text style={{ color: colors.onPrimary, fontWeight: '900', fontSize: 22 }}>G</Text></View><AppText title>GymBase</AppText></View>
      <View style={[styles.hero, { backgroundColor: colors.hero }]}>
        <Text style={[styles.eyebrow, { color: colors.accent }]}>GYMBASE</Text>
        <Text style={[styles.headline, { color: colors.heroText }]}>Tập có kế hoạch.{ '\n' }Tiến bộ cùng nhau.</Text>
        <Text style={[styles.copy, { color: colors.heroText }]}>Đăng nhập để xem bảng tin và chia sẻ tiến trình tập luyện.</Text>
      </View>
      <Card>
        <AppText title>Đăng nhập</AppText>
        <Controller control={control} name="userName" render={({ field, fieldState }) => <TextField label="Tên đăng nhập" autoCapitalize="none" autoComplete="username" value={field.value} onChangeText={field.onChange} onBlur={field.onBlur} error={fieldState.error?.message} returnKeyType="next" />} />
        <Controller control={control} name="password" render={({ field, fieldState }) => <TextField label="Mật khẩu" secureTextEntry autoComplete="password" value={field.value} onChangeText={field.onChange} onBlur={field.onBlur} error={fieldState.error?.message} onSubmitEditing={() => void submit()} returnKeyType="go" />} />
        {submitError ? <AppText>{submitError}</AppText> : null}
        <Button label="Đăng nhập" loading={isSubmitting} onPress={() => void submit()} />
      </Card>
    </Screen>
  );
}
const styles = StyleSheet.create({ brandRow: { flexDirection: 'row', gap: 12, alignItems: 'center', paddingVertical: 12 }, mark: { width: 44, height: 44, borderRadius: 14, alignItems: 'center', justifyContent: 'center' }, hero: { borderRadius: 28, padding: 28, gap: 22 }, eyebrow: { fontSize: 11, fontWeight: '800', letterSpacing: 1.6 }, headline: { fontSize: 36, lineHeight: 43, fontWeight: '800', letterSpacing: -1 }, copy: { fontSize: 16, lineHeight: 25, opacity: 0.85 } });

