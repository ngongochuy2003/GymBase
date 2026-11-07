import { StyleSheet, Text, View } from 'react-native';
import { useSession } from '@/core/auth/session';
import { AppText, EmptyState, ErrorState, LoadingState, Screen } from '@/shared/components/ui';
import { useTheme } from '@/shared/theme/theme';
import { SessionCard } from '../components/session-card';
import { useDemoCompletion, useSessions } from '../hooks/use-sessions';

export function DashboardScreen() {
  const { user } = useSession();
  const { colors } = useTheme();
  const query = useSessions();
  const toggle = useDemoCompletion();
  const sessions = query.data ?? [];
  const completed = sessions.filter((session) => session.completed).length;
  return (
    <Screen>
      <View style={styles.topline}>
        <Text style={[styles.brand, { color: colors.text }]}>
          gymbase<Text style={{ color: colors.primary }}>.</Text>
        </Text>
        <View style={[styles.badge, { backgroundColor: colors.accent }]}>
          <Text style={styles.badgeText}>BẢN DEMO</Text>
        </View>
      </View>
      <View style={{ gap: 4 }}>
        <AppText muted>Chào {user?.name ?? 'bạn'},</AppText>
        <AppText title>Sẵn sàng cho phiên bản{'\n'}khỏe hơn của bạn?</AppText>
      </View>
      <View style={[styles.hero, { backgroundColor: colors.hero }]}>
        <Text style={[styles.eyebrow, { color: colors.accent }]}>HÀNH TRÌNH HÔM NAY</Text>
        <View style={styles.heroRow}>
          <View style={{ flex: 1, gap: 10 }}>
            <Text style={[styles.heroTitle, { color: colors.heroText }]}>
              Đều đặn.{'\n'}Rồi sẽ khác biệt.
            </Text>
            <Text style={{ color: colors.heroText, opacity: 0.8, lineHeight: 22 }}>
              Mỗi buổi tập là một bước tiến.
            </Text>
          </View>
          <View style={[styles.ring, { borderColor: colors.accent }]}>
            <Text style={{ color: colors.accent, fontSize: 30, fontWeight: '800' }}>
              {completed}/{sessions.length}
            </Text>
            <Text style={{ color: colors.heroText, fontSize: 11 }}>BUỔI TẬP</Text>
          </View>
        </View>
      </View>
      <View style={styles.stats}>
        {[
          ['KẾ HOẠCH', `${sessions.length} buổi`],
          ['THỜI LƯỢNG', `${sessions.reduce((sum, item) => sum + item.durationMinutes, 0)} phút`],
        ].map(([label, value]) => (
          <View
            key={label}
            style={[styles.stat, { backgroundColor: colors.surface, borderColor: colors.border }]}
          >
            <AppText muted>{label}</AppText>
            <AppText title>{value}</AppText>
          </View>
        ))}
      </View>
      <View style={{ gap: 4 }}>
        <AppText title>Lịch tập của bạn</AppText>
        <AppText muted>Dữ liệu minh họa · Thay đổi chỉ lưu trong phiên demo.</AppText>
      </View>
      {query.isPending ? (
        <LoadingState />
      ) : query.isError ? (
        <ErrorState message={query.error.message} onRetry={() => void query.refetch()} />
      ) : sessions.length === 0 ? (
        <EmptyState message="Chưa có buổi tập. Lịch tập mới sẽ xuất hiện tại đây." />
      ) : (
        sessions.map((session) => (
          <SessionCard key={session.id} session={session} onToggle={() => toggle(session.id)} />
        ))
      )}
      <AppText muted>Tiến trình của riêng bạn. Một ngày, một bước.</AppText>
    </Screen>
  );
}

const styles = StyleSheet.create({
  topline: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingVertical: 8,
  },
  brand: { fontSize: 27, fontWeight: '900', letterSpacing: -1 },
  badge: { paddingHorizontal: 12, paddingVertical: 7, borderRadius: 20 },
  badgeText: { color: '#183C2C', fontSize: 10, fontWeight: '800', letterSpacing: 1 },
  hero: { padding: 24, gap: 20, borderRadius: 24 },
  eyebrow: { fontSize: 10, letterSpacing: 1.8, fontWeight: '800' },
  heroRow: { flexDirection: 'row', alignItems: 'center', gap: 12 },
  heroTitle: { fontSize: 27, lineHeight: 34, fontWeight: '800', letterSpacing: -0.5 },
  ring: {
    width: 94,
    height: 94,
    borderWidth: 5,
    borderRadius: 47,
    alignItems: 'center',
    justifyContent: 'center',
    gap: 2,
  },
  stats: { flexDirection: 'row', gap: 12 },
  stat: { flex: 1, padding: 16, borderWidth: 1, borderRadius: 18, gap: 6 },
});
