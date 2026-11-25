import { useState } from 'react';
import { Pressable, ScrollView, View } from 'react-native';
import { useRouter } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import { DauLoTrinh } from '../components/quan-ly/dau-lo-trinh';
import { HopThoaiDongLoTrinh, MenuLoTrinh } from '../components/quan-ly/hop-thoai-dong-lo-trinh';
import { TabCheckIn } from '../components/quan-ly/tab-check-in';
import { TabHomNay } from '../components/quan-ly/tab-hom-nay';
import { TabKeHoach } from '../components/quan-ly/tab-ke-hoach';
import { TabTienDo } from '../components/quan-ly/tab-tien-do';
import { useThongBaoDanhGia, xoaThongBaoDanhGia } from '../data/danh-gia-check-in-store';
import { dongLoTrinh } from '../data/dong-lo-trinh-store';
import { useChiTietQuanLy, useLoTrinh } from '../hooks/use-lo-trinh';
import type { CheckInQuanLy, LoaiCheckIn } from '../types';

type Tab = 'hom_nay' | 'ke_hoach' | 'tien_do' | 'check_in';

const CAC_TAB: readonly { gia: Tab; nhan: string }[] = [
  { gia: 'hom_nay', nhan: 'Hôm nay' },
  { gia: 'ke_hoach', nhan: 'Kế hoạch' },
  { gia: 'tien_do', nhan: 'Tiến độ' },
  { gia: 'check_in', nhan: 'Check-in' },
];

/** Nút trên thanh hành động dưới cùng. */
function NutHanhDong({
  icon,
  nhan,
  onPress,
  tat = false,
}: {
  icon: string;
  nhan: string;
  onPress(): void;
  tat?: boolean;
}) {
  const { colors } = useTheme();
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={nhan}
      accessibilityState={{ disabled: tat }}
      disabled={tat}
      onPress={onPress}
      className={cn(
        'min-w-0 flex-1 flex-row items-center justify-center gap-1.5 rounded-xl border border-border bg-card px-2 py-3',
        tat && 'opacity-40',
      )}
    >
      <Icon name={icon} size={18} color={colors.text} />
      <Text className="text-sm font-medium" numberOfLines={1}>
        {nhan}
      </Text>
    </Pressable>
  );
}

/**
 * PT-03 Chi tiết lộ trình học viên (góc nhìn PT), cho cả lộ trình đơn lẫn nhóm. Đầu màn là tên, người tham gia, PT và tuần
 * hiện tại; bốn tab Hôm nay, Kế hoạch, Tiến độ, Check-in; thanh hành động dưới cùng. Menu `...` có "Đóng lộ trình" (N28);
 * lộ trình đã đóng chỉ xem: khóa sửa kế hoạch và ẩn mục đóng. Mở từ "Xem chi tiết" ở PT-01.
 */
