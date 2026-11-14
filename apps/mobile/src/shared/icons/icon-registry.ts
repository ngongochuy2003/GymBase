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

// --- Bài viết ---
registerIcon('heart', Ionicons, 'heart-outline');
registerIcon('heart-filled', Ionicons, 'heart');
registerIcon('comment', Ionicons, 'chatbubble-outline');
registerIcon('send', Ionicons, 'paper-plane-outline');
registerIcon('bookmark', Ionicons, 'bookmark-outline');
registerIcon('bookmark-filled', Ionicons, 'bookmark');

// --- Soạn bài ---
registerIcon('image', Ionicons, 'image-outline');
registerIcon('add-square', Feather, 'plus-square');
registerIcon('person', Ionicons, 'person-outline');
registerIcon('hashtag', Feather, 'hash');
registerIcon('globe', Ionicons, 'globe-outline');
registerIcon('followers', Ionicons, 'people-outline');
