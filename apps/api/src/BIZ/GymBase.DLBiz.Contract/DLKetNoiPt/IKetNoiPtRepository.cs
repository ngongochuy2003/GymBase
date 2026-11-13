using GymBase.Base.Crud;
using GymBase.Entities.Biz;

namespace GymBase.DLBiz.Contract.DLKetNoiPt;

/// <summary>Truy cập kết nối PT với học viên. CRUD lấy từ <see cref="IBaseRepository{TEntity}"/>.</summary>
public interface IKetNoiPtRepository : IBaseRepository<ket_noi_pt>
{
    /// <summary>
    /// Kết nối đang <c>dang_hoat_dong</c> giữa PT và học viên; null nếu không có. Mỗi cặp chỉ có tối đa một kết nối đang
    /// mở (unique index), nên kết quả không bao giờ mơ hồ.
    /// </summary>
    Task<ket_noi_pt?> DLFindDangHoatDongAsync(Guid ptId, Guid hocVienId, CancellationToken cancellationToken);
}
