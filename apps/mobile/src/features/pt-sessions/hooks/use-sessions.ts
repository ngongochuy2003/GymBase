import { useQuery, useQueryClient } from '@tanstack/react-query';
import { env } from '@/core/config/env';
import { ApiError } from '@/core/api/api-error';
import { demoSessionsRepository } from '../api/demo-sessions';
import type { PtSession, SessionsRepository } from '../types/session';

export const sessionKeys = {
  all: ['pt-sessions'] as const,
  list: ['pt-sessions', 'demo-list'] as const,
};
const apiNotReady: SessionsRepository = {
  async list() {
    throw new ApiError('API buổi tập chưa được tích hợp.');
  },
};

export function useSessions(
  repository = env.dataMode === 'demo' ? demoSessionsRepository : apiNotReady,
) {
  return useQuery({ queryKey: sessionKeys.list, queryFn: () => repository.list() });
}

// Local preview only: does not confirm a billable PT session or change a package balance.
export function useDemoCompletion() {
  const client = useQueryClient();
  return (id: string) =>
    client.setQueryData<PtSession[]>(sessionKeys.list, (previous) =>
      previous?.map((session) =>
        session.id === id ? { ...session, completed: !session.completed } : session,
      ),
    );
}
