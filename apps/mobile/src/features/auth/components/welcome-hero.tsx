import { useCallback, useEffect, useRef, useState, type ComponentRef } from 'react';
import {
  AccessibilityInfo,
  Image,
  ScrollView,
  StyleSheet,
  View,
  type ImageSourcePropType,
  type LayoutChangeEvent,
  type NativeScrollEvent,
  type NativeSyntheticEvent,
} from 'react-native';
import Animated, {
  interpolateColor,
  useAnimatedStyle,
  useSharedValue,
  withTiming,
} from 'react-native-reanimated';
import { Appear, systemReduceMotion } from '@/shared/motion';
import { useTheme } from '@/shared/theme/theme';
import { WAVE_HEIGHT, WaveEdge } from './wave-edge';

type Slide = { image: ImageSourcePropType; title: string };

// TODO(ASSET): ảnh tạm lấy từ Pexels (xem assets/images/NGUON-ANH.md); thay bằng ảnh trong file thiết kế khi có.
const SLIDES: Slide[] = [
  {
    image: require('../../../../assets/images/welcome-tap-luyen.jpg'),
    title: 'Kết nối PT,\ntheo dõi lộ trình\nvà phát triển bản thân\ncùng GymBase',
  },
  {
    image: require('../../../../assets/images/welcome-dinh-duong.jpg'),
    title: 'Ghi lại bữa ăn\nvà buổi tập,\nnhận phản hồi\ntừ PT của bạn',
  },
  {
    image: require('../../../../assets/images/welcome-huan-luyen-vien.jpg'),
    title: 'Đồng hành cùng PT\ntrên từng buổi tập,\nmỗi ngày một tiến bộ',
  },
];

/** Phủ kín khung; phải có width và height tường minh vì ảnh `require` mang sẵn kích thước gốc. */
const IMAGE_FILL = [StyleSheet.absoluteFill, { width: '100%', height: '100%' }] as const;

/** Chiều cao (%) của lớp mờ ở đầu ảnh, nơi ảnh tan vào nền trang. */
const FADE_HEIGHT_PERCENT = 30;

/** `#RRGGBB` thành `rgba(...)`, để dựng gradient cùng màu nền trang nhưng trong suốt dần. */
function withAlpha(hex: string, alpha: number) {
  const value = Number.parseInt(hex.slice(1), 16);
  return `rgba(${(value >> 16) & 255}, ${(value >> 8) & 255}, ${value & 255}, ${alpha})`;
}

/**
 * Chữ trắng phải đọc được trên mọi ảnh. React Native chỉ cho một lớp bóng nên bóng đặt đậm và rộng; phần bảo đảm chính là lớp
 * tối bên trái (`TEXT_SCRIM`) nằm sau chữ: ảnh bên phải vẫn sáng, còn nền chữ luôn đủ tối.
 */
const TITLE_SHADOW = {
  textShadowColor: 'rgba(0, 0, 0, 0.55)',
  textShadowOffset: { width: 0, height: 1 },
  textShadowRadius: 8,
} as const;

/** Kiểu chữ tiêu đề viết bằng style (không dùng className) vì `Animated.Text` không nhận class của NativeWind ổn định. */
const TITLE_STYLE = {
  paddingHorizontal: 28,
  fontSize: 26,
  fontWeight: '600',
  lineHeight: 34,
  color: '#FFFFFF',
} as const;

/** Đỉnh khối chữ nằm ở 70% chiều cao hero, và luôn cách đường sóng ít nhất khoảng này. */
const TITLE_TOP_RATIO = 0.55;
const TITLE_BOTTOM_GAP = 12;

const DOT_STYLE = { width: 6, height: 6, borderRadius: 3 } as const;

/** Lớp tối từ mép trái, nhạt dần sang phải, phủ vùng chữ (khoảng 78% chiều ngang). */
const TEXT_SCRIM = {
  position: 'absolute',
  top: 0,
  bottom: 0,
  left: 0,
  width: '78%',
  backgroundImage:
    'linear-gradient(to right, rgba(0, 0, 0, 0.5) 0%, rgba(0, 0, 0, 0.34) 50%, rgba(0, 0, 0, 0) 100%)',
} as const;

/** Chữ của slide hiện lên (mờ và trượt lên 12px) mỗi khi slide đó trở thành slide đang xem. */
function SlideTitle({
  active,
  heroHeight,
  children,
}: {
  active: boolean;
  heroHeight: number;
  children: string;
}) {
  const progress = useSharedValue(1);
  const first = useRef(true);
  useEffect(() => {
    // Lần dựng đầu để nguyên (cả hero đã hiện dần); từ lần sau mới chạy hiệu ứng khi đổi slide.
    if (first.current) {
      first.current = false;
      progress.value = active ? 1 : 0;
      return;
    }
    progress.value = withTiming(active ? 1 : 0, {
      duration: active ? 450 : 200,
      reduceMotion: systemReduceMotion(),
    });
  }, [active, progress]);
  const style = useAnimatedStyle(() => ({
    opacity: progress.value,
    transform: [{ translateY: (1 - progress.value) * 12 }],
  }));
  // Chữ bắt đầu ở 70% chiều cao hero, nhưng không để 4 dòng tràn xuống dưới đường sóng (hero thấp thì chữ lên cao hơn).
  const textHeight = children.split('\n').length * TITLE_STYLE.lineHeight;
  const topPadding = Math.max(
    0,
    Math.min(
      heroHeight * TITLE_TOP_RATIO,
      heroHeight - WAVE_HEIGHT - TITLE_BOTTOM_GAP - textHeight,
    ),
  );
  return (
    <Animated.Text
      style={[TITLE_STYLE, TITLE_SHADOW, { paddingTop: Math.round(topPadding) }, style]}
    >
      {children}
    </Animated.Text>
  );
}

