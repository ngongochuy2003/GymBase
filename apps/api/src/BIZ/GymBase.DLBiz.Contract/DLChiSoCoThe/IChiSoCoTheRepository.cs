using GymBase.Entities.Biz;

namespace GymBase.DLBiz.Contract.DLChiSoCoThe;

/// <summary>Truy cập bảng <c>chi_so_co_the</c>. Bảng chỉ thêm, có khóa chính riêng nên không dùng CRUD chung.</summary>
public interface IChiSoCoTheRepository
{
    /// <summary>Bản ghi mới nhất của học viên (theo <c>ngay_do</c>, sau đó <c>ngay_tao</c>); null nếu chưa có.</summary>
    Task<chi_so_co_the?> DLFindMoiNhatAsync(Guid hocVienId, CancellationToken cancellationToken);
}
