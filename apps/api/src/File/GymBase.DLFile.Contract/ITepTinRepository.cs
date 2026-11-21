using GymBase.Base.Crud;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;

namespace GymBase.DLFile.Contract;

public interface ITepTinRepository : IBaseRepository<tep_tin>
{
    Task<IReadOnlyList<tep_tin>> DLGetPendingMigrationAsync(string provider, int limit, CancellationToken ct = default);

    /// <summary>Các tệp đã gắn (<c>da_lien_ket</c>) vào một đối tượng với một mục đích, mới nhất trước.</summary>
    Task<IReadOnlyList<tep_tin>> DLFindLinkedAsync(
        LoaiDoiTuongTepTin loaiDoiTuong, Guid doiTuongId, string mucDich, CancellationToken ct = default);
}
