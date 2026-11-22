import { useState } from 'react';
import { KeyboardAvoidingView, Platform, Pressable, ScrollView, View } from 'react-native';
import { useRouter } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Button } from '@/components/ui/button';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { TextField } from '@/shared/components/ui';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import { DauManCheckIn, GiaiThichPt } from '../components/check-in-bua-an';
import { DaGhi, TheNhiemVuGhi } from '../components/ghi-chi-so';
import { KhungThe } from '../components/khung-the';
import { SoSanhAnh } from '../components/so-sanh-anh';
import { luuAnhCoThe } from '../data/check-in-store';
import { useChonMotAnh } from '../hooks/use-chon-mot-anh';
import { useNhiemVuAnhCoThe } from '../hooks/use-lo-trinh';
import type { QuyenXemAnh } from '../types';
import { gioHienTai } from '../utils/ngay';

/** Độ dài tối đa của ghi chú (cùng giới hạn N6 với các ghi chú khác). */
const TOI_DA_KY_TU_GHI_CHU = 300;

/** Hai chế độ xem ảnh (N33). Mặc định "Chỉ mình tôi" vì ảnh cơ thể là dữ liệu riêng tư. */
const QUYEN_XEM: readonly {
  gia: QuyenXemAnh;
  nhan: string;
  icon: string;
  moTa: string;
}[] = [
  {
    gia: 'rieng_tu',
    nhan: 'Chỉ mình tôi',
    icon: 'lock',
    moTa: 'Chỉ mình bạn xem được ảnh này. Bạn có thể cho PT xem bất cứ lúc nào.',
  },
  {
    gia: 'cho_pt',
    nhan: 'Cho PT xem',
    icon: 'person-check',
    moTa: 'PT xem được ảnh này trong tiến độ của bạn để so sánh các mốc.',
  },
];

/**
 * Ghi ảnh cơ thể (HV-08): ảnh mốc trước đặt cạnh ảnh mới để chụp cùng góc; chụp hoặc chọn một ảnh, chọn ai xem được (N33),
 * rồi lưu. Ghi trực tiếp, không gửi PT duyệt (N16). Đã ghi rồi thì mở lại với ảnh đã lưu để thay.
 */
