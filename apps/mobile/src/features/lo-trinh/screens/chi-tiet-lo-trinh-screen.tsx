import { Pressable, ScrollView, View } from 'react-native';
import { useRouter } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Text } from '@/components/ui/text';
import { Icon } from '@/shared/icons';
import { Rise } from '@/shared/motion';
import { useTheme } from '@/shared/theme/theme';
import {
  KhoiAnhMoc,
  KhoiCanNang,
  KhoiChiSo,
  KhoiDanhGiaPt,
  KhoiMucTieu,
} from '../components/chi-tiet-tien-do';
import { useChiTietLoTrinh } from '../hooks/use-lo-trinh';

/**
 * Chi tiết lộ trình của học viên: tiến độ mục tiêu, biểu đồ cân nặng, chỉ số tuân thủ, ảnh cơ thể theo mốc và đánh giá
 * của PT. Mở từ thẻ ở HV-01.
 */
// TODO(ROUTE): thiết kế HV-02 có thêm ảnh bìa, bốn lối tắt và bốn tab (Hôm nay, Kế hoạch, Tiến độ, Ảnh); màn này mới
//   làm phần tổng quan tiến độ theo ảnh mẫu (HV-07, tab Tổng quan).
export function ChiTietLoTrinhScreen({ id }: { id?: string }) {
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const { colors } = useTheme();
  const chiTiet = useChiTietLoTrinh(id);

  return (
    <View className="flex-1 bg-background" style={{ paddingTop: insets.top }}>
      <View className="flex-row items-center gap-3 px-4 py-3">
        <Pressable
          accessibilityRole="button"
          accessibilityLabel="Quay lại"
          hitSlop={8}
          onPress={() => router.back()}
        >
          <Icon name="back" size={28} color={colors.text} />
        </Pressable>
        <Text accessibilityRole="header" className="flex-1 text-2xl font-bold">
          Chi tiết lộ trình
        </Text>
      </View>

      {chiTiet ? (
        <ScrollView
          className="flex-1"
          contentContainerClassName="w-full max-w-[680px] gap-4 self-center px-4"
          contentContainerStyle={{ paddingBottom: 24 + insets.bottom }}
        >
          {/* Các khối trồi lên lần lượt (cách nhau 90ms); số đếm và biểu đồ chạy bên trong từng khối. */}
          <Rise>
            <KhoiMucTieu
              loTrinh={chiTiet.loTrinh}
              // Điểm đo cuối cùng là cân nặng hiện tại; chưa có điểm nào thì coi như chưa đổi so với lúc bắt đầu.
              canNangHienTai={chiTiet.tienDo.canNang.at(-1)?.kg ?? chiTiet.loTrinh.mucTieu.tu}
              homNay={chiTiet.homNay}
            />
          </Rise>
          <Rise delay={90}>
            <KhoiCanNang
              diem={chiTiet.tienDo.canNang}
              donVi={chiTiet.loTrinh.mucTieu.donVi}
              onMoChiTiet={() => router.push({ pathname: '/chi-tiet-can-nang', params: { id } })}
            />
          </Rise>
          <Rise delay={180}>
            <KhoiChiSo
              buoiTap={chiTiet.tienDo.buoiTap}
              buaAn={chiTiet.tienDo.buaAn}
              tuanThu={chiTiet.loTrinh.tuanThu}
            />
          </Rise>
          <Rise delay={270}>
            <KhoiAnhMoc
              anh={chiTiet.tienDo.anhMoc}
              onXemTatCa={() => router.push({ pathname: '/anh-theo-moc', params: { id } })}
            />
          </Rise>
          {chiTiet.tienDo.danhGia ? (
            <Rise delay={360}>
              <KhoiDanhGiaPt danhGia={chiTiet.tienDo.danhGia} />
            </Rise>
          ) : null}
        </ScrollView>
      ) : (
        <View className="items-center p-8">
          <Text variant="muted">Không tìm thấy lộ trình.</Text>
        </View>
      )}
    </View>
  );
}
