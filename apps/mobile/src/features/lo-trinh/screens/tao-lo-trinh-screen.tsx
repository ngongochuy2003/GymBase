import { useMemo, useState } from 'react';
import {
  KeyboardAvoidingView,
  Platform,
  Pressable,
  ScrollView,
  TextInput,
  View,
} from 'react-native';
import { Button } from '@/components/ui/button';
import { Text } from '@/components/ui/text';
import { useRouter } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { SelectField, type SelectOption } from '@/shared/components/select-field';
import { TextField } from '@/shared/components/ui';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import { ChonHocVien } from '../components/form/chon-hoc-vien';
import { KeHoachDinhDuong } from '../components/form/ke-hoach-dinh-duong';
import { KeHoachTapLuyen } from '../components/form/ke-hoach-tap-luyen';
import { HangForm, KhoiForm } from '../components/form/thanh-phan-form';
import { TheoDoiDinhKy, type MucTheoDoi } from '../components/form/theo-doi-dinh-ky';
import { taoBanBeMau } from '../data/seed';
import type { BuaAnKeHoach, LichTapTheoTuan, NguoiLoTrinh } from '../types';

const SO_KY_TU_GHI_CHU = 300;

const MUC_TIEU: readonly SelectOption[] = [
  { ma: 'tang_can', ten: 'Tăng cân' },
  { ma: 'giam_can', ten: 'Giảm cân' },
  { ma: 'tang_co', ten: 'Tăng cơ' },
  { ma: 'giam_mo', ten: 'Giảm mỡ' },
  { ma: 'duy_tri', ten: 'Duy trì thể trạng' },
  { ma: 'suc_ben', ten: 'Tăng sức bền' },
];

/** `id` là số tuần. */
const THOI_GIAN: readonly SelectOption[] = [
  { ma: '4', ten: '1 tháng (4 tuần)' },
  { ma: '8', ten: '2 tháng (8 tuần)' },
  { ma: '12', ten: '3 tháng (12 tuần)' },
  { ma: '16', ten: '4 tháng (16 tuần)' },
  { ma: '24', ten: '6 tháng (24 tuần)' },
];

/** Giá trị điền sẵn cho form, để xem đủ giao diện. */
// TODO(ROUTE): form bắt đầu trống hoặc nạp bản nháp của PT (adr.md, N22); bỏ giá trị điền sẵn này.
const BUA_AN_MAU: BuaAnKeHoach[] = [
  { id: 'sang', ten: 'Bữa sáng', gio: '07:00', bat: true },
  { id: 'trua', ten: 'Bữa trưa', gio: '12:00', bat: true },
  { id: 'toi', ten: 'Bữa tối', gio: '19:00', bat: true },
];
const LICH_TAP_MAU: LichTapTheoTuan = {
  chung: [
    { thu: 1, ten: 'Upper A', gio: '17:30' },
    { thu: 3, ten: 'Lower A', gio: '17:30' },
    { thu: 5, ten: 'Upper B', gio: '17:30' },
  ],
  rieng: {},
};
const GHI_CHU_MAU =
  'Tập trung tăng khối lượng cơ và kiểm soát mỡ. Duy trì ăn đủ 3 bữa, đúng giờ check-in và hoàn thành buổi tập hằng ngày. Ngủ 7–8 giờ/đêm và uống đủ nước (2,5–3 lít/ngày).';

interface Loi {
  ten?: string;
  mucTieu?: string;
  thoiGian?: string;
  hocVien?: string;
}

