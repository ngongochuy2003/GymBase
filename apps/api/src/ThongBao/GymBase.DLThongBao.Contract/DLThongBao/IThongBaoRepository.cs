using GymBase.Base.Crud;
using GymBase.Entities.Biz;

namespace GymBase.DLThongBao.Contract;

public interface IThongBaoRepository : IBaseRepository<thong_bao>
{
    /// <summary>
    /// Dòng push cần gửi: <c>cho_gui</c> đã tới <c>thoi_diem_du_kien</c> và hết thời gian chờ thử lại, hoặc <c>dang_gui</c> mà worker
    /// nhận từ trước <paramref name="nhanTruoc"/> (worker đã chết). Cũ nhất trước, tối đa <paramref name="soLuong"/> dòng.
    /// </summary>
    Task<IReadOnlyList<thong_bao>> DLFindDueAsync(DateTime now, DateTime nhanTruoc, int soLuong, CancellationToken ct = default);

    /// <summary>
    /// Giành một dòng: chỉ thành công khi dòng vẫn đúng trạng thái và mốc nhận như <paramref name="daThay"/>, nên nhiều worker
    /// không gửi trùng một dòng. Trả <c>true</c> cho worker thắng.
    /// </summary>
    Task<bool> DLTryClaimAsync(thong_bao daThay, DateTime now, CancellationToken ct = default);

    Task<thong_bao?> DLFindByMaChongTrungAsync(Guid taiKhoanId, string maChongTrung, CancellationToken ct = default);

    /// <summary>
    /// Hộp thư của một tài khoản: dòng đã tới <c>thoi_diem_du_kien</c> (dòng hẹn giờ chưa tới thì chưa hiện), mới nhất trước.
    /// Sắp theo <c>thoi_diem_du_kien</c> giảm dần rồi <c>Id</c> giảm dần; bỏ qua <paramref name="boQua"/> dòng đầu.
    /// </summary>
    Task<IReadOnlyList<thong_bao>> DLFindInboxAsync(
        Guid taiKhoanId, DateTime now, bool chiChuaDoc, int boQua, int soLuong, CancellationToken ct = default);

    Task<int> DLCountUnreadAsync(Guid taiKhoanId, DateTime now, CancellationToken ct = default);

    /// <summary>Đánh dấu đã đọc mọi dòng chưa đọc (đã hiện) của tài khoản; trả số dòng đổi.</summary>
    Task<int> DLMarkAllReadAsync(Guid taiKhoanId, DateTime now, CancellationToken ct = default);
}
