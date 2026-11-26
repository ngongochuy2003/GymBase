using GymBase.DLDI.Contract.DLDanhMuc;
using GymBase.ModuleContracts.DanhMuc;

namespace GymBase.BLDI.BLDanhMuc;

/// <summary>Hiện thực <see cref="IDanhMucTraCuu"/> cho nhóm khác kiểm tra mã danh mục.</summary>
public sealed class DanhMucTraCuu(IDanhMucRepository danhMucRepository) : IDanhMucTraCuu
{
    public Task<bool> BLKiemTraDangDungAsync(string maDinhDanh, string ma, CancellationToken cancellationToken) =>
        danhMucRepository.DLExistsByMaAsync(maDinhDanh, ma, cancellationToken);
}
