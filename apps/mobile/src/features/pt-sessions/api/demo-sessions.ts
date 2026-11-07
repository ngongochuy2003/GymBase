import type { PtSession, SessionsRepository } from '../types/session';

const fixtures: PtSession[] = [
  {
    id: 'demo-1',
    title: 'Sức mạnh thân trên',
    coach: 'PT Hoàng Nam',
    time: '17:30',
    durationMinutes: 45,
    exercises: 6,
    completed: false,
  },
  {
    id: 'demo-2',
    title: 'Giãn cơ & phục hồi',
    coach: 'Tự tập',
    time: '19:00',
    durationMinutes: 20,
    exercises: 4,
    completed: false,
  },
];

export const demoSessionsRepository: SessionsRepository = {
  async list() {
    await new Promise((resolve) => setTimeout(resolve, 200));
    return fixtures.map((session) => ({ ...session }));
  },
};
