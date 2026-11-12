using GymBase.Base.Crud;
using GymBase.Entities.Biz;

namespace GymBase.DLBiz.Contract;

/// <summary>Tổng hợp số lượt thích, số bình luận và trạng thái đã thích của người dùng hiện tại trên một bài viết.</summary>
public sealed record PostInteractionSummary(int so_luot_thich, int so_binh_luan, bool da_thich);

/// <summary>Truy cập dữ liệu bảng <c>tuong_tac_bai_viet</c> (lượt thích và bình luận).</summary>
public interface ITuongTacBaiVietRepository : IBaseRepository<tuong_tac_bai_viet>
{
    /// <summary>Tổng hợp lượt thích, bình luận còn hiệu lực (<c>ngay_xoa == null</c>) cho danh sách bài viết.</summary>
    Task<IReadOnlyDictionary<Guid, PostInteractionSummary>> GetSummariesByPostIdsAsync(
        IReadOnlyCollection<Guid> postIds,
        Guid? currentUserId,
        CancellationToken cancellationToken);

    /// <summary>Tìm bản ghi lượt thích của một tài khoản trên một bài viết (kể cả đã xóa mềm để khôi phục hoặc bật/tắt).</summary>
    Task<tuong_tac_bai_viet?> FindLikeByUserAsync(
        Guid baiVietId,
        Guid taiKhoanId,
        CancellationToken cancellationToken);

    /// <summary>Lấy các bình luận còn hiệu lực của một bài viết, sắp theo thời gian tạo tăng dần.</summary>
    Task<IReadOnlyList<tuong_tac_bai_viet>> GetCommentsByPostIdAsync(
        Guid baiVietId,
        CancellationToken cancellationToken);
}
