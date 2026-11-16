import type { FilterChipItem } from '@/shared/ui-kit';
import type { PostActionTemplate } from '../types';

export const feedFilters: FilterChipItem[] = [
  { id: 'for-you', label: 'Dành cho bạn', icon: 'home' },
  { id: 'trending', label: 'Xu hướng', icon: 'flame' },
  { id: 'workout', label: 'Tập luyện', icon: 'dumbbell' },
  { id: 'knowledge', label: 'Kiến thức', icon: 'book' },
];

export const postActionTemplates: PostActionTemplate[] = [
  { id: 'like', icon: 'heart', activeIcon: 'heart-filled', toggle: true, showCount: true },
  { id: 'comment', icon: 'comment', showCount: true },
  { id: 'share', icon: 'send' },
  { id: 'save', icon: 'bookmark', activeIcon: 'bookmark-filled', toggle: true, align: 'end' },
];
