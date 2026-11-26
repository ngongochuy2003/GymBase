import { useEffect, useState } from 'react';
import { AccessibilityInfo } from 'react-native';

/**
 * Số đếm từ 0 lên `dich` (chậm dần về cuối) để hiện trong chữ. Chữ không chạy được bằng giá trị chia sẻ của Reanimated nên
 * dựng bằng state, nhưng chỉ state của con số được làm tròn: React bỏ qua khi số hiển thị không đổi, nên 60 khung hình mỗi
 * giây chỉ vẽ lại chữ khi số thật sự nhảy. Bật "giảm chuyển động" của hệ thống thì trả `dich` ngay.
 * `chuSo` là số chữ số thập phân; `tre` là thời gian chờ (ms) trước khi bắt đầu.
 */
export function useSoDemLen(dich: number, chuSo = 0, thoiLuong = 1000, tre = 0): number {
  const [hien, setHien] = useState(0);

  useEffect(() => {
    const he = 10 ** chuSo;
    const lamTron = (so: number) => Math.round(so * he) / he;
    let huy = false;
    let hen: ReturnType<typeof setTimeout> | undefined;
    let khung: number | undefined;

    void AccessibilityInfo.isReduceMotionEnabled().then((giam) => {
      if (huy) return;
      if (giam) {
        setHien(lamTron(dich));
        return;
      }
      hen = setTimeout(() => {
        let batDau: number | undefined;
        const buoc = (bay: number) => {
          batDau ??= bay;
          const x = Math.min(1, (bay - batDau) / thoiLuong);
          setHien(lamTron(dich * (1 - (1 - x) ** 3)));
          if (x < 1 && !huy) khung = requestAnimationFrame(buoc);
        };
        khung = requestAnimationFrame(buoc);
      }, tre);
    });

    return () => {
      huy = true;
      if (hen) clearTimeout(hen);
      if (khung !== undefined) cancelAnimationFrame(khung);
    };
  }, [dich, chuSo, thoiLuong, tre]);

  return hien;
}
