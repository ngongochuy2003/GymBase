import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { StyleSheet, Text, View } from 'react-native';
import { useSession } from '@/core/auth/session';
import { env } from '@/core/config/env';
import { AppText, Button, Card, Screen, TextField } from '@/shared/components/ui';
import { useTheme } from '@/shared/theme/theme';
import { demoSchema, type DemoForm } from '../schemas/demo-schema';

export function LoginScreen() {
  const { enterDemo } = useSession();
  const { colors } = useTheme();
  const { control, handleSubmit } = useForm<DemoForm>({
    resolver: zodResolver(demoSchema),
    defaultValues: { name: '' },
  });
  return (
    <Screen>
      <View style={styles.brandRow}>
        <View style={[styles.mark, { backgroundColor: colors.primary }]}>
          <Text style={{ color: colors.onPrimary, fontWeight: '900', fontSize: 22 }}>G</Text>
        </View>
        <AppText title>gymbase</AppText>
      </View>
      <View style={[styles.hero, { backgroundColor: colors.hero }]}>
        <Text style={[styles.eyebrow, { color: colors.accent }]}>MỖI NGÀY MỘT CHÚT TIẾN BỘ</Text>
        <Text style={[styles.headline, { color: colors.heroText }]}>
          Tập có kế hoạch.{'\n'}Tiến bộ cùng nhau.
        </Text>
        <Text style={[styles.copy, { color: colors.heroText }]}>
          Một nơi để bạn và PT đồng hành trên từng buổi tập.
        </Text>
        <View style={styles.tags}>
          {['Lịch tập', 'Tiến trình', 'Kết nối PT'].map((t) => (
            <View key={t} style={styles.tag}>
              <Text style={{ color: colors.heroText }}>{t}</Text>
            </View>
          ))}
        </View>
      </View>
      <Card>
        <AppText title>Bắt đầu cùng GymBase</AppText>
        <AppText muted>Bản trải nghiệm • Dữ liệu minh họa, chưa kết nối backend.</AppText>
        {env.dataMode === 'demo' ? (
          <>
            <Controller
              control={control}
              name="name"
              render={({ field, fieldState }) => (
                <TextField
                  label="Tên của bạn"
                  placeholder="Ví dụ: Minh Anh"
                  autoComplete="name"
                  maxLength={40}
                  value={field.value}
                  onChangeText={field.onChange}
                  onBlur={field.onBlur}
                  error={fieldState.error?.message}
                  returnKeyType="go"
                  onSubmitEditing={() => void handleSubmit(({ name }) => enterDemo(name))()}
                />
              )}
            />
            <Button
              label="Khám phá bản demo"
              onPress={handleSubmit(({ name }) => enterDemo(name))}
            />
          </>
        ) : (
          <AppText>
            Chế độ API chưa được tích hợp đăng nhập. Cần hoàn thiện backend trước khi sử dụng.
          </AppText>
        )}
      </Card>
      <AppText muted>
        Không cần tài khoản. Phiên demo kết thúc khi bạn tải lại hoặc thoát ứng dụng.
      </AppText>
    </Screen>
  );
}

const styles = StyleSheet.create({
  brandRow: { flexDirection: 'row', gap: 12, alignItems: 'center', paddingVertical: 12 },
  mark: { width: 44, height: 44, borderRadius: 14, alignItems: 'center', justifyContent: 'center' },
  hero: { borderRadius: 28, padding: 28, gap: 22 },
  eyebrow: { fontSize: 11, fontWeight: '800', letterSpacing: 1.6 },
  headline: { fontSize: 36, lineHeight: 43, fontWeight: '800', letterSpacing: -1 },
  copy: { fontSize: 16, lineHeight: 25, opacity: 0.85 },
  tags: { flexDirection: 'row', gap: 8, flexWrap: 'wrap' },
  tag: {
    borderRadius: 20,
    paddingHorizontal: 12,
    paddingVertical: 7,
    backgroundColor: '#FFFFFF18',
  },
});
