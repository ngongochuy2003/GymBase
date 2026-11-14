import { ScrollView, StyleSheet, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { theme } from '../theme/tokens';
import { AppHeader, BottomTabBar, SubHeader } from '../ui-kit';
import { getSection } from './section-registry';
import type { ScreenRendererProps } from './types';

/** Dựng một màn hình từ `ScreenConfig`: header, thanh phụ, các section theo registry, tab bar. */
export function ScreenRenderer({ config, onTabPress, overlay }: ScreenRendererProps) {
  return (
    <SafeAreaView style={styles.root} edges={['top', 'left', 'right', 'bottom']}>
      <View style={styles.top}>
        {config.header ? <AppHeader {...config.header} /> : null}
        {config.subHeader ? <SubHeader {...config.subHeader} /> : null}
      </View>
      <ScrollView
        keyboardShouldPersistTaps="handled"
        contentContainerStyle={styles.content}
        style={styles.scroll}
      >
        {config.sections
          .filter((section) => section.visible !== false)
          .map((section) => {
            const Component = getSection(section.type);
            if (!Component) {
              return (
                <View key={section.id} accessibilityRole="alert" style={styles.unknown}>
                  <Text style={styles.unknownText}>
                    {`Section chưa đăng ký: "${section.type}" (${section.id})`}
                  </Text>
                </View>
              );
            }
            return <Component key={section.id} {...(section.props ?? {})} />;
          })}
      </ScrollView>
      <BottomTabBar tabs={config.bottomTabs} activeId={config.activeTabId} onChange={onTabPress} />
      {overlay}
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  root: { flex: 1, backgroundColor: theme.colors.background },
  top: {
    paddingHorizontal: theme.spacing.xl,
    paddingTop: theme.spacing.sm,
    paddingBottom: theme.spacing.md,
    gap: theme.spacing.lg,
  },
  scroll: { flex: 1 },
  content: {
    paddingHorizontal: theme.spacing.xl,
    paddingBottom: theme.spacing.xl,
    gap: theme.spacing.lg,
    width: '100%',
    maxWidth: 680,
    alignSelf: 'center',
  },
  unknown: {
    padding: theme.spacing.lg,
    borderRadius: theme.radius.card,
    borderWidth: 1,
    borderStyle: 'dashed',
    borderColor: theme.colors.muted,
  },
  unknownText: { color: theme.colors.muted, fontSize: theme.font.size.sm },
});
