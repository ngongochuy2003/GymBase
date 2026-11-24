import { useCallback } from 'react';
import { useInfiniteQuery, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useRouter, type Href } from 'expo-router';
import { thongBaoApi } from './thong-bao-api';

export const THONG_BAO_QUERY_KEY = ['thong-bao'] as const;
const SO_LUONG_MOT_TRANG = 20;

/** Số thông báo chưa đọc, cho huy hiệu trên chuông. Chưa tải xong hoặc lỗi thì coi là 0. */
export function useSoChuaDoc(): number {
  const query = useQuery({
    queryKey: [...THONG_BAO_QUERY_KEY, 'so-chua-doc'],
    queryFn: () => thongBaoApi.demChuaDoc(),
  });
  return query.data ?? 0;
}

/** Hộp thư, tải thêm theo trang. Dòng trùng Id (do dòng mới chen vào giữa các lần tải) được bỏ. */
export function useHopThu() {
  const query = useInfiniteQuery({
    queryKey: [...THONG_BAO_QUERY_KEY, 'hop-thu'],
    queryFn: ({ pageParam }) => thongBaoApi.layHopThu({ boQua: pageParam, soLuong: SO_LUONG_MOT_TRANG }),
    initialPageParam: 0,
    getNextPageParam: (trangCuoi, cacTrang) =>
      trangCuoi.con_nua ? cacTrang.reduce((tong, trang) => tong + trang.data.length, 0) : undefined,
  });

  const thongBaos = [
    ...new Map((query.data?.pages.flatMap((trang) => trang.data) ?? []).map((t) => [t.id, t])).values(),
  ];
  return { ...query, thongBaos };
}

function useLamMoiSauKhiDoi<TVars>(mutationFn: (vars: TVars) => Promise<void>) {
  const client = useQueryClient();
  return useMutation({
    mutationFn,
    onSuccess: () => client.invalidateQueries({ queryKey: THONG_BAO_QUERY_KEY }),
  });
}

export const useDanhDauDaDoc = () => useLamMoiSauKhiDoi((id: string) => thongBaoApi.danhDauDaDoc(id));

export const useDanhDauTatCaDaDoc = () => useLamMoiSauKhiDoi(() => thongBaoApi.danhDauTatCaDaDoc());

export const useGuiThongBaoThu = () => useLamMoiSauKhiDoi(() => thongBaoApi.guiThu());

/** Cho chuông ở các header: có chưa đọc không, và mở màn thông báo. */
export function useThongBaoHeader() {
  const router = useRouter();
  const soChuaDoc = useSoChuaDoc();
  const moThongBao = useCallback(() => router.push('/thong-bao' as Href), [router]);
  return { soChuaDoc, coChuaDoc: soChuaDoc > 0, moThongBao };
}
