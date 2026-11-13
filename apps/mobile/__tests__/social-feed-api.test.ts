import type { AxiosInstance } from 'axios';
import {
  createFeedRepository,
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
        anh_dai_dien_url: 'https://cdn.test/avatar.jpg',
      },
      noi_dung: 'Buổi Push hôm nay.',
      loai_bai_viet: 'hon_hop',
      che_do_hien_thi: 'cong_khai',
      trang_thai: 'da_dang',
      chu_de: 'workout',
      hinh_anh_urls: ['https://cdn.test/push.jpg'],
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
    expect(post.imageUri).toBe('https://cdn.test/push.jpg');
    expect(post.counts?.like).toBe(12);
    expect(post.counts?.comment).toBe(3);
    expect(post.initialActive?.like).toBe(true);
  });


  test('toBaiVietCreateDto maps composer payload to backend snake_case DTO', () => {
    const dto = toBaiVietCreateDto(
      {
        content: 'Hôm nay tập chân #tapluyen',
        mediaUris: ['https://cdn.test/post.jpg'],
        topicIds: ['workout'],
        visibilityId: 'followers',
      },
    );

    expect(dto).toEqual({
      noi_dung: 'Hôm nay tập chân #tapluyen',
      che_do_hien_thi: 'nguoi_theo_doi',
      chu_de: 'workout',
      hinh_anh_urls: ['https://cdn.test/post.jpg'],
    });
  });

  test('repositories use only API responses and do not substitute demo data', async () => {
    const createdDto = { id: 'post-real-1', noi_dung: 'Bài viết mới', che_do_hien_thi: 'cong_khai' };
    const mockClient = {
      get: jest.fn().mockResolvedValue({ data: { data: [], meta: {} } }),
      post: jest.fn().mockResolvedValue({ data: createdDto }),
    } as unknown as AxiosInstance;

    const feedRepo = createFeedRepository(mockClient);
    expect(await feedRepo.getFeedPosts('for-you')).toEqual([]);

    const composerRepo = createComposerRepository(mockClient);
    const created = await composerRepo.createPost({
      content: 'Bài viết mới', mediaUris: [], topicIds: ['workout'], visibilityId: 'public',
    });
    expect(created).toEqual(createdDto);
  });
});
