import { useEffect } from 'react';
import { AppState, Platform } from 'react-native';
import { useQueryClient } from '@tanstack/react-query';
import { useRouter } from 'expo-router';
import { duongDanCuaThongBao } from './dieu-huong';
import { THONG_BAO_QUERY_KEY } from './hooks';
import { gopLanGoi } from './gop-lan-goi';
import { layNotificationAdapter } from './notification-adapter';
import { setupNotificationChannelsAsync, xinQuyenThongBaoAsync } from './notification-service';
import { dongBoThietBiAsync } from './thiet-bi';

/**
 * Giữ thông báo chạy khi đã đăng nhập; không vẽ gì. Mount trong nhóm đã đăng nhập, nên đăng xuất thì tự gỡ.
 * - Vào app: tạo kênh, xin quyền, đăng ký thiết bị và token.
 * - Quay lại foreground: đăng ký lại và tải lại hộp thư (push chỉ là cái chuông, hộp thư mới là nguồn sự thật).
 * - Token đổi: đăng ký lại. Nhận push hoặc bấm vào push: tải lại hộp thư, bấm thì mở màn tương ứng.
 */
export function ThongBaoSync() {
  const client = useQueryClient();
  const router = useRouter();

  useEffect(() => {
    let dangChay = true;
    const lamMoi = () => void client.invalidateQueries({ queryKey: THONG_BAO_QUERY_KEY });
    // Các lần đồng bộ đến chồng nhau (vào app, foreground, token đổi) được gộp, tránh nhiều request đăng ký cùng lúc.
    const dongBoMotLan = gopLanGoi(() => dongBoThietBiAsync());
    const dongBo = () => void dongBoMotLan();

    void (async () => {
      await setupNotificationChannelsAsync().catch(() => undefined);
      await xinQuyenThongBaoAsync().catch(() => undefined);
      if (dangChay) dongBo();
    })();

    const adapter = Platform.OS === 'web' ? null : layNotificationAdapter();
    const dangKy = adapter
      ? [
          adapter.addPushTokenListener(dongBo),
          adapter.addNotificationReceivedListener(lamMoi),
          // TODO(THONGBAO): bấm push khi app đã bị tắt hẳn chưa mở màn tương ứng; thêm useLastNotificationResponse.
          adapter.addNotificationResponseReceivedListener(() => {
            lamMoi();
            router.push(duongDanCuaThongBao());
          }),
        ]
      : [];
    const appState = AppState.addEventListener('change', (state) => {
      if (state === 'active') {
        lamMoi();
        dongBo();
      }
    });

    return () => {
      dangChay = false;
      appState.remove();
      // Bản giả của expo-notifications trong Jest trả về undefined thay vì subscription.
      dangKy.forEach((sub) => (sub as { remove?: () => void } | undefined)?.remove?.());
    };
  }, [client, router]);

  return null;
}
