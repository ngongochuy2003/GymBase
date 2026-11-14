import { useMutation, useQueryClient } from '@tanstack/react-query';
import { composerRepository, type ComposerRepository } from '../api/composer-api';
import type { ComposerPayload } from '../types';

export interface CreatePostInput {
  payload: ComposerPayload;
}

export function useCreatePostMutation(repository: ComposerRepository = composerRepository) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ payload }: CreatePostInput) => repository.createPost(payload),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['feed', 'posts'] });
    },
  });
}
