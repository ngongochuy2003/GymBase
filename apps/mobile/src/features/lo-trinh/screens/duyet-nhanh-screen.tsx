import { useState } from 'react';
import {
  Image,
  KeyboardAvoidingView,
  Modal,
  Platform,
  Pressable,
  ScrollView,
  View,
} from 'react-native';
import { useRouter } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Button } from '@/components/ui/button';
import { Text } from '@/components/ui/text';
import { DauManCheckIn } from '../components/check-in-bua-an';
import { ChipLoc } from '../components/chip-loc';
import { TheDuyetNhanh } from '../components/the-duyet-nhanh';
import { guiDanhGia, phienBanHienTai } from '../data/danh-gia-check-in-store';
import { useCheckInChoDuyet, type CheckInChoDuyet } from '../hooks/use-lo-trinh';
import type { LoaiCheckIn } from '../types';

type LocLoai = 'tat_ca' | LoaiCheckIn;

const LOAI: readonly { gia: LocLoai; nhan: string }[] = [
  { gia: 'tat_ca', nhan: 'Tất cả' },
  { gia: 'bua_an', nhan: 'Bữa ăn' },
  { gia: 'buoi_tap', nhan: 'Buổi tập' },
];

/**
 * PT-05 Duyệt nhanh check-in (N43): ảnh lớn của nhiều học viên trên một màn, duyệt hoặc yêu cầu bổ sung ngay trên thẻ, hoặc
 * chọn nhiều bản hoặc duyệt tất cả bản đang hiện một lần. Bản học viên vừa sửa thì không ghi, báo PT xem lại (N13). Có `loTrinhId` thì chỉ lộ
 * trình đó.
 */
