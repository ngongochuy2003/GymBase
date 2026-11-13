/** Người dùng do API trả ra (`TaiKhoanDto`); không bao giờ chứa mật khẩu. */
export type AuthUser = {
  id: string;
  ten_dang_nhap: string | null;
  ho_ten: string;
  email: string | null;
  so_dien_thoai: string | null;
  vai_tro: string;
  dang_hoat_dong: boolean;
  ngay_tao: string;
  ngay_chinh_sua: string | null;
};

/**
 * Kết quả của login, register, refresh. `tai_khoan_moi` true khi tài khoản vừa được tạo trong chính lần gọi đó,
 * để app hiện màn "Đăng ký thành công" đúng một lần.
 */
export type AuthResult = {
  access_token: string;
  expires_in: number;
  refresh_token: string;
  user: AuthUser;
  tai_khoan_moi: boolean;
};
