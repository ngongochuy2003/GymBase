import { useState } from 'react';
import { Modal, Pressable, View } from 'react-native';
import { Button } from '@/components/ui/button';
import { Text } from '@/components/ui/text';
import { Icon } from '@/shared/icons';
import { useTheme } from '@/shared/theme/theme';

const BUOC_PHUT = 5;

function tachGio(gio: string): { h: number; m: number } {
  const [h = 0, m = 0] = gio.split(':').map(Number);
  return { h, m };
}

function ghepGio(h: number, m: number): string {
  return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}`;
}

function BoTang({
  nhan,
  giaTri,
  onTang,
  onGiam,
}: {
  nhan: string;
  giaTri: string;
  onTang(): void;
  onGiam(): void;
}) {
  const { colors } = useTheme();
  return (
    <View className="items-center gap-2">
      <Pressable
        accessibilityRole="button"
        accessibilityLabel={`Tăng ${nhan}`}
        onPress={onTang}
        className="h-10 w-16 items-center justify-center rounded-xl bg-secondary"
      >
        <Icon name="chevron-up" size={22} color={colors.text} />
      </Pressable>
      <Text className="text-3xl font-bold text-foreground">{giaTri}</Text>
      <Pressable
        accessibilityRole="button"
        accessibilityLabel={`Giảm ${nhan}`}
        onPress={onGiam}
        className="h-10 w-16 items-center justify-center rounded-xl bg-secondary"
      >
        <Icon name="chevron-down" size={22} color={colors.text} />
      </Pressable>
    </View>
  );
}

export interface ChonGioProps {
  /** `HH:mm`. */
  gio: string;
  onDoi(gio: string): void;
  tieuDe: string;
}

/** Viên giờ có icon đồng hồ; bấm mở bảng chọn giờ và phút (phút nhảy 5). */
export function ChonGio({ gio, onDoi, tieuDe }: ChonGioProps) {
  const { colors } = useTheme();
  const [mo, setMo] = useState(false);
  const [tam, setTam] = useState(tachGio(gio));

  const doiGio = (buoc: number) => setTam((t) => ({ ...t, h: (t.h + buoc + 24) % 24 }));
  const doiPhut = (buoc: number) =>
    setTam((t) => ({ ...t, m: (t.m + buoc * BUOC_PHUT + 60) % 60 }));

  return (
    <>
      <Pressable
        accessibilityRole="button"
        accessibilityLabel={`${tieuDe}, ${gio}`}
        onPress={() => {
          setTam(tachGio(gio));
          setMo(true);
        }}
        className="flex-row items-center gap-1.5 rounded-xl border border-border bg-background px-3 py-2"
      >
        <Icon name="clock" size={18} color={colors.text} />
        <Text className="text-base text-foreground">{gio}</Text>
      </Pressable>
      <Modal visible={mo} transparent animationType="slide" onRequestClose={() => setMo(false)}>
        <Pressable
          accessibilityLabel="Đóng"
          onPress={() => setMo(false)}
          className="flex-1 justify-end bg-black/40"
        >
          <Pressable className="gap-5 rounded-t-3xl bg-background px-5 pb-8 pt-5">
            <Text className="text-lg font-bold text-foreground">{tieuDe}</Text>
            <View className="flex-row items-center justify-center gap-6">
              <BoTang
                nhan="giờ"
                giaTri={String(tam.h).padStart(2, '0')}
                onTang={() => doiGio(1)}
                onGiam={() => doiGio(-1)}
              />
              <Text className="text-3xl font-bold text-foreground">:</Text>
              <BoTang
                nhan="phút"
                giaTri={String(tam.m).padStart(2, '0')}
                onTang={() => doiPhut(1)}
                onGiam={() => doiPhut(-1)}
              />
            </View>
            <Button
              shape="pill"
              accessibilityLabel="Xong"
              onPress={() => {
                onDoi(ghepGio(tam.h, tam.m));
                setMo(false);
              }}
              className="min-h-0 border-0 px-0 py-3"
            >
              <Text>Xong</Text>
            </Button>
          </Pressable>
        </Pressable>
      </Modal>
    </>
  );
}
