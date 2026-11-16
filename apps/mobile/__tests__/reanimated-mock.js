/**
 * Bản giả tối thiểu của `react-native-reanimated` cho Jest: animation chạy trên luồng UI của native nên trong test giá trị đổi
 * ngay và `entering` bị bỏ qua. Cần vì `expo-router/testing-library` tự `jest.mock` Reanimated bằng `react-native-reanimated/mock`
 * (gói này không còn tệp đó nên rơi về `{}`); jest.config.js trỏ đường dẫn `react-native-reanimated/mock` vào tệp này.
 */
class Builder {
  duration() {
    return this;
  }
  delay() {
    return this;
  }
  reduceMotion() {
    return this;
  }
  springify() {
    return this;
  }
  damping() {
    return this;
  }
}

module.exports = {
  __esModule: true,
  // `View` và `Text` lấy lười (lúc dựng) để không require `react-native` khi đang nạp mock.
  default: {
    get View() {
      return require('react-native').View;
    },
    get Text() {
      return require('react-native').Text;
    },
  },
  Keyframe: Builder,
  FadeIn: new Builder(),
  ZoomIn: new Builder(),
  ReduceMotion: { System: 'system', Always: 'always', Never: 'never' },
  useSharedValue: (value) => ({ value }),
  useAnimatedStyle: (factory) => factory(),
  withTiming: (value) => value,
  withDelay: (_delay, value) => value,
  withSpring: (value) => value,
  interpolateColor: (_progress, _input, output) => output[0],
};
