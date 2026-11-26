import { useState } from 'react';
import { ActivityIndicator, Pressable, Text, View } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { FieldLabel } from '@/shared/components/field-label';
import { UploadError, uploadMultipartFile } from '@/shared/file/file-uploader';
import { PDF_TOI_DA_BYTE, pickPdf } from '@/shared/file/pick-pdf';
import { useTheme } from '@/shared/theme/theme';
import type { ChungChiPt } from '../api/profile-api';

const LABEL = 'Chứng chỉ (nếu có)';
const UPLOAD_ERROR = 'Không tải được tệp lên. Kiểm tra mạng và thử lại.';

/** Câu báo lỗi theo mã HTTP của server; lỗi không có mã (mất mạng...) dùng câu chung. */
function uploadErrorMessage(error: unknown): string {
  if (!(error instanceof UploadError)) {
    // Bản dev kèm lỗi gốc (ví dụ 'Network request failed') để biết yêu cầu hỏng ở đâu; bản chạy thật chỉ hiện câu chung.
    return __DEV__
      ? `${UPLOAD_ERROR} [${error instanceof Error ? error.message : String(error)}]`
      : UPLOAD_ERROR;
  }
  if (error.status === 401) return 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.';
  // Hạn mức trong ngày: câu của server đã đủ rõ nên hiện nguyên văn.
  if (error.code === 'FILE_QUOTA_EXCEEDED') return error.message;
  if (error.code === 'FILE_TOO_LARGE' || error.status === 413)
    return 'Tệp quá lớn. Vui lòng chọn tệp nhỏ hơn.';
  if (error.status === 400 || error.status === 415 || error.code === 'STORAGE_REJECTED')
    return 'Tệp không hợp lệ. Vui lòng chọn một tệp PDF khác.';
  return `Máy chủ chưa nhận được tệp (lỗi ${error.status}). Vui lòng thử lại sau.`;
}

/** `2048` thành "2 KB", `3,5 MB`: đủ để người dùng nhận ra tệp mình chọn. */
export function formatDungLuong(byte: number): string {
  if (byte < 1024) return `${byte} B`;
  if (byte < 1024 * 1024) return `${Math.round(byte / 1024)} KB`;
  return `${(byte / (1024 * 1024)).toFixed(1).replace('.', ',')} MB`;
}

type Props = {
  value: ChungChiPt | null;
  onChange(value: ChungChiPt | null): void;
  /** Báo đang tải lên, để màn hình khóa nút lưu cho tới khi xong. */
  onUploadingChange?(uploading: boolean): void;
  /** Lỗi từ form (server từ chối tệp). */
  error?: string;
};

/**
 * Ô chọn chứng chỉ PT dạng PDF. Chọn xong tải lên ngay (`purpose = chung_chi_pt`); tải thành công thì giữ `file_id` để gửi
 * cùng hồ sơ. Bấm dấu × để bỏ tệp. Tệp đã tải lên mà không gắn vào hồ sơ nằm lại ở trạng thái `da_tai_len` trên backend,
 * chờ job dọn file (chưa có).
 */
export function ChungChiField({ value, onChange, onUploadingChange, error }: Props) {
  const { colors } = useTheme();
  const [uploading, setUploading] = useState(false);
  const [localError, setLocalError] = useState<string | undefined>();

  const setBusy = (busy: boolean) => {
    setUploading(busy);
    onUploadingChange?.(busy);
  };

  const choose = async () => {
    if (uploading) return;
    setLocalError(undefined);
    let picked;
    try {
      picked = await pickPdf();
    } catch {
      setLocalError('Không mở được hộp chọn tệp. Vui lòng thử lại.');
      return;
    }
    if (!picked) return;
    if (picked.size !== undefined && picked.size > PDF_TOI_DA_BYTE) {
      setLocalError(`Tệp quá lớn. Tối đa ${formatDungLuong(PDF_TOI_DA_BYTE)}.`);
      return;
    }
    setBusy(true);
    try {
      const result = await uploadMultipartFile(picked.uri, 'chung_chi_pt', {
        contentType: 'application/pdf',
        fileName: picked.name,
        sizeBytes: picked.size,
      });
      onChange({
        file_id: result.file_id,
        ten_tep: result.original_name || picked.name,
        kich_thuoc_byte: result.size,
      });
    } catch (error) {
      if (__DEV__) console.warn('[ChungChiField] Tải chứng chỉ lỗi:', picked.uri, error);
      setLocalError(uploadErrorMessage(error));
    } finally {
      setBusy(false);
    }
  };

  const shownError = localError ?? error;

  return (
    <View className="gap-2">
      <FieldLabel label={LABEL} />
      {uploading ? (
        <View
          accessibilityRole="progressbar"
          accessibilityLabel="Đang tải chứng chỉ lên"
          className="h-[52px] flex-row items-center gap-3 rounded-xl border border-input bg-card px-4"
        >
          <ActivityIndicator color={colors.primary} />
          <Text className="flex-1 text-base text-muted-foreground">Đang tải lên…</Text>
        </View>
      ) : value ? (
        <View className="min-h-[52px] flex-row items-center gap-3 rounded-xl border border-input bg-card px-4 py-2">
          <Ionicons name="document-text-outline" size={22} color={colors.primary} />
          <View className="flex-1">
            <Text numberOfLines={1} className="text-base text-foreground">
              {value.ten_tep}
            </Text>
            <Text className="text-sm text-muted-foreground">
              PDF · {formatDungLuong(value.kich_thuoc_byte)}
            </Text>
          </View>
          <Pressable
            accessibilityRole="button"
            accessibilityLabel="Xóa chứng chỉ"
            hitSlop={10}
            onPress={() => {
              setLocalError(undefined);
              onChange(null);
            }}
          >
            <Ionicons name="close" size={22} color={colors.muted} />
          </Pressable>
        </View>
      ) : (
        <Pressable
          accessibilityRole="button"
          accessibilityLabel="Chọn tệp PDF chứng chỉ"
          onPress={() => void choose()}
          className="h-[52px] flex-row items-center gap-3 rounded-xl border border-input bg-card px-4 active:opacity-75"
        >
          <Ionicons name="document-attach-outline" size={22} color={colors.primary} />
          <Text className="flex-1 text-base text-muted-foreground">Chọn tệp PDF</Text>
        </Pressable>
      )}
      {shownError ? (
        <Text accessibilityRole="alert" className="text-sm text-destructive">
          {shownError}
        </Text>
      ) : (
        <Text className="text-sm text-muted-foreground">
          Chỉ nhận tệp PDF, tối đa {formatDungLuong(PDF_TOI_DA_BYTE)}. Tệp được lưu riêng tư.
        </Text>
      )}
    </View>
  );
}
