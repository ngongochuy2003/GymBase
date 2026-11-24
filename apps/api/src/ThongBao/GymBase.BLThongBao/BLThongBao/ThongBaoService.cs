using GymBase.Base.Exceptions;
using GymBase.BLThongBao.BLDayThongBao;
using GymBase.DLThongBao.Contract;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using GymBase.ModuleContracts.ThongBao;

namespace GymBase.BLThongBao.BLThongBao;

/// <summary>
/// Hộp thư thông báo: tạo dòng (cho các nhóm khác, qua <see cref="IThongBaoTao"/>) và đọc, đánh dấu đã đọc cho app.
/// <c>thong_bao</c> là nguồn sự thật; push chỉ là cái chuông (ADR-2, quyết định 1).
/// </summary>
public sealed class ThongBaoService(
    IThongBaoRepository thongBaos,
    ICaiDatThongBaoRepository caiDats,
    TimeProvider timeProvider) : IThongBaoTao
{
    private const int TieuDeToiDa = 160;
    private const int MaChongTrungToiDa = 160;
    private const int LoaiDoiTuongToiDa = 32;
    private const int SoLuongToiDa = 50;

    public async Task<Guid> BLTaoAsync(TaoThongBaoRequest request, CancellationToken ct = default)
    {
        var loai = KiemTra(request);

        var daCo = await thongBaos.DLFindByMaChongTrungAsync(request.TaiKhoanId, request.MaChongTrung, ct);
        if (daCo != null)
        {
            return daCo.Id;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var dong = new thong_bao
        {
            tai_khoan_id = request.TaiKhoanId,
            loai_thong_bao = loai,
            tieu_de = request.TieuDe.Trim(),
            noi_dung = request.NoiDung,
            loai_doi_tuong = request.LoaiDoiTuong,
            doi_tuong_id = request.DoiTuongId,
            thoi_diem_du_kien = request.ThoiDiemDuKien ?? now,
            trang_thai_gui = TrangThaiGui.cho_gui,
            kenh = await ChonKenhAsync(request.TaiKhoanId, loai, ct),
            ma_chong_trung = request.MaChongTrung,
            ngay_tao = now,
        };

        try
        {
            await thongBaos.DLInsertAsync(dong, ct);
        }
        catch (ConflictException)
        {
            // Hai request cùng mã chống trùng chạy song song: dòng kia thắng, trả Id của nó.
            var thang = await thongBaos.DLFindByMaChongTrungAsync(request.TaiKhoanId, request.MaChongTrung, ct);
            return thang?.Id ?? throw new ConflictException();
        }

        return dong.Id;
    }

    /// <summary>
    /// Hộp thư của người dùng, mới nhất trước, chỉ gồm dòng đã tới giờ. Phân trang theo <paramref name="boQua"/>; app bỏ trùng theo Id
    /// nên dòng mới chen vào giữa các lần tải không gây hại.
    /// </summary>
    public async Task<HopThuDto> BLLayHopThuAsync(Guid taiKhoanId, bool chiChuaDoc, int boQua, int soLuong, CancellationToken ct = default)
    {
        soLuong = Math.Clamp(soLuong, 1, SoLuongToiDa);
        boQua = Math.Max(0, boQua);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var dongs = await thongBaos.DLFindInboxAsync(taiKhoanId, now, chiChuaDoc, boQua, soLuong + 1, ct);
        return new HopThuDto(dongs.Take(soLuong).Select(ToDto).ToList(), dongs.Count > soLuong);
    }

    public Task<int> BLDemChuaDocAsync(Guid taiKhoanId, CancellationToken ct = default) =>
        thongBaos.DLCountUnreadAsync(taiKhoanId, timeProvider.GetUtcNow().UtcDateTime, ct);

    /// <summary>Đánh dấu một dòng đã đọc; gọi lại thì giữ nguyên mốc đọc đầu tiên.</summary>
    /// <exception cref="NotFoundException">Không có dòng, hoặc dòng của người khác (không lộ là có tồn tại).</exception>
    public async Task BLDanhDauDaDocAsync(Guid taiKhoanId, Guid thongBaoId, CancellationToken ct = default)
    {
        var dong = await thongBaos.DLGetByIdAsync(thongBaoId, ct);
        if (dong == null || dong.tai_khoan_id != taiKhoanId)
        {
            throw new NotFoundException();
        }

        if (dong.ngay_da_doc == null)
        {
            dong.ngay_da_doc = timeProvider.GetUtcNow().UtcDateTime;
            await thongBaos.DLUpdateAsync(dong, ct);
        }
    }

    public Task<int> BLDanhDauTatCaDaDocAsync(Guid taiKhoanId, CancellationToken ct = default) =>
        thongBaos.DLMarkAllReadAsync(taiKhoanId, timeProvider.GetUtcNow().UtcDateTime, ct);

    /// <summary>Loại đã tắt thì ghi dòng nhưng không chuông (quyết định 19); còn lại theo kênh mặc định của loại.</summary>
    private async Task<KenhThongBao> ChonKenhAsync(Guid taiKhoanId, LoaiThongBao loai, CancellationToken ct)
    {
        var caiDat = await caiDats.DLFindByTaiKhoanAsync(taiKhoanId, ct);
        var daTat = caiDat.Any(c => c.loai_thong_bao == loai && !c.bat);
        return daTat ? KenhThongBao.chi_hop_thu : ThongBaoPolicy.KenhMacDinh(loai);
    }

    private static LoaiThongBao KiemTra(TaoThongBaoRequest request)
    {
        var loi = new Dictionary<string, string[]>();

        var loaiHopLe = Enum.TryParse<LoaiThongBao>(request.LoaiThongBao, ignoreCase: false, out var loai) && Enum.IsDefined(loai);
        if (!loaiHopLe)
        {
            loi["loai_thong_bao"] = ["Loại thông báo không hợp lệ."];
        }

        if (string.IsNullOrWhiteSpace(request.TieuDe) || request.TieuDe.Trim().Length > TieuDeToiDa)
        {
            loi["tieu_de"] = [$"Tiêu đề bắt buộc và tối đa {TieuDeToiDa} ký tự."];
        }

        if (string.IsNullOrWhiteSpace(request.MaChongTrung) || request.MaChongTrung.Length > MaChongTrungToiDa)
        {
            loi["ma_chong_trung"] = [$"Mã chống trùng bắt buộc và tối đa {MaChongTrungToiDa} ký tự."];
        }

        if (request.LoaiDoiTuong is { Length: > LoaiDoiTuongToiDa })
        {
            loi["loai_doi_tuong"] = [$"Loại đối tượng tối đa {LoaiDoiTuongToiDa} ký tự."];
        }

        if (loi.Count > 0)
        {
            throw new ValidationFailedException(loi);
        }

        return loai;
    }

    private static ThongBaoDto ToDto(thong_bao t) => new(
        t.Id, t.loai_thong_bao.ToString(), t.tieu_de, t.noi_dung, t.loai_doi_tuong, t.doi_tuong_id, t.thoi_diem_du_kien, t.ngay_da_doc != null);
}
