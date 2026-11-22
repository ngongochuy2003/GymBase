import type { ComponentType } from 'react';
import { Feather, Ionicons, MaterialCommunityIcons } from '@expo/vector-icons';

export interface IconGlyphProps {
  name: string;
  size?: number;
  color?: string;
}

type IconComponent = ComponentType<IconGlyphProps>;

interface IconEntry {
  family: IconComponent;
  glyph: string;
}

function entry(family: unknown, glyph: string): IconEntry {
  return { family: family as IconComponent, glyph };
}

const registry = new Map<string, IconEntry>();

/** Đăng ký (hoặc ghi đè) một icon: `registerIcon('flame', Ionicons, 'flame-outline')`. */
export function registerIcon(name: string, family: unknown, glyph: string): void {
  registry.set(name, entry(family, glyph));
}

export function getIcon(name: string): IconEntry | undefined {
  return registry.get(name);
}

// --- Điều hướng / chung ---
registerIcon('home', Ionicons, 'home-outline');
registerIcon('home-filled', Ionicons, 'home');
registerIcon('route', MaterialCommunityIcons, 'chart-bar');
registerIcon('compass', Ionicons, 'search-outline');
registerIcon('plus', Feather, 'plus');
registerIcon('message', Ionicons, 'chatbubble-outline');
registerIcon('user', Ionicons, 'person-outline');
registerIcon('search', Ionicons, 'search-outline');
registerIcon('bell', Ionicons, 'notifications-outline');
registerIcon('back', Feather, 'chevron-left');
registerIcon('chevron-left', Feather, 'chevron-left');
registerIcon('chevron-right', Feather, 'chevron-right');
registerIcon('zoom-in', Feather, 'zoom-in');
registerIcon('zoom-out', Feather, 'zoom-out');
registerIcon('more', Feather, 'more-horizontal');
registerIcon('close', Ionicons, 'close');
registerIcon('check', Feather, 'check');
registerIcon('chevron-down', Feather, 'chevron-down');

// --- Chip / chủ đề ---
registerIcon('flame', Ionicons, 'flame-outline');
registerIcon('dumbbell', MaterialCommunityIcons, 'dumbbell');
registerIcon('book', Ionicons, 'book-outline');
registerIcon('meal', MaterialCommunityIcons, 'silverware-fork-knife');
registerIcon('progress', MaterialCommunityIcons, 'chart-bar');
registerIcon('tip', Ionicons, 'bulb-outline');

// --- Bài viết & Tương tác ---
registerIcon('heart', Ionicons, 'heart-outline');
registerIcon('heart-filled', Ionicons, 'heart');
registerIcon('comment', Ionicons, 'chatbubble-outline');
registerIcon('send', Ionicons, 'paper-plane-outline');
registerIcon('send-filled', Ionicons, 'paper-plane');
registerIcon('repeat', Ionicons, 'repeat-outline');
registerIcon('bookmark', Ionicons, 'bookmark-outline');
registerIcon('bookmark-filled', Ionicons, 'bookmark');
registerIcon('thumbs-up', Ionicons, 'thumbs-up-outline');
registerIcon('thumbs-up-filled', Ionicons, 'thumbs-up');
registerIcon('thumbs-down', Ionicons, 'thumbs-down-outline');
registerIcon('thumbs-down-filled', Ionicons, 'thumbs-down');
registerIcon('camera', Ionicons, 'camera-outline');
registerIcon('happy', Ionicons, 'happy-outline');
registerIcon('sparkles', Ionicons, 'sparkles-outline');

// --- Soạn bài ---
registerIcon('image', Ionicons, 'image-outline');
registerIcon('add-square', Feather, 'plus-square');
registerIcon('person', Ionicons, 'person-outline');
registerIcon('hashtag', Feather, 'hash');
registerIcon('globe', Ionicons, 'globe-outline');
registerIcon('followers', Ionicons, 'people-outline');
registerIcon('video', Ionicons, 'videocam-outline');
registerIcon('play', Ionicons, 'play');
registerIcon('pause', Ionicons, 'pause');
registerIcon('volume-mute', Ionicons, 'volume-mute-outline');
registerIcon('volume-high', Ionicons, 'volume-high-outline');

// --- Lộ trình ---
registerIcon('route-mine', MaterialCommunityIcons, 'account-arrow-right-outline');
registerIcon('route-manage', Ionicons, 'people-outline');
registerIcon('target', MaterialCommunityIcons, 'target');
registerIcon('trend', Feather, 'trending-up');
registerIcon('images', Ionicons, 'images-outline');
registerIcon('calendar', Ionicons, 'calendar-outline');
registerIcon('calendar-check', MaterialCommunityIcons, 'calendar-check-outline');
registerIcon('stats', MaterialCommunityIcons, 'chart-box-outline');
registerIcon('bowl', MaterialCommunityIcons, 'noodles');
registerIcon('scale', MaterialCommunityIcons, 'scale-bathroom');
registerIcon('check-circle', Ionicons, 'checkmark-circle');
registerIcon('clipboard-check', MaterialCommunityIcons, 'clipboard-check-outline');
registerIcon('alert', Ionicons, 'alert-circle-outline');
registerIcon('clock', Ionicons, 'time-outline');
registerIcon('edit', Ionicons, 'create-outline');
registerIcon('person-check', MaterialCommunityIcons, 'account-check-outline');
registerIcon('close-circle', Ionicons, 'close-circle-outline');
registerIcon('circle-half', MaterialCommunityIcons, 'circle-half-full');
registerIcon('minus', Feather, 'minus');
registerIcon('lock', Feather, 'lock');
registerIcon('clipboard', MaterialCommunityIcons, 'clipboard-text-outline');
registerIcon('note', MaterialCommunityIcons, 'note-text-outline');
registerIcon('chevron-up', Feather, 'chevron-up');
