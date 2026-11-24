import { useCallback, useMemo } from 'react';
import { useRouter, type Href } from 'expo-router';
import { useThongBaoHeader } from '../notifications';
import type { HeaderAction, TabItem } from '../ui-kit';

/** Toàn bộ tab của ứng dụng; mỗi màn hình chọn tập con và thứ tự bằng `pickTabs`. */
export const MAIN_TABS: Record<string, TabItem> = {
  home: { id: 'home', label: 'Trang chủ', icon: 'home', activeIcon: 'home-filled', href: '/home' },
  route: { id: 'route', label: 'Lộ trình', icon: 'route', href: '/route' },
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
  { id: 'notifications', icon: 'bell', label: 'Thông báo' },
];

/**
 * Action bên phải AppHeader kèm hành vi: chuông hiện chấm khi có thông báo chưa đọc và mở màn thông báo.
 * Dùng cho `header` của ScreenConfig: `header: { ...headerActions }`.
 */
export function useHeaderActions(): { rightActions: HeaderAction[]; onActionPress(id: string): void } {
  const { coChuaDoc, moThongBao } = useThongBaoHeader();
  const rightActions = useMemo(
    () => DEFAULT_HEADER_ACTIONS.map((a) => (a.id === 'notifications' ? { ...a, badge: coChuaDoc } : a)),
    [coChuaDoc],
  );
  const onActionPress = useCallback(
    (id: string) => {
      if (id === 'notifications') moThongBao();
    },
    [moThongBao],
  );
  return { rightActions, onActionPress };
}

/** Điều hướng khi bấm tab; tab chưa có `href` (chưa có màn hình) thì bỏ qua. */
export function useTabNavigation(): (tab: TabItem) => void {
  const router = useRouter();
  return (tab) => {
    if (!tab.href) return;
    if (tab.id === 'compose') router.push(tab.href as Href);
    else router.navigate(tab.href as Href);
  };
}
