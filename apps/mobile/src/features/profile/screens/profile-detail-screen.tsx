import { useEffect, useState } from 'react';
import { Controller, useForm, useWatch, type FieldErrors, type FieldPath } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Pressable, Text, View } from 'react-native';
import Animated, { useAnimatedStyle, useSharedValue, withTiming } from 'react-native-reanimated';
import { Ionicons } from '@expo/vector-icons';
import { Redirect, router, useLocalSearchParams } from 'expo-router';
import { AuthHeader } from '@/features/auth';
import { ApiError } from '@/core/api/api-error';
import { profileApi, type HoSoLuuInput } from '../api/profile-api';
import { MultiSelectChips } from '@/shared/components/multi-select-chips';
import { SelectField } from '@/shared/components/select-field';
import { Button, Screen, TextField } from '@/shared/components/ui';
import { useDanhMuc } from '@/shared/hooks/use-danh-muc';
import { cn } from '@/lib/utils';
import { Slide, systemReduceMotion } from '@/shared/motion';
import { useTheme } from '@/shared/theme/theme';
import { ChungChiField } from '../components/chung-chi-field';
import { toIsoNgaySinh } from '../schemas/personal-info-schema';
import {
  coKhoiHocVien,
  coKhoiPt,
  createProfileDetailSchema,
  GIOI_THIEU_TOI_DA,
  HOC_VIEN_FIELDS,
  parseSo,
  PT_FIELDS,
  type ProfileDetailForm,
} from '../schemas/profile-detail-schema';
import { useProfileDraft } from '../state/profile-draft';
import { parseVaiTro } from '../types/vai-tro';

const LOAD_ERROR = 'Không tải được danh sách. Vui lòng thử lại.';
const CONNECTION_CODES = ['NETWORK', 'TIMEOUT'];
const PERSONAL_FIELDS = ['ten_hien_thi', 'ngay_sinh', 'gioi_tinh', 'khu_vuc_hoat_dong'];

type Khoi = 'hoc_vien' | 'pt';
type Failure = { message: string; retryable: boolean; personal: boolean };

const TAB_LABEL: Record<Khoi, string> = {
  hoc_vien: 'Hồ sơ học viên',
  pt: 'Hồ sơ PT',
};

/**
 * Hai tab chỉ có khi chọn "cả hai"; tab có lỗi nhập liệu hiện chấm đỏ. Nền chọn trượt giữa hai tab: lớp `absolute inset-1`
 * trùng đúng vùng của hai tab (cùng đơn vị với `p-1` của khung) nên đo chiều rộng của nó là biết mỗi tab rộng bao nhiêu.
 */
function KhoiTabs({
  active,
  onChange,
  loi,
}: {
  active: Khoi;
  onChange(khoi: Khoi): void;
  loi: Record<Khoi, boolean>;
}) {
  const { colors } = useTheme();
  const [trackWidth, setTrackWidth] = useState(0);
  const tabWidth = trackWidth / 2;
  const position = useSharedValue(active === 'pt' ? 1 : 0);

  useEffect(() => {
    position.value = withTiming(active === 'pt' ? 1 : 0, {
      duration: 240,
      reduceMotion: systemReduceMotion(),
    });
  }, [active, position]);

  const indicator = useAnimatedStyle(() => ({
    transform: [{ translateX: position.value * tabWidth }],
  }));

  return (
    <View accessibilityRole="tablist" className="rounded-full bg-secondary p-1">
      <View
        pointerEvents="none"
        className="absolute inset-1"
        onLayout={(event) => setTrackWidth(event.nativeEvent.layout.width)}
      >
        <Animated.View
          style={[
            {
              position: 'absolute',
              top: 0,
              bottom: 0,
              left: 0,
              width: tabWidth,
              borderRadius: 999,
              backgroundColor: colors.surface,
            },
            indicator,
          ]}
        />
      </View>
      <View className="flex-row">
        {(['hoc_vien', 'pt'] as const).map((khoi) => {
          const selected = active === khoi;
          return (
            <Pressable
              key={khoi}
              accessibilityRole="tab"
              accessibilityLabel={TAB_LABEL[khoi]}
              accessibilityState={{ selected }}
              onPress={() => onChange(khoi)}
              className="flex-1 flex-row items-center justify-center gap-1 rounded-full px-3 py-2.5"
            >
              <Text
                className={cn(
                  'text-sm font-bold',
                  selected ? 'text-foreground' : 'text-muted-foreground',
                )}
              >
                {TAB_LABEL[khoi]}
              </Text>
              {loi[khoi] ? <Text className="text-sm font-bold text-destructive">•</Text> : null}
            </Pressable>
          );
        })}
      </View>
    </View>
  );
}

