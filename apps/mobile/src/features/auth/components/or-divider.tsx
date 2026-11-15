import { Text, View } from 'react-native';

/** Dòng "Hoặc tiếp tục với" nằm giữa hai đường kẻ. */
export function OrDivider() {
  return (
    <View className="flex-row items-center gap-3">
      <View className="h-px flex-1 bg-border" />
      <Text className="text-sm text-muted-foreground">Hoặc tiếp tục với</Text>
      <View className="h-px flex-1 bg-border" />
    </View>
  );
}
