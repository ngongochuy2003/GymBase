using GymBase.DLBiz.Contract;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using GymBase.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymBase.DLBiz.MySql;

/// <summary>Truy cập bảng <c>tuong_tac_bai_viet</c> (lượt thích và bình luận) bằng EF Core (MySQL).</summary>
public sealed class TuongTacBaiVietRepository(AppDbContext db)
    : BaseRepository<tuong_tac_bai_viet>(db), ITuongTacBaiVietRepository
{
    public async Task<IReadOnlyDictionary<Guid, PostInteractionSummary>> DLGetSummariesByPostIdsAsync(
        IReadOnlyCollection<Guid> postIds,
        Guid? currentUserId,
        CancellationToken cancellationToken)
    {
        if (postIds.Count == 0)
        {
            return new Dictionary<Guid, PostInteractionSummary>();
        }

        var interactions = await Set
            .AsNoTracking()
            .Where(t => postIds.Contains(t.bai_viet_id) && t.ngay_xoa == null)
            .Select(t => new { t.bai_viet_id, t.tai_khoan_id, t.loai_tuong_tac })
            .ToListAsync(cancellationToken);

        var result = new Dictionary<Guid, PostInteractionSummary>(postIds.Count);
        foreach (var postId in postIds)
        {
            result[postId] = new PostInteractionSummary(0, 0, false);
        }

        foreach (var group in interactions.GroupBy(x => x.bai_viet_id))
        {
            var likes = 0;
            var comments = 0;
            var likedByCurrent = false;

            foreach (var item in group)
            {
                if (item.loai_tuong_tac == LoaiTuongTac.thich)
                {
                    likes++;
                    if (currentUserId.HasValue && item.tai_khoan_id == currentUserId.Value)
                    {
                        likedByCurrent = true;
                    }
                }
                else if (item.loai_tuong_tac == LoaiTuongTac.binh_luan)
                {
                    comments++;
                }
            }

            result[group.Key] = new PostInteractionSummary(likes, comments, likedByCurrent);
        }

        return result;
    }

    public Task<tuong_tac_bai_viet?> DLFindLikeByUserAsync(
        Guid baiVietId,
        Guid taiKhoanId,
        CancellationToken cancellationToken) =>
        Set.AsNoTracking()
            .OrderByDescending(t => t.ngay_xoa == null)
            .ThenByDescending(t => t.ngay_tao)
            .FirstOrDefaultAsync(
                t => t.bai_viet_id == baiVietId
                     && t.tai_khoan_id == taiKhoanId
                     && t.loai_tuong_tac == LoaiTuongTac.thich,
                cancellationToken);

    public async Task<IReadOnlyList<tuong_tac_bai_viet>> DLGetCommentsByPostIdAsync(
        Guid baiVietId,
        CancellationToken cancellationToken) =>
        await Set.AsNoTracking()
            .Where(t =>
                t.bai_viet_id == baiVietId
                && t.loai_tuong_tac == LoaiTuongTac.binh_luan
                && t.ngay_xoa == null)
            .OrderBy(t => t.ngay_tao)
            .ThenBy(t => t.Id)
            .ToListAsync(cancellationToken);
}
