import type { AxiosInstance } from 'axios';
import { createMobHttpClient } from './mobile-client';

/** Một mục danh mục: app hiển thị `ten`, gửi lại `ma` (docs/api/danh-muc.md). */
export type DanhMucItem = {
  ma: string;
  ten: string;
  mo_ta: string | null;
  cap: number | null;
  danh_muc_cap_tren_id: string | null;
};

export type DanhMucNhom = { ma_dinh_danh: string; items: DanhMucItem[] };

let sharedClient: AxiosInstance | undefined;

/** Endpoint `/api/v1/mob/danh-muc`: dữ liệu tham chiếu cho dropdown, chỉ gồm mục đang dùng. */
export const danhMucApi = {
  async get(maDinhDanhs: readonly string[]): Promise<DanhMucNhom[]> {
    sharedClient ??= createMobHttpClient();
    const { data } = await sharedClient.get<DanhMucNhom[]>('/danh-muc', {
      params: { ma_dinh_danh: maDinhDanhs.join(',') },
    });
    return data;
  },
};
