import { getMobClient } from '@/core/api/mobile-client';
import type { HopThuDto } from './notification.types';

export interface HopThuQuery {
  chiChuaDoc?: boolean;
  boQua?: number;
  soLuong?: number;
}

/** Các endpoint hộp thư `/api/v1/mob/thong-bao`. */
export const thongBaoApi = {
  async layHopThu({ chiChuaDoc = false, boQua = 0, soLuong = 20 }: HopThuQuery = {}): Promise<HopThuDto> {
    const { data } = await getMobClient().get<HopThuDto>('/thong-bao', {
      params: { chi_chua_doc: chiChuaDoc, bo_qua: boQua, so_luong: soLuong },
    });
    return data;
  },
  async demChuaDoc(): Promise<number> {
    const { data } = await getMobClient().get<{ so_chua_doc: number }>('/thong-bao/so-chua-doc');
    return data.so_chua_doc;
  },
  async danhDauDaDoc(id: string): Promise<void> {
    await getMobClient().post(`/thong-bao/${id}/da-doc`);
  },
  async danhDauTatCaDaDoc(): Promise<void> {
    await getMobClient().post('/thong-bao/da-doc-tat-ca');
  },
  /** Chỉ chạy ở môi trường Development của API: tạo một thông báo cho chính mình. */
  async guiThu(): Promise<void> {
    await getMobClient().post('/thong-bao/thu-nghiem', {});
  },
};
