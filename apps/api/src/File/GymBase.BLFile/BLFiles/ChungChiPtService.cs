using GymBase.Base.Exceptions;
using GymBase.BLFile.BLStorage;
using GymBase.DLFile.Contract;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using GymBase.ModuleContracts.TepTin;
using Microsoft.Extensions.Logging;

namespace GymBase.BLFile.BLFiles;

/// <summary>
/// Gắn tệp chứng chỉ PT vào hồ sơ. Tệp do chính chủ tải lên trước (<c>POST /files</c> với <c>purpose = chung_chi_pt</c>, kiểu PDF),
/// rồi hồ sơ tham chiếu <c>file_id</c>. Mỗi hồ sơ có tối đa một chứng chỉ: gắn tệp mới thì tệp cũ bị gỡ và xóa khỏi kho lưu trữ.
/// </summary>
public sealed class ChungChiPtService(
    ITepTinRepository tepTins,
    IStorageResolver storageResolver,
    TimeProvider timeProvider,
    ILogger<ChungChiPtService> logger) : IChungChiPtLienKet
{
    private const string Field = "chung_chi_tep_tin_id";

    public async Task<ChungChiPtDto?> BLDatChungChiAsync(Guid taiKhoanId, Guid? tepTinId, CancellationToken cancellationToken)
    {
        var current = await tepTins.DLFindLinkedAsync(
            LoaiDoiTuongTepTin.ho_so_nguoi_dung, taiKhoanId, IChungChiPtLienKet.MucDich, cancellationToken);

        if (tepTinId is not { } id)
        {
            await RetireAsync(current, keepId: null, cancellationToken);
            return null;
        }

        var file = current.FirstOrDefault(t => t.Id == id) ?? await FindUsableAsync(taiKhoanId, id, cancellationToken);
        if (file.trang_thai != TrangThaiTepTin.da_lien_ket)
        {
            file.loai_doi_tuong = LoaiDoiTuongTepTin.ho_so_nguoi_dung;
            file.doi_tuong_id = taiKhoanId;
            file.trang_thai = TrangThaiTepTin.da_lien_ket;
            file.ngay_xac_nhan = timeProvider.GetUtcNow().UtcDateTime;
            await tepTins.DLUpdateAsync(file, cancellationToken);
        }

        await RetireAsync(current, keepId: file.Id, cancellationToken);
        return ToDto(file);
    }

    public async Task<ChungChiPtDto?> BLLayChungChiAsync(Guid taiKhoanId, CancellationToken cancellationToken)
    {
        var current = await tepTins.DLFindLinkedAsync(
            LoaiDoiTuongTepTin.ho_so_nguoi_dung, taiKhoanId, IChungChiPtLienKet.MucDich, cancellationToken);
        return current.Count == 0 ? null : ToDto(current[0]);
    }

    /// <exception cref="ValidationFailedException">Tệp không tồn tại, của người khác, sai mục đích hoặc kiểu, hoặc đã bị xóa.</exception>
    private async Task<tep_tin> FindUsableAsync(Guid taiKhoanId, Guid id, CancellationToken cancellationToken)
    {
        var file = await tepTins.DLGetByIdAsync(id, cancellationToken);
        var usable = file is not null
            && file.nguoi_tai_len_id == taiKhoanId
            && file.muc_dich == IChungChiPtLienKet.MucDich
            && file.trang_thai == TrangThaiTepTin.da_tai_len
            && file.loai_mime == "application/pdf";
        return usable
            ? file!
            : throw new ValidationFailedException(Field, "Tệp chứng chỉ không hợp lệ hoặc đã hết hạn. Vui lòng chọn lại tệp PDF.");
    }

    /// <summary>Gỡ các chứng chỉ cũ (trừ <paramref name="keepId"/>): đánh dấu đã xóa và xóa khỏi kho lưu trữ, lỗi xóa chỉ ghi log.</summary>
    private async Task RetireAsync(IReadOnlyList<tep_tin> files, Guid? keepId, CancellationToken cancellationToken)
    {
        foreach (var old in files.Where(f => f.Id != keepId))
        {
            old.trang_thai = TrangThaiTepTin.da_xoa;
            old.ngay_xac_nhan = timeProvider.GetUtcNow().UtcDateTime;
            await tepTins.DLUpdateAsync(old, cancellationToken);
            try
            {
                await storageResolver.Resolve(old.s3_bucket).DeleteAsync(old.s3_key, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Không xóa được tệp chứng chỉ {FileId} khỏi kho lưu trữ.", old.Id);
            }
        }
    }

    private static ChungChiPtDto ToDto(tep_tin file) =>
        new(file.Id, file.ten_tep_goc ?? "chung-chi.pdf", file.kich_thuoc_byte);
}
