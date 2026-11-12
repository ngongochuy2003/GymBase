import type { AxiosInstance } from 'axios';
import {
  createFeedRepository,
  filterFeedPostsByTab,
  formatRelativeTimeVi,
  mapBaiVietDtoToFeedPost,
  type BaiVietDto,
} from '@/features/feed';
import { createComposerRepository, toBaiVietCreateDto } from '@/features/composer';

describe('social-feed-api & composer-api', () => {
  test('formatRelativeTimeVi formats relative Vietnamese labels', () => {
    const now = new Date('2026-11-11T16:00:00Z');
    expect(formatRelativeTimeVi('2026-11-11T15:59:30Z', now)).toBe('Vừa xong');
    expect(formatRelativeTimeVi('2026-11-11T15:30:00Z', now)).toBe('30 phút trước');
    expect(formatRelativeTimeVi('2026-11-11T10:00:00Z', now)).toBe('6 giờ trước');
    expect(formatRelativeTimeVi('2026-11-09T10:00:00Z', now)).toBe('2 ngày trước');
  });

  test('mapBaiVietDtoToFeedPost maps backend DTO into FeedPostData', () => {
    const now = new Date('2026-11-11T16:00:00Z');
    const dto: BaiVietDto = {
      id: '0193f0c0-0000-7000-8000-000000000001',
      tac_gia_id: '0193f0a0-0000-7000-8000-000000000002',
      tac_gia: {
        id: '0193f0a0-0000-7000-8000-000000000002',
        ten_hien_thi: 'Minh Khôi',
        anh_dai_dien_url: 'https://picsum.photos/seed/gymbase-minh-khoi/160/160',
      },
      noi_dung: 'Buổi Push hôm nay.',
      loai_bai_viet: 'hon_hop',
      che_do_hien_thi: 'cong_khai',
      trang_thai: 'da_dang',
      chu_de: 'workout',
      hinh_anh_urls: ['https://picsum.photos/seed/gymbase-push/800/500'],
      so_luot_thich: 12,
      so_binh_luan: 3,
      da_thich: true,
      ngay_dang: '2026-11-11T10:00:00Z',
      ngay_tao: '2026-11-11T10:00:00Z',
      ngay_chinh_sua: null,
    };

    const post = mapBaiVietDtoToFeedPost(dto, now);
    expect(post.id).toBe(dto.id);
    expect(post.author.name).toBe('Minh Khôi');
    expect(post.timeLabel).toBe('6 giờ trước');
    expect(post.imageUri).toBe('https://picsum.photos/seed/gymbase-push/800/500');
    expect(post.counts?.like).toBe(12);
    expect(post.counts?.comment).toBe(3);
    expect(post.initialActive?.like).toBe(true);
  });

  test('filterFeedPostsByTab filters workout and knowledge tabs', () => {
    const sample = [
      {
        id: '1',
        author: { name: 'A', avatarUri: '' },
        timeLabel: '1 giờ trước',
        content: 'Buổi Push hôm nay',
        topicId: 'workout',
        counts: { like: 10, comment: 2 },
      },
      {
        id: '2',
        author: { name: 'B', avatarUri: '' },
        timeLabel: '2 giờ trước',
        content: 'Meal prep đơn giản cho 1 ngày siết mỡ',
        topicId: 'meal-prep',
        counts: { like: 50, comment: 5 },
      },
    ];

    expect(filterFeedPostsByTab(sample, 'workout')).toHaveLength(1);
    expect(filterFeedPostsByTab(sample, 'knowledge')).toHaveLength(1);
    expect(filterFeedPostsByTab(sample, 'trending')[0]?.id).toBe('2');
  });

  test('toBaiVietCreateDto maps composer payload to backend snake_case DTO', () => {
    const dto = toBaiVietCreateDto(
      {
        content: 'Hôm nay tập chân #tapluyen',
        mediaUris: ['https://picsum.photos/seed/a/600/450'],
        topicIds: ['workout'],
        visibilityId: 'followers',
      },
      'Minh Khôi',
    );

    expect(dto).toEqual({
      noi_dung: 'Hôm nay tập chân #tapluyen',
      che_do_hien_thi: 'nguoi_theo_doi',
      chu_de: 'workout',
      hinh_anh_urls: ['https://picsum.photos/seed/a/600/450'],
      ten_tac_gia: 'Minh Khôi',
    });
  });

  test('repositories return fallback data cleanly in test mode', async () => {
    const mockClient = {
      get: jest.fn(),
      post: jest.fn(),
    } as unknown as AxiosInstance;

    const feedRepo = createFeedRepository(mockClient);
    const posts = await feedRepo.getFeedPosts('for-you');
    expect(posts.length).toBeGreaterThanOrEqual(2);

    const composerRepo = createComposerRepository(mockClient);
    const created = await composerRepo.createPost(
      {
        content: 'Bài viết mới',
        mediaUris: [],
        topicIds: ['workout'],
        visibilityId: 'public',
      },
      'Minh Khôi',
    );
    expect(created.noi_dung).toBe('Bài viết mới');
    expect(created.che_do_hien_thi).toBe('cong_khai');
  });
});
