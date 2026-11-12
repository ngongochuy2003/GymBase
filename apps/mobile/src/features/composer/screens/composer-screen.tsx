import { useMemo } from 'react';
import { useRouter } from 'expo-router';
import { useSession } from '@/core/auth/session';
import { currentUser } from '@/shared/mock/current-user';
import { DEFAULT_HEADER_ACTIONS, useTabNavigation } from '@/shared/navigation';
import { ScreenRenderer, registerSection, type ScreenConfig } from '@/shared/screen-config';
import { useToast } from '@/shared/ui-kit';
import { ComposerCard } from '../components/composer-card';
import { TopicSelector } from '../components/topic-selector';
import { VisibilitySelector } from '../components/visibility-selector';
import { useComposerState } from '../hooks/use-composer-state';
import { useCreatePostMutation } from '../hooks/use-create-post';
import {
  composerTabs,
  initialMedia,
  mockMediaPool,
  toolbarDefinitions,
  topics,
  visibilityOptions,
} from '../mock/composer-config';

registerSection('composer', ComposerCard);
registerSection('topicSelector', TopicSelector);
registerSection('visibilitySelector', VisibilitySelector);

const INITIAL_TOPICS = ['workout'];

export function ComposerScreen() {
  const { user } = useSession();
  const router = useRouter();
  const onTabPress = useTabNavigation();
  const { toast, show } = useToast();
  const createPostMutation = useCreatePostMutation();
  const state = useComposerState({
    initialMedia,
    initialTopicIds: INITIAL_TOPICS,
    initialVisibilityId: visibilityOptions[0]?.id ?? '',
    mediaPool: mockMediaPool,
  });

  const authorName = user?.name ?? currentUser.name;
  const visibility =
    visibilityOptions.find((o) => o.id === state.visibilityId) ?? visibilityOptions[0];

  const config = useMemo<ScreenConfig>(() => {
    const handlers: Record<string, () => void> = {
      'add-media': state.addMedia,
      'tag-trainer': () => show('Gắn PT sẽ có ở phiên bản sau'),
      'add-hashtag': () =>
        state.appendText(state.text === '' || state.text.endsWith(' ') ? '#' : ' #'),
    };
    const cycleVisibility = () => {
      const index = visibilityOptions.findIndex((o) => o.id === state.visibilityId);
      const next = visibilityOptions[(index + 1) % visibilityOptions.length];
      if (next) state.setVisibilityId(next.id);
    };
    return {
      id: 'composer',
      header: {
        rightActions: DEFAULT_HEADER_ACTIONS,
        avatarUri: currentUser.avatarUri,
        onAvatarPress: () => router.navigate('/profile'),
      },
      subHeader: {
        title: 'Đăng bài',
        onBack: () => (router.canGoBack() ? router.back() : router.navigate('/home')),
        action: {
          label: 'Đăng',
          disabled: !state.canSubmit || createPostMutation.isPending,
          onPress: () => {
            console.log('[composer] payload', state.payload);
            show('Đã đăng (mock)');
            const submittedPayload = state.payload;
            state.reset();
            createPostMutation.mutate({ payload: submittedPayload, authorName });
          },
        },
      },
      sections: [
        {
          id: 'composer',
          type: 'composer',
          props: {
            author: { name: authorName, avatarUri: currentUser.avatarUri },
            visibility: { label: visibility?.label ?? '', icon: visibility?.icon ?? 'globe' },
            onVisibilityPress: cycleVisibility,
            text: state.text,
            onTextChange: state.setText,
            media: state.media,
            maxMedia: state.maxMedia,
            onAddMedia: state.addMedia,
            onRemoveMedia: state.removeMedia,
            toolbarActions: toolbarDefinitions.map((definition) => ({
              ...definition,
              onPress: handlers[definition.id] ?? (() => undefined),
            })),
          },
        },
        {
          id: 'topics',
          type: 'topicSelector',
          props: {
            topics,
            selectedIds: state.topicIds,
            multiple: false,
            onChange: state.setTopicIds,
          },
        },
        {
          id: 'visibility',
          type: 'visibilitySelector',
          props: {
            options: visibilityOptions,
            value: state.visibilityId,
            onChange: state.setVisibilityId,
          },
        },
      ],
      bottomTabs: composerTabs,
      activeTabId: undefined,
    };
  }, [state, show, router, authorName, visibility, createPostMutation]);

  return <ScreenRenderer config={config} onTabPress={onTabPress} overlay={toast} />;
}
