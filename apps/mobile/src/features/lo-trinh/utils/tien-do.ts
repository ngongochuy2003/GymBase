/** Số thập phân kiểu Việt, một chữ số sau dấu phẩy: `6,5`. */
export function soThapPhan(so: number): string {
  return so.toFixed(1).replace('.', ',');
}

/**
 * Phần trăm đã đi từ cân nặng bắt đầu `tu` tới mục tiêu `den`, kẹp trong 0 đến 100. Dùng được cho cả tăng cân
 * (`den > tu`) lẫn giảm cân (`den < tu`).
 */
export function phanTramMucTieu(hienTai: number, tu: number, den: number): number {
  if (den === tu) return 100;
  const tiLe = (hienTai - tu) / (den - tu);
  return Math.round(Math.max(0, Math.min(1, tiLe)) * 100);
}

/** Số "đẹp" gần `x` (1, 2, 5 nhân lũy thừa 10): làm tròn gần nhất, hoặc làm tròn lên khi `lenTren`. */
function soDep(x: number, lenTren: boolean): number {
  const mu = Math.floor(Math.log10(x));
  const phan = x / 10 ** mu;
  const dep = lenTren
    ? phan <= 1
      ? 1
      : phan <= 2
        ? 2
        : phan <= 5
          ? 5
          : 10
    : phan < 1.5
      ? 1
      : phan < 3
        ? 2
        : phan < 7
          ? 5
          : 10;
  return dep * 10 ** mu;
}

/**
 * Khoảng trục dọc của biểu đồ cân nặng, co giãn theo chính các điểm đo (không cố định): bước chia là số đẹp (1, 2, 5…)
 * sao cho có khoảng `soVach` vạch, hai đầu làm tròn ra ngoài và chừa lề để bao hết điểm. Trả vạch chia từ thấp lên cao.
 */
export function khoangTrucY(
  diem: readonly number[],
  soVach = 5,
): { min: number; max: number; buoc: number; moc: number[] } {
  let thap = Math.min(...diem);
  let cao = Math.max(...diem);
  if (!Number.isFinite(thap) || !Number.isFinite(cao)) {
    thap = 0;
    cao = 1;
  }
  if (cao === thap) {
    thap -= 1;
    cao += 1;
  }
  const buoc = soDep(soDep(cao - thap, true) / (soVach - 1), false);
  let min = Math.floor(thap / buoc) * buoc;
  let max = Math.ceil(cao / buoc) * buoc;
  // Chừa lề: điểm thấp nhất và cao nhất không nằm sát mép trục (cách ít nhất một phần tư bước chia).
  if (thap - min < buoc / 4) min -= buoc;
  if (max - cao < buoc / 4) max += buoc;
  const moc: number[] = [];
  for (let v = min; v <= max + buoc / 1e6; v += buoc) moc.push(Math.round(v * 1000) / 1000);
  return { min: moc[0] ?? min, max: moc.at(-1) ?? max, buoc, moc };
}

/** Cân nặng hợp lệ khi ghi (kg), chặn gõ nhầm như 0 hoặc 6600. */
export const CAN_NANG_TOI_THIEU = 20;
export const CAN_NANG_TOI_DA = 300;

/** Đọc chữ nhập (chấp nhận dấu phẩy hoặc dấu chấm) thành cân nặng một chữ số thập phân; `null` khi trống hoặc ngoài khoảng. */
export function docCanNang(chu: string): number | null {
  const sach = chu.trim();
  if (sach === '') return null;
  const so = Number(sach.replace(',', '.'));
  if (!Number.isFinite(so) || so < CAN_NANG_TOI_THIEU || so > CAN_NANG_TOI_DA) return null;
  return Math.round(so * 10) / 10;
}

/** Tăng hoặc giảm `buoc` kg so với chữ đang nhập (hoặc `goc` khi chưa nhập được số), trả chữ đã định dạng, kẹp trong khoảng hợp lệ. */
export function chinhCanNang(chu: string, buoc: number, goc: number): string {
  const hienTai = docCanNang(chu) ?? goc;
  const moi = Math.round((hienTai + buoc) * 10) / 10;
  return soThapPhan(Math.min(CAN_NANG_TOI_DA, Math.max(CAN_NANG_TOI_THIEU, moi)));
}

/** Cân nặng ghi ở đầu tuần `tuan` (lần cân thứ nhất); `undefined` khi tuần đó chưa có lần cân nào. */
export function canNangTaiTuan(diem: readonly { tuan: number; kg: number }[], tuan: number) {
  return diem.find((d) => d.tuan === tuan)?.kg;
}
