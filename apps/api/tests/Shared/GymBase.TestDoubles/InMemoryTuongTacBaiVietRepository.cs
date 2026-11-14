using GymBase.DLBiz.Contract;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;

namespace GymBase.TestDoubles;

/// <summary><see cref="ITuongTacBaiVietRepository"/> trong bộ nhớ dùng cho unit/integration test.</summary>
public sealed class InMemoryTuongTacBaiVietRepository(InMemoryStore<tuong_tac_bai_viet> store)
    : InMemoryBaseRepository<tuong_tac_bai_viet>(store), ITuongTacBaiVietRepository
{
    public Task<IReadOnlyDictionary<Guid, PostInteractionSummary>> DLGetSummariesByPostIdsAsync(
        IReadOnlyCollection<Guid> postIds,
        Guid? currentUserId,
        CancellationToken cancellationToken)
    {
        var active = Store.All()
            .Where(t => postIds.Contains(t.bai_viet_id) && t.ngay_xoa is null)
            .ToList();

        var dict = new Dictionary<Guid, PostInteractionSummary>();
        foreach (var postId in postIds)
        {
            var postInteractions = active.Where(t => t.bai_viet_id == postId).ToList();
            var likes = postInteractions.Count(t => t.loai_tuong_tac == LoaiTuongTac.thich);
            var comments = postInteractions.Count(t => t.loai_tuong_tac == LoaiTuongTac.binh_luan);
            var liked = currentUserId.HasValue
                        && postInteractions.Any(t => t.loai_tuong_tac == LoaiTuongTac.thich && t.tai_khoan_id == currentUserId.Value);
            dict[postId] = new PostInteractionSummary(likes, comments, liked);
        }

        return Task.FromResult<IReadOnlyDictionary<Guid, PostInteractionSummary>>(dict);
    }

    public Task<tuong_tac_bai_viet?> DLFindLikeByUserAsync(
        Guid baiVietId,
        Guid taiKhoanId,
        CancellationToken cancellationToken) =>
        Task.FromResult(Store.All().FirstOrDefault(
            t => t.bai_viet_id == baiVietId
                 && t.tai_khoan_id == taiKhoanId
                 && t.loai_tuong_tac == LoaiTuongTac.thich));

    public Task<IReadOnlyList<tuong_tac_bai_viet>> DLGetCommentsByPostIdAsync(
        Guid baiVietId,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<tuong_tac_bai_viet>>(
            Store.All()
                .Where(t => t.bai_viet_id == baiVietId && t.loai_tuong_tac == LoaiTuongTac.binh_luan && t.ngay_xoa is null)
                .OrderBy(t => t.ngay_tao)
                .ToList());
}
