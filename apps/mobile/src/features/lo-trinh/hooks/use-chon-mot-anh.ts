import { useState } from 'react';
import * as ImagePicker from 'expo-image-picker';

/**
 * Mở camera hoặc thư viện để lấy đúng một ảnh. Trả `uri` khi chọn được; `loi` khi bị từ chối quyền; rỗng cả hai khi người
 * dùng hủy. `tenAnh` dùng trong thông báo xin quyền, ví dụ "ảnh buổi tập".
 */
export async function layMotAnh(
  nguon: 'camera' | 'thu_vien',
  tenAnh: string,
): Promise<{ uri?: string; loi?: string }> {
  const quyen =
    nguon === 'camera'
      ? await ImagePicker.requestCameraPermissionsAsync()
      : await ImagePicker.requestMediaLibraryPermissionsAsync();
  if (!quyen.granted) {
    return {
      loi:
        nguon === 'camera'
          ? `GymBase cần quyền camera để chụp ${tenAnh}.`
          : `GymBase cần quyền truy cập thư viện ảnh để chọn ${tenAnh}.`,
    };
  }
  const ketQua =
    nguon === 'camera'
      ? await ImagePicker.launchCameraAsync({ mediaTypes: ['images'], quality: 0.8 })
      : await ImagePicker.launchImageLibraryAsync({ mediaTypes: ['images'], quality: 0.8 });
  const dau = ketQua.canceled ? undefined : ketQua.assets[0];
  return { uri: dau?.uri };
}

/**
 * Trạng thái và thao tác chọn đúng một ảnh (chụp bằng camera hoặc chọn từ thư viện) cho các màn ghi nhận. `daCo` là ảnh
 * đã lưu từ trước (khi mở lại để sửa); `tenAnh` dùng trong thông báo xin quyền, ví dụ "ảnh buổi tập".
 */
export function useChonMotAnh(daCo: string | undefined, tenAnh: string) {
  const [anh, setAnh] = useState<string | undefined>(daCo);
  const [loi, setLoi] = useState<string>();

  const chon = async (nguon: 'camera' | 'thu_vien') => {
    setLoi(undefined);
    const ketQua = await layMotAnh(nguon, tenAnh);
    if (ketQua.loi) setLoi(ketQua.loi);
    if (ketQua.uri) setAnh(ketQua.uri);
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
