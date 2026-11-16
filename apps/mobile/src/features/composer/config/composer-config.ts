import type { TopicItem } from '../components/topic-selector';
import type { VisibilityOption } from '../components/visibility-selector';
import type { ToolbarAction } from '../components/toolbar-actions';
import { pickTabs } from '@/shared/navigation';

export const topics: TopicItem[] = [
  { id: 'workout', label: 'Tập luyện', icon: 'dumbbell' },
  { id: 'meal-prep', label: 'Meal prep', icon: 'meal' },
  { id: 'progress', label: 'Tiến độ', icon: 'progress' },
  { id: 'tips', label: 'Mẹo hay', icon: 'tip' },
];

export const visibilityOptions: VisibilityOption[] = [
  {
    id: 'public',
    label: 'Công khai',
    description: 'Tất cả mọi người trên GymBase đều có thể xem.',
    icon: 'globe',
  },
  {
    id: 'followers',
    label: 'Chỉ người theo dõi',
    description: 'Chỉ những người theo dõi bạn mới có thể xem.',
    icon: 'followers',
  },
];

/** Cấu hình thanh công cụ (không kèm hành vi; hành vi gắn theo id ở màn hình). */
export const toolbarDefinitions: Omit<ToolbarAction, 'onPress'>[] = [
  { id: 'add-media', label: 'Thêm ảnh', icon: 'image' },
  { id: 'tag-trainer', label: 'Gắn PT', icon: 'person' },
  { id: 'add-hashtag', label: 'Thêm hashtag', icon: 'hashtag' },
];

export const composerTabs = pickTabs(['home', 'route', 'compose', 'messages', 'profile']);