// TODO(ROUTE): "Chỉnh sửa kế hoạch" (form PT-02 nạp dữ liệu hiện có) và "Nhắn tin" chưa có màn; hai nút này chưa dẫn đi đâu.
export function ChiTietLoTrinhHocVienScreen({ id }: { id?: string }) {
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const { colors } = useTheme();
  const { homNay } = useLoTrinh();
  const chiTiet = useChiTietQuanLy(id);
  const [tab, setTab] = useState<Tab>('hom_nay');
  const [moMenu, setMoMenu] = useState(false);
  const [moDong, setMoDong] = useState(false);
  // Loại lọc sẵn cho tab Check-in khi đi từ ô chỉ số ở tab Tiến độ (N42).
  const [loaiCheckIn, setLoaiCheckIn] = useState<'tat_ca' | LoaiCheckIn>('tat_ca');
  const thongBao = useThongBaoDanhGia();

  const moCheckIn = (c: CheckInQuanLy) =>
    router.push({
      pathname: '/danh-gia-check-in',
      params: { loTrinhId: id, checkInId: c.id },
    });

  const dangChay = chiTiet?.loTrinh.trangThai === 'dang_hoat_dong';

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
          Chi tiết lộ trình học viên
        </Text>
        {chiTiet && dangChay ? (
          <Pressable
            accessibilityRole="button"
            accessibilityLabel="Tùy chọn lộ trình"
            hitSlop={8}
            onPress={() => setMoMenu(true)}
          >
            <Icon name="more" size={24} color={colors.text} />
          </Pressable>
        ) : null}
      </View>

      {chiTiet ? (
        <>
          <ScrollView
            className="flex-1"
            contentContainerClassName="w-full max-w-[680px] gap-4 self-center px-4"
            contentContainerStyle={{ paddingBottom: 24 }}
          >
            {thongBao ? (
              <View
                accessibilityRole="alert"
                className="flex-row items-center gap-2 rounded-xl bg-success/15 px-3 py-3"
              >
                <Icon name="check-circle" size={20} color={colors.primary} />
                <Text className="flex-1 text-sm font-medium">{thongBao}</Text>
                <Pressable
                  accessibilityRole="button"
                  accessibilityLabel="Đóng thông báo"
                  hitSlop={8}
                  onPress={xoaThongBaoDanhGia}
                >
                  <Icon name="close" size={18} color={colors.muted} />
                </Pressable>
              </View>
            ) : null}
            <DauLoTrinh chiTiet={chiTiet} />

            <View accessibilityRole="tablist" className="flex-row flex-wrap gap-2">
              {CAC_TAB.map((t) => {
                const dangChon = tab === t.gia;
                return (
                  <Pressable
                    key={t.gia}
                    accessibilityRole="tab"
                    accessibilityState={{ selected: dangChon }}
                    onPress={() => {
                      setLoaiCheckIn('tat_ca');
                      setTab(t.gia);
                    }}
                    className={cn(
                      'rounded-full border px-4 py-2',
                      dangChon ? 'border-transparent bg-primary' : 'border-border bg-card',
                    )}
                  >
                    <Text
                      className={cn(
                        'text-sm font-medium',
                        dangChon ? 'text-primary-foreground' : 'text-foreground',
                      )}
                    >
                      {t.nhan}
                    </Text>
                  </Pressable>
                );
              })}
            </View>

            {tab === 'hom_nay' ? (
              <TabHomNay
                chiTiet={chiTiet}
                onXemCheckIn={moCheckIn}
                onDuyetNhanh={
                  dangChay
                    ? () => router.push({ pathname: '/duyet-nhanh', params: { loTrinhId: id } })
                    : undefined
                }
              />
            ) : null}
            {tab === 'ke_hoach' ? <TabKeHoach chiTiet={chiTiet} /> : null}
            {tab === 'tien_do' ? (
              <TabTienDo
                chiTiet={chiTiet}
                homNay={homNay}
                onXemCheckIn={(loai) => {
                  setLoaiCheckIn(loai);
                  setTab('check_in');
                }}
              />
            ) : null}
            {tab === 'check_in' ? (
              <TabCheckIn
                checkIn={chiTiet.checkIn}
                laNhom={chiTiet.loTrinh.loai === 'nhom'}
                onMo={moCheckIn}
                loaiBanDau={loaiCheckIn}
              />
            ) : null}
          </ScrollView>

          <View
            className="flex-row gap-2 border-t border-border bg-background px-4 pt-3"
            style={{ paddingBottom: 12 + insets.bottom }}
          >
            <NutHanhDong
              icon="edit"
              nhan="Chỉnh sửa kế hoạch"
              tat={!dangChay}
              // TODO(ROUTE): mở form PT-02 nạp dữ liệu hiện có, thay đổi áp dụng từ hôm nay (adr.md, N5, N32; luong-man-hinh.md, PT-03).
              onPress={() => undefined}
            />
            <NutHanhDong
              icon="message"
              nhan="Nhắn tin"
              // TODO(ROUTE): mở cuộc trò chuyện với học viên, hoặc với nhóm (phần 15 của tài liệu thiết kế).
              onPress={() => undefined}
            />
            <NutHanhDong icon="trend" nhan="Xem tiến độ" onPress={() => setTab('tien_do')} />
          </View>

          <MenuLoTrinh
            hien={moMenu}
            onDong={() => setMoMenu(false)}
            onDongLoTrinh={() => {
              setMoMenu(false);
              setMoDong(true);
            }}
          />
          <HopThoaiDongLoTrinh
            hien={moDong}
            tenLoTrinh={chiTiet.loTrinh.ten}
            soNguoi={chiTiet.thanhVien.length}
            onHuy={() => setMoDong(false)}
            onXacNhan={(dong) => {
              // TODO(ROUTE): gọi API đóng lộ trình (lưu lý do, người đóng, thời điểm; thông báo học viên, N28).
              dongLoTrinh(chiTiet.loTrinh.id, dong);
              setMoDong(false);
            }}
          />
        </>
      ) : (
        <View className="items-center p-8">
          <Text variant="muted">Không tìm thấy lộ trình.</Text>
        </View>
      )}
    </View>
  );
}
