import { memo, useCallback, useMemo, useRef, useState, type ComponentRef } from 'react';
import {
  ActivityIndicator,
  KeyboardAvoidingView,
  Modal,
  Platform,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';
import { useOptionalSession } from '@/core/auth/session';
import { Icon } from '@/shared/icons';
import { theme } from '@/shared/theme/tokens';
import { RemoteImage } from '@/shared/ui-kit';
import {
  formatRelativeTimeVi,
  type BinhLuanDto,
} from '../api/feed-api';
import {
  useAddCommentMutation,
  usePostComments,
} from '../hooks/use-post-comments';
import type { FeedPostData } from '../types';

export interface CommentBottomSheetProps {
  visible: boolean;
  post: FeedPostData | null;
  onClose: () => void;
  currentUserAvatar?: string;
}

type SortFilter = 'relevant' | 'newest';

function formatStatsCount(count?: number): string {
  if (!count || count <= 0) return '0';
  if (count >= 1000) {
    const k = (count / 1000).toFixed(1).replace('.0', '');
    return `${k}K`;
  }
  return count.toLocaleString('vi-VN');
}

interface CommentItemProps {
  comment: BinhLuanDto;
  isReply?: boolean;
  onReplyPress: (comment: BinhLuanDto) => void;
}

const CommentItem = memo(function CommentItem({
  comment,
  isReply = false,
  onReplyPress,
}: CommentItemProps) {
  const [liked, setLiked] = useState(false);
  const [disliked, setDisliked] = useState(false);
  const [likeCount, setLikeCount] = useState(0);

  const handleToggleLike = useCallback(() => {
    setLiked((prev) => {
      const next = !prev;
      setLikeCount((c) => (next ? c + 1 : Math.max(0, c - 1)));
      if (next) setDisliked(false);
      return next;
    });
  }, []);

  const handleToggleDislike = useCallback(() => {
    setDisliked((prev) => {
      const next = !prev;
      if (next && liked) {
        setLiked(false);
        setLikeCount((c) => Math.max(0, c - 1));
      }
      return next;
    });
  }, [liked]);

  return (
    <View
      testID={`comment-item-${comment.id}`}
      style={[styles.commentRow, isReply && styles.replyRow]}
    >
      <RemoteImage
        uri={comment.anh_dai_dien_url ?? undefined}
        circle
        style={isReply ? styles.replyAvatar : styles.commentAvatar}
      />
      <View style={styles.commentBody}>
        {/* Hàng tên & thời gian & nút tùy chọn */}
        <View style={styles.commentHeader}>
          <Text style={styles.authorName} numberOfLines={1}>
            {comment.ten_nguoi_dung}
          </Text>
          <Text style={styles.timeDot}> · </Text>
          <Text style={styles.timeLabel}>
            {formatRelativeTimeVi(comment.ngay_tao)}
          </Text>
          <View style={styles.spacer} />
          <Pressable
            accessibilityRole="button"
            accessibilityLabel="Tùy chọn bình luận"
            hitSlop={8}
            style={styles.moreButton}
          >
            <Icon name="more" size={theme.iconSize.sm} color={theme.colors.muted} />
          </Pressable>
        </View>

        {/* Nội dung bình luận */}
        <Text style={styles.commentContent}>{comment.noi_dung}</Text>

        {/* Hàng action tương tác bên dưới comment */}
        <View style={styles.commentActions}>
          <Pressable
            accessibilityRole="button"
            accessibilityLabel="Thích bình luận"
            onPress={handleToggleLike}
            hitSlop={6}
            style={styles.commentActionBtn}
          >
            <Icon
              name={liked ? 'thumbs-up-filled' : 'thumbs-up'}
              size={theme.iconSize.xs + 2}
              color={liked ? theme.colors.primary : theme.colors.muted}
            />
          </Pressable>

          <Pressable
            accessibilityRole="button"
            accessibilityLabel="Không thích bình luận"
            onPress={handleToggleDislike}
            hitSlop={6}
            style={styles.commentActionBtn}
          >
            <Icon
              name={disliked ? 'thumbs-down-filled' : 'thumbs-down'}
              size={theme.iconSize.xs + 2}
              color={disliked ? theme.colors.text : theme.colors.muted}
            />
          </Pressable>

          <Pressable
            testID={`reply-btn-${comment.id}`}
            accessibilityRole="button"
            accessibilityLabel="Trả lời bình luận"
            onPress={() => onReplyPress(comment)}
            hitSlop={6}
            style={styles.commentActionBtn}
          >
            <Text style={styles.replyButtonText}>Trả lời</Text>
          </Pressable>

          {likeCount > 0 ? (
            <View style={styles.reactionBadge}>
              <View style={styles.thumbMiniBadge}>
                <Icon name="thumbs-up-filled" size={10} color="#FFFFFF" />
              </View>
              <Text style={styles.reactionCountText}>{likeCount}</Text>
            </View>
          ) : null}
        </View>
      </View>
    </View>
  );
});

export function CommentBottomSheet({
  visible,
  post,
  onClose,
  currentUserAvatar,
}: CommentBottomSheetProps) {
  const session = useOptionalSession();
  const user = session?.user;
  const inputRef = useRef<ComponentRef<typeof TextInput>>(null);

  const [filter, setFilter] = useState<SortFilter>('relevant');
  const [inputText, setInputText] = useState('');
  const [replyingTo, setReplyingTo] = useState<BinhLuanDto | null>(null);

  const postId = post?.id;
  const { data: comments = [], isLoading } = usePostComments(visible ? postId : undefined);
  const { mutate: addComment, isPending: isSubmitting } = useAddCommentMutation(postId ?? '');

  // Sắp xếp bình luận
  const sortedComments = useMemo(() => {
    const list = [...comments];
    if (filter === 'newest') {
      list.sort((a, b) => new Date(b.ngay_tao).getTime() - new Date(a.ngay_tao).getTime());
    }
    return list;
  }, [comments, filter]);

  // Phân tách bình luận gốc và câu trả lời
  const groupedComments = useMemo(() => {
    const rootComments: BinhLuanDto[] = [];
    const repliesMap = new Map<string, BinhLuanDto[]>();

    for (const c of sortedComments) {
      if (c.tuong_tac_cha_id) {
        const existing = repliesMap.get(c.tuong_tac_cha_id) ?? [];
        existing.push(c);
        repliesMap.set(c.tuong_tac_cha_id, existing);
      } else {
        rootComments.push(c);
      }
    }
    return { rootComments, repliesMap };
  }, [sortedComments]);

  const handleToggleFilter = useCallback(() => {
    setFilter((prev) => (prev === 'relevant' ? 'newest' : 'relevant'));
  }, []);

  const handleReplyPress = useCallback((comment: BinhLuanDto) => {
    setReplyingTo(comment);
    try {
      inputRef.current?.focus();
    } catch {
      // bỏ qua lỗi focus trong test/môi trường không hỗ trợ
    }
  }, []);

  const handleCancelReply = useCallback(() => {
    setReplyingTo(null);
  }, []);

  const handleSendComment = useCallback(() => {
    const trimmed = inputText.trim();
    if (!trimmed || !postId || isSubmitting) return;

    addComment(
      {
        noi_dung: trimmed,
        tuong_tac_cha_id: replyingTo?.id ?? null,
      },
      {
        onSuccess: () => {
          setInputText('');
          setReplyingTo(null);
        },
      },
    );
  }, [addComment, inputText, isSubmitting, postId, replyingTo]);

  if (!visible || !post) return null;

  const totalLikes = post.counts?.like ?? 0;
  const totalComments = comments.length > 0 ? comments.length : (post.counts?.comment ?? 0);
  const userAvatarUri = currentUserAvatar ?? (user as unknown as { anh_dai_dien_url?: string })?.anh_dai_dien_url;

  return (
    <Modal
      testID="comment-bottom-sheet-modal"
      visible={visible}
      transparent
      animationType="slide"
      onRequestClose={onClose}
    >
      <View testID="comment-sheet-container" style={styles.modalOverlay}>
        <Pressable
          testID="comment-modal-backdrop"
          style={styles.backdrop}
          onPress={onClose}
        />

        <KeyboardAvoidingView
          behavior={Platform.OS === 'ios' ? 'padding' : undefined}
          style={styles.sheetContainer}
        >
          {/* Thanh kéo handle bar */}
          <View style={styles.handleBarContainer}>
            <View style={styles.handleBar} />
          </View>

          {/* Header Modal */}
          <View style={styles.header}>
            <Text style={styles.headerTitle}>Bình luận</Text>
            <Pressable
              testID="comment-modal-close-button"
              accessibilityRole="button"
              accessibilityLabel="Đóng bình luận"
              onPress={onClose}
              hitSlop={8}
              style={styles.closeButton}
            >
              <Icon name="close" size={theme.iconSize.sm + 2} color={theme.colors.text} />
            </Pressable>
          </View>

          {/* 1. Hàng thống kê tương tác: Like (4,2K), bình luận (182), chia sẻ (6) + reaction icon cụm tròn */}
          <View style={styles.statsBar}>
            <View style={styles.statsLeft}>
              <View style={styles.statItem}>
                <Icon name="thumbs-up-filled" size={theme.iconSize.xs + 2} color={theme.colors.muted} />
                <Text style={styles.statText}>{formatStatsCount(totalLikes)}</Text>
              </View>
              <View style={styles.statItem}>
                <Icon name="comment" size={theme.iconSize.xs + 2} color={theme.colors.muted} />
                <Text style={styles.statText}>{formatStatsCount(totalComments)}</Text>
              </View>
              <View style={styles.statItem}>
                <Icon name="send" size={theme.iconSize.xs + 2} color={theme.colors.muted} />
                <Text style={styles.statText}>6</Text>
              </View>
            </View>

            {/* Cụm icon reaction xếp chồng giống trong ảnh: Like xanh + Tim đỏ */}
            <View style={styles.reactionIconsGroup}>
              <View style={[styles.reactionBubble, styles.bubbleBlue]}>
                <Icon name="thumbs-up-filled" size={11} color="#FFFFFF" />
              </View>
              <View style={[styles.reactionBubble, styles.bubbleRed]}>
                <Icon name="heart-filled" size={11} color="#FFFFFF" />
              </View>
            </View>
          </View>

          {/* 2. Bộ lọc: Phù hợp nhất ▼ / Mới nhất ▼ */}
          <View style={styles.filterBar}>
            <Pressable
              accessibilityRole="button"
              accessibilityLabel="Chọn bộ lọc bình luận"
              onPress={handleToggleFilter}
              style={styles.filterButton}
            >
              <Text style={styles.filterText}>
                {filter === 'relevant' ? 'Phù hợp nhất' : 'Mới nhất'}
              </Text>
              <Icon name="chevron-down" size={theme.iconSize.xs} color={theme.colors.text} />
            </Pressable>
          </View>

          {/* 3. Danh sách bình luận */}
          <ScrollView
            testID="comment-list-scrollview"
            style={styles.listContainer}
            contentContainerStyle={styles.listContent}
            showsVerticalScrollIndicator={false}
            keyboardShouldPersistTaps="handled"
          >
            {isLoading && comments.length === 0 ? (
              <View style={styles.centered}>
                <ActivityIndicator size="small" color={theme.colors.primary} />
                <Text style={styles.loadingText}>Đang tải bình luận...</Text>
              </View>
            ) : groupedComments.rootComments.length === 0 ? (
              <View style={styles.emptyContainer}>
                <Text style={styles.emptyText}>Chưa có bình luận nào.</Text>
                <Text style={styles.emptySubText}>Hãy là người đầu tiên chia sẻ suy nghĩ của bạn!</Text>
              </View>
            ) : (
              groupedComments.rootComments.map((rootComment) => {
                const replies = groupedComments.repliesMap.get(rootComment.id) ?? [];
                return (
                  <View key={rootComment.id} style={styles.commentBlock}>
                    <CommentItem
                      comment={rootComment}
                      onReplyPress={handleReplyPress}
                    />
                    {replies.map((reply) => (
                      <CommentItem
                        key={reply.id}
                        comment={reply}
                        isReply
                        onReplyPress={handleReplyPress}
                      />
                    ))}
                  </View>
                );
              })
            )}
          </ScrollView>

          {/* 4. Thanh đang trả lời (nếu có) */}
          {replyingTo ? (
            <View testID="replying-bar" style={styles.replyingBar}>
              <Text style={styles.replyingText} numberOfLines={1}>
                Đang trả lời <Text style={styles.replyingName}>@{replyingTo.ten_nguoi_dung}</Text>
              </Text>
              <Pressable
                accessibilityRole="button"
                accessibilityLabel="Hủy trả lời"
                onPress={handleCancelReply}
                hitSlop={6}
              >
                <Icon name="close" size={theme.iconSize.xs} color={theme.colors.muted} />
              </Pressable>
            </View>
          ) : null}

          {/* 5. Ô nhập bình luận cố định ở đáy */}
          <View style={styles.inputContainer}>
            <RemoteImage
              uri={userAvatarUri}
              circle
              style={styles.inputAvatar}
            />

            <View style={styles.inputBox}>
              <TextInput
                ref={inputRef}
                testID="comment-text-input"
                style={styles.textInput}
                placeholder="Viết bình luận..."
                placeholderTextColor={theme.colors.muted}
                value={inputText}
                onChangeText={setInputText}
                multiline
                maxLength={500}
              />

              {/* Hàng icon tiện ích và nút gửi */}
              <View style={styles.inputToolbar}>
                <View style={styles.toolbarIcons}>
                  <Pressable hitSlop={6} style={styles.toolIconBtn}>
                    <Icon name="sparkles" size={theme.iconSize.sm} color={theme.colors.muted} />
                  </Pressable>
                  <Pressable hitSlop={6} style={styles.toolIconBtn}>
                    <Icon name="happy" size={theme.iconSize.sm} color={theme.colors.muted} />
                  </Pressable>
                  <Pressable hitSlop={6} style={styles.toolIconBtn}>
                    <Icon name="camera" size={theme.iconSize.sm} color={theme.colors.muted} />
                  </Pressable>
                </View>

                <Pressable
                  testID="comment-submit-button"
                  accessibilityRole="button"
                  accessibilityLabel="Gửi bình luận"
                  onPress={handleSendComment}
                  disabled={!inputText.trim() || isSubmitting}
                  hitSlop={8}
                  style={[
                    styles.sendButton,
                    Boolean(inputText.trim()) && styles.sendButtonActive,
                  ]}
                >
                  {isSubmitting ? (
                    <ActivityIndicator size="small" color="#FFFFFF" />
                  ) : (
                    <Icon
                      name="send-filled"
                      size={theme.iconSize.sm}
                      color={inputText.trim() ? theme.colors.primary : theme.colors.muted}
                    />
                  )}
                </Pressable>
              </View>
            </View>
          </View>
        </KeyboardAvoidingView>
      </View>
    </Modal>
  );
}

const styles = StyleSheet.create({
  modalOverlay: {
    flex: 1,
    backgroundColor: theme.colors.overlay,
    justifyContent: 'flex-end',
  },
  backdrop: {
    ...StyleSheet.absoluteFill,
  },
  sheetContainer: {
    backgroundColor: theme.colors.card,
    borderTopLeftRadius: theme.radius.card,
    borderTopRightRadius: theme.radius.card,
    maxHeight: '85%',
    height: '80%',
    paddingBottom: Platform.OS === 'ios' ? 24 : 12,
  },
  handleBarContainer: {
    alignItems: 'center',
    paddingVertical: theme.spacing.xs + 2,
  },
  handleBar: {
    width: 36,
    height: 4,
    borderRadius: theme.radius.chip,
    backgroundColor: theme.colors.border,
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: theme.spacing.md,
    paddingBottom: theme.spacing.xs,
  },
  headerTitle: {
    fontSize: theme.font.size.md,
    fontWeight: theme.font.weight.bold,
    color: theme.colors.text,
  },
  closeButton: {
    padding: theme.spacing.xs,
  },
  // Hàng thống kê: Like, Comment, Share & Cụm bubble reaction
  statsBar: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: theme.spacing.md,
    paddingVertical: theme.spacing.xs + 2,
    borderBottomWidth: 1,
    borderBottomColor: theme.colors.border,
  },
  statsLeft: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: theme.spacing.lg,
  },
  statItem: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: theme.spacing.xs + 2,
  },
  statText: {
    fontSize: theme.font.size.xs,
    color: theme.colors.muted,
    fontWeight: theme.font.weight.medium,
  },
  reactionIconsGroup: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  reactionBubble: {
    width: 20,
    height: 20,
    borderRadius: 10,
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 1.5,
    borderColor: theme.colors.card,
  },
  bubbleBlue: {
    backgroundColor: '#1877F2',
    zIndex: 2,
  },
  bubbleRed: {
    backgroundColor: '#FA383E',
    marginLeft: -6,
    zIndex: 1,
  },
  // Bộ lọc Phù hợp nhất
  filterBar: {
    paddingHorizontal: theme.spacing.md,
    paddingVertical: theme.spacing.xs + 2,
  },
  filterButton: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: theme.spacing.xs,
    alignSelf: 'flex-start',
  },
  filterText: {
    fontSize: theme.font.size.sm,
    fontWeight: theme.font.weight.bold,
    color: theme.colors.text,
  },
  // Danh sách bình luận
  listContainer: {
    flex: 1,
  },
  listContent: {
    paddingHorizontal: theme.spacing.md,
    paddingVertical: theme.spacing.xs,
    gap: theme.spacing.md,
  },
  commentBlock: {
    gap: theme.spacing.sm,
  },
  commentRow: {
    flexDirection: 'row',
    gap: theme.spacing.sm,
    alignItems: 'flex-start',
  },
  replyRow: {
    paddingLeft: 38,
  },
  commentAvatar: {
    width: 36,
    height: 36,
    borderRadius: 18,
    marginTop: 2,
  },
  replyAvatar: {
    width: 28,
    height: 28,
    borderRadius: 14,
    marginTop: 2,
  },
  commentBody: {
    flex: 1,
  },
  commentHeader: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  authorName: {
    fontSize: theme.font.size.sm,
    fontWeight: theme.font.weight.bold,
    color: theme.colors.text,
  },
  timeDot: {
    fontSize: theme.font.size.xs,
    color: theme.colors.muted,
  },
  timeLabel: {
    fontSize: theme.font.size.xs,
    color: theme.colors.muted,
  },
  spacer: {
    flex: 1,
  },
  moreButton: {
    padding: 2,
  },
  commentContent: {
    fontSize: theme.font.size.sm,
    lineHeight: 20,
    color: theme.colors.text,
    marginTop: 2,
  },
  commentActions: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: theme.spacing.md,
    marginTop: theme.spacing.xs + 2,
  },
  commentActionBtn: {
    paddingVertical: 2,
    paddingHorizontal: 2,
  },
  replyButtonText: {
    fontSize: theme.font.size.xs,
    fontWeight: theme.font.weight.medium,
    color: theme.colors.muted,
  },
  reactionBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 3,
    marginLeft: 'auto',
  },
  thumbMiniBadge: {
    width: 16,
    height: 16,
    borderRadius: 8,
    backgroundColor: '#1877F2',
    justifyContent: 'center',
    alignItems: 'center',
  },
  reactionCountText: {
    fontSize: theme.font.size.xs - 1,
    color: theme.colors.muted,
    fontWeight: theme.font.weight.medium,
  },
  // Ô nhập bình luận cố định ở đáy
  replyingBar: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: theme.spacing.md,
    paddingVertical: theme.spacing.xs,
    backgroundColor: theme.colors.surfaceMuted,
    borderTopWidth: 1,
    borderTopColor: theme.colors.border,
  },
  replyingText: {
    fontSize: theme.font.size.xs,
    color: theme.colors.muted,
  },
  replyingName: {
    fontWeight: theme.font.weight.bold,
    color: theme.colors.primary,
  },
  inputContainer: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    gap: theme.spacing.sm,
    paddingHorizontal: theme.spacing.md,
    paddingTop: theme.spacing.xs + 4,
    borderTopWidth: 1,
    borderTopColor: theme.colors.border,
    backgroundColor: theme.colors.card,
  },
  inputAvatar: {
    width: 36,
    height: 36,
    borderRadius: 18,
    marginTop: 4,
  },
  inputBox: {
    flex: 1,
    backgroundColor: theme.colors.surfaceMuted,
    borderRadius: 16,
    borderWidth: 1,
    borderColor: theme.colors.border,
    paddingHorizontal: theme.spacing.sm + 2,
    paddingVertical: theme.spacing.xs + 2,
  },
  textInput: {
    fontSize: theme.font.size.sm,
    color: theme.colors.text,
    minHeight: 36,
    maxHeight: 100,
    paddingTop: 0,
    paddingBottom: 4,
    textAlignVertical: 'top',
  },
  inputToolbar: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingTop: 4,
  },
  toolbarIcons: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: theme.spacing.sm + 2,
  },
  toolIconBtn: {
    padding: 2,
  },
  sendButton: {
    padding: 4,
  },
  sendButtonActive: {
    opacity: 1,
  },
  centered: {
    alignItems: 'center',
    justifyContent: 'center',
    paddingVertical: theme.spacing.xl,
    gap: theme.spacing.xs,
  },
  loadingText: {
    fontSize: theme.font.size.xs,
    color: theme.colors.muted,
  },
  emptyContainer: {
    alignItems: 'center',
    justifyContent: 'center',
    paddingVertical: theme.spacing.xl,
    gap: theme.spacing.xs,
  },
  emptyText: {
    fontSize: theme.font.size.sm,
    fontWeight: theme.font.weight.bold,
    color: theme.colors.text,
  },
  emptySubText: {
    fontSize: theme.font.size.xs,
    color: theme.colors.muted,
    textAlign: 'center',
  },
});
