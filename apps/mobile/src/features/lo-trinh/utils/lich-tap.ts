import type { BuoiTapTuan, LichTapTheoTuan } from '../types';

const TEN_THU = ['Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'Chủ nhật'];

/** Tên thứ trong tuần: 1 là Thứ 2 đến 7 là Chủ nhật. */
export function tenThu(thu: number): string {
  return TEN_THU[thu - 1] ?? '';
}

/** Danh sách buổi tập hiệu lực của một tuần: tuần 1 là bản chung, tuần sau theo bản chung trừ tuần đã sửa riêng. */
export function buoiTapCuaTuan(lich: LichTapTheoTuan, tuan: number): BuoiTapTuan[] {
  return tuan === 1 ? lich.chung : (lich.rieng[tuan] ?? lich.chung);
}

/** Ghi danh sách buổi tập của một tuần: tuần 1 sửa bản chung, tuần khác tách riêng tuần đó (adr.md, N29). */
export function ghiBuoiTapCuaTuan(
  lich: LichTapTheoTuan,
  tuan: number,
  moi: BuoiTapTuan[],
): LichTapTheoTuan {
  return tuan === 1 ? { ...lich, chung: moi } : { ...lich, rieng: { ...lich.rieng, [tuan]: moi } };
}

/** Sửa một phần của buổi tập rơi vào thứ `thu` trong tuần `tuan`; không có buổi đó thì giữ nguyên. */
export function suaBuoiTapCuaTuan(
  lich: LichTapTheoTuan,
  tuan: number,
  thu: number,
  phan: Partial<BuoiTapTuan>,
): LichTapTheoTuan {
  return ghiBuoiTapCuaTuan(
    lich,
    tuan,
    buoiTapCuaTuan(lich, tuan).map((b) => (b.thu === thu ? { ...b, ...phan } : b)),
  );
}

/** Bỏ buổi tập rơi vào thứ `thu` khỏi tuần `tuan`. */
export function xoaBuoiTapCuaTuan(
  lich: LichTapTheoTuan,
  tuan: number,
  thu: number,
): LichTapTheoTuan {
  return ghiBuoiTapCuaTuan(
    lich,
    tuan,
    buoiTapCuaTuan(lich, tuan).filter((b) => b.thu !== thu),
  );
}

let demBaiTap = 0;

/** Id tạm cho bài tập vừa thêm trong form, duy nhất trong phiên (id thật do backend cấp khi lưu). */
// TODO(ROUTE): bỏ khi backend cấp id cho bài tập lúc lưu lộ trình.
export function taoIdBaiTap(): string {
  demBaiTap += 1;
  return `bt-moi-${demBaiTap}`;
}
