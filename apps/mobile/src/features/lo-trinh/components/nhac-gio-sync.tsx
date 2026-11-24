import { useEffect, useMemo } from 'react';
import { AppState } from 'react-native';
import { dongBoNhacGioAsync, gopLanGoi, SO_NGAY_TOI_DA } from '@/shared/notifications';
import { taoKeHoachMau } from '../data/seed';
import { useLoTrinh } from '../hooks/use-lo-trinh';
import type { KeHoachLoTrinh, LoTrinhCuaToi } from '../types';
import { suKienNhacCuaLoTrinh } from '../utils/nhac-gio';

/** Dữ liệu mới nhất mà lần đồng bộ sắp chạy sẽ dùng; `null` khi component đã gỡ (đăng xuất) thì không đồng bộ nữa. */
let dauVao: { cuaToi: readonly LoTrinhCuaToi[]; keHoach: KeHoachLoTrinh } | null = null;

/** Các lần gọi chồng nhau được gộp; mỗi lần chạy tính lại từ giờ hiện tại và dữ liệu mới nhất. */
const dongBo = gopLanGoi(async () => {
  if (!dauVao) return;
  const homNay = new Date();
  await dongBoNhacGioAsync(suKienNhacCuaLoTrinh(dauVao.cuaToi, dauVao.keHoach, homNay, SO_NGAY_TOI_DA), { now: homNay });
});

/**
 * Giữ lịch nhắc ăn, nhắc tập trên máy khớp với kế hoạch; không vẽ gì. Mount trong nhóm đã đăng nhập (ADR-2, quyết định 14):
 * đồng bộ khi vào app, khi kế hoạch hoặc check-in đổi, và mỗi lần quay lại foreground (cũng là lúc quyền thông báo có thể vừa
 * đổi trong cài đặt hệ thống, và sang ngày mới thì cửa sổ nhắc được nạp lại).
 */
export function NhacGioSync() {
  const { cuaToi } = useLoTrinh();
  // TODO(ROUTE): kế hoạch ăn và lịch tập của học viên lấy từ API lộ trình (muc_ke_hoach, K10); hiện dùng kế hoạch mẫu.
  const keHoach = useMemo(() => taoKeHoachMau(), []);

  useEffect(() => {
    dauVao = { cuaToi, keHoach };
    void dongBo();
  }, [cuaToi, keHoach]);

  useEffect(() => {
    const sub = AppState.addEventListener('change', (state) => {
      if (state === 'active') void dongBo();
    });
    return () => {
      sub.remove();
      dauVao = null;
    };
  }, []);

  return null;
}
