import { Modal, View } from 'react-native';
import { Button } from '@/components/ui/button';
import { Text } from '@/components/ui/text';
import type { TraLoiLoiMoi } from '../data/loi-moi-store';

const NOI_DUNG: Record<TraLoiLoiMoi, { tieuDe: string; moTa: string; nut: string }> = {
  tham_gia: {
    tieuDe: 'Tham gia lộ trình?',
    moTa: 'Lộ trình sẽ vào "Lộ trình của tôi" và PT thấy bạn đã tham gia.',
    nut: 'Tham gia',
  },
  tu_choi: {
    tieuDe: 'Từ chối lời mời?',
    moTa: 'Lời mời sẽ biến mất. Muốn tham gia lại thì PT phải mời lần nữa.',
    nut: 'Từ chối',
  },
};

/** Hộp xác nhận trước khi tham gia hoặc từ chối lời mời; `traLoi` rỗng thì đóng. */
export function HopThoaiLoiMoi({
  traLoi,
  tenLoTrinh,
  onHuy,
  onXacNhan,
}: {
  traLoi: TraLoiLoiMoi | null;
  tenLoTrinh: string;
  onHuy(): void;
  onXacNhan(traLoi: TraLoiLoiMoi): void;
}) {
  if (!traLoi) return null;
  const noiDung = NOI_DUNG[traLoi];
  return (
    <Modal visible transparent animationType="fade" onRequestClose={onHuy}>
      <View className="flex-1 items-center justify-center bg-black/40 px-5">
        <View
          accessibilityViewIsModal
          className="w-full max-w-[420px] gap-4 rounded-3xl bg-background p-5"
        >
          <View className="gap-1">
            <Text accessibilityRole="header" className="text-xl font-bold">
              {noiDung.tieuDe}
            </Text>
            <Text variant="muted" className="text-sm leading-5">
              {tenLoTrinh}. {noiDung.moTa}
            </Text>
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
              variant={traLoi === 'tu_choi' ? 'destructive' : 'default'}
              shape="pill"
              accessibilityLabel={`Xác nhận ${noiDung.nut.toLowerCase()}`}
              onPress={() => onXacNhan(traLoi)}
              className="flex-1"
            >
              <Text>{noiDung.nut}</Text>
            </Button>
          </View>
        </View>
      </View>
    </Modal>
  );
}