// TODO(ROUTE): HV-08 chưa có bản thiết kế (luong-man-hinh.md, HV-08); màn này dựng theo khuôn các màn check-in.
export function GhiAnhCoTheScreen({
  loTrinhId,
  nhiemVuId,
}: {
  loTrinhId?: string;
  nhiemVuId?: string;
}) {
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const { colors } = useTheme();
  const du = useNhiemVuAnhCoThe(loTrinhId, nhiemVuId);
  const { anh, loi: loiAnh, chup, thuVien, xoa } = useChonMotAnh(du?.ghi?.anh, 'ảnh cơ thể');
  const [quyenXem, setQuyenXem] = useState<QuyenXemAnh>(du?.ghi?.quyenXem ?? 'rieng_tu');
  const [ghiChu, setGhiChu] = useState(du?.ghi?.ghiChu ?? '');
  const [thieuAnh, setThieuAnh] = useState(false);

  const luu = () => {
    if (!du) return;
    if (!anh) {
      setThieuAnh(true);
      return;
    }
    // TODO(ROUTE): upload ảnh (shared/file) vào chi_so_co_the kèm cờ hiển thị (K7, N33) rồi invalidate tiến độ; chưa có API.
    luuAnhCoThe(
      du.nhiemVu.id,
      { anh, ghiChu: ghiChu.trim(), quyenXem, gioGhi: gioHienTai() },
      'Đã lưu ảnh cơ thể',
    );
    router.back();
  };

  const lua = QUYEN_XEM.find((q) => q.gia === quyenXem);

  return (
    <KeyboardAvoidingView
      className="flex-1 bg-background"
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      style={{ paddingTop: insets.top }}
    >
      {du ? (
        <>
          <DauManCheckIn
            tieuDe="Ghi ảnh cơ thể"
            phuDe={`Lộ trình: ${du.loTrinh.ten}`}
            onQuayLai={() => router.back()}
          />
          <ScrollView
            className="flex-1"
            contentContainerClassName="w-full max-w-[680px] gap-4 self-center px-4"
            contentContainerStyle={{ paddingBottom: 24 + insets.bottom }}
          >
            <TheNhiemVuGhi icon="camera" tieuDe={du.nhiemVu.tieuDe} moTa={du.nhiemVu.moTa} />

            {du.ghi ? (
              <DaGhi
                gioGhi={du.ghi.gioGhi}
                noiDung="Bạn có thể thay ảnh hoặc đổi chế độ xem bên dưới."
              />
            ) : null}

            <KhungThe icon="camera" tieuDe="Ảnh cơ thể" chuPhu={`Đã thêm ${anh ? 1 : 0}/1 ảnh`}>
              <SoSanhAnh
                anhMocTruoc={du.anhMocTruoc}
                tuanMoi={du.loTrinh.tuanHienTai}
                anh={anh}
                nhanAnh="Ảnh cơ thể đã thêm"
                onChup={chup}
                onThuVien={thuVien}
                onXoa={xoa}
              />
              <Text variant="muted" className="text-sm leading-5">
                Chụp cùng góc, cùng ánh sáng và cùng khoảng cách với ảnh mốc trước để so sánh tiến
                độ dễ hơn.
              </Text>
              {loiAnh ? (
                <Text accessibilityRole="alert" variant="destructive" className="text-sm">
                  {loiAnh}
                </Text>
              ) : null}
              {thieuAnh && !anh ? (
                <Text accessibilityRole="alert" variant="destructive" className="text-sm">
                  Chọn một ảnh cơ thể
                </Text>
              ) : null}
            </KhungThe>

            <KhungThe icon="edit" tieuDe="Ghi chú" tieuDePhu="(không bắt buộc)">
              <View>
                <TextField
                  hideLabel
                  multiline
                  label="Ghi chú ảnh cơ thể"
                  placeholder="Ví dụ: Chụp buổi sáng, sau khi ngủ dậy."
                  value={ghiChu}
                  onChangeText={setGhiChu}
                  maxLength={TOI_DA_KY_TU_GHI_CHU}
                  textAlignVertical="top"
                  containerClassName="min-h-[110px] pb-8"
                />
                <Text variant="muted" className="absolute bottom-2 right-3 text-xs">
                  {ghiChu.length}/{TOI_DA_KY_TU_GHI_CHU}
                </Text>
              </View>
            </KhungThe>

            <KhungThe icon="lock" tieuDe="Ai xem được ảnh">
              <View accessibilityRole="radiogroup" className="flex-row gap-2">
                {QUYEN_XEM.map((q) => {
                  const dangChon = quyenXem === q.gia;
                  return (
                    <Pressable
                      key={q.gia}
                      accessibilityRole="radio"
                      accessibilityState={{ checked: dangChon }}
                      accessibilityLabel={q.nhan}
                      onPress={() => setQuyenXem(q.gia)}
                      className={cn(
                        'flex-1 flex-row items-center justify-center gap-1.5 rounded-full border px-2 py-3',
                        dangChon ? 'border-transparent bg-primary' : 'border-border bg-card',
                      )}
                    >
                      <Icon
                        name={q.icon}
                        size={18}
                        color={dangChon ? colors.onPrimary : colors.muted}
                      />
                      <Text
                        className={cn(
                          'text-sm font-medium',
                          dangChon ? 'text-primary-foreground' : 'text-muted-foreground',
                        )}
                        numberOfLines={1}
                      >
                        {q.nhan}
                      </Text>
                    </Pressable>
                  );
                })}
              </View>
              {lua ? (
                <Text variant="muted" className="text-sm leading-5">
                  {lua.moTa}
                </Text>
              ) : null}
            </KhungThe>

            <Button
              shape="pill"
              accessibilityLabel={du.ghi ? 'Cập nhật ảnh cơ thể' : 'Lưu ảnh cơ thể'}
              onPress={luu}
              className="min-h-[56px]"
            >
              <Text>{du.ghi ? 'Cập nhật ảnh cơ thể' : 'Lưu ảnh cơ thể'}</Text>
            </Button>

            <GiaiThichPt
              tieuDe="PT xem trong tiến độ"
              noiDung="Ảnh được ghi thẳng vào tiến độ của bạn, không cần PT duyệt. Chế độ xem do bạn chọn ở trên."
            />
          </ScrollView>
        </>
      ) : (
        <>
          <DauManCheckIn
            tieuDe="Ghi ảnh cơ thể"
            phuDe="Không tìm thấy nhiệm vụ"
            onQuayLai={() => router.back()}
          />
          <View className="items-center p-8">
            <Text variant="muted">Không tìm thấy nhiệm vụ ảnh cơ thể.</Text>
          </View>
        </>
      )}
    </KeyboardAvoidingView>
  );
}
