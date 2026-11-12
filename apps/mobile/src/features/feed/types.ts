export interface FeedPostData {
  id: string;
  author: { name: string; avatarUri: string };
  timeLabel: string;
  content: string;
  imageUri?: string;
  topicId?: string;
  /** Số đếm theo id action (like, comment...). */
  counts?: Record<string, number>;
  /** Trạng thái bật ban đầu theo id action (đã thích, đã lưu...). */
  initialActive?: Record<string, boolean>;
}

/** Mẫu cấu hình một action trên PostCard; thêm action mới = thêm một object. */
export interface PostActionTemplate {
  id: string;
  icon: string;
  activeIcon?: string;
  /** Có thể bật/tắt (tim, bookmark). */
  toggle?: boolean;
  /** Hiện số lấy từ `post.counts[id]`. */
  showCount?: boolean;
  /** Căn phải hàng action. */
  align?: 'start' | 'end';
}
