using GymBase.DLAuth.Contract.DLDangNhapNgoai;
using GymBase.Entities.Auth;
using GymBase.Entities.Auth.Enums;

namespace GymBase.TestDoubles;

/// <summary>Repository liên kết đăng nhập ngoài giả cho test.</summary>
public sealed class InMemoryDangNhapNgoaiRepository(InMemoryStore<dang_nhap_ngoai> store)
    : InMemoryBaseRepository<dang_nhap_ngoai>(store), IDangNhapNgoaiRepository
{
    public Task<dang_nhap_ngoai?> DLFindByMaNgoaiAsync(NhaCungCap nhaCungCap, string maNgoai, CancellationToken cancellationToken) =>
        Task.FromResult(Store.All().FirstOrDefault(e => e.nha_cung_cap == nhaCungCap && e.ma_ngoai == maNgoai));
}
