import { useEffect } from 'react';
import { useToast } from '@/shared/ui-kit';
import { useThongBaoCheckIn, xoaThongBao } from '../data/check-in-store';

/**
 * Hiện thông báo sau khi gửi check-in ("Đã gửi check-in bữa trưa, PT sẽ xem và phản hồi sớm") trên màn đang hiển thị khi
 * quay về từ form. Đặt phần tử trả về ở cuối màn hình.
 */
export function useToastCheckIn() {
  const { toast, show } = useToast();
  const thongBao = useThongBaoCheckIn();
  useEffect(() => {
    if (!thongBao) return;
    show(thongBao);
    xoaThongBao();
  }, [thongBao, show]);
  return toast;
}
