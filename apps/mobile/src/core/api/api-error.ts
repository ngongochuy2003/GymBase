import { isAxiosError } from 'axios';

/** Lỗi theo từng trường, khóa là tên trường JSON (ví dụ `ten_dang_nhap`). */
export type FieldErrors = Record<string, string[]>;

/**
 * Lỗi đã chuẩn hóa từ response của API (ProblemDetails, xem docs/api/quy-uoc-response.md).
 * `message` luôn hiển thị được cho người dùng; rẽ nhánh theo `code`, không so chuỗi `message`.
 */
export class ApiError extends Error {
  constructor(
    message: string,
    public readonly status?: number,
    public readonly code = 'UNKNOWN',
    public readonly fieldErrors: FieldErrors = {},
    public readonly traceId?: string,
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

/** Các trường của ProblemDetails mà client dùng. */
interface ProblemDetails {
  detail?: string;
  code?: string;
  trace_id?: string;
  errors?: FieldErrors;
}

/** Câu dự phòng khi response không có ProblemDetails, hoặc lỗi 5xx (không hiện nội dung từ server). */
const FALLBACK_MESSAGES: Record<number, string> = {
  400: 'Thông tin chưa hợp lệ. Vui lòng kiểm tra lại.',
  401: 'Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.',
  403: 'Bạn chưa có quyền thực hiện thao tác này.',
  404: 'Không tìm thấy dữ liệu.',
  409: 'Dữ liệu bị trùng hoặc đã thay đổi. Vui lòng tải lại.',
  422: 'Không thể thực hiện thao tác này.',
};
const BUSY_MESSAGE = 'Dịch vụ đang bận. Vui lòng thử lại.';

const isString = (value: unknown): value is string => typeof value === 'string' && value.length > 0;

function readFieldErrors(value: unknown): FieldErrors {
  if (typeof value !== 'object' || value === null) return {};
  const entries = Object.entries(value).flatMap(([field, messages]) =>
    Array.isArray(messages) && messages.every(isString) ? [[field, messages as string[]] as const] : [],
  );
  return Object.fromEntries(entries);
}

function readProblem(data: unknown): ProblemDetails {
  if (typeof data !== 'object' || data === null) return {};
  const body = data as Record<string, unknown>;
  return {
    detail: isString(body.detail) ? body.detail : undefined,
    code: isString(body.code) ? body.code : undefined,
    trace_id: isString(body.trace_id) ? body.trace_id : undefined,
    errors: readFieldErrors(body.errors),
  };
}

export function toApiError(error: unknown): ApiError {
  if (error instanceof ApiError) return error;
  if (!isAxiosError(error)) return new ApiError('Có lỗi xảy ra. Vui lòng thử lại.');
  if (error.code === 'ECONNABORTED' || error.code === 'ETIMEDOUT') {
    return new ApiError('Kết nối quá thời gian. Vui lòng thử lại.', undefined, 'TIMEOUT');
  }

  const status = error.response?.status;
  if (!status) {
    return new ApiError('Không thể kết nối. Kiểm tra mạng và thử lại.', undefined, 'NETWORK');
  }

  const problem = readProblem(error.response?.data);
  const isClientError = status < 500;
  return new ApiError(
    // Lỗi 4xx: detail của server viết sẵn cho người dùng. Lỗi 5xx: không hiện gì từ server.
    (isClientError && problem.detail) || FALLBACK_MESSAGES[status] || BUSY_MESSAGE,
    status,
    problem.code ?? `HTTP_${status}`,
    isClientError ? (problem.errors ?? {}) : {},
    problem.trace_id,
  );
}
