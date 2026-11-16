import { memo, useEffect, useMemo, useState } from 'react';
import {
  Modal,
  PanResponder,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { Icon } from '@/shared/icons';
import { theme } from '@/shared/theme/tokens';
import { RemoteImage } from '@/shared/ui-kit';

export interface PostImageViewerProps {
  visible: boolean;
  images: string[];
  initialIndex?: number;
  onClose(): void;
}

/**
 * Trình xem ảnh chi tiết toàn màn hình (Fullscreen Lightbox).
 * - Hỗ trợ zoom in / zoom out (pinch hoặc nút điều khiển).
 * - Vuốt từ phải sang trái: sang ảnh kế tiếp; nếu hết ảnh -> thoát về bài viết chi tiết.
 * - Vuốt từ trái sang phải: sang ảnh trước đó; nếu ở ảnh đầu tiên -> thoát về bài viết chi tiết.
 */
export const PostImageViewer = memo(function PostImageViewer({
  visible,
  images,
  initialIndex = 0,
  onClose,
}: PostImageViewerProps) {
  const [currentIndex, setCurrentIndex] = useState(initialIndex);
  const [zoomScale, setZoomScale] = useState(1);

  useEffect(() => {
    if (visible) {
      setCurrentIndex(Math.max(0, Math.min(initialIndex, Math.max(0, images.length - 1))));
      setZoomScale(1);
    }
  }, [initialIndex, images.length, visible]);

  const currentUri = images[currentIndex];

  const handleNext = () => {
    if (currentIndex < images.length - 1) {
      setCurrentIndex((prev) => prev + 1);
      setZoomScale(1);
    } else {
      // Hết ảnh -> thoát về chi tiết bài viết
      onClose();
    }
  };

  const handlePrev = () => {
    if (currentIndex > 0) {
      setCurrentIndex((prev) => prev - 1);
      setZoomScale(1);
    } else {
      // Ở ảnh đầu tiên vuốt lùi -> thoát về chi tiết bài viết
      onClose();
    }
  };

  const handleZoomIn = () => {
    setZoomScale((prev) => Math.min(prev + 0.5, 3.5));
  };

  const handleZoomOut = () => {
    setZoomScale((prev) => Math.max(prev - 0.5, 1));
  };

  const panResponder = useMemo(
    () =>
      PanResponder.create({
        onMoveShouldSetPanResponder: (_evt, gestureState) => {
          // Khi đang phóng to (zoomScale > 1.05), ưu tiên cuộn/xem chi tiết ảnh thay vì chuyển ảnh
          if (zoomScale > 1.05) return false;
          return (
            Math.abs(gestureState.dx) > 20 &&
            Math.abs(gestureState.dx) > Math.abs(gestureState.dy) * 1.3
          );
        },
        onPanResponderRelease: (_evt, gestureState) => {
          if (zoomScale > 1.05) return;
          const isSwipeLeft = gestureState.dx < -50 || gestureState.vx < -0.3;
          const isSwipeRight = gestureState.dx > 50 || gestureState.vx > 0.3;

          if (isSwipeLeft) {
            handleNext();
          } else if (isSwipeRight) {
            handlePrev();
          }
        },
      }),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [currentIndex, images.length, onClose, zoomScale],
  );

  if (!visible || !images || images.length === 0) return null;

  return (
    <Modal
      visible={visible}
      transparent
      animationType="fade"
      onRequestClose={onClose}
      statusBarTranslucent
    >
      <View
        testID="post-image-viewer-container"
        style={styles.container}
        {...panResponder.panHandlers}
      >
        <SafeAreaView style={styles.safeArea}>
          {/* Thanh công cụ phía trên */}
          <View style={styles.topBar}>
            <Pressable
              testID="post-image-viewer-close"
              accessibilityRole="button"
              accessibilityLabel="Đóng xem ảnh"
              onPress={onClose}
              hitSlop={12}
              style={styles.iconButton}
            >
              <Icon name="close" size={24} color="#FFFFFF" />
            </Pressable>

            <Text testID="post-image-viewer-counter" style={styles.counterText}>
              {`${currentIndex + 1} / ${images.length}`}
            </Text>

            {/* Cụm nút phóng to / thu nhỏ */}
            <View style={styles.zoomControls}>
              <Pressable
                testID="post-image-viewer-zoom-out"
                accessibilityRole="button"
                accessibilityLabel="Thu nhỏ ảnh"
                onPress={handleZoomOut}
                disabled={zoomScale <= 1}
                hitSlop={8}
                style={[styles.iconButton, zoomScale <= 1 && styles.disabledButton]}
              >
                <Icon name="zoom-out" size={22} color="#FFFFFF" />
              </Pressable>

              <Pressable
                testID="post-image-viewer-zoom-in"
                accessibilityRole="button"
                accessibilityLabel="Phóng to ảnh"
                onPress={handleZoomIn}
                disabled={zoomScale >= 3.5}
                hitSlop={8}
                style={[styles.iconButton, zoomScale >= 3.5 && styles.disabledButton]}
              >
                <Icon name="zoom-in" size={22} color="#FFFFFF" />
              </Pressable>
            </View>
          </View>

          {/* Vùng hiển thị ảnh phóng to thu nhỏ */}
          <ScrollView
            testID="post-image-viewer-scrollview"
            style={styles.scrollContainer}
            contentContainerStyle={styles.scrollContent}
            maximumZoomScale={4}
            minimumZoomScale={1}
            centerContent
            showsHorizontalScrollIndicator={false}
            showsVerticalScrollIndicator={false}
          >
            {currentUri ? (
              <View
                testID="post-image-viewer-transform"
                style={[
                  styles.imageWrapper,
                  {
                    transform: [{ scale: zoomScale }],
                  },
                ]}
              >
                <RemoteImage
                  uri={currentUri}
                  accessibilityLabel={`Ảnh chi tiết ${currentIndex + 1}`}
                  style={styles.image}
                  resizeMode="contain"
                />
              </View>
            ) : null}
          </ScrollView>

          {/* Nút điều hướng ảnh trái / phải cho cảm ứng và thử nghiệm */}
          {images.length > 1 ? (
            <View style={styles.navButtonsRow} pointerEvents="box-none">
              <Pressable
                testID="post-image-viewer-prev"
                accessibilityRole="button"
                accessibilityLabel="Ảnh trước"
                onPress={handlePrev}
                style={[styles.navArrow, styles.navArrowLeft]}
              >
                <Icon name="chevron-left" size={24} color="#FFFFFF" />
              </Pressable>

              <Pressable
                testID="post-image-viewer-next"
                accessibilityRole="button"
                accessibilityLabel="Ảnh kế tiếp"
                onPress={handleNext}
                style={[styles.navArrow, styles.navArrowRight]}
              >
                <Icon name="chevron-right" size={24} color="#FFFFFF" />
              </Pressable>
            </View>
          ) : null}
        </SafeAreaView>
      </View>
    </Modal>
  );
});

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#000000',
  },
  safeArea: {
    flex: 1,
  },
  topBar: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: theme.spacing.md,
    paddingVertical: theme.spacing.sm,
    zIndex: 10,
  },
  iconButton: {
    padding: theme.spacing.xs + 2,
    justifyContent: 'center',
    alignItems: 'center',
  },
  disabledButton: {
    opacity: 0.35,
  },
  counterText: {
    color: '#FFFFFF',
    fontSize: theme.font.size.md,
    fontWeight: theme.font.weight.bold,
  },
  zoomControls: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: theme.spacing.xs,
  },
  scrollContainer: {
    flex: 1,
  },
  scrollContent: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  imageWrapper: {
    width: '100%',
    height: '100%',
    justifyContent: 'center',
    alignItems: 'center',
  },
  image: {
    width: '100%',
    height: '100%',
  },
  navButtonsRow: {
    position: 'absolute',
    left: 0,
    right: 0,
    top: '50%',
    flexDirection: 'row',
    justifyContent: 'space-between',
    paddingHorizontal: theme.spacing.sm,
    zIndex: 5,
  },
  navArrow: {
    width: 40,
    height: 40,
    borderRadius: 20,
    backgroundColor: 'rgba(0, 0, 0, 0.5)',
    justifyContent: 'center',
    alignItems: 'center',
  },
  navArrowLeft: {},
  navArrowRight: {},
});
