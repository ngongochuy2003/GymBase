import { useCallback, useState } from 'react';
import {
  ActivityIndicator,
  Modal,
  Pressable,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { Icon } from '@/shared/icons';
import { theme } from '@/shared/theme/tokens';
import type { FeedPostData, ReportReasonId } from '../types';

type SheetStep = 'main' | 'more' | 'report';

const REPORT_REASONS: { id: ReportReasonId; label: string }[] = [
  { id: 'spam', label: 'Tin rác / Spam' },
  { id: 'gia_mao', label: 'Thông tin sai sự thật / Giả mạo' },
  { id: 'quay_roi', label: 'Quấy rối hoặc xúc phạm' },
  { id: 'noi_dung_xau', label: 'Nội dung không phù hợp' },
  { id: 'khac', label: 'Lý do khác' },
];

export interface PostOptionsSheetProps {
  post: FeedPostData | null;
  isSaved: boolean;
  isReporting?: boolean;
  onClose(): void;
  onInterested(post: FeedPostData): void;
  onNotInterested(post: FeedPostData): void;
  onToggleSave(post: FeedPostData): void;
  onReport(post: FeedPostData, reason: ReportReasonId): void;
  onCopyLink(post: FeedPostData): void;
}

export function PostOptionsSheet({
  post,
  isSaved,
  isReporting = false,
  onClose,
  onInterested,
  onNotInterested,
  onToggleSave,
  onReport,
  onCopyLink,
}: PostOptionsSheetProps) {
  const [step, setStep] = useState<SheetStep>('main');

  const handleClose = useCallback(() => {
    setStep('main');
    onClose();
  }, [onClose]);

  if (!post) {
    return null;
  }

  return (
    <Modal
      visible={Boolean(post)}
      transparent
      animationType="slide"
      onRequestClose={handleClose}
    >
      <View style={styles.overlay}>
        <Pressable
          testID="options-sheet-backdrop"
          style={styles.backdrop}
          onPress={handleClose}
        />
        <View style={styles.sheet}>
          <View style={styles.handleBar} />

          {step === 'main' ? (
            <View style={styles.groups}>
              <View style={styles.groupCard}>
                <Pressable
                  testID="option-interested"
                  accessibilityRole="button"
                  style={styles.row}
                  onPress={() => {
                    onInterested(post);
                    handleClose();
                  }}
                >
                  <Icon name="plus-circle" size={theme.iconSize.md} color={theme.colors.text} />
                  <Text style={styles.rowText}>Quan tâm</Text>
                </Pressable>
                <View style={styles.rowDivider} />
                <Pressable
                  testID="option-not-interested"
                  accessibilityRole="button"
                  style={styles.row}
                  onPress={() => {
                    onNotInterested(post);
                    handleClose();
                  }}
                >
                  <Icon name="minus-circle" size={theme.iconSize.md} color={theme.colors.text} />
                  <Text style={styles.rowText}>Không quan tâm</Text>
                </Pressable>
              </View>

              <View style={styles.groupCard}>
                <Pressable
                  testID="option-save-post"
                  accessibilityRole="button"
                  style={styles.row}
                  onPress={() => {
                    onToggleSave(post);
                    handleClose();
                  }}
                >
                  <Icon
                    name={isSaved ? 'bookmark-filled' : 'bookmark'}
                    size={theme.iconSize.md}
                    color={isSaved ? theme.colors.primary : theme.colors.text}
                  />
                  <Text style={styles.rowText}>
                    {isSaved ? 'Bỏ lưu bài viết' : 'Lưu bài viết'}
                  </Text>
                </Pressable>
                <View style={styles.rowDivider} />
                <Pressable
                  testID="option-report-post"
                  accessibilityRole="button"
                  style={styles.row}
                  onPress={() => setStep('report')}
                >
                  <Icon name="alert-circle" size={theme.iconSize.md} color={theme.colors.text} />
                  <Text style={styles.rowText}>Báo cáo bài viết</Text>
                </Pressable>
                <View style={styles.rowDivider} />
                <Pressable
                  testID="option-more-choices"
                  accessibilityRole="button"
                  style={styles.row}
                  onPress={() => setStep('more')}
                >
                  <Text style={[styles.rowText, styles.rowTextFlex]}>Lựa chọn khác</Text>
                  <Icon name="chevron-right" size={theme.iconSize.sm + 2} color={theme.colors.muted} />
                </Pressable>
              </View>
            </View>
          ) : null}

          {step === 'more' ? (
            <View style={styles.subView}>
              <View style={styles.subHeader}>
                <Pressable
                  testID="options-back-button"
                  accessibilityRole="button"
                  accessibilityLabel="Quay lại"
                  onPress={() => setStep('main')}
                  hitSlop={8}
                  style={styles.backButton}
                >
                  <Icon name="chevron-left" size={theme.iconSize.md + 2} color={theme.colors.text} />
                </Pressable>
                <Text style={styles.subHeaderTitle}>Lựa chọn khác</Text>
                <View style={styles.backButton} />
              </View>

              <View style={styles.groupCard}>
                <Pressable
                  testID="option-copy-link"
                  accessibilityRole="button"
                  style={styles.row}
                  onPress={() => {
                    onCopyLink(post);
                    handleClose();
                  }}
                >
                  <Icon name="copy" size={theme.iconSize.md} color={theme.colors.text} />
                  <Text style={styles.rowText}>Sao chép liên kết</Text>
                </Pressable>
              </View>
            </View>
          ) : null}

          {step === 'report' ? (
            <View style={styles.subView}>
              <View style={styles.subHeader}>
                <Pressable
                  testID="report-back-button"
                  accessibilityRole="button"
                  accessibilityLabel="Quay lại"
                  onPress={() => setStep('main')}
                  hitSlop={8}
                  style={styles.backButton}
                >
                  <Icon name="chevron-left" size={theme.iconSize.md + 2} color={theme.colors.text} />
                </Pressable>
                <Text style={styles.subHeaderTitle}>Báo cáo bài viết</Text>
                <View style={styles.backButton} />
              </View>

              <Text style={styles.reportSubtitle}>
                Hãy chọn lý do bạn muốn báo cáo bài viết này:
              </Text>

              <View style={styles.groupCard}>
                {REPORT_REASONS.map((reason, index) => (
                  <View key={reason.id}>
                    {index > 0 ? <View style={styles.rowDivider} /> : null}
                    <Pressable
                      testID={`report-reason-${reason.id}`}
                      accessibilityRole="button"
                      disabled={isReporting}
                      style={styles.row}
                      onPress={() => onReport(post, reason.id)}
                    >
                      <Text style={[styles.rowText, styles.rowTextFlex]}>{reason.label}</Text>
                      {isReporting ? (
                        <ActivityIndicator size="small" color={theme.colors.primary} />
                      ) : (
                        <Icon
                          name="chevron-right"
                          size={theme.iconSize.sm + 2}
                          color={theme.colors.muted}
                        />
                      )}
                    </Pressable>
                  </View>
                ))}
              </View>
            </View>
          ) : null}
        </View>
      </View>
    </Modal>
  );
}

const styles = StyleSheet.create({
  overlay: {
    flex: 1,
    justifyContent: 'flex-end',
    backgroundColor: theme.colors.overlay,
  },
  backdrop: {
    flex: 1,
  },
  sheet: {
    backgroundColor: theme.colors.background,
    borderTopLeftRadius: theme.radius.card,
    borderTopRightRadius: theme.radius.card,
    paddingHorizontal: theme.spacing.lg,
    paddingTop: theme.spacing.sm,
    paddingBottom: theme.spacing.xl + theme.spacing.sm,
    gap: theme.spacing.md,
  },
  handleBar: {
    width: 40,
    height: 4,
    borderRadius: 2,
    backgroundColor: theme.colors.placeholder,
    alignSelf: 'center',
    marginBottom: theme.spacing.xs,
  },
  groups: {
    gap: theme.spacing.md,
  },
  subView: {
    gap: theme.spacing.md,
  },
  subHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingVertical: theme.spacing.xs,
  },
  backButton: {
    width: 32,
    alignItems: 'flex-start',
    justifyContent: 'center',
  },
  subHeaderTitle: {
    fontSize: theme.font.size.lg,
    fontWeight: theme.font.weight.bold,
    color: theme.colors.text,
    textAlign: 'center',
  },
  reportSubtitle: {
    fontSize: theme.font.size.sm,
    color: theme.colors.muted,
    paddingHorizontal: theme.spacing.xs,
  },
  groupCard: {
    backgroundColor: theme.colors.card,
    borderRadius: 16,
    overflow: 'hidden',
    borderWidth: 1,
    borderColor: theme.colors.border,
  },
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: theme.spacing.md,
    paddingHorizontal: theme.spacing.lg,
    paddingVertical: theme.spacing.md + 2,
  },
  rowDivider: {
    height: StyleSheet.hairlineWidth,
    backgroundColor: theme.colors.border,
    marginLeft: theme.spacing.lg,
  },
  rowText: {
    fontSize: theme.font.size.md,
    fontWeight: theme.font.weight.medium,
    color: theme.colors.text,
  },
  rowTextFlex: {
    flex: 1,
  },
});
