import { useState } from 'react';
import { Pressable, View } from 'react-native';
import { Text } from '@/components/ui/text';
import { cn } from '@/lib/utils';
import { taoTienDoMau } from '../../data/seed';
import type { ChiTietQuanLy, LoaiCheckIn } from '../../types';
import { thanhVienDangThamGia } from '../../utils/nhiem-vu-nhom';
import { KhoiAnhMoc, KhoiCanNang, KhoiChiSo, KhoiMucTieu } from '../chi-tiet-tien-do';
import { AnhDaiDien } from '../anh-nguoi';
import { KhungThe } from '../khung-the';

/**
 * Tab Tiến độ của chi tiết lộ trình PT (PT-03): cùng các khối với tiến độ của học viên (HV-07, tab Tổng quan) nhưng cho
 * một thành viên. Lộ trình nhóm có hàng chip chọn thành viên vì cân nặng và ảnh là của từng người; chỉ ảnh học viên cho
 * PT xem mới hiện (N33).
 */
// TODO(ROUTE): số liệu tiến độ từng thành viên lấy từ API (chi_so_co_the, nhat_ky_hoat_dong); đang dùng dữ liệu mẫu chung.
export function TabTienDo({
  chiTiet,
  homNay,
  onXemCheckIn,
}: {
  chiTiet: ChiTietQuanLy;
  homNay: Date;
  /** Bấm ô chỉ số thì sang tab Check-in lọc theo loại (N42). */
  onXemCheckIn?(loai: LoaiCheckIn): void;
}) {
  const { loTrinh, pt, mucTieu, tuanHienTai } = chiTiet;
  const nguoi = thanhVienDangThamGia(chiTiet.thanhVien);
  const laNhom = loTrinh.loai === 'nhom';
  const [chon, setChon] = useState(nguoi[0]?.id);
  const thanhVien = nguoi.find((t) => t.id === chon) ?? nguoi[0];

  // React Compiler tự ghi nhớ; tiến độ mẫu dựng lại nhẹ nên không cần useMemo.
  const tienDo = thanhVien
    ? taoTienDoMau({
        id: `${loTrinh.id}-${thanhVien.id}`,
        mucTieu,
        tuanHienTai,
        tienDo: loTrinh.tienDo,
        pt,
      })
    : undefined;

  if (!thanhVien || !tienDo) {
    return (
      <Text variant="muted" className="px-1 text-sm">
        Chưa có học viên nào tham gia nên chưa có tiến độ.
      </Text>
    );
  }
  const anhChoPt = tienDo.anhMoc.filter((a) => a.quyenXem === 'cho_pt');
  const soAnhRiengTu = tienDo.anhMoc.length - anhChoPt.length;
  const tong = tienDo.buoiTap.tong + tienDo.buaAn.tong;
  const tuanThu =
    tong === 0 ? 0 : Math.round(((tienDo.buoiTap.xong + tienDo.buaAn.xong) / tong) * 100);

  return (
    <View className="gap-4">
      {laNhom ? (
        <View accessibilityRole="radiogroup" className="flex-row flex-wrap gap-2">
          {nguoi.map((t) => {
            const dangChon = t.id === thanhVien.id;
            return (
              <Pressable
                key={t.id}
                accessibilityRole="radio"
                accessibilityState={{ checked: dangChon }}
                accessibilityLabel={`Tiến độ của ${t.ten}`}
                onPress={() => setChon(t.id)}
                className={cn(
                  'flex-row items-center gap-1.5 rounded-full border py-1 pl-1 pr-3',
                  dangChon ? 'border-transparent bg-primary' : 'border-border bg-card',
                )}
              >
                <AnhDaiDien uri={t.anh} ten={t.ten} className="size-6" />
                <Text
                  className={cn(
                    'text-sm font-medium',
                    dangChon ? 'text-primary-foreground' : 'text-foreground',
                  )}
                >
                  {t.ten}
                </Text>
              </Pressable>
            );
          })}
        </View>
      ) : null}

      {/* Đổi thành viên thì dựng lại để biểu đồ và số đếm chạy lại từ đầu. */}
      <View key={thanhVien.id} className="gap-4">
        <KhoiMucTieu
          loTrinh={{ ten: loTrinh.ten, mucTieu, ngayKetThuc: loTrinh.ngayKetThuc }}
          canNangHienTai={tienDo.canNang.at(-1)?.kg ?? mucTieu.tu}
          homNay={homNay}
        />
        <KhoiCanNang diem={tienDo.canNang} donVi={mucTieu.donVi} />
        <KhoiChiSo
          buoiTap={tienDo.buoiTap}
          buaAn={tienDo.buaAn}
          tuanThu={tuanThu}
          onMoLichSu={onXemCheckIn}
        />
        {anhChoPt.length > 0 ? (
          <KhoiAnhMoc anh={anhChoPt} />
        ) : (
          <KhungThe icon="images" tieuDe="Ảnh cơ thể theo mốc">
            <Text variant="muted" className="text-sm">
              Học viên chưa cho PT xem ảnh nào.
            </Text>
          </KhungThe>
        )}
        {soAnhRiengTu > 0 ? (
          <Text variant="muted" className="px-1 text-xs">
            {soAnhRiengTu} ảnh để chế độ Chỉ mình tôi nên không hiện ở đây.
          </Text>
        ) : null}
      </View>
    </View>
  );
}
