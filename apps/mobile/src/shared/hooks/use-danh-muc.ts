import { useQuery } from '@tanstack/react-query';
import { danhMucApi, type DanhMucItem } from '@/shared/api/danh-muc-api';

/**
 * Danh mục cho dropdown, nhóm theo `ma_dinh_danh`. Loại chưa có mục nào đang dùng không xuất hiện trong kết quả nên
 * `items(ma)` luôn trả mảng (có thể rỗng). Dùng `staleTime` mặc định của app.
 */
export function useDanhMuc(maDinhDanhs: readonly string[]) {
  const query = useQuery({
    queryKey: ['danh-muc', ...maDinhDanhs],
    queryFn: () => danhMucApi.get(maDinhDanhs),
  });
  return {
    isPending: query.isPending,
    isError: query.isError,
    retry: () => void query.refetch(),
    items: (maDinhDanh: string): DanhMucItem[] =>
      query.data?.find((nhom) => nhom.ma_dinh_danh === maDinhDanh)?.items ?? [],
  };
}
