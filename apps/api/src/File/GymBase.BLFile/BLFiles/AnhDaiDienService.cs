using GymBase.Base.Exceptions;
using GymBase.BLFile.BLStorage;
using GymBase.DLFile.Contract;
using GymBase.Entities.Biz.Enums;
using GymBase.ModuleContracts.TepTin;
using Microsoft.Extensions.Logging;

namespace GymBase.BLFile.BLFiles;

/// <summary>
/// Ảnh đại diện của hồ sơ. Ảnh do chính chủ tải lên trước (<c>POST /files</c> với <c>purpose = avatar</c>), rồi hồ sơ tham chiếu
/// <c>file_id</c>. Mỗi tài khoản giữ một ảnh: ảnh cũ bị đánh dấu đã xóa và xóa khỏi kho lưu trữ.
/// </summary>
public sealed class AnhDaiDienService(
    ITepTinRepository tepTins,
    IStorageResolver storageResolver,
    TimeProvider timeProvider,
    ILogger<AnhDaiDienService> logger) : IAnhDaiDienLienKet
{
    private const string Field = "anh_dai_dien_tep_tin_id";

    public async Task BLGanAnhDaiDienAsync(Guid taiKhoanId, Guid tepTinId, CancellationToken cancellationToken)
    {
        var file = await tepTins.DLGetByIdAsync(tepTinId, cancellationToken);
        var cuaMinh = file is not null
            && file.nguoi_tai_len_id == taiKhoanId
            && file.muc_dich == IAnhDaiDienLienKet.MucDich
            && file.loai_mime.StartsWith("image/", StringComparison.Ordinal);
        var dangGan = cuaMinh
            && file!.trang_thai == TrangThaiTepTin.da_lien_ket
            && file.loai_doi_tuong == LoaiDoiTuongTepTin.ho_so_nguoi_dung
            && file.doi_tuong_id == taiKhoanId;
        if (dangGan)
        {
            return;
        }

        if (!cuaMinh || file!.trang_thai != TrangThaiTepTin.da_tai_len)
        {
            throw new ValidationFailedException(Field, "Ảnh đại diện không hợp lệ hoặc đã hết hạn. Vui lòng chọn lại ảnh.");
        }

        file.loai_doi_tuong = LoaiDoiTuongTepTin.ho_so_nguoi_dung;
        file.doi_tuong_id = taiKhoanId;
        file.trang_thai = TrangThaiTepTin.da_lien_ket;
        file.ngay_xac_nhan = timeProvider.GetUtcNow().UtcDateTime;
        await tepTins.DLUpdateAsync(file, cancellationToken);
    }

    public async Task BLGoAnhCuAsync(Guid taiKhoanId, Guid giuLaiId, CancellationToken cancellationToken)
    {
        var cacAnh = await tepTins.DLFindLinkedAsync(
            LoaiDoiTuongTepTin.ho_so_nguoi_dung, taiKhoanId, IAnhDaiDienLienKet.MucDich, cancellationToken);
        foreach (var cu in cacAnh.Where(t => t.Id != giuLaiId))
        {
            cu.trang_thai = TrangThaiTepTin.da_xoa;
            cu.ngay_xac_nhan = timeProvider.GetUtcNow().UtcDateTime;
            await tepTins.DLUpdateAsync(cu, cancellationToken);
            try
            {
                await storageResolver.Resolve(cu.s3_bucket).DeleteAsync(cu.s3_key, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Không xóa được ảnh đại diện cũ {FileId} khỏi kho lưu trữ.", cu.Id);
            }
        }
    }
}
