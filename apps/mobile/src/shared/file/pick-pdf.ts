import * as DocumentPicker from 'expo-document-picker';

/** Dung lượng tối đa của tệp tải lên; khớp `Storage:MaxDocumentBytes` của backend (mặc định 10 MB). */
export const PDF_TOI_DA_BYTE = 10 * 1024 * 1024;

export type PickedPdf = {
  uri: string;
  name: string;
  /** Dung lượng (byte); có thể không biết trên một số nền tảng. */
  size: number | undefined;
};

/** Mở hộp chọn tệp PDF. Trả `null` khi người dùng hủy. */
export async function pickPdf(): Promise<PickedPdf | null> {
  const result = await DocumentPicker.getDocumentAsync({
    type: 'application/pdf',
    copyToCacheDirectory: true,
    multiple: false,
  });
  const asset = result.canceled ? undefined : result.assets?.[0];
  if (!asset) return null;
  return { uri: asset.uri, name: asset.name, size: asset.size ?? undefined };
}
