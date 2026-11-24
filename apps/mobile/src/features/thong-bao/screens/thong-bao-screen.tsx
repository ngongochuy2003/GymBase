import { useCallback } from 'react';
import { FlatList, Pressable, View } from 'react-native';
import { useRouter } from 'expo-router';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Button } from '@/components/ui/button';
import { Text } from '@/components/ui/text';
import { toApiError } from '@/core/api/api-error';
import { Icon } from '@/shared/icons';
import {
  duongDanCuaThongBao,
  useDanhDauDaDoc,
  useDanhDauTatCaDaDoc,
  useHopThu,
  type ThongBaoDto,
} from '@/shared/notifications';
import { EmptyState, ErrorState, LoadingState } from '@/shared/components/ui';
import { useTheme } from '@/shared/theme/theme';
import { DongThongBao } from '../components/dong-thong-bao';
import { KhuVucThu } from '../components/khu-vuc-thu';

/** Hộp thông báo: mới nhất trước, kéo xuống để tải lại, cuộn tới cuối để tải thêm. Bấm một dòng thì đánh dấu đã đọc và mở màn tương ứng. */
export function ThongBaoScreen() {
  const router = useRouter();
  const insets = useSafeAreaInsets();
  const { colors } = useTheme();
  const hopThu = useHopThu();
  const { mutate: danhDauDaDoc } = useDanhDauDaDoc();
  const tatCaDaDoc = useDanhDauTatCaDaDoc();

  const coChuaDoc = hopThu.thongBaos.some((t) => !t.da_doc);

  const handleBack = useCallback(() => {
    if (router.canGoBack()) router.back();
    else router.navigate('/home');
  }, [router]);

  const handlePress = useCallback(
    (thongBao: ThongBaoDto) => {
      if (!thongBao.da_doc) danhDauDaDoc(thongBao.id);
      router.push(duongDanCuaThongBao(thongBao));
    },
    [danhDauDaDoc, router],
  );

  const handleEndReached = useCallback(() => {
    if (hopThu.hasNextPage && !hopThu.isFetchingNextPage) void hopThu.fetchNextPage();
  }, [hopThu]);

  return (
    <View className="flex-1 bg-background" style={{ paddingTop: insets.top }}>
      <View className="flex-row items-center gap-2 px-4 py-2">
        <Pressable accessibilityRole="button" accessibilityLabel="Quay lại" hitSlop={8} onPress={handleBack}>
          <Icon name="back" size={28} color={colors.text} />
        </Pressable>
        <Text variant="large" className="flex-1">
          Thông báo
        </Text>
        <Button
          variant="link"
          size="sm"
          disabled={!coChuaDoc}
          loading={tatCaDaDoc.isPending}
          onPress={() => tatCaDaDoc.mutate(undefined)}
        >
          <Text>Đọc tất cả</Text>
        </Button>
      </View>

      {hopThu.isPending ? (
        <LoadingState />
      ) : hopThu.isError ? (
        <View className="p-4">
          <ErrorState
            message={toApiError(hopThu.error).message}
            onRetry={() => void hopThu.refetch()}
          />
        </View>
      ) : (
        <FlatList
          className="flex-1"
          data={hopThu.thongBaos}
          keyExtractor={(item) => item.id}
          renderItem={({ item }) => <DongThongBao thongBao={item} onPress={handlePress} />}
          onEndReached={handleEndReached}
          onEndReachedThreshold={0.4}
          refreshing={hopThu.isRefetching && !hopThu.isFetchingNextPage}
          onRefresh={() => void hopThu.refetch()}
          ListEmptyComponent={
            <View className="p-4">
              <EmptyState message="Chưa có thông báo nào." />
            </View>
          }
        />
      )}

      {__DEV__ ? <KhuVucThu paddingBottom={insets.bottom + 16} /> : null}
    </View>
  );
}
