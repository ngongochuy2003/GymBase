export interface CurrentUser {
  id: string;
  name: string;
  avatarUri: string;
}

/** Người dùng giả cho giao diện cộng đồng; tên được thay bằng tên phiên demo khi có. */
export const currentUser: CurrentUser = {
  id: 'u-minh-khoi',
  name: 'Minh Khôi',
  avatarUri: 'https://picsum.photos/seed/gymbase-minh-khoi/160/160',
};
