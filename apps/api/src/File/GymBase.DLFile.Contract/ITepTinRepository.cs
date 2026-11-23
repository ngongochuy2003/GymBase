using GymBase.Base.Crud;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;

namespace GymBase.DLFile.Contract;

public interface ITepTinRepository : IBaseRepository<tep_tin>
{
    /// <summary>Các tệp theo danh sách <c>Id</c>, bỏ qua Id không có. Không bảo đảm thứ tự.</summary>
    Task<IReadOnlyList<tep_tin>> DLFindByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);

    /// <summary>Mọi tệp một tài khoản đã xin upload từ <paramref name="tuNgayUtc"/> (mọi trạng thái), để đếm hạn mức.</summary>
    Task<IReadOnlyList<tep_tin>> DLFindByUploaderSinceAsync(Guid nguoiTaiLenId, DateTime tuNgayUtc, CancellationToken ct = default);

    /// <summary>Các tệp đã gắn (<c>da_lien_ket</c>) vào một đối tượng với một mục đích, mới nhất trước.</summary>
    Task<IReadOnlyList<tep_tin>> DLFindLinkedAsync(
        LoaiDoiTuongTepTin loaiDoiTuong, Guid doiTuongId, string mucDich, CancellationToken ct = default);
}
