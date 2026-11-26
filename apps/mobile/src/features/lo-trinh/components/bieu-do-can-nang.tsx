import { useEffect, useRef, useState, type ComponentRef } from 'react';
import { Pressable, ScrollView, View } from 'react-native';
import Animated, { useAnimatedStyle } from 'react-native-reanimated';
import Svg, { Circle, Defs, Line, LinearGradient, Path, Stop } from 'react-native-svg';
import { Text } from '@/components/ui/text';
import { useTheme } from '@/shared/theme/theme';
import { useChayLen } from '../hooks/use-chay-len';
import type { DiemCanNang } from '../types';
import { khoangTrucY, soThapPhan } from '../utils/tien-do';

/** Số điểm tối đa hiện vừa một màn hình; nhiều hơn thì cuộn ngang. */
const TOI_DA_DIEM = 8;
const CHIEU_CAO = 150;
/** Chừa phía trên khung vẽ cho ô giá trị của điểm đang chọn và nhãn vạch trên cùng. */
const LE_TREN = 44;
const CHIEU_CAO_NHAN_TUAN = 36;
/** Bề rộng cột nhãn trục dọc. */
const LE_TRAI = 40;
const CHAM_THUONG = 3.5;
const CHAM_CHON = 6;
const VUNG_CHAM = 40;
const RONG_O_GIA_TRI = 76;

interface Diem {
  x: number;
  y: number;
}

/** Đường cong mượt đi qua các điểm: tiếp tuyến nằm ngang tại mỗi điểm nên không bị vọt lên quá điểm đo. */
function duongCong(diem: readonly Diem[]): string {
  const [dau, ...con] = diem;
  if (!dau) return '';
  let duong = `M ${dau.x} ${dau.y}`;
  let truoc = dau;
  for (const d of con) {
    const giua = (truoc.x + d.x) / 2;
    duong += ` C ${giua} ${truoc.y} ${giua} ${d.y} ${d.x} ${d.y}`;
    truoc = d;
  }
  return duong;
}

/** Số chữ số thập phân vừa đủ để các vạch trục dọc không trùng nhau. */
function soChuSoVach(buoc: number): number {
  if (Number.isInteger(buoc)) return 0;
  return Number.isInteger(buoc * 10) ? 1 : 2;
}

/**
 * Biểu đồ đường cân nặng: đường cong, vùng tô gradient bên dưới, lưới nét đứt. Trục dọc co giãn theo dữ liệu và chừa lề
 * dưới điểm thấp nhất. Bấm vào một điểm để xem cân nặng lần cân đó (mặc định là điểm mới nhất). Một màn hình chứa tối đa
 * 8 điểm; nhiều hơn thì khung vẽ cuộn ngang, cột nhãn trục dọc đứng yên.
 *
 * Animation vào màn chạy trên luồng UI: lớp dữ liệu (đường, vùng tô, điểm) là một khối SVG vẽ một lần rồi được
 * Reanimated co giãn theo chiều dọc từ đáy lên, nên không phải vẽ lại SVG ở mỗi khung hình. Lưới nằm ở lớp riêng, đứng yên.
 */
