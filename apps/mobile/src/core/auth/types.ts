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
  /** True cho tới khi người dùng bấm một trong hai nút của màn onboarding sau lần đăng nhập đầu tiên. */
  is_dang_nhap_lan_dau: boolean;
};

/** Kết quả của login, đăng nhập Google, refresh. `user.is_dang_nhap_lan_dau` cho app biết có hiện onboarding không. */
export type AuthResult = {
  access_token: string;
  expires_in: number;
  refresh_token: string;
  user: AuthUser;
};
