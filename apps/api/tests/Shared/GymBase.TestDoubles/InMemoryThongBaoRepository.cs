using GymBase.DLThongBao.Contract;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;

namespace GymBase.TestDoubles;

public sealed class InMemoryThongBaoRepository(InMemoryStore<thong_bao> store) : InMemoryBaseRepository<thong_bao>(store), IThongBaoRepository
{
    public Task<IReadOnlyList<thong_bao>> DLFindDueAsync(DateTime now, DateTime nhanTruoc, int soLuong, CancellationToken ct = default)
    {
        var list = Store.All()
            .Where(t => t.kenh == KenhThongBao.push
                && ((t.trang_thai_gui == TrangThaiGui.cho_gui && t.thoi_diem_du_kien <= now && (t.thoi_diem_thu_lai == null || t.thoi_diem_thu_lai <= now))
                    || (t.trang_thai_gui == TrangThaiGui.dang_gui && t.thoi_diem_nhan < nhanTruoc)))
            .OrderBy(t => t.thoi_diem_du_kien)
            .Take(soLuong)
            .ToList();
        return Task.FromResult<IReadOnlyList<thong_bao>>(list);
    }

    public Task<bool> DLTryClaimAsync(thong_bao daThay, DateTime now, CancellationToken ct = default)
    {
        lock (Store)
        {
            var hienTai = Store.Find(daThay.Id);
            if (hienTai == null || hienTai.trang_thai_gui != daThay.trang_thai_gui || hienTai.thoi_diem_nhan != daThay.thoi_diem_nhan)
            {
                return Task.FromResult(false);
            }

            hienTai.trang_thai_gui = TrangThaiGui.dang_gui;
            hienTai.thoi_diem_nhan = now;
            return Task.FromResult(Store.TryReplace(hienTai));
        }
    }

    public Task<thong_bao?> DLFindByMaChongTrungAsync(Guid taiKhoanId, string maChongTrung, CancellationToken ct = default) =>
        Task.FromResult(Store.All().FirstOrDefault(t => t.tai_khoan_id == taiKhoanId && t.ma_chong_trung == maChongTrung));

    public Task<IReadOnlyList<thong_bao>> DLFindInboxAsync(
        Guid taiKhoanId, DateTime now, bool chiChuaDoc, int boQua, int soLuong, CancellationToken ct = default)
    {
        var list = Store.All()
            .Where(t => t.tai_khoan_id == taiKhoanId && t.thoi_diem_du_kien <= now && (!chiChuaDoc || t.ngay_da_doc == null))
            .OrderByDescending(t => t.thoi_diem_du_kien).ThenByDescending(t => t.Id)
            .Skip(boQua).Take(soLuong)
            .ToList();
        return Task.FromResult<IReadOnlyList<thong_bao>>(list);
    }

    public Task<int> DLCountUnreadAsync(Guid taiKhoanId, DateTime now, CancellationToken ct = default) =>
        Task.FromResult(Store.All().Count(t => t.tai_khoan_id == taiKhoanId && t.thoi_diem_du_kien <= now && t.ngay_da_doc == null));

    public Task<int> DLMarkAllReadAsync(Guid taiKhoanId, DateTime now, CancellationToken ct = default)
    {
        var dem = 0;
        foreach (var t in Store.All().Where(t => t.tai_khoan_id == taiKhoanId && t.thoi_diem_du_kien <= now && t.ngay_da_doc == null))
        {
            t.ngay_da_doc = now;
            Store.TryReplace(t);
            dem++;
        }

        return Task.FromResult(dem);
    }
}
