import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import {
  Pressable,
  StyleSheet,
  View,
  type StyleProp,
  type ViewStyle,
} from 'react-native';
import { useVideoPlayer, VideoView } from 'expo-video';
import { Icon } from '@/shared/icons';
import { theme } from '@/shared/theme/tokens';
import { resolveMediaUrl } from '@/core/config/api-origin';

export interface PostVideoPlayerProps {
  uri: string;
  style?: StyleProp<ViewStyle>;
  aspectRatio?: number;
  autoPlay?: boolean;
  loop?: boolean;
  onPress?(): void;
  accessibilityLabel?: string;
}

export function PostVideoPlayer({
  uri,
  style,
  aspectRatio = 4 / 5,
  autoPlay = false,
  loop = true,
  onPress,
  accessibilityLabel = 'Video bài viết',
}: PostVideoPlayerProps) {
  const [isPlaying, setIsPlaying] = useState(autoPlay);
  const [isMuted, setIsMuted] = useState(true);
  const [showControls, setShowControls] = useState(true);
  const hideControlsTimer = useRef<ReturnType<typeof setTimeout> | null>(null);

  const resolvedUri = useMemo(() => resolveMediaUrl(uri) ?? uri, [uri]);

  const player = useVideoPlayer(resolvedUri, (p) => {
    p.loop = loop;
    p.muted = isMuted;
    if (autoPlay) {
      try {
        p.play();
      } catch {
        // Tránh throw nếu autoplay chưa sẵn sàng
      }
    }
  });

  useEffect(() => {
    if (!player) return;

    const playListener = player.addListener?.('playingChange', ({ isPlaying: playing }: { isPlaying: boolean }) => {
      setIsPlaying(playing);
    });

    const muteListener = player.addListener?.('mutedChange', ({ muted }: { muted: boolean }) => {
      setIsMuted(muted);
    });

    return () => {
      playListener?.remove?.();
      muteListener?.remove?.();
    };
  }, [player]);

  const resetHideTimer = useCallback(() => {
    setShowControls(true);
    if (hideControlsTimer.current) {
      clearTimeout(hideControlsTimer.current);
    }
    hideControlsTimer.current = setTimeout(() => {
      setShowControls(false);
    }, 2500);
  }, []);

  const togglePlay = useCallback(() => {
    if (!player) return;
    try {
      if (isPlaying) {
        player.pause();
        setIsPlaying(false);
        setShowControls(true);
      } else {
        player.play();
        setIsPlaying(true);
        resetHideTimer();
      }
    } catch {
      // Fallback
    }
  }, [isPlaying, player, resetHideTimer]);

  const toggleMute = useCallback(() => {
    if (!player) return;
    try {
      // eslint-disable-next-line react-hooks/immutability
      player.muted = !isMuted;
      setIsMuted(!isMuted);
      resetHideTimer();
    } catch {
      // Fallback
    }
  }, [isMuted, player, resetHideTimer]);

  return (
    <View
      testID="post-video-player-container"
      accessibilityRole="none"
      accessibilityLabel={accessibilityLabel}
      style={[
        styles.container,
        aspectRatio ? { aspectRatio } : undefined,
        style,
      ]}
    >
      <VideoView
        player={player}
        style={styles.video}
        contentFit="cover"
        nativeControls={false}
      />

      {/* Vùng bấm tương tác */}
      <Pressable
        testID="post-video-tap-area"
        onPress={() => {
          if (onPress) {
            onPress();
          } else {
            togglePlay();
          }
        }}
        style={styles.touchArea}
      >
        {/* Nút Play/Pause ở chính giữa */}
        {(!isPlaying || showControls) && (
          <Pressable
            testID="post-video-toggle-play"
            accessibilityRole="button"
            accessibilityLabel={isPlaying ? 'Tạm dừng video' : 'Phát video'}
            onPress={togglePlay}
            hitSlop={8}
            style={styles.centerButton}
          >
            <Icon
              name={isPlaying ? 'pause' : 'play'}
              size={24}
              color={theme.colors.onPrimary}
            />
          </Pressable>
        )}

        {/* Nút bật/tắt tiếng ở góc dưới */}
        {showControls && (
          <Pressable
            testID="post-video-toggle-mute"
            accessibilityRole="button"
            accessibilityLabel={isMuted ? 'Bật âm thanh' : 'Tắt âm thanh'}
            onPress={toggleMute}
            hitSlop={8}
            style={styles.muteButton}
          >
            <Icon
              name={isMuted ? 'volume-mute' : 'volume-high'}
              size={16}
              color={theme.colors.onPrimary}
            />
          </Pressable>
        )}
      </Pressable>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    width: '100%',
    backgroundColor: theme.colors.surfaceMuted,
    borderRadius: theme.radius.image,
    overflow: 'hidden',
    position: 'relative',
  },
  video: {
    width: '100%',
    height: '100%',
  },
  touchArea: {
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    alignItems: 'center',
    justifyContent: 'center',
  },
  centerButton: {
    width: 48,
    height: 48,
    borderRadius: 24,
    backgroundColor: theme.colors.overlay,
    alignItems: 'center',
    justifyContent: 'center',
  },
  muteButton: {
    position: 'absolute',
    bottom: theme.spacing.sm,
    right: theme.spacing.sm,
    width: 32,
    height: 32,
    borderRadius: 16,
    backgroundColor: theme.colors.overlay,
    alignItems: 'center',
    justifyContent: 'center',
  },
});
