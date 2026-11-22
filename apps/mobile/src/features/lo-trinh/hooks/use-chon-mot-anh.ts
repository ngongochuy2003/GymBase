import { useState } from 'react';
import * as ImagePicker from 'expo-image-picker';

/**
 * Trạng thái và thao tác chọn đúng một ảnh (chụp bằng camera hoặc chọn từ thư viện) cho các màn ghi nhận. `daCo` là ảnh
 * đã lưu từ trước (khi mở lại để sửa); `tenAnh` dùng trong thông báo xin quyền, ví dụ "ảnh buổi tập".
 */
export function useChonMotAnh(daCo: string | undefined, tenAnh: string) {
  const [anh, setAnh] = useState<string | undefined>(daCo);
  const [loi, setLoi] = useState<string>();

  const chon = async (nguon: 'camera' | 'thu_vien') => {
    setLoi(undefined);
    const quyen =
      nguon === 'camera'
        ? await ImagePicker.requestCameraPermissionsAsync()
        : await ImagePicker.requestMediaLibraryPermissionsAsync();
    if (!quyen.granted) {
      setLoi(
        nguon === 'camera'
          ? `GymBase cần quyền camera để chụp ${tenAnh}.`
          : `GymBase cần quyền truy cập thư viện ảnh để chọn ${tenAnh}.`,
      );
      return;
    }
    const ketQua =
      nguon === 'camera'
        ? await ImagePicker.launchCameraAsync({ mediaTypes: ['images'], quality: 0.8 })
        : await ImagePicker.launchImageLibraryAsync({ mediaTypes: ['images'], quality: 0.8 });
    const dau = ketQua.canceled ? undefined : ketQua.assets[0];
    if (dau) setAnh(dau.uri);
  };

  return {
    anh,
    /** Lỗi xin quyền camera hoặc thư viện, `undefined` khi không có. */
    loi,
    chup: () => void chon('camera'),
    thuVien: () => void chon('thu_vien'),
    xoa: () => setAnh(undefined),
  };
}
