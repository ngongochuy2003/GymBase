import { Platform } from 'react-native';
import * as SecureStore from 'expo-secure-store';

const SAVED_POSTS_KEY = 'gymbase.saved_posts';

function isWeb(): boolean {
  return (
    Platform.OS === 'web' ||
    (typeof window !== 'undefined' && typeof window.localStorage !== 'undefined')
  );
}

// In-memory cache để đọc nhanh không cần await
let memoryCache: Set<string> | null = null;
const listeners = new Set<() => void>();

function notifyListeners(): void {
  for (const listener of listeners) {
    try {
      listener();
    } catch {
      // bỏ qua lỗi listener
    }
  }
}

export const savedPostsStorage = {
  subscribe(listener: () => void): () => void {
    listeners.add(listener);
    return () => {
      listeners.delete(listener);
    };
  },

  async getSavedPostIds(): Promise<string[]> {
    if (memoryCache !== null) {
      return Array.from(memoryCache);
    }

    try {
      let raw: string | null = null;
      if (isWeb()) {
        raw = window.localStorage.getItem(SAVED_POSTS_KEY);
      } else {
        raw = await SecureStore.getItemAsync(SAVED_POSTS_KEY);
      }

      if (!raw) {
        memoryCache = new Set<string>();
        return [];
      }

      const parsed = JSON.parse(raw) as string[];
      memoryCache = new Set(Array.isArray(parsed) ? parsed : []);
      return Array.from(memoryCache);
    } catch {
      memoryCache = new Set<string>();
      return [];
    }
  },

  isSaved(postId: string): boolean {
    if (memoryCache !== null) {
      return memoryCache.has(postId);
    }
    // Nếu chưa nạp từ storage, gọi đọc ngầm
    void savedPostsStorage.getSavedPostIds();
    return false;
  },

  async toggleSavePost(postId: string): Promise<boolean> {
    const ids = await savedPostsStorage.getSavedPostIds();
    const set = new Set(ids);
    let nextSaved = false;

    if (set.has(postId)) {
      set.delete(postId);
      nextSaved = false;
    } else {
      set.add(postId);
      nextSaved = true;
    }

    memoryCache = set;
    const array = Array.from(set);
    const raw = JSON.stringify(array);

    if (isWeb()) {
      try {
        window.localStorage.setItem(SAVED_POSTS_KEY, raw);
      } catch {
        /* ignore */
      }
    } else {
      try {
        await SecureStore.setItemAsync(SAVED_POSTS_KEY, raw);
      } catch {
        /* ignore */
      }
    }

    notifyListeners();
    return nextSaved;
  },

  async clear(): Promise<void> {
    memoryCache = new Set();
    if (isWeb()) {
      try {
        window.localStorage.removeItem(SAVED_POSTS_KEY);
      } catch {
        /* ignore */
      }
    } else {
      try {
        await SecureStore.deleteItemAsync(SAVED_POSTS_KEY);
      } catch {
        /* ignore */
      }
    }
    notifyListeners();
  },
};
