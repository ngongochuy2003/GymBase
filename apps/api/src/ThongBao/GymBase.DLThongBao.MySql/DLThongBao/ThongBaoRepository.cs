using GymBase.DLThongBao.Contract;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using GymBase.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymBase.DLThongBao.MySql;

public sealed class ThongBaoRepository(AppDbContext db) : BaseRepository<thong_bao>(db), IThongBaoRepository
{
    public async Task<IReadOnlyList<thong_bao>> DLFindDueAsync(DateTime now, DateTime nhanTruoc, int soLuong, CancellationToken ct = default) =>
        await Set
            .AsNoTracking()
            .Where(t => t.kenh == KenhThongBao.push
                && ((t.trang_thai_gui == TrangThaiGui.cho_gui && t.thoi_diem_du_kien <= now && (t.thoi_diem_thu_lai == null || t.thoi_diem_thu_lai <= now))
                    || (t.trang_thai_gui == TrangThaiGui.dang_gui && t.thoi_diem_nhan < nhanTruoc)))
            .OrderBy(t => t.thoi_diem_du_kien)
            .Take(soLuong)
            .ToListAsync(ct);

    public async Task<bool> DLTryClaimAsync(thong_bao daThay, DateTime now, CancellationToken ct = default)
    {
        var id = daThay.Id;
        var trangThai = daThay.trang_thai_gui;
        var mocNhan = daThay.thoi_diem_nhan;
        var capNhat = await Set
            .Where(t => t.Id == id && t.trang_thai_gui == trangThai && t.thoi_diem_nhan == mocNhan)
            .ExecuteUpdateAsync(
                s => s.SetProperty(t => t.trang_thai_gui, TrangThaiGui.dang_gui).SetProperty(t => t.thoi_diem_nhan, now),
                ct);
        return capNhat == 1;
    }

    public Task<thong_bao?> DLFindByMaChongTrungAsync(Guid taiKhoanId, string maChongTrung, CancellationToken ct = default) =>
        Set.AsNoTracking().FirstOrDefaultAsync(t => t.tai_khoan_id == taiKhoanId && t.ma_chong_trung == maChongTrung, ct);

    public async Task<IReadOnlyList<thong_bao>> DLFindInboxAsync(
        Guid taiKhoanId, DateTime now, bool chiChuaDoc, int boQua, int soLuong, CancellationToken ct = default) =>
        await Set
            .AsNoTracking()
            .Where(t => t.tai_khoan_id == taiKhoanId && t.thoi_diem_du_kien <= now && (!chiChuaDoc || t.ngay_da_doc == null))
            .OrderByDescending(t => t.thoi_diem_du_kien).ThenByDescending(t => t.Id)
            .Skip(boQua).Take(soLuong)
            .ToListAsync(ct);

    public Task<int> DLCountUnreadAsync(Guid taiKhoanId, DateTime now, CancellationToken ct = default) =>
        Set.CountAsync(t => t.tai_khoan_id == taiKhoanId && t.thoi_diem_du_kien <= now && t.ngay_da_doc == null, ct);

    public Task<int> DLMarkAllReadAsync(Guid taiKhoanId, DateTime now, CancellationToken ct = default) =>
        Set
            .Where(t => t.tai_khoan_id == taiKhoanId && t.thoi_diem_du_kien <= now && t.ngay_da_doc == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.ngay_da_doc, now), ct);
}
