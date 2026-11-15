import { Animated, ScrollView, StyleSheet, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { theme } from '../theme/tokens';
import { AppHeader, BottomTabBar, SubHeader } from '../ui-kit';
import { getSection } from './section-registry';
import type { ScreenRendererProps } from './types';
import { useCollapsibleHeader } from './use-collapsible-header';

/** Dựng một màn hình từ `ScreenConfig`: header, thanh phụ, các section theo registry, tab bar. */
export function ScreenRenderer({ config, onTabPress, overlay }: ScreenRendererProps) {
  const visibleSections = config.sections.filter((section) => section.visible !== false);
  const hasHeader = Boolean(config.header || config.subHeader);
  const isCollapsible = config.collapsibleHeader !== false && hasHeader;

  const {
    headerTranslateY,
    headerOpacity,
    isHeaderVisible,
    headerHeight,
    onScroll,
    onHeaderLayout,
  } = useCollapsibleHeader({
    enabled: isCollapsible,
    initialHeaderHeight: config.subHeader ? 104 : 56,
  });

  return (
    <SafeAreaView style={styles.root} edges={['top', 'left', 'right', 'bottom']}>
      <View style={styles.container}>
        {hasHeader ? (
          <Animated.View
            testID="screen-header-container"
            onLayout={onHeaderLayout}
            pointerEvents={isHeaderVisible ? 'auto' : 'none'}
            style={[
              styles.top,
              isCollapsible && [
                styles.collapsibleTop,
                {
                  transform: [{ translateY: headerTranslateY }],
                  opacity: headerOpacity,
                },
              ],
            ]}
          >
            {config.header ? <AppHeader {...config.header} /> : null}
            {config.subHeader ? <SubHeader {...config.subHeader} /> : null}
          </Animated.View>
        ) : null}
        <ScrollView
          testID="screen-scroll-view"
          keyboardShouldPersistTaps="handled"
          contentContainerStyle={[
            styles.content,
            isCollapsible && { paddingTop: headerHeight },
          ]}
          style={styles.scroll}
          onScroll={isCollapsible ? onScroll : undefined}
          scrollEventThrottle={16}
        >
          {visibleSections.map((section) => {
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
      </View>
      <BottomTabBar tabs={config.bottomTabs} activeId={config.activeTabId} onChange={onTabPress} />
      {overlay}
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  root: { flex: 1, backgroundColor: theme.colors.background },
  container: {
    flex: 1,
    position: 'relative',
    overflow: 'hidden',
  },
  top: {
    paddingHorizontal: theme.spacing.lg - 2,
    paddingTop: theme.spacing.sm - 3,
    paddingBottom: theme.spacing.sm - 3,
    gap: theme.spacing.lg,
    backgroundColor: theme.colors.background,
  },
  collapsibleTop: {
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    zIndex: 10,
    borderBottomWidth: StyleSheet.hairlineWidth,
    borderBottomColor: theme.colors.border,
  },
  scroll: { flex: 1 },
  content: {
    paddingHorizontal: 0,
    paddingBottom: theme.spacing.xl,
    gap: theme.spacing.md,
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