// TODO(ROUTE): PT-05 chưa có bản thiết kế; ảnh phóng to chưa kéo, thu phóng được.
export function DuyetNhanhScreen({ loTrinhId }: { loTrinhId?: string }) {
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const tatCa = useCheckInChoDuyet(loTrinhId);
  const [loai, setLoai] = useState<LocLoai>('tat_ca');
  const [daChon, setDaChon] = useState<readonly string[]>([]);
  // Các bản đang chờ xác nhận duyệt; `null` là chưa hỏi.
  const [hoi, setHoi] = useState<readonly CheckInChoDuyet[] | null>(null);
  const [anh, setAnh] = useState<string | null>(null);
  const [thongBao, setThongBao] = useState<string>();
  // Phiên bản PT đã thấy của từng bản; chỉ cập nhật sau khi đã báo là bản đó vừa đổi.
  const [daXem, setDaXem] = useState<Readonly<Record<string, number>>>({});

  const danhSach = tatCa.filter((m) => loai === 'tat_ca' || m.checkIn.loai === loai);
  // Chỉ tính các bản đang hiện, để không duyệt nhầm bản đã bị bộ lọc giấu đi.
  const chonDuoc = danhSach.filter((m) => daChon.includes(m.checkIn.id));

  const gui = (
    cacMuc: readonly CheckInChoDuyet[],
    trangThai: 'da_duyet' | 'can_bo_sung',
    nhanXet = '',
  ) => {
    const daDoi: CheckInChoDuyet[] = [];
    let xong = 0;
    for (const m of cacMuc) {
      const ketQua = guiDanhGia(
        m.checkIn,
        { loTrinhId: m.loTrinhId, trangThai, nhanXet },
        daXem[m.checkIn.id] ?? m.checkIn.phienBan,
      );
      if (ketQua === 'ok') xong += 1;
      if (ketQua === 'da_doi') daDoi.push(m);
    }
    setDaXem((cu) => ({
      ...cu,
      ...Object.fromEntries(daDoi.map((m) => [m.checkIn.id, phienBanHienTai(m.checkIn)])),
    }));
    setDaChon([]);
    const phan = [
      xong > 0 ? `Đã gửi đánh giá cho ${xong} check-in.` : '',
      daDoi.length > 0
        ? `${daDoi.length} check-in học viên vừa sửa, chưa ghi. Hãy xem lại rồi đánh giá.`
        : '',
    ];
    setThongBao(phan.filter(Boolean).join(' ') || undefined);
  };

  return (
    <KeyboardAvoidingView
      className="flex-1 bg-background"
      behavior={Platform.OS === 'ios' ? 'padding' : undefined}
      style={{ paddingTop: insets.top }}
    >
      <DauManCheckIn
        tieuDe="Duyệt nhanh"
        phuDe={`${tatCa.length} check-in chờ duyệt`}
        onQuayLai={() => router.back()}
      />
      <ScrollView
        className="flex-1"
        keyboardShouldPersistTaps="handled"
        contentContainerClassName="w-full max-w-[680px] gap-3 self-center px-4"
        contentContainerStyle={{ paddingBottom: 24 + insets.bottom }}
      >
        <ChipLoc nhom="loại" luaChon={LOAI} chon={loai} onChon={setLoai} />
        {thongBao ? (
          <View accessibilityRole="alert" className="rounded-xl bg-secondary p-3">
            <Text className="text-sm">{thongBao}</Text>
          </View>
        ) : null}
        {danhSach.length === 0 ? (
          <Text variant="muted" className="py-8 text-center text-sm">
            Không còn check-in nào chờ duyệt.
          </Text>
        ) : (
          <>
            {danhSach.map((m) => (
              <TheDuyetNhanh
                key={m.checkIn.id}
                muc={m}
                daChon={daChon.includes(m.checkIn.id)}
                onChon={() =>
                  setDaChon((cu) =>
                    cu.includes(m.checkIn.id)
                      ? cu.filter((id) => id !== m.checkIn.id)
                      : [...cu, m.checkIn.id],
                  )
                }
                onDuyet={() => gui([m], 'da_duyet')}
                onBoSung={(nhanXet) => gui([m], 'can_bo_sung', nhanXet)}
                onXemAnh={setAnh}
                onXemChiTiet={() =>
                  router.push({
                    pathname: '/danh-gia-check-in',
                    params: { loTrinhId: m.loTrinhId, checkInId: m.checkIn.id },
                  })
                }
              />
            ))}
          </>
        )}
      </ScrollView>

      {danhSach.length > 0 ? (
        <View
          className="flex-row gap-3 border-t border-border bg-background px-4 pt-3"
          style={{ paddingBottom: 12 + insets.bottom }}
        >
          <Button
            variant="outline"
            shape="pill"
            accessibilityLabel={`Duyệt tất cả ${danhSach.length} check-in`}
            onPress={() => setHoi(danhSach)}
            className="flex-1"
          >
            <Text>Duyệt tất cả</Text>
          </Button>
          <Button
            shape="pill"
            accessibilityLabel={`Duyệt ${chonDuoc.length} check-in đã chọn`}
            disabled={chonDuoc.length === 0}
            onPress={() => setHoi(chonDuoc)}
            className="flex-1"
          >
            <Text>Duyệt ({chonDuoc.length})</Text>
          </Button>
        </View>
      ) : null}

      {hoi ? (
        <Modal visible transparent animationType="fade" onRequestClose={() => setHoi(null)}>
          <View className="flex-1 items-center justify-center bg-black/40 px-5">
            <View
              accessibilityViewIsModal
              className="w-full max-w-[420px] gap-4 rounded-3xl bg-background p-5"
            >
              <Text accessibilityRole="header" className="text-xl font-bold">
                Duyệt {hoi.length} check-in?
              </Text>
              <Text variant="muted" className="text-sm leading-5">
                Duyệt rồi thì học viên không sửa được nữa.
              </Text>
              <View className="flex-row gap-3">
                <Button
                  variant="outline"
                  shape="pill"
                  accessibilityLabel="Hủy"
                  onPress={() => setHoi(null)}
                  className="flex-1"
                >
                  <Text>Hủy</Text>
                </Button>
                <Button
                  shape="pill"
                  accessibilityLabel="Xác nhận duyệt"
                  onPress={() => {
                    setHoi(null);
                    gui(hoi, 'da_duyet');
                  }}
                  className="flex-1"
                >
                  <Text>Duyệt</Text>
                </Button>
              </View>
            </View>
          </View>
        </Modal>
      ) : null}

      {anh ? (
        <Modal visible transparent animationType="fade" onRequestClose={() => setAnh(null)}>
          <Pressable
            accessibilityRole="button"
            accessibilityLabel="Đóng ảnh"
            onPress={() => setAnh(null)}
            className="flex-1 items-center justify-center bg-black/90"
          >
            <Image source={{ uri: anh }} resizeMode="contain" className="size-full" />
          </Pressable>
        </Modal>
      ) : null}
    </KeyboardAvoidingView>
  );
}
