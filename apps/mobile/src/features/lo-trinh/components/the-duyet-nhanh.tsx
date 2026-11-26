import { useState } from 'react';
import { Image, Pressable, View } from 'react-native';
import { Button } from '@/components/ui/button';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { TextField } from '@/shared/components/ui';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';
import type { CheckInChoDuyet } from '../hooks/use-lo-trinh';
import { AnhDaiDien } from './anh-nguoi';
import { KET_QUA_BUOI_TAP } from './check-in-buoi-tap';
import { VienNhan } from './quan-ly/thanh-phan';

const TOI_DA_KY_TU_NHAN_XET = 300;

const KIEU_VIEN = {
  thanh_cong: { nen: 'bg-success/15', chu: 'text-success' },
  canh_bao: { nen: 'bg-warning/15', chu: 'text-warning' },
  loi: { nen: 'bg-destructive/15', chu: 'text-destructive' },
};

/** Một check-in trong màn Duyệt nhanh: người gửi, kết quả, ảnh lớn, ghi chú và hai nút duyệt (N43). */
export function TheDuyetNhanh({
  muc,
  daChon,
  onChon,
  onDuyet,
  onBoSung,
  onXemAnh,
  onXemChiTiet,
}: {
  muc: CheckInChoDuyet;
  daChon: boolean;
  onChon(): void;
  onDuyet(): void;
  onBoSung(nhanXet: string): void;
  onXemAnh(uri: string): void;
  onXemChiTiet(): void;
}) {
  const { colors } = useTheme();
  const [moNhanXet, setMoNhanXet] = useState(false);
  const [nhanXet, setNhanXet] = useState('');
  const [thieu, setThieu] = useState(false);
  const { checkIn, tenLoTrinh } = muc;
  const bua = checkIn.bua?.checkIn;
  const buoi = checkIn.buoiTap?.checkIn;
  const anh = bua ? bua.anh : buoi?.anh ? [buoi.anh] : [];
  const ghiChu = bua?.ghiChu ?? buoi?.ghiChu;
  const ten = `${checkIn.tieuDe} của ${checkIn.hocVien.ten}`;
  const ketQua = bua
    ? bua.trangThaiBua === 'da_an'
      ? { nhan: 'Đã ăn', kieu: 'thanh_cong' as const }
      : { nhan: 'Bỏ bữa', kieu: 'loi' as const }
    : buoi
      ? KET_QUA_BUOI_TAP[buoi.trangThaiBuoi]
      : undefined;

  return (
    <View
      className={cn(
        'gap-3 rounded-2xl border bg-card p-4',
        daChon ? 'border-primary' : 'border-border',
      )}
    >
      <View className="flex-row items-center gap-3">
        <Pressable
          accessibilityRole="checkbox"
          accessibilityState={{ checked: daChon }}
          accessibilityLabel={`Chọn ${ten}`}
          hitSlop={8}
          onPress={onChon}
          className={cn(
            'size-7 items-center justify-center rounded-lg border-2',
            daChon ? 'border-primary bg-primary' : 'border-input',
          )}
        >
          {daChon ? <Icon name="check" size={16} color={colors.onPrimary} /> : null}
        </Pressable>
        <AnhDaiDien uri={checkIn.hocVien.anh} ten={checkIn.hocVien.ten} className="size-10" />
        <View className="min-w-0 flex-1">
          <Text className="font-bold" numberOfLines={1}>
            {checkIn.hocVien.ten}
          </Text>
          <Text variant="muted" className="text-xs" numberOfLines={1}>
            {tenLoTrinh}
          </Text>
        </View>
        {ketQua ? <VienNhan nhan={ketQua.nhan} {...KIEU_VIEN[ketQua.kieu]} /> : null}
      </View>

      <Pressable
        accessibilityRole="button"
        accessibilityLabel={`Xem chi tiết ${ten}`}
        onPress={onXemChiTiet}
        className="flex-row items-center gap-2"
      >
        <Icon
          name={checkIn.loai === 'bua_an' ? 'meal' : 'dumbbell'}
          size={18}
          color={colors.text}
        />
        <Text className="flex-1 text-sm font-medium">
          {checkIn.tieuDe} • {checkIn.thoiDiem}
        </Text>
        <Icon name="chevron-right" size={18} color={colors.muted} />
      </Pressable>

      {anh.length > 0 ? (
        <View className="flex-row gap-2">
          {anh.map((uri, i) => (
            <Pressable
              key={uri}
              accessibilityRole="imagebutton"
              accessibilityLabel={`Phóng to ảnh ${i + 1} của ${ten}`}
              onPress={() => onXemAnh(uri)}
              className="aspect-square flex-1"
            >
              <Image
                source={{ uri }}
                resizeMode="cover"
                className="size-full rounded-xl bg-secondary"
              />
            </Pressable>
          ))}
        </View>
      ) : (
        <Text variant="muted" className="text-sm">
          Không gửi ảnh.
        </Text>
      )}

      {ghiChu ? <Text className="text-sm leading-5">{ghiChu}</Text> : null}

      {moNhanXet ? (
        <View className="gap-2">
          <TextField
            hideLabel
            multiline
            label={`Nội dung cần bổ sung cho ${ten}`}
            placeholder="Học viên cần bổ sung gì?"
            value={nhanXet}
            onChangeText={(chu) => {
              setNhanXet(chu);
              setThieu(false);
            }}
            maxLength={TOI_DA_KY_TU_NHAN_XET}
            textAlignVertical="top"
            containerClassName="min-h-[72px]"
          />
          {thieu ? (
            <Text accessibilityRole="alert" variant="destructive" className="text-sm">
              Nhập nội dung cần học viên bổ sung
            </Text>
          ) : null}
        </View>
      ) : null}

      <View className="flex-row gap-2">
        <Button
          variant="outline"
          shape="pill"
          size="sm"
          accessibilityLabel={moNhanXet ? `Gửi yêu cầu bổ sung ${ten}` : `Cần bổ sung ${ten}`}
          onPress={() => {
            if (!moNhanXet) {
              setMoNhanXet(true);
            } else if (!nhanXet.trim()) {
              setThieu(true);
            } else {
              onBoSung(nhanXet.trim());
            }
          }}
          className="flex-1"
        >
          <Text>{moNhanXet ? 'Gửi yêu cầu' : 'Cần bổ sung'}</Text>
        </Button>
        <Button
          shape="pill"
          size="sm"
          accessibilityLabel={`Duyệt ${ten}`}
          onPress={onDuyet}
          className="flex-1"
        >
          <Text>Duyệt</Text>
        </Button>
      </View>
    </View>
  );
}
