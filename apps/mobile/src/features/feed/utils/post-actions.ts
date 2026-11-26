import { Share } from 'react-native';
import * as Linking from 'expo-linking';

export function buildPostLink(postId: string): string {
  try {
    return Linking.createURL('/post-detail', { queryParams: { postId } });
  } catch {
    return `gymbase://post-detail?postId=${encodeURIComponent(postId)}`;
  }
}

export async function copyPostLink(postId: string): Promise<'copied' | 'shared'> {
  const url = buildPostLink(postId);
  const clipboard = globalThis.navigator?.clipboard;
  if (clipboard && typeof clipboard.writeText === 'function') {
    await clipboard.writeText(url);
    return 'copied';
  }

  await Share.share({ message: url, url });
  return 'shared';
}
