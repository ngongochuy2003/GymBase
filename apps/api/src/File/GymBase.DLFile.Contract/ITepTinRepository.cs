using GymBase.Base.Crud;
using GymBase.Entities.Biz;

namespace GymBase.DLFile.Contract;

public interface ITepTinRepository : IBaseRepository<tep_tin>
{
    Task<IReadOnlyList<tep_tin>> DLGetPendingMigrationAsync(string provider, int limit, CancellationToken ct = default);
}
