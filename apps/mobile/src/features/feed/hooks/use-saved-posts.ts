import { useCallback, useEffect, useState } from 'react';
import { savedPostsStorage } from '../storage/saved-posts-storage';

export function useSavedPosts() {
  const [savedIds, setSavedIds] = useState<string[]>([]);
  const [isLoaded, setIsLoaded] = useState(false);

  useEffect(() => {
    let active = true;
    savedPostsStorage.getSavedPostIds().then((ids) => {
      if (active) {
        setSavedIds(ids);
        setIsLoaded(true);
      }
    });

    const unsubscribe = savedPostsStorage.subscribe(() => {
      savedPostsStorage.getSavedPostIds().then((ids) => {
        if (active) {
          setSavedIds(ids);
        }
      });
    });

    return () => {
      active = false;
      unsubscribe();
    };
  }, []);

  const toggleSave = useCallback(async (postId: string) => {
    return savedPostsStorage.toggleSavePost(postId);
  }, []);

  const isSaved = useCallback(
    (postId: string) => savedIds.includes(postId),
    [savedIds],
  );

  return {
    savedIds,
    isLoaded,
    toggleSave,
    isSaved,
  };
}
