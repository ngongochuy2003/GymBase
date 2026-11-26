import { useState } from 'react';
import { ActivityIndicator, Image, Pressable, Text, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import * as ImagePicker from 'expo-image-picker';
import { useColorScheme } from 'nativewind';
import { UploadError, uploadMultipartFile } from '@/shared/file/file-uploader';
import { palette } from '@/shared/theme/palette';

const UPLOAD_ERROR = 'Không tải được ảnh lên. Kiểm tra mạng và thử lại.';

/** Ảnh đại diện đã tải lên: `file_id` gửi cùng hồ sơ, `uri` (ảnh trên máy) chỉ để xem trước. */
export type AnhDaiDien = { file_id: string; uri: string };

type Props = {
  value: AnhDaiDien | null;
  onChange(value: AnhDaiDien): void;
  /** Báo đang tải lên, để màn hình khóa nút đi tiếp cho tới khi xong. */
  onUploadingChange?(uploading: boolean): void;
};

/** Ô tròn chọn ảnh đại diện từ thư viện, cắt vuông. Chọn xong tải lên ngay (`purpose = avatar`). */
export function AnhDaiDienField({ value, onChange, onUploadingChange }: Props) {
  const { colorScheme } = useColorScheme();
  const colors = palette[colorScheme === 'dark' ? 'dark' : 'light'];
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string>();

  const setBusy = (busy: boolean) => {
    setUploading(busy);
    onUploadingChange?.(busy);
  };

  const choose = async () => {
    setError(undefined);
    let uri: string | undefined;
    try {
      const quyen = await ImagePicker.requestMediaLibraryPermissionsAsync();
      if (!quyen.granted) {
        setError('GymBase cần quyền truy cập thư viện ảnh để chọn ảnh đại diện.');
        return;
      }
      const ketQua = await ImagePicker.launchImageLibraryAsync({
        mediaTypes: ['images'],
        allowsEditing: true,
        aspect: [1, 1],
        quality: 0.9,
      });
      uri = ketQua.canceled ? undefined : ketQua.assets[0]?.uri;
    } catch {
      setError('Không mở được thư viện ảnh. Vui lòng thử lại.');
      return;
    }
    if (!uri) return;
    setBusy(true);
    try {
      const result = await uploadMultipartFile(uri, 'avatar');
      onChange({ file_id: result.file_id, uri });
    } catch (err) {
      if (__DEV__) console.warn('[AnhDaiDienField] Tải ảnh lỗi:', err);
      // Hạn mức trong ngày: câu của server đã đủ rõ nên hiện nguyên văn.
      setError(
        err instanceof UploadError && err.code === 'FILE_QUOTA_EXCEEDED'
          ? err.message
          : UPLOAD_ERROR,
      );
    } finally {
      setBusy(false);
    }
  };

  const label = value ? 'Đổi ảnh đại diện' : 'Thêm ảnh đại diện';

  return (
    <View className="items-center gap-2">
      <Pressable
        accessibilityRole="button"
        accessibilityLabel={label}
        accessibilityState={{ disabled: uploading, busy: uploading }}
        disabled={uploading}
        onPress={() => void choose()}
        className="h-24 w-24 items-center justify-center overflow-hidden rounded-full border border-input bg-card active:opacity-75"
      >
        {uploading ? (
          <ActivityIndicator color={colors.primary} />
        ) : value ? (
          <Image source={{ uri: value.uri }} className="h-24 w-24" />
        ) : (
          <Ionicons name="camera-outline" size={32} color={colors.primary} />
        )}
      </Pressable>
      <Text className="text-sm text-muted-foreground">{label}</Text>
      {error ? (
        <Text accessibilityRole="alert" className="text-center text-sm text-destructive">
          {error}
        </Text>
      ) : null}
    </View>
  );
}
