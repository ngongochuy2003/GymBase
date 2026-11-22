import { useEffect, useMemo, useState } from 'react';
import { Pressable, ScrollView, View } from 'react-native';
import { Button } from '@/components/ui/button';
import { Text } from '@/components/ui/text';
import Animated, {
  Easing,
  FadeInDown,
  useAnimatedStyle,
  useSharedValue,
  withTiming,
} from 'react-native-reanimated';
import { useRouter } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { useSession } from '@/core/auth/session';
import { cn } from '@/lib/utils';
import { MainHeader } from '@/shared/components/main-header';
import { MainTabBar } from '@/shared/components/main-tab-bar';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import { BoLocQuanLy } from '../components/bo-loc-quan-ly';
import { KhoiHomNay } from '../components/khoi-hom-nay';
import { TheLoTrinhCuaToi } from '../components/the-lo-trinh-cua-toi';
import { TheLoTrinhQuanLy } from '../components/the-lo-trinh-quan-ly';
import { useLoTrinh } from '../hooks/use-lo-trinh';
import { useToastCheckIn } from '../hooks/use-toast-check-in';
import type { ThangLoc, TrangThaiLoTrinh } from '../types';
import { locLoTrinhQuanLy, thangHienTai } from '../utils/ngay';

type SubTab = 'cua_toi' | 'quan_ly';

const SUB_TABS: { id: SubTab; nhan: string; icon: string }[] = [
  { id: 'cua_toi', nhan: 'Lộ trình của tôi', icon: 'route-mine' },
  { id: 'quan_ly', nhan: 'Lộ trình tôi quản lý', icon: 'route-manage' },
];

function SubTabs({ chon, onChon }: { chon: SubTab; onChon(tab: SubTab): void }) {
  const { colors } = useTheme();
  // Viên thuốc nền trượt trái phải theo tab đang chọn; rộng bằng nửa vùng trong (đã trừ padding p-1 hai bên).
  const [rongVung, setRongVung] = useState(0);
  const rongVien = Math.max(0, (rongVung - 8) / SUB_TABS.length);
  const viTri = useSharedValue(0);
  useEffect(() => {
    viTri.value = withTiming(SUB_TABS.findIndex((tab) => tab.id === chon) * rongVien, {
      duration: 250,
      easing: Easing.out(Easing.cubic),
    });
  }, [chon, rongVien, viTri]);
  const kieuVien = useAnimatedStyle(() => ({
    width: rongVien,
    transform: [{ translateX: viTri.value }],
  }));
  return (
    <View
      accessibilityRole="tablist"
      onLayout={(e) => setRongVung(e.nativeEvent.layout.width)}
      className="flex-row rounded-full border border-border bg-card p-1"
    >
      <Animated.View
        pointerEvents="none"
        style={kieuVien}
        className="absolute bottom-1 left-1 top-1 rounded-full bg-primary"
      />
      {SUB_TABS.map((tab) => {
        const dangChon = tab.id === chon;
        return (
          <Pressable
            key={tab.id}
            accessibilityRole="tab"
            accessibilityState={{ selected: dangChon }}
            onPress={() => onChon(tab.id)}
            className="flex-1 flex-row items-center justify-center gap-2 rounded-full px-2 py-2"
          >
            <Icon name={tab.icon} size={20} color={dangChon ? colors.onPrimary : colors.text} />
            <Text
              className={cn(
                'text-sm font-medium',
                dangChon ? 'text-primary-foreground' : 'text-foreground',
              )}
              numberOfLines={1}
            >
              {tab.nhan}
            </Text>
          </Pressable>
        );
      })}
    </View>
  );
}

function TieuDeNhom({ children }: { children: string }) {
  return <Text className="px-1 pt-2 text-lg font-bold text-foreground">{children}</Text>;
}

function TrangThaiTrong({ icon, tieuDe, moTa }: { icon: string; tieuDe: string; moTa: string }) {
  const { colors } = useTheme();
  return (
    <View className="items-center gap-2 rounded-3xl border border-border bg-card px-6 py-10">
      <Icon name={icon} size={36} color={colors.muted} />
      <Text className="text-base font-bold text-foreground">{tieuDe}</Text>
      <Text className="text-center text-sm text-muted-foreground">{moTa}</Text>
    </View>
  );
}

function ONumThongKe({ giaTri, nhan, icon }: { giaTri: number; nhan: string; icon: string }) {
  const { colors } = useTheme();
  return (
    <View className="flex-1 flex-row items-center gap-2 rounded-2xl border border-border bg-card px-2.5 py-3">
      <Icon name={icon} size={26} color={colors.text} />
      <View className="flex-1">
        <Text className="text-[22px] font-bold leading-7 text-foreground">{giaTri}</Text>
        <Text className="text-xs text-muted-foreground" numberOfLines={2}>
          {nhan}
        </Text>
      </View>
    </View>
  );
}

