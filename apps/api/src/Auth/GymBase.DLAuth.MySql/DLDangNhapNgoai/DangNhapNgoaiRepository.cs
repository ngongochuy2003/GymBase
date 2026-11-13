using GymBase.DLAuth.Contract.DLDangNhapNgoai;
using GymBase.Entities.Auth;
using GymBase.Entities.Auth.Enums;
using GymBase.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymBase.DLAuth.MySql.DLDangNhapNgoai;

/// <summary>Truy cập bảng <c>dang_nhap_ngoai</c>.</summary>
public sealed class DangNhapNgoaiRepository(AppDbContext db) : BaseRepository<dang_nhap_ngoai>(db), IDangNhapNgoaiRepository
{
    public Task<dang_nhap_ngoai?> DLFindByMaNgoaiAsync(NhaCungCap nhaCungCap, string maNgoai, CancellationToken cancellationToken) =>
        Set.FirstOrDefaultAsync(e => e.nha_cung_cap == nhaCungCap && e.ma_ngoai == maNgoai, cancellationToken);
}
