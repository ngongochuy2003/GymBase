import { useEffect, useState } from 'react';
import { Controller, useForm, useWatch } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Pressable, Text, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { router, useLocalSearchParams } from 'expo-router';
import { AuthHeader } from '@/features/auth';
import { useSession } from '@/core/auth/session';
import { FieldLabel } from '@/shared/components/field-label';
import { SelectField } from '@/shared/components/select-field';
import { Button, Screen, TextField } from '@/shared/components/ui';
import { useDanhMuc } from '@/shared/hooks/use-danh-muc';
import { useTheme } from '@/shared/theme/theme';
import {
  formatNgaySinhInput,
  personalInfoSchema,
  type PersonalInfoForm,
} from '../schemas/personal-info-schema';
import { PrivateInfoModal, type PrivateField } from '../components/private-info-modal';
import { useProfileDraft } from '../state/profile-draft';
import { GIOI_TINH_OPTIONS } from '../types/gioi-tinh';
import { parseVaiTro } from '../types/vai-tro';

const LOAD_ERROR = 'Không tải được danh sách. Vui lòng thử lại.';

/** Nút (?) cạnh nhãn: mở giải thích thông tin riêng tư dùng để làm gì và ai được xem. */
function HelpButton({ label, onPress }: { label: string; onPress(): void }) {
  const { colors } = useTheme();
  return (
    <Pressable accessibilityRole="button" accessibilityLabel={label} hitSlop={10} onPress={onPress}>
      <Ionicons name="help-circle-outline" size={20} color={colors.muted} />
    </Pressable>
  );
}

/**
 * Màn 03A: thông tin cá nhân (phase 1 của thiết lập hồ sơ). Không gọi API lưu: dữ liệu vào bản nháp dùng chung, màn
 * theo vai trò (03B) gửi `PUT /ho-so` một lần ở cuối.
 */
export function PersonalInfoScreen() {
  const { user } = useSession();
  const { loai } = useLocalSearchParams<{ loai?: string }>();
  const vaiTro = parseVaiTro(loai);
  const { draft, update } = useProfileDraft();
  const danhMuc = useDanhMuc(['khu_vuc']);
  const [helpField, setHelpField] = useState<PrivateField | null>(null);

  const { control, handleSubmit } = useForm<PersonalInfoForm>({
    resolver: zodResolver(personalInfoSchema),
    defaultValues: {
      ten_hien_thi: draft.ten_hien_thi || user?.ho_ten || '',
      ngay_sinh: draft.ngay_sinh,
      gioi_tinh: draft.gioi_tinh,
      khu_vuc_hoat_dong: draft.khu_vuc_hoat_dong,
    },
  });

  // Ghi bản nháp ngay khi gõ để quay lại từ bước sau (hoặc rời màn rồi vào lại) vẫn còn dữ liệu.
  const values = useWatch({ control });
  useEffect(() => {
    update(values as Partial<PersonalInfoForm>);
  }, [values, update]);

  const next = handleSubmit((form) => {
    update(form);
    router.push(`/profile-setup/chi-tiet?loai=${vaiTro}`);
  });

  const loadError = danhMuc.isError ? LOAD_ERROR : undefined;

  return (
    <Screen bottomInset>
      <AuthHeader onBack={() => router.back()} />
      <View className="gap-3 py-4">
        <Text className="text-[28px] font-bold leading-9 text-foreground text-center">Thông tin cá nhân</Text>
        <Text className="text-base leading-6 text-muted-foreground text-center">
          Cho GymBase biết đôi chút về bạn để cá nhân hóa trải nghiệm.
        </Text>
      </View>

      <Controller
        control={control}
        name="ten_hien_thi"
        render={({ field, fieldState }) => (
          <TextField
            label="Tên hiển thị *"
            value={field.value}
            onChangeText={field.onChange}
            onBlur={field.onBlur}
            inputRef={field.ref}
            autoCapitalize="words"
            error={fieldState.error?.message}
          />
        )}
      />
      <Controller
        control={control}
        name="ngay_sinh"
        render={({ field, fieldState }) => (
          <View className="gap-2">
            <View className="flex-row items-center gap-2">
              <FieldLabel label="Ngày sinh" />
              <HelpButton
                label="Ngày sinh dùng để làm gì?"
                onPress={() => setHelpField('ngay_sinh')}
              />
            </View>
            <TextField
              hideLabel
              label="Ngày sinh"
              placeholder="dd/MM/yyyy"
              value={field.value}
              onChangeText={(text) => field.onChange(formatNgaySinhInput(text))}
              onBlur={field.onBlur}
              inputRef={field.ref}
              keyboardType="number-pad"
              maxLength={10}
              error={fieldState.error?.message}
            />
          </View>
        )}
      />
      <Controller
        control={control}
        name="gioi_tinh"
        render={({ field, fieldState }) => (
          <SelectField
            label="Giới tính"
            value={field.value || null}
            options={GIOI_TINH_OPTIONS}
            onChange={field.onChange}
            error={fieldState.error?.message}
          />
        )}
      />
      <Controller
        control={control}
        name="khu_vuc_hoat_dong"
        render={({ field, fieldState }) => (
          <SelectField
            label="Khu vực"
            value={field.value || null}
            options={danhMuc.items('khu_vuc')}
            onChange={field.onChange}
            loading={danhMuc.isPending}
            loadError={loadError}
            onRetry={danhMuc.retry}
            error={fieldState.error?.message}
          />
        )}
      />

      <Button pill label="Tiếp tục" trailingIcon="chevron-forward" onPress={() => void next()} />
      <PrivateInfoModal field={helpField} onClose={() => setHelpField(null)} />
    </Screen>
  );
}
