import { AxiosError, AxiosHeaders, type AxiosAdapter } from 'axios';
import { createApiClient, type AuthTransport } from '@/core/api/client';
import { toApiError } from '@/core/api/api-error';

test.each([401, 403, 409, 422, 500])('maps HTTP %s without leaking server details', (status) => {
  const error = new AxiosError('private server trace');
  error.response = {
    status,
    statusText: 'Error',
    headers: {},
    data: { stack: 'secret' },
    config: { headers: new AxiosHeaders() },
  };
  const mapped = toApiError(error);
  expect(mapped.status).toBe(status);
  expect(mapped.message).not.toContain('secret');
  expect(mapped.message).not.toContain('private');
});

function httpError(status: number, data: unknown) {
  const error = new AxiosError('server error');
  error.response = {
    status,
    statusText: 'Error',
    headers: {},
    data,
    config: { headers: new AxiosHeaders() },
  };
  return error;
}

test('reads code, detail, field errors and trace id from ProblemDetails', () => {
  const mapped = toApiError(
    httpError(409, {
      status: 409,
      title: 'Dữ liệu bị xung đột',
      detail: "Tên đăng nhập 'an.nv' đã tồn tại.",
      code: 'USER_USERNAME_TAKEN',
      trace_id: '00-abc-01',
      errors: { tai_khoan: ["Tên đăng nhập 'an.nv' đã tồn tại."], bad: [1] },
    }),
  );
  expect(mapped).toMatchObject({
    status: 409,
    code: 'USER_USERNAME_TAKEN',
    message: "Tên đăng nhập 'an.nv' đã tồn tại.",
    traceId: '00-abc-01',
  });
  expect(mapped.fieldErrors).toEqual({ tai_khoan: ["Tên đăng nhập 'an.nv' đã tồn tại."] });
});

test('never shows server detail for 5xx but keeps code and trace id', () => {
  const mapped = toApiError(
    httpError(500, { detail: 'Connection string secret', code: 'INTERNAL_ERROR', trace_id: 't-1' }),
  );
  expect(mapped.message).not.toContain('secret');
  expect(mapped.code).toBe('INTERNAL_ERROR');
  expect(mapped.traceId).toBe('t-1');
  expect(mapped.fieldErrors).toEqual({});
});

test('falls back to status based code and message without ProblemDetails', () => {
  const mapped = toApiError(httpError(404, 'Not Found'));
  expect(mapped.code).toBe('HTTP_404');
  expect(mapped.message).toBe('Không tìm thấy dữ liệu.');
});

test('maps timeout and offline failures', () => {
  expect(toApiError(new AxiosError('timeout', 'ECONNABORTED')).code).toBe('TIMEOUT');
  expect(toApiError(new AxiosError('offline', 'ERR_NETWORK')).code).toBe('NETWORK');
});

test('concurrent unauthorized requests share one refresh and retry with new token', async () => {
  let token = 'old';
  const auth: AuthTransport = {
    getAccessToken: jest.fn(async () => token),
    refresh: jest.fn(async () => {
      await new Promise((resolve) => setTimeout(resolve, 10));
      token = 'new';
      return token;
    }),
    onUnauthorized: jest.fn(async () => undefined),
  };
  const adapter: AxiosAdapter = async (config) => {
    const response = { status: 200, statusText: 'OK', headers: {}, data: { ok: true }, config };
    if (config.headers.get('Authorization') === 'Bearer old') {
      throw new AxiosError('expired', 'ERR_BAD_REQUEST', config, undefined, {
        ...response,
        status: 401,
      });
    }
    return response;
  };
  const client = createApiClient('https://example.test', auth, { adapter });
  const results = await Promise.all([client.get('/one'), client.get('/two')]);
  expect(results.every((result) => result.data.ok)).toBe(true);
  expect(auth.refresh).toHaveBeenCalledTimes(1);
  expect(auth.onUnauthorized).not.toHaveBeenCalled();
});

test('does not loop when the refreshed token is rejected', async () => {
  const auth: AuthTransport = {
    getAccessToken: async () => 'invalid',
    refresh: jest.fn(async () => 'invalid'),
    onUnauthorized: jest.fn(async () => undefined),
  };
  let requests = 0;
  const adapter: AxiosAdapter = async (config) => {
    requests++;
    throw new AxiosError('expired', 'ERR_BAD_REQUEST', config, undefined, {
      status: 401,
      statusText: 'Unauthorized',
      headers: {},
      data: {},
      config,
    });
  };
  await expect(
    createApiClient('https://example.test', auth, { adapter }).get('/private'),
  ).rejects.toMatchObject({ status: 401 });
  expect(requests).toBe(2);
  expect(auth.refresh).toHaveBeenCalledTimes(1);
  expect(auth.onUnauthorized).toHaveBeenCalled();
});