/** Chấm trang: chấm đang xem đổi màu và phóng to nhẹ, chuyển mượt thay vì đổi tức thì. */
function PageDot({ active }: { active: boolean }) {
  const { colors } = useTheme();
  const progress = useSharedValue(active ? 1 : 0);
  useEffect(() => {
    progress.value = withTiming(active ? 1 : 0, {
      duration: 250,
      reduceMotion: systemReduceMotion(),
    });
  }, [active, progress]);
  const inactiveColor = withAlpha(colors.muted, 0.3);
  const activeColor = colors.primary;
  const style = useAnimatedStyle(() => ({
    backgroundColor: interpolateColor(progress.value, [0, 1], [inactiveColor, activeColor]),
    transform: [{ scale: 1 + 0.4 * progress.value }],
  }));
  return <Animated.View style={[DOT_STYLE, style]} />;
}

/** Tự chuyển ảnh sau mỗi khoảng này; người dùng vuốt thì tính lại từ đầu. */
const AUTOPLAY_MS = 5000;

/**
 * Hero của màn chào mừng: carousel ảnh vuốt ngang, mỗi ảnh có lớp phủ tối và chữ trắng căn trái, kèm chấm trang.
 * Tự chuyển ảnh, trừ khi hệ thống bật giảm chuyển động.
 */
export function WelcomeHero() {
  const { colors } = useTheme();
  const scroller = useRef<ComponentRef<typeof ScrollView>>(null);
  const [size, setSize] = useState({ width: 0, height: 0 });
  const { width, height } = size;
  const [index, setIndex] = useState(0);
  const [reduceMotion, setReduceMotion] = useState(false);

  useEffect(() => {
    let active = true;
    void AccessibilityInfo.isReduceMotionEnabled().then((enabled) => {
      if (active) setReduceMotion(enabled);
    });
    return () => {
      active = false;
    };
  }, []);

  const goTo = useCallback(
    (next: number) => {
      scroller.current?.scrollTo({ x: next * width, animated: true });
      setIndex(next);
    },
    [width],
  );

  // Đổi `index` (do tự chuyển hoặc do vuốt) thì hẹn lại lượt chuyển kế tiếp.
  useEffect(() => {
    if (reduceMotion || width === 0) return;
    const timer = setTimeout(() => goTo((index + 1) % SLIDES.length), AUTOPLAY_MS);
    return () => clearTimeout(timer);
  }, [goTo, index, reduceMotion, width]);

  const onLayout = (event: LayoutChangeEvent) => setSize(event.nativeEvent.layout);
  const onSwipeEnd = (event: NativeSyntheticEvent<NativeScrollEvent>) => {
    if (width > 0) setIndex(Math.round(event.nativeEvent.contentOffset.x / width));
  };

  return (
    <Appear delay={60} style={{ flex: 1, minHeight: 160, marginTop: -12, marginBottom: 8 }}>
      {/* Tràn hết chiều ngang (bù lề 24 của Screen); đầu ảnh mờ dần vào nền trang, chân ảnh cắt cong. */}
      <View onLayout={onLayout} className="-mx-6 flex-1 overflow-hidden">
        <ScrollView
          ref={scroller}
          horizontal
          pagingEnabled
          showsHorizontalScrollIndicator={false}
          onMomentumScrollEnd={onSwipeEnd}
        >
          {SLIDES.map((slide, position) => (
            <View
              key={slide.title}
              style={{ width: width || undefined, height: height ? height - 1 : undefined }}
              className="bg-primary-dark"
            >
              <Image
                source={slide.image}
                accessibilityIgnoresInvertColors
                resizeMode="cover"
                style={IMAGE_FILL}
              />
              <View className="absolute inset-0 bg-black/10" />
              <View pointerEvents="none" style={TEXT_SCRIM} />
              <SlideTitle active={position === index} heroHeight={height}>
                {slide.title}
              </SlideTitle>
            </View>
          ))}
        </ScrollView>
        {/* Gradient thật của React Native (backgroundImage): mịn, không còn dải. Ease nhẹ bằng điểm dừng giữa. */}
        <View
          pointerEvents="none"
          style={{
            position: 'absolute',
            top: 0,
            left: 0,
            right: 0,
            height: `${FADE_HEIGHT_PERCENT}%`,
            backgroundImage: `linear-gradient(to bottom, ${withAlpha(colors.background, 1)} 0%, ${withAlpha(colors.background, 0.55)} 40%, ${withAlpha(colors.background, 0.18)} 75%, ${withAlpha(colors.background, 0)} 100%)`,
          }}
        />
        <WaveEdge width={width} />
      </View>
      <View
        accessible
        accessibilityLabel={`Ảnh ${index + 1} trên ${SLIDES.length}`}
        className="-mt-4 flex-row items-center justify-center gap-2"
      >
        {SLIDES.map((slide, position) => (
          <PageDot key={slide.title} active={position === index} />
        ))}
      </View>
    </Appear>
  );
}
