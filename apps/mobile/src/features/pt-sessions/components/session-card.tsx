import { Text, View } from 'react-native';
import { AppText, Button, Card } from '@/shared/components/ui';
import { useTheme } from '@/shared/theme/theme';
import type { PtSession } from '../types/session';

export function SessionCard({ session, onToggle }: { session: PtSession; onToggle(): void }) {
  const { colors } = useTheme();
  return (
    <Card>
      <View style={{ flexDirection: 'row', justifyContent: 'space-between', gap: 12 }}>
        <Text style={{ color: colors.primary, fontWeight: '800', fontSize: 16 }}>
          {session.time}
        </Text>
        <AppText muted>
          {session.completed ? 'Đã hoàn thành' : `${session.durationMinutes} phút`}
        </AppText>
      </View>
      <AppText title>{session.title}</AppText>
      <AppText muted>
        {session.coach} · {session.exercises} bài tập
      </AppText>
      <Button
        secondary={session.completed}
        label={
          session.completed ? `Bỏ đánh dấu: ${session.title}` : `Đánh dấu đã tập: ${session.title}`
        }
        onPress={onToggle}
      />
    </Card>
  );
}
