using GymBase.Base.Crud;
using GymBase.Base.Paging;
using GymBase.Entities.Biz;

namespace GymBase.DLBiz.Contract;

/// <summary>Truy cập dữ liệu bảng <c>bai_viet</c> và các thực thể liên quan trực tiếp trên bảng tin (hồ sơ tác giả, tệp đính kèm).</summary>
public interface IBaiVietRepository : IBaseRepository<bai_viet>
{
    /// <summary>
    /// Lấy danh sách bài viết cho bảng tin theo tab (<c>for-you</c>, <c>trending</c>, <c>workout</c>, <c>knowledge</c>).
    /// Chỉ trả bài viết ở trạng thái <c>da_dang</c> mà người dùng hiện tại được phép xem.
    /// </summary>
    Task<PagingResult<bai_viet>> GetFeedPagingAsync(
        string? tab,
        Guid? currentUserId,
        PagingRequest request,
        CancellationToken cancellationToken);

    /// <summary>Lấy danh sách tệp đính kèm (<c>loai_doi_tuong = bai_viet</c>) theo danh sách Id bài viết, sắp theo <c>thu_tu_hien_thi</c>.</summary>
    Task<IReadOnlyDictionary<Guid, IReadOnlyList<tep_tin>>> GetMediaByPostIdsAsync(
        IReadOnlyCollection<Guid> postIds,
        CancellationToken cancellationToken);

    /// <summary>Lưu danh sách đường dẫn ảnh/video đính kèm của bài viết vào bảng <c>tep_tin</c>.</summary>
    Task AttachMediaUrlsAsync(
        bai_viet post,
        IReadOnlyList<string> mediaUrls,
        string? chuDe,
        DateTime utcNow,
        CancellationToken cancellationToken);

    /// <summary>Đọc hồ sơ công khai (<c>ho_so_nguoi_dung</c>) theo danh sách Id tài khoản.</summary>
    Task<IReadOnlyDictionary<Guid, ho_so_nguoi_dung>> GetProfilesByAccountIdsAsync(
        IReadOnlyCollection<Guid> accountIds,
        CancellationToken cancellationToken);

    /// <summary>Đọc đường dẫn ảnh đại diện từ bảng <c>tep_tin</c> theo danh sách Id tệp tin.</summary>
    Task<IReadOnlyDictionary<Guid, string>> GetAvatarUrlsByFileIdsAsync(
        IReadOnlyCollection<Guid> fileIds,
        CancellationToken cancellationToken);
}
