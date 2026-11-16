import { useRouter, type Href } from 'expo-router';
import type { HeaderAction, TabItem } from '../ui-kit';

/** Toàn bộ tab của ứng dụng; mỗi màn hình chọn tập con và thứ tự bằng `pickTabs`. */
export const MAIN_TABS: Record<string, TabItem> = {
  home: { id: 'home', label: 'Trang chủ', icon: 'home', activeIcon: 'home-filled', href: '/home' },
  route: { id: 'route', label: 'Lộ trình', icon: 'route' },
  discover: { id: 'discover', label: 'Khám phá', icon: 'compass' },
  compose: { id: 'compose', label: 'Đăng bài', icon: 'plus', isCenter: true, href: '/compose' },
  messages: { id: 'messages', label: 'Tin nhắn', icon: 'message' },
  profile: { id: 'profile', label: 'Cá nhân', icon: 'user', href: '/profile' },
};

export function pickTabs(ids: readonly string[]): TabItem[] {
  return ids.flatMap((id) => {
    const tab = MAIN_TABS[id];
    return tab ? [tab] : [];
  });
}

/** Action mặc định bên phải AppHeader. */
export const DEFAULT_HEADER_ACTIONS: HeaderAction[] = [
  { id: 'search', icon: 'search', label: 'Tìm kiếm' },
  { id: 'notifications', icon: 'bell', label: 'Thông báo', badge: true },
];

/** Điều hướng khi bấm tab; tab chưa có `href` (chưa có màn hình) thì bỏ qua. */
export function useTabNavigation(): (tab: TabItem) => void {
  const router = useRouter();
  return (tab) => {
    if (!tab.href) return;
    if (tab.id === 'compose') router.push(tab.href as Href);
    else router.navigate(tab.href as Href);
  };
}