export function BieuDoCanNang({ diem, donVi }: { diem: readonly DiemCanNang[]; donVi: string }) {
  const { colors } = useTheme();
  const cuon = useRef<ComponentRef<typeof ScrollView>>(null);
  const [rongKhung, setRongKhung] = useState(0);
  const [chon, setChon] = useState(diem.length - 1);
  const tienTrinh = useChayLen(1100, 200);
  const kieuLopDuLieu = useAnimatedStyle(() => ({
    opacity: tienTrinh.value,
    transform: [{ scaleY: tienTrinh.value }],
  }));
  // Ô giá trị chỉ hiện ở 30% cuối của animation, khi các điểm đã gần về đúng chỗ.
  const kieuOGiaTri = useAnimatedStyle(() => ({
    opacity: Math.max(0, (tienTrinh.value - 0.7) / 0.3),
  }));
  const dangChon = Math.max(0, Math.min(chon, diem.length - 1));
  const { min, max, buoc, moc } = khoangTrucY(diem.map((d) => d.kg));
  const soChuSo = soChuSoVach(buoc);

  const oRong = rongKhung / Math.min(Math.max(diem.length, 1), TOI_DA_DIEM);
  const rongNoiDung = Math.max(rongKhung, oRong * diem.length);
  const cao = LE_TREN + CHIEU_CAO;
  const yCua = (kg: number) => LE_TREN + CHIEU_CAO * (1 - (kg - min) / (max - min));
  const toaDo: Diem[] = diem.map((d, i) => ({ x: oRong * (i + 0.5), y: yCua(d.kg) }));
  const duong = duongCong(toaDo);
  const dau = toaDo[0];
  const cuoi = toaDo.at(-1);
  const vungTo = dau && cuoi ? `${duong} L ${cuoi.x} ${cao} L ${dau.x} ${cao} Z` : '';
  const diemChon = toaDo[dangChon];
  const giaTriChon = diem[dangChon];

  // Nhiều hơn 8 điểm thì mở sẵn ở đoạn mới nhất (bên phải). Cuộn tới đúng vị trí tính ra (không dùng scrollToEnd, vì trên
  // Android nó hay chạy trước khi khung đo xong nội dung) và gọi lại sau khi nội dung dựng xong.
  const cuonToiCuoi = (rongTong: number) => {
    if (diem.length <= TOI_DA_DIEM || rongKhung <= 0) return;
    cuon.current?.scrollTo({ x: Math.max(0, rongTong - rongKhung), y: 0, animated: false });
  };
  useEffect(() => {
    if (diem.length <= TOI_DA_DIEM || rongKhung <= 0) return;
    const hen = requestAnimationFrame(() => {
      cuon.current?.scrollTo({ x: Math.max(0, rongNoiDung - rongKhung), y: 0, animated: false });
    });
    return () => cancelAnimationFrame(hen);
  }, [diem.length, rongKhung, rongNoiDung]);

  return (
    <View>
      <Text variant="muted" className="text-xs">
        ({donVi})
      </Text>
      <View className="flex-row">
        <View style={{ width: LE_TRAI, height: cao + CHIEU_CAO_NHAN_TUAN }}>
          {moc.map((v) => (
            <Text
              key={v}
              variant="muted"
              className="absolute text-right text-xs"
              style={{ left: 0, width: LE_TRAI - 8, top: yCua(v) - 8 }}
            >
              {v.toFixed(soChuSo).replace('.', ',')}
            </Text>
          ))}
        </View>
        <ScrollView
          ref={cuon}
          horizontal
          className="flex-1"
          showsHorizontalScrollIndicator={false}
          onLayout={(e) => setRongKhung(e.nativeEvent.layout.width)}
          onContentSizeChange={(rongTong) => cuonToiCuoi(rongTong)}
        >
          {rongKhung > 0 ? (
            <View style={{ width: rongNoiDung, height: cao + CHIEU_CAO_NHAN_TUAN }}>
              <View className="absolute" style={{ left: 0, top: 0 }} pointerEvents="none">
                <Svg width={rongNoiDung} height={cao}>
                  {moc.map((v) => (
                    <Line
                      key={v}
                      x1={0}
                      x2={rongNoiDung}
                      y1={yCua(v)}
                      y2={yCua(v)}
                      stroke={colors.border}
                      strokeWidth={1}
                      strokeDasharray="4 4"
                    />
                  ))}
                </Svg>
              </View>
              <Animated.View
                pointerEvents="none"
                style={[
                  { position: 'absolute', left: 0, top: 0, transformOrigin: 'bottom' },
                  kieuLopDuLieu,
                ]}
              >
                <Svg width={rongNoiDung} height={cao}>
                  <Defs>
                    <LinearGradient id="vung-can-nang" x1="0" y1="0" x2="0" y2="1">
                      <Stop offset="0" stopColor={colors.primary} stopOpacity={0.28} />
                      <Stop offset="1" stopColor={colors.primary} stopOpacity={0.02} />
                    </LinearGradient>
                  </Defs>
                  {vungTo ? <Path d={vungTo} fill="url(#vung-can-nang)" /> : null}
                  {diemChon ? (
                    <Line
                      x1={diemChon.x}
                      x2={diemChon.x}
                      y1={diemChon.y}
                      y2={cao}
                      stroke={colors.primary}
                      strokeWidth={1}
                      strokeDasharray="3 3"
                    />
                  ) : null}
                  <Path
                    d={duong}
                    fill="none"
                    stroke={colors.primary}
                    strokeWidth={2.5}
                    strokeLinecap="round"
                    strokeLinejoin="round"
                  />
                  {toaDo.map((d, i) => (
                    <Circle
                      key={`${diem[i]?.tuan}-${diem[i]?.lan}`}
                      cx={d.x}
                      cy={d.y}
                      r={i === dangChon ? CHAM_CHON : CHAM_THUONG}
                      fill={colors.primary}
                      stroke={colors.surface}
                      strokeWidth={2}
                    />
                  ))}
                </Svg>
              </Animated.View>
              {toaDo.map((d, i) => (
                <Pressable
                  key={`${diem[i]?.tuan}-${diem[i]?.lan}`}
                  accessibilityRole="button"
                  accessibilityLabel={`Tuần ${diem[i]?.tuan} lần ${diem[i]?.lan}: ${soThapPhan(diem[i]?.kg ?? 0)} ${donVi}`}
                  accessibilityState={{ selected: i === dangChon }}
                  onPress={() => setChon(i)}
                  className="absolute"
                  style={{
                    width: VUNG_CHAM,
                    height: VUNG_CHAM,
                    left: d.x - VUNG_CHAM / 2,
                    top: d.y - VUNG_CHAM / 2,
                  }}
                />
              ))}
              {diemChon && giaTriChon ? (
                <Animated.View
                  pointerEvents="none"
                  style={[
                    {
                      position: 'absolute',
                      width: RONG_O_GIA_TRI,
                      left: Math.max(
                        0,
                        Math.min(rongNoiDung - RONG_O_GIA_TRI, diemChon.x - RONG_O_GIA_TRI / 2),
                      ),
                      top: Math.max(0, diemChon.y - 38),
                      alignItems: 'center',
                      borderRadius: 999,
                      paddingVertical: 4,
                      backgroundColor: colors.primary,
                    },
                    kieuOGiaTri,
                  ]}
                >
                  <Text className="text-sm font-bold text-primary-foreground">
                    {soThapPhan(giaTriChon.kg)} {donVi}
                  </Text>
                </Animated.View>
              ) : null}
              {toaDo.map((d, i) => {
                const laDiemChon = i === dangChon;
                // Mỗi tuần cân 2 lần: chỉ đề nhãn tuần ở lần cân đầu, và ở điểm đang chọn để biết điểm đó thuộc tuần nào.
                const coNhan = diem[i]?.lan === 1 || laDiemChon;
                return (
                  <Text
                    key={`${diem[i]?.tuan}-${diem[i]?.lan}`}
                    variant="muted"
                    className={
                      laDiemChon
                        ? 'absolute text-center text-[11px] font-bold leading-4 text-foreground'
                        : 'absolute text-center text-[11px] leading-4'
                    }
                    style={{ width: oRong, left: d.x - oRong / 2, top: cao + 8 }}
                  >
                    {coNhan ? `Tuần\n${diem[i]?.tuan ?? ''}` : ''}
                  </Text>
                );
              })}
            </View>
          ) : null}
        </ScrollView>
      </View>
    </View>
  );
}
