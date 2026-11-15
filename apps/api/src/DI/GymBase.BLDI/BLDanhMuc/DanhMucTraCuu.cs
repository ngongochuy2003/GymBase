using GymBase.DLDI.Contract.DLDanhMuc;
using GymBase.ModuleContracts.DanhMuc;

namespace GymBase.BLDI.BLDanhMuc;

/// <summary>Hiện thực <see cref="IDanhMucTraCuu"/> cho nhóm khác kiểm tra mã danh mục.</summary>
public sealed class DanhMucTraCuu(IDanhMucRepository danhMucRepository) : IDanhMucTraCuu
{
    public async Task<bool> BLKiemTraDangDungAsync(string maDinhDanh, string ma, CancellationToken cancellationToken)
    {
        var cacMuc = await danhMucRepository.DLFindDangDungAsync([maDinhDanh], cancellationToken);
        return cacMuc.Any(e => e.ma == ma);
    }
}