/** PT-02 Tạo lộ trình. Hiện chỉ giữ dữ liệu trong state, chưa gọi API. */
export function TaoLoTrinhScreen() {
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const { colors } = useTheme();
  const banBe = useMemo(() => taoBanBeMau(), []);

  const [ten, setTen] = useState('Tăng 10 kg trong 3 tháng');
  const [mucTieu, setMucTieu] = useState<string | null>('tang_can');
  const [thoiGian, setThoiGian] = useState<string | null>('12');
  const [hocVien, setHocVien] = useState<NguoiLoTrinh[]>(() => banBe.slice(0, 1));
  const [buaAn, setBuaAn] = useState<BuaAnKeHoach[]>(BUA_AN_MAU);
  const [lichTap, setLichTap] = useState<LichTapTheoTuan>(LICH_TAP_MAU);
  const [tuanDangSua, setTuanDangSua] = useState(1);
  const [canNang, setCanNang] = useState<MucTheoDoi>({ bat: true, tanSuat: '2' });
  const [anhCoThe, setAnhCoThe] = useState<MucTheoDoi>({ bat: true, tanSuat: '2' });
  const [ghiChu, setGhiChu] = useState(GHI_CHU_MAU);
  const [loi, setLoi] = useState<Loi>({});

  const soTuan = Number(thoiGian ?? 12);
  const tuanHienThi = Math.min(tuanDangSua, soTuan);

  const kiemTra = (): Loi => ({
    ten: ten.trim() ? undefined : 'Nhập tên lộ trình',
    mucTieu: mucTieu ? undefined : 'Chọn mục tiêu',
    thoiGian: thoiGian ? undefined : 'Chọn thời gian',
    hocVien: hocVien.length > 0 ? undefined : 'Chọn ít nhất một học viên',
  });

  const taoLoTrinh = () => {
    const ketQua = kiemTra();
    setLoi(ketQua);
    if (Object.values(ketQua).some(Boolean)) return;
    // TODO(ROUTE): gọi API tạo lộ trình, rồi vào PT-03 của lộ trình vừa tạo (adr.md, N23 lời mời, N24 đơn hoặc nhóm).
    router.back();
  };

  return (
    <KeyboardAvoidingView
      className="flex-1 bg-background"
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      style={{ paddingTop: insets.top }}
    >
      <View className="flex-row items-center gap-3 px-4 py-3">
        <Pressable
          accessibilityRole="button"
          accessibilityLabel="Quay lại"
          hitSlop={8}
          onPress={() => router.back()}
        >
          <Icon name="back" size={28} color={colors.text} />
        </Pressable>
        <Text accessibilityRole="header" className="flex-1 text-2xl font-bold text-foreground">
          Tạo lộ trình
        </Text>
        <Pressable
          accessibilityRole="button"
          accessibilityLabel="Lưu nháp"
          // TODO(ROUTE): lưu nháp và ở lại màn này (adr.md, N22); chưa quyết nơi lưu nháp (K8).
          onPress={() => undefined}
          className="flex-row items-center gap-1.5 rounded-full border border-border bg-card px-4 py-2"
        >
          <Icon name="bookmark" size={18} color={colors.text} />
          <Text className="text-sm font-medium text-foreground">Lưu nháp</Text>
        </Pressable>
      </View>

      <ScrollView
        className="flex-1"
        contentContainerClassName="w-full max-w-[680px] gap-4 self-center px-4 pb-6"
        keyboardShouldPersistTaps="handled"
      >
        <KhoiForm
          icon="clipboard"
          tieuDe="Thông tin cơ bản"
          moTa="Thiết lập mục tiêu và đối tượng cho lộ trình."
        >
          <HangForm nhan="Tên lộ trình" batBuoc>
            <TextField
              hideLabel
              label="Tên lộ trình"
              value={ten}
              onChangeText={setTen}
              maxLength={100}
              placeholder="Ví dụ: Tăng 10 kg trong 3 tháng"
              error={loi.ten}
              containerClassName="h-11 px-3"
            />
          </HangForm>
          <HangForm nhan="Mục tiêu" batBuoc>
            <SelectField
              hideLabel
              singleLine
              label="Mục tiêu"
              icon="target"
              value={mucTieu}
              options={MUC_TIEU}
              onChange={setMucTieu}
              placeholder="Chọn mục tiêu"
              error={loi.mucTieu}
              triggerClassName="min-h-0 h-11"
            />
          </HangForm>
          <HangForm nhan="Thời gian" batBuoc>
            <SelectField
              hideLabel
              singleLine
              label="Thời gian"
              icon="calendar"
              value={thoiGian}
              options={THOI_GIAN}
              onChange={setThoiGian}
              placeholder="Chọn thời gian"
              error={loi.thoiGian}
              triggerClassName="min-h-0 h-11"
            />
          </HangForm>
          <HangForm nhan="Học viên tham gia" batBuoc loi={loi.hocVien} canTren>
            <ChonHocVien
              banBe={banBe}
              daChon={hocVien}
              onDoi={setHocVien}
              coLoi={Boolean(loi.hocVien)}
            />
          </HangForm>
        </KhoiForm>

        <KeHoachDinhDuong buaAn={buaAn} onDoi={setBuaAn} />

        <KeHoachTapLuyen
          lich={lichTap}
          soTuan={soTuan}
          tuanDangSua={tuanHienThi}
          onDoiTuan={setTuanDangSua}
          onDoiLich={setLichTap}
        />

        <TheoDoiDinhKy
          canNang={canNang}
          anhCoThe={anhCoThe}
          onDoiCanNang={setCanNang}
          onDoiAnhCoThe={setAnhCoThe}
        />

        <KhoiForm
          icon="note"
          tieuDe="Ghi chú cho học viên"
          moTa="Hướng dẫn, lưu ý hoặc yêu cầu đặc biệt cho học viên."
        >
          <View className="rounded-xl border border-border bg-background px-3 pb-2 pt-2">
            <TextInput
              accessibilityLabel="Ghi chú cho học viên"
              value={ghiChu}
              onChangeText={setGhiChu}
              maxLength={SO_KY_TU_GHI_CHU}
              multiline
              textAlignVertical="top"
              placeholder="Nhập ghi chú cho học viên"
              placeholderTextColor={colors.muted}
              className="min-h-[96px] text-base text-foreground"
            />
            <Text className="self-end text-xs text-muted-foreground">
              {ghiChu.length}/{SO_KY_TU_GHI_CHU}
            </Text>
          </View>
        </KhoiForm>
      </ScrollView>

      <View
        className="border-t border-border bg-background px-4 pt-3"
        style={{ paddingBottom: 12 + insets.bottom }}
      >
        <Button
          shape="pill"
          accessibilityLabel="Tạo lộ trình"
          onPress={taoLoTrinh}
          className="min-h-0 border-0 px-0 py-3.5"
        >
          <Text>Tạo lộ trình</Text>
        </Button>
      </View>
    </KeyboardAvoidingView>
  );
}
