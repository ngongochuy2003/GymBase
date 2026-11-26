import { useState } from 'react';
import { KeyboardAvoidingView, Platform, ScrollView, View } from 'react-native';
import { useRouter } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Button } from '@/components/ui/button';
import { Text } from '@/components/ui/text';
import { TextField } from '@/shared/components/ui';
import { DauManCheckIn } from '../components/check-in-bua-an';
import { ChonMotAnh } from '../components/chon-mot-anh';
import { ChonGio } from '../components/form/chon-gio';
import { KhungThe } from '../components/khung-the';
import { datBuaAn, layBanNhap, useBanNhapLoTrinh, xoaBua } from '../data/ban-nhap-lo-trinh-store';
import { layMotAnh } from '../hooks/use-chon-mot-anh';
import type { BuaAnKeHoach } from '../types';
import { tenBua } from '../utils/bua-an';

/** Độ dài tối đa của mô tả món (cùng giới hạn N6 với các ghi chú khác). */
const TOI_DA_KY_TU_MO_TA = 300;
const TOI_DA_CHU_SO_KCAL = 4;
const TOI_DA_KY_TU_TEN = 40;

/**
 * PT-02a Chi tiết bữa ăn (màn con của PT-02): PT đặt giờ ăn, món ăn kèm ghi chú món ưu tiên, năng lượng nhập tay (N30) và
 * ảnh minh họa món. Sửa tới đâu ghi vào bản nháp của form tới đó nên quay lại PT-02 là đủ, không cần nút lưu. Bữa lặp
 * mỗi ngày: đây là kế hoạch bữa mà học viên thấy ở HV-04.
 */
// TODO(ROUTE): PT-02a chưa có bản thiết kế (luong-man-hinh.md, PT-02a); màn này tự thiết kế theo khuôn các màn check-in.
//   Chưa có thực đơn khác nhau theo ngày; ảnh món chưa upload (shared/file), chỉ giữ đường dẫn cục bộ.
export function DatBuaAnScreen({ buaId }: { buaId?: string }) {
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const { buaAn } = useBanNhapLoTrinh();
  const bua = buaAn.find((b) => b.id === buaId);
  const [loiAnh, setLoiAnh] = useState<string>();

  // Đọc bản nháp ngay lúc sửa (không dùng `buaAn` của lần vẽ này) vì chọn ảnh xong mới ghi, lúc đó state có thể đã cũ.
  const sua = (phan: Partial<BuaAnKeHoach>) =>
    datBuaAn(layBanNhap().buaAn.map((b) => (b.id === buaId ? { ...b, ...phan } : b)));
  const layAnh = async (nguon: 'camera' | 'thu_vien') => {
    const ketQua = await layMotAnh(nguon, 'ảnh món ăn');
    setLoiAnh(ketQua.loi);
    if (ketQua.uri) sua({ anh: ketQua.uri });
  };

  return (
    <KeyboardAvoidingView
      className="flex-1 bg-background"
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      style={{ paddingTop: insets.top }}
    >
      <DauManCheckIn
        tieuDe="Chi tiết bữa ăn"
        phuDe={bua ? tenBua(bua) : 'Không tìm thấy bữa ăn'}
        onQuayLai={() => router.back()}
      />
      {bua ? (
        <>
          <ScrollView
            className="flex-1"
            keyboardShouldPersistTaps="handled"
            contentContainerClassName="w-full max-w-[680px] gap-4 self-center px-4"
            contentContainerStyle={{ paddingBottom: 24 }}
          >
            <KhungThe icon="edit" tieuDe="Tên bữa">
              <TextField
                hideLabel
                label="Tên bữa"
                placeholder="Ví dụ: Bữa phụ chiều"
                value={bua.ten}
                onChangeText={(ten) => sua({ ten })}
                maxLength={TOI_DA_KY_TU_TEN}
                containerClassName="h-11 px-3"
              />
            </KhungThe>

            <KhungThe icon="clock" tieuDe="Thời gian ăn">
              <View className="flex-row items-center justify-between gap-3">
                <Text variant="muted" className="flex-1 text-sm">
                  Học viên nhận nhắc check-in vào giờ này mỗi ngày.
                </Text>
                <ChonGio
                  gio={bua.gio}
                  tieuDe={`Giờ ${tenBua(bua)}`}
                  onDoi={(gio) => sua({ gio })}
                />
              </View>
            </KhungThe>

            <KhungThe icon="meal" tieuDe="Món ăn">
              <View>
                <TextField
                  hideLabel
                  multiline
                  label="Mô tả món ăn"
                  placeholder="Ví dụ: Yến mạch với sữa, 3 trứng luộc, 1 chuối. Ưu tiên trứng luộc, hạn chế dầu mỡ."
                  value={bua.moTa ?? ''}
                  onChangeText={(moTa) => sua({ moTa })}
                  maxLength={TOI_DA_KY_TU_MO_TA}
                  textAlignVertical="top"
                  containerClassName="min-h-[110px] pb-8"
                />
                <Text variant="muted" className="absolute bottom-2 right-3 text-xs">
                  {(bua.moTa ?? '').length}/{TOI_DA_KY_TU_MO_TA}
                </Text>
              </View>
              <Text variant="muted" className="text-sm leading-5">
                Ghi cả món ưu tiên hoặc điều cần tránh. Học viên xem được nội dung này trước khi
                check-in.
              </Text>
            </KhungThe>

            <KhungThe icon="flame" tieuDe="Năng lượng">
              <View className="flex-row items-center gap-3">
                <View className="w-28">
                  <TextField
                    hideLabel
                    label="Năng lượng bữa ăn (kcal)"
                    placeholder="550"
                    keyboardType="number-pad"
                    value={bua.kcal === undefined ? '' : String(bua.kcal)}
                    onChangeText={(chu) => {
                      const so = chu.replace(/\D/g, '').slice(0, TOI_DA_CHU_SO_KCAL);
                      sua({ kcal: so ? Number(so) : undefined });
                    }}
                    containerClassName="h-11 px-3"
                  />
                </View>
                <Text className="text-base">kcal</Text>
              </View>
              <Text variant="muted" className="text-sm leading-5">
                PT nhập tay, không cần cộng từng món (N30).
              </Text>
            </KhungThe>

            <KhungThe icon="images" tieuDe="Ảnh món ăn" tieuDePhu="(không bắt buộc)">
              <ChonMotAnh
                anh={bua.anh}
                nhanAnh="Ảnh món ăn đã thêm"
                onChup={() => void layAnh('camera')}
                onThuVien={() => void layAnh('thu_vien')}
                onXoa={() => sua({ anh: undefined })}
              />
              {loiAnh ? (
                <Text accessibilityRole="alert" variant="destructive" className="text-sm">
                  {loiAnh}
                </Text>
              ) : null}
            </KhungThe>

            <Button
              variant="outline"
              shape="pill"
              accessibilityLabel="Xóa bữa"
              onPress={() => {
                router.back();
                xoaBua(bua.id);
              }}
            >
              <Text className="text-destructive">Xóa bữa</Text>
            </Button>
          </ScrollView>

          <View
            className="border-t border-border bg-background px-4 pt-3"
            style={{ paddingBottom: 12 + insets.bottom }}
          >
            <Button
              shape="pill"
              accessibilityLabel="Xong"
              onPress={() => router.back()}
              className="min-h-[56px]"
            >
              <Text>Xong</Text>
            </Button>
          </View>
        </>
      ) : (
        <View className="items-center p-8">
          <Text variant="muted">Không tìm thấy bữa ăn.</Text>
        </View>
      )}
    </KeyboardAvoidingView>
  );
}
