import type { FeedPostData } from '../types';

export const feedPosts: FeedPostData[] = [
  {
    id: 'post-1',
    author: {
      name: 'Minh Khôi',
      avatarUri: 'https://picsum.photos/seed/gymbase-minh-khoi/160/160',
    },
    timeLabel: '6 giờ trước',
    content: 'Buổi Push hôm nay.\nTập trung ngực – vai – tay sau, cảm giác rất ổn!',
    imageUri: 'https://picsum.photos/seed/gymbase-push/800/500',
    counts: { like: 1248, comment: 86 },
  },
  {
    id: 'post-2',
    author: { name: 'Lan Anh', avatarUri: 'https://picsum.photos/seed/gymbase-lan-anh/160/160' },
    timeLabel: '10 giờ trước',
    content: 'Meal prep đơn giản cho 1 ngày siết mỡ.',
    imageUri: 'https://picsum.photos/seed/gymbase-mealprep/800/500',
    counts: { like: 562, comment: 24 },
  },
];
