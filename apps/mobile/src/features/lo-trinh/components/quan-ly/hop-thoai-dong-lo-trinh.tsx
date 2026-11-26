import { useState } from 'react';
import { KeyboardAvoidingView, Modal, Platform, Pressable, View } from 'react-native';
import { Button } from '@/components/ui/button';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { TextField } from '@/shared/components/ui';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import type { DongLoTrinh } from '../../data/dong-lo-trinh-store';

/** Độ dài tối đa của lý do đóng (cùng giới hạn N6 với các ghi chú khác). */
const TOI_DA_KY_TU_LY_DO = 300;

const KET_QUA: readonly { gia: DongLoTrinh['trangThai']; nhan: string; moTa: string }[] = [
  { gia: 'hoan_thanh', nhan: 'Hoàn thành', moTa: 'Học viên đã đạt hoặc gần đạt mục tiêu.' },
  {
    gia: 'that_bai',
    nhan: 'Thất bại',
    moTa: 'Dừng lộ trình vì không thể tiếp tục hoặc không đạt.',
  },
];

/** Menu `...` của chi tiết lộ trình: hiện "Đóng lộ trình" khi lộ trình còn đang chạy. */
export function MenuLoTrinh({
  hien,
  onDong,
  onDongLoTrinh,
}: {
  hien: boolean;
  onDong(): void;
  onDongLoTrinh(): void;
}) {
  const { colors } = useTheme();
  return (
    <Modal visible={hien} transparent animationType="slide" onRequestClose={onDong}>
      <Pressable
        accessibilityLabel="Đóng menu"
        onPress={onDong}
        className="flex-1 justify-end bg-black/40"
      >
        <Pressable className="gap-1 rounded-t-3xl bg-background px-5 pb-8 pt-5">
          <Text className="pb-2 text-lg font-bold">Tùy chọn lộ trình</Text>
          <Pressable
            accessibilityRole="button"
            accessibilityLabel="Đóng lộ trình"
            onPress={onDongLoTrinh}
            className="flex-row items-center gap-3 rounded-xl py-3"
          >
            <Icon name="lock" size={22} color={colors.error} />
            <View className="flex-1">
              <Text className="text-base font-medium text-destructive">Đóng lộ trình</Text>
              <Text variant="muted" className="text-sm">
                Kết thúc trước hạn, học viên chỉ còn xem được.
              </Text>
            </View>
          </Pressable>
        </Pressable>
      </Pressable>
    </Modal>
  );
}

/**
 * Hộp xác nhận đóng lộ trình trước hạn (N28): PT chọn kết quả (hoàn thành hoặc thất bại, đề xuất cho M19) và bắt buộc nhập lý
 * do. Đóng rồi thì lộ trình chỉ xem và học viên được thông báo.
 */
export function HopThoaiDongLoTrinh({
  hien,
  tenLoTrinh,
  soNguoi,
  onHuy,
  onXacNhan,
}: {
  hien: boolean;
  tenLoTrinh: string;
  soNguoi: number;
  onHuy(): void;
  onXacNhan(dong: DongLoTrinh): void;
}) {
  const [ketQua, setKetQua] = useState<DongLoTrinh['trangThai']>('hoan_thanh');
  const [lyDo, setLyDo] = useState('');
  const [thieuLyDo, setThieuLyDo] = useState(false);

  const xacNhan = () => {
    const sach = lyDo.trim();
    if (!sach) {
      setThieuLyDo(true);
      return;
    }
    onXacNhan({ trangThai: ketQua, lyDo: sach });
    setLyDo('');
    setThieuLyDo(false);
    setKetQua('hoan_thanh');
  };

  return (
    <Modal visible={hien} transparent animationType="fade" onRequestClose={onHuy}>
      <KeyboardAvoidingView
        behavior={Platform.OS === 'ios' ? 'padding' : undefined}
        className="flex-1 items-center justify-center bg-black/40 px-5"
      >
        <View
          accessibilityViewIsModal
          className="w-full max-w-[420px] gap-4 rounded-3xl bg-background p-5"
        >
          <View className="gap-1">
            <Text accessibilityRole="header" className="text-xl font-bold">
              Đóng lộ trình?
            </Text>
            <Text variant="muted" className="text-sm leading-5">
              {tenLoTrinh}. Sau khi đóng, lộ trình chỉ để xem;{' '}
              {soNguoi > 1 ? `cả ${soNguoi} học viên` : 'học viên'} sẽ nhận thông báo.
            </Text>
          </View>

          <View accessibilityRole="radiogroup" className="gap-2">
            {KET_QUA.map((k) => {
              const dangChon = ketQua === k.gia;
              return (
                <Pressable
                  key={k.gia}
                  accessibilityRole="radio"
                  accessibilityState={{ checked: dangChon }}
                  accessibilityLabel={k.nhan}
                  onPress={() => setKetQua(k.gia)}
                  className={cn(
                    'rounded-xl border px-3 py-2.5',
                    dangChon ? 'border-primary bg-secondary' : 'border-border bg-card',
                  )}
                >
                  <Text className="font-bold">{k.nhan}</Text>
                  <Text variant="muted" className="text-sm">
                    {k.moTa}
                  </Text>
                </Pressable>
              );
            })}
          </View>

          <View className="gap-1">
            <View>
              <TextField
                hideLabel
                multiline
                label="Lý do đóng lộ trình"
                placeholder="Nhập lý do đóng lộ trình (bắt buộc)"
                value={lyDo}
                onChangeText={(chu) => {
                  setLyDo(chu);
                  if (chu.trim()) setThieuLyDo(false);
                }}
                maxLength={TOI_DA_KY_TU_LY_DO}
                textAlignVertical="top"
                containerClassName="min-h-[96px] pb-8"
              />
              <Text variant="muted" className="absolute bottom-2 right-3 text-xs">
                {lyDo.length}/{TOI_DA_KY_TU_LY_DO}
              </Text>
            </View>
            {thieuLyDo ? (
              <Text accessibilityRole="alert" variant="destructive" className="text-sm">
                Nhập lý do đóng lộ trình
              </Text>
            ) : null}
          </View>

          <View className="flex-row gap-3">
            <Button
              variant="outline"
              shape="pill"
              accessibilityLabel="Hủy"
              onPress={onHuy}
              className="flex-1"
            >
              <Text>Hủy</Text>
            </Button>
            <Button
              variant="destructive"
              shape="pill"
              accessibilityLabel="Xác nhận đóng lộ trình"
              onPress={xacNhan}
              className="flex-1"
            >
              <Text>Đóng lộ trình</Text>
            </Button>
          </View>
        </View>
      </KeyboardAvoidingView>
    </Modal>
  );
}
