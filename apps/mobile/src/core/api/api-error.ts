import { isAxiosError } from 'axios';

export class ApiError extends Error {
  constructor(
    message: string,
    public readonly status?: number,
    public readonly code = 'UNKNOWN',
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

export function toApiError(error: unknown): ApiError {
  if (error instanceof ApiError) return error;
  if (!isAxiosError(error)) return new ApiError('Có lỗi xảy ra. Vui lòng thử lại.');
  if (error.code === 'ECONNABORTED' || error.code === 'ETIMEDOUT') {
    return new ApiError('Kết nối quá thời gian. Vui lòng thử lại.', undefined, 'TIMEOUT');
  }
  const status = error.response?.status;
  const messages: Record<number, string> = {
    401: 'Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.',
    403: 'Bạn chưa có quyền thực hiện thao tác này.',
    409: 'Dữ liệu đã thay đổi. Vui lòng tải lại.',
    422: 'Thông tin chưa hợp lệ. Vui lòng kiểm tra lại.',
  };
  return new ApiError(
    status
      ? (messages[status] ?? 'Dịch vụ đang bận. Vui lòng thử lại.')
      : 'Không thể kết nối. Kiểm tra mạng và thử lại.',
    status,
    status ? `HTTP_${status}` : 'NETWORK',
  );
}