const coLoi = (errors: FieldErrors<ProfileDetailForm>, fields: readonly string[]) =>
  fields.some((name) => name in errors);

/**
 * Màn 03B: thiết lập theo vai trò và là bước cuối của luồng. Chọn "học viên" chỉ hiện khối học viên, chọn "PT" chỉ hiện khối
 * PT, chọn "cả hai" hiện hai tab. Gửi `PUT /ho-so` một lần, gồm dữ liệu cá nhân đã nhập ở 03A (bản nháp dùng chung) và dữ
 * liệu của khối thuộc vai trò.
 */
export function ProfileDetailScreen() {
  const { colors } = useTheme();
  const { loai } = useLocalSearchParams<{ loai?: string }>();
  const vaiTro = parseVaiTro(loai);
  const hocVien = coKhoiHocVien(vaiTro);
  const pt = coKhoiPt(vaiTro);
  const { draft, update, reset } = useProfileDraft();
  const danhMuc = useDanhMuc(['muc_tieu_tap_luyen', 'trinh_do_tap', 'chuyen_mon_pt']);
  const [tab, setTab] = useState<Khoi>(hocVien ? 'hoc_vien' : 'pt');
  const [failure, setFailure] = useState<Failure | null>(null);
  const [uploadingChungChi, setUploadingChungChi] = useState(false);

  const { control, handleSubmit, setError, formState } = useForm<ProfileDetailForm>({
    resolver: zodResolver(createProfileDetailSchema(vaiTro)),
    defaultValues: {
      muc_tieu_tap_luyen: draft.muc_tieu_tap_luyen,
      trinh_do_tap: draft.trinh_do_tap,
      chieu_cao_cm: draft.chieu_cao_cm,
      can_nang_kg: draft.can_nang_kg,
      chuyen_mon: draft.chuyen_mon,
      so_nam_kinh_nghiem: draft.so_nam_kinh_nghiem,
      gioi_thieu: draft.gioi_thieu,
      chung_chi: draft.chung_chi,
    },
  });

  // Ghi bản nháp ngay khi gõ để quay lại màn cá nhân rồi trở lại vẫn còn dữ liệu.
  const values = useWatch({ control });
  useEffect(() => {
    update(values as Partial<ProfileDetailForm>);
  }, [values, update]);

  /** Chuyển sang tab có ô lỗi để người dùng thấy lỗi ngay. */
  const showTabOfError = (name: string) => {
    if (!(hocVien && pt)) return;
    if ((HOC_VIEN_FIELDS as readonly string[]).includes(name)) setTab('hoc_vien');
    else if ((PT_FIELDS as readonly string[]).includes(name)) setTab('pt');
  };

  const onInvalid = (errors: FieldErrors<ProfileDetailForm>) => {
    // Chỉ chuyển tab khi tab đang xem không có lỗi, để không giật người dùng đang sửa dở.
    const dangXem = tab === 'hoc_vien' ? HOC_VIEN_FIELDS : PT_FIELDS;
    if (coLoi(errors, dangXem)) return;
    const first = Object.keys(errors)[0];
    if (first) showTabOfError(first);
  };

  const submit = handleSubmit(async (form) => {
    setFailure(null);
    const input: HoSoLuuInput = {
      loai_nguoi_dung: vaiTro,
      ten_hien_thi: draft.ten_hien_thi.trim(),
      ngay_sinh: draft.ngay_sinh.trim() ? toIsoNgaySinh(draft.ngay_sinh) : undefined,
      gioi_tinh: draft.gioi_tinh || undefined,
      khu_vuc_hoat_dong: draft.khu_vuc_hoat_dong || undefined,
      ...(hocVien && {
        muc_tieu_tap_luyen: form.muc_tieu_tap_luyen,
        trinh_do_tap: form.trinh_do_tap,
        chieu_cao_cm: parseSo(form.chieu_cao_cm),
        can_nang_kg: parseSo(form.can_nang_kg),
      }),
      ...(pt && {
        chuyen_mon: form.chuyen_mon,
        so_nam_kinh_nghiem: parseSo(form.so_nam_kinh_nghiem),
        gioi_thieu: form.gioi_thieu.trim() || undefined,
        chung_chi_tep_tin_id: form.chung_chi?.file_id,
      }),
    };
    try {
      await profileApi.save(input);
    } catch (error) {
      const apiError = error instanceof ApiError ? error : null;
      let shown = false;
      for (const name of [...HOC_VIEN_FIELDS, ...PT_FIELDS]) {
        const message =
          apiError?.fieldErrors[name === 'chung_chi' ? 'chung_chi_tep_tin_id' : name]?.[0];
        if (message) {
          setError(name as FieldPath<ProfileDetailForm>, { message });
          showTabOfError(name);
          shown = true;
        }
      }
      const personal = PERSONAL_FIELDS.some((name) => apiError?.fieldErrors[name]);
      // Lỗi ở trường của màn cá nhân không có ô để hiện ở đây nên hiện thành thông báo kèm nút quay lại sửa.
      if (!shown || personal) {
        setFailure({
          message: apiError?.message ?? 'Có lỗi xảy ra. Vui lòng thử lại.',
          retryable: apiError ? CONNECTION_CODES.includes(apiError.code) : false,
          personal,
        });
      }
      return;
    }
    // Lưu xong: xóa bản nháp và thay màn thiết lập bằng màn kết quả (đã gửi hồ sơ PT hoặc chào mừng) để back không quay lại.
    reset();
    router.replace(`/profile-setup/hoan-tat?loai=${vaiTro}`);
  }, onInvalid);

  // Vào thẳng màn này (mở lại app, đường dẫn trực tiếp) khi chưa nhập thông tin cá nhân thì quay về bước đó.
  // Chỉ tên hiển thị là bắt buộc; ngày sinh, giới tính, khu vực là tùy chọn.
  if (draft.ten_hien_thi.trim() === '')
    return <Redirect href={`/profile-setup/ca-nhan?loai=${vaiTro}`} />;

  const loadError = danhMuc.isError ? LOAD_ERROR : undefined;
  const { errors } = formState;
  const khoiDangXem: Khoi = hocVien && pt ? tab : hocVien ? 'hoc_vien' : 'pt';

  return (
    <Screen bottomInset>
      <AuthHeader onBack={() => router.back()} />
      <View className="gap-3 py-4">
        <Text
          accessibilityRole="header"
          className="text-center text-[28px] font-bold leading-9 text-foreground"
        >
          Thiết lập hồ sơ
        </Text>
        <Text className="text-center text-base leading-6 text-muted-foreground">
          Hoàn thiện thông tin để cá nhân hóa trải nghiệm và sử dụng đầy đủ tính năng.
        </Text>
      </View>

      {hocVien && pt ? (
        <KhoiTabs
          active={tab}
          onChange={setTab}
          loi={{
            hoc_vien: coLoi(errors, HOC_VIEN_FIELDS),
            pt: coLoi(errors, PT_FIELDS),
          }}
        />
      ) : null}

      <Slide
        key={khoiDangXem}
        enabled={hocVien && pt}
        from={khoiDangXem === 'pt' ? 'right' : 'left'}
      >
        <View className="gap-5">
          <View className="flex-row items-center gap-3 border-b border-border pb-3">
            <Ionicons
              name={khoiDangXem === 'hoc_vien' ? 'person-outline' : 'ribbon-outline'}
              size={22}
              color={colors.primary}
            />
            <Text className="text-lg font-bold text-foreground">
              {khoiDangXem === 'hoc_vien' ? 'Thông tin học viên' : 'Thông tin PT'}
            </Text>
          </View>

          {khoiDangXem === 'hoc_vien' ? (
            <>
              <Controller
                control={control}
                name="muc_tieu_tap_luyen"
                render={({ field, fieldState }) => (
                  <SelectField
                    label="Mục tiêu tập luyện *"
                    placeholder="Chọn mục tiêu"
                    value={field.value || null}
                    options={danhMuc.items('muc_tieu_tap_luyen')}
                    onChange={field.onChange}
                    loading={danhMuc.isPending}
                    loadError={loadError}
                    onRetry={danhMuc.retry}
                    error={fieldState.error?.message}
                  />
                )}
              />
              <Controller
                control={control}
                name="trinh_do_tap"
                render={({ field, fieldState }) => (
                  <SelectField
                    label="Trình độ tập *"
                    placeholder="Chọn trình độ"
                    value={field.value || null}
                    options={danhMuc.items('trinh_do_tap')}
                    onChange={field.onChange}
                    loading={danhMuc.isPending}
                    loadError={loadError}
                    onRetry={danhMuc.retry}
                    error={fieldState.error?.message}
                  />
                )}
              />
              <View className="flex-row items-start gap-3">
                <View className="flex-1">
                  <Controller
                    control={control}
                    name="chieu_cao_cm"
                    render={({ field, fieldState }) => (
                      <TextField
                        label="Chiều cao *"
                        placeholder="175"
                        value={field.value}
                        onChangeText={field.onChange}
                        onBlur={field.onBlur}
                        inputRef={field.ref}
                        keyboardType="decimal-pad"
                        trailing={<Text className="text-base text-muted-foreground">cm</Text>}
                        error={fieldState.error?.message}
                      />
                    )}
                  />
                </View>
                <View className="flex-1">
                  <Controller
                    control={control}
                    name="can_nang_kg"
                    render={({ field, fieldState }) => (
                      <TextField
                        label="Cân nặng *"
                        placeholder="70"
                        value={field.value}
                        onChangeText={field.onChange}
                        onBlur={field.onBlur}
                        inputRef={field.ref}
                        keyboardType="decimal-pad"
                        trailing={<Text className="text-base text-muted-foreground">kg</Text>}
                        error={fieldState.error?.message}
                      />
                    )}
                  />
                </View>
              </View>
            </>
          ) : (
            <>
              <Controller
                control={control}
                name="chuyen_mon"
                render={({ field, fieldState }) => (
                  <MultiSelectChips
                    label="Chuyên môn *"
                    value={field.value}
                    options={danhMuc.items('chuyen_mon_pt')}
                    onChange={field.onChange}
                    loading={danhMuc.isPending}
                    loadError={loadError}
                    onRetry={danhMuc.retry}
                    error={fieldState.error?.message}
                  />
                )}
              />
              <Controller
                control={control}
                name="so_nam_kinh_nghiem"
                render={({ field, fieldState }) => (
                  <TextField
                    label="Số năm kinh nghiệm *"
                    placeholder="3"
                    value={field.value}
                    onChangeText={field.onChange}
                    onBlur={field.onBlur}
                    inputRef={field.ref}
                    keyboardType="number-pad"
                    trailing={<Text className="text-base text-muted-foreground">năm</Text>}
                    error={fieldState.error?.message}
                  />
                )}
              />
              <Controller
                control={control}
                name="chung_chi"
                render={({ field, fieldState }) => (
                  <ChungChiField
                    value={field.value}
                    onChange={field.onChange}
                    onUploadingChange={setUploadingChungChi}
                    error={fieldState.error?.message}
                  />
                )}
              />
              <Controller
                control={control}
                name="gioi_thieu"
                render={({ field, fieldState }) => (
                  <TextField
                    label="Giới thiệu bản thân"
                    placeholder="Vài dòng về kinh nghiệm và phong cách huấn luyện của bạn"
                    value={field.value}
                    onChangeText={field.onChange}
                    onBlur={field.onBlur}
                    inputRef={field.ref}
                    multiline
                    maxLength={GIOI_THIEU_TOI_DA}
                    textAlignVertical="top"
                    error={fieldState.error?.message}
                  />
                )}
              />
            </>
          )}
        </View>
      </Slide>

      {failure ? (
        <View className="gap-2">
          <Text accessibilityRole="alert" className="text-destructive">
            {failure.message}
          </Text>
          {failure.personal ? (
            <Button label="Sửa thông tin cá nhân" secondary onPress={() => router.back()} />
          ) : null}
          {failure.retryable ? (
            <Button label="Thử lại" secondary onPress={() => void submit()} />
          ) : null}
        </View>
      ) : null}

      <Button
        pill
        label="Lưu thiết lập"
        loading={formState.isSubmitting}
        disabled={uploadingChungChi}
        onPress={() => void submit()}
      />
    </Screen>
  );
}
