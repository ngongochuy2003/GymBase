// Mặc định người dùng hủy hộp chọn tệp; test cụ thể đặt `getDocumentAsync.mockResolvedValueOnce(...)` khi cần chọn tệp.
export const getDocumentAsync = jest.fn(async () => ({ canceled: true, assets: null }));
