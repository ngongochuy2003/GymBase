import React from 'react';
import { View } from 'react-native';

export const useVideoPlayer = jest.fn(() => ({
  play: jest.fn(),
  pause: jest.fn(),
  playing: false,
  loop: false,
  muted: false,
  currentTime: 0,
  addListener: jest.fn(() => ({ remove: jest.fn() })),
}));

export const VideoView = (props: React.ComponentProps<typeof View>) => (
  <View testID="video-view" {...props} />
);

export const createVideoPlayer = jest.fn(() => ({
  play: jest.fn(),
  pause: jest.fn(),
  playing: false,
  loop: false,
  muted: false,
}));
