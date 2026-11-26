export type FilePurpose = 'avatar' | 'gym_photo' | 'progress' | 'post' | 'check_in' | 'chung_chi_pt';

export type MediaPurpose = FilePurpose;

/** Kết quả một lần upload đã được backend xác nhận. Dùng `file_id` để gắn vào đối tượng; xem file qua URL ký (không có URL cố định). */
export interface FileUploadResult {
  file_id: string;
  original_name: string;
  content_type: string;
  /** Dung lượng thật (byte) do backend đo trên storage. */
  size: number;
  purpose: string;
}

export type UploadState = 'idle' | 'compressing' | 'uploading' | 'confirming' | 'done' | 'error';

export interface UploadOptions {
  contentType?: string;
  /** Tên tệp gửi lên; tệp tài liệu (PDF) dùng tên người dùng đã chọn thay vì tên trong bộ nhớ đệm. */
  fileName?: string;
  /** Dung lượng tệp (byte) nếu đã biết (ví dụ từ bộ chọn tệp); chỉ là ước lượng để backend kiểm sớm. */
  sizeBytes?: number;
  onProgress?: (percent: number) => void;
  signal?: AbortSignal;
  /** Số lần thử lại khi lỗi mạng hoặc storage trả 5xx (mặc định 2). */
  maxRetries?: number;
  /** Chờ trước lần thử lại đầu tiên (ms); các lần sau gấp đôi. */
  initialRetryDelayMs?: number;
}
