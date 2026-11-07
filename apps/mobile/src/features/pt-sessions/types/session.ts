export type PtSession = {
  id: string;
  title: string;
  coach: string;
  time: string;
  durationMinutes: number;
  exercises: number;
  completed: boolean;
};
export interface SessionsRepository {
  list(): Promise<PtSession[]>;
}