export function LoTrinhScreen() {
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const { user } = useSession();
  const { colors } = useTheme();
  const { homNay, cuaToi, quanLy } = useLoTrinh();
  const toast = useToastCheckIn();
  const [subTab, setSubTab] = useState<SubTab>('cua_toi');
  const [trangThai, setTrangThai] = useState<TrangThaiLoTrinh>('dang_hoat_dong');
  const [thang, setThang] = useState<ThangLoc>(() => thangHienTai(homNay));

  const dangChay = useMemo(
    () => cuaToi.filter((lo) => lo.trangThai === 'dang_hoat_dong'),
    [cuaToi],
  );
  const daKetThuc = useMemo(
    () => cuaToi.filter((lo) => lo.trangThai !== 'dang_hoat_dong'),
    [cuaToi],
  );
  const quanLyDaLoc = useMemo(
    () => locLoTrinhQuanLy(quanLy, { trangThai, thang }),
    [quanLy, trangThai, thang],
  );
  const soHocVien = quanLyDaLoc.reduce((tong, lo) => tong + lo.nguoiThamGia.length, 0);
  const soChoDuyet = quanLyDaLoc.reduce((tong, lo) => tong + lo.soCheckInChoDuyet, 0);
  const soLichTap = quanLyDaLoc.reduce((tong, lo) => tong + lo.soBuoiTapHomNay, 0);
  const nhiemVuHomNay = dangChay[0]?.nhiemVuHomNay ?? [];

  return (
    <View className="flex-1 bg-background" style={{ paddingTop: insets.top }}>
      <MainHeader ten={user?.ho_ten} badgeThongBao />
      <ScrollView
        className="flex-1"
        contentContainerClassName="w-full max-w-[680px] gap-4 self-center px-4 pb-8 pt-2"
        keyboardShouldPersistTaps="handled"
      >
        <SubTabs chon={subTab} onChon={setSubTab} />

        <Animated.View key={subTab} entering={FadeInDown.duration(250)} className="gap-4">
          {subTab === 'cua_toi' ? (
            <>
              {dangChay.length > 0 ? (
                <>
                  {dangChay.map((lo) => (
                    <TheLoTrinhCuaToi key={lo.id} loTrinh={lo} />
                  ))}
                  {dangChay[0] ? (
                    <KhoiHomNay
                      loTrinhId={dangChay[0].id}
                      nhiemVu={nhiemVuHomNay}
                      homNay={homNay}
                    />
                  ) : null}
                </>
              ) : (
                <TrangThaiTrong
                  icon="route-mine"
                  tieuDe="Chưa có lộ trình đang thực hiện"
                  moTa="Khi PT mời bạn vào một lộ trình và bạn chấp nhận, lộ trình sẽ hiện ở đây."
                />
              )}
              {daKetThuc.length > 0 ? (
                <>
                  <TieuDeNhom>{`Đã kết thúc (${daKetThuc.length})`}</TieuDeNhom>
                  {daKetThuc.map((lo) => (
                    <TheLoTrinhCuaToi key={lo.id} loTrinh={lo} />
                  ))}
                </>
              ) : null}
            </>
          ) : (
            <>
              <View className="flex-row gap-3">
                <ONumThongKe giaTri={soHocVien} nhan="học viên" icon="route-manage" />
                <ONumThongKe giaTri={soChoDuyet} nhan="check‑in cần duyệt" icon="clipboard-check" />
                <ONumThongKe giaTri={soLichTap} nhan="lịch tập hôm nay" icon="calendar" />
              </View>
              <Button
                shape="pill"
                accessibilityLabel="Tạo lộ trình"
                // TODO(ROUTE): PT-02 nạp sẵn bản nháp của PT nếu có (adr.md, N22).
                onPress={() => router.push('/tao-lo-trinh')}
                className="min-h-0 gap-2 border-0 px-0 py-2.5"
              >
                <Icon name="plus" size={20} color={colors.onPrimary} />
                <Text>Tạo lộ trình</Text>
              </Button>
              <BoLocQuanLy
                trangThai={trangThai}
                thang={thang}
                onDoiTrangThai={setTrangThai}
                onDoiThang={setThang}
              />
              <Text className="px-1 text-sm text-muted-foreground">{`${quanLyDaLoc.length} lộ trình`}</Text>
              {quanLyDaLoc.length > 0 ? (
                quanLyDaLoc.map((lo) => (
                  <TheLoTrinhQuanLy key={lo.id} loTrinh={lo} homNay={homNay} />
                ))
              ) : (
                <TrangThaiTrong
                  icon="route-manage"
                  tieuDe="Không có lộ trình nào"
                  moTa="Thử đổi trạng thái hoặc tháng, hoặc bấm Tạo lộ trình để bắt đầu."
                />
              )}
            </>
          )}
        </Animated.View>
      </ScrollView>
      <MainTabBar activeId="route" />
      {toast}
    </View>
  );
}
