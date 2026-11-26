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
    Task<PagingResult<bai_viet>> DLGetFeedPagingAsync(
        string? tab,
        Guid? currentUserId,
        PagingRequest request,
        CancellationToken cancellationToken);

    /// <summary>Lấy danh sách tệp đính kèm (<c>loai_doi_tuong = bai_viet</c>) theo danh sách Id bài viết, sắp theo <c>thu_tu_hien_thi</c>.</summary>
    Task<IReadOnlyDictionary<Guid, IReadOnlyList<tep_tin>>> DLGetMediaByPostIdsAsync(
        IReadOnlyCollection<Guid> postIds,
        CancellationToken cancellationToken);

    /// <summary>
    /// Lưu danh sách URL ngoài (http/https) của nội dung mẫu vào bảng <c>tep_tin</c> (dòng "external"). Tệp người dùng tải lên được gắn
    /// bằng <c>IFileLienKet</c>, không đi qua đây.
    /// </summary>
    Task DLAttachMediaUrlsAsync(
        bai_viet post,
        IReadOnlyList<string> mediaUrls,
        string? chuDe,
        DateTime utcNow,
        CancellationToken cancellationToken);

    /// <summary>Đọc hồ sơ công khai (<c>ho_so_nguoi_dung</c>) theo danh sách Id tài khoản.</summary>
    Task<IReadOnlyDictionary<Guid, ho_so_nguoi_dung>> DLGetProfilesByAccountIdsAsync(
        IReadOnlyCollection<Guid> accountIds,
        CancellationToken cancellationToken);

    /// <summary>Các bài viết theo danh sách Id, bỏ qua Id không có. Không bảo đảm thứ tự.</summary>
    Task<IReadOnlyList<bai_viet>> DLFindByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);

    /// <summary>
    /// Đọc URL ngoài (http/https) của ảnh đại diện lưu dạng dòng "external" trong <c>tep_tin</c>. Tệp lưu ở MinIO/S3 không có trong kết quả:
    /// lấy URL ký qua <c>IFileUrlProvider</c>.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, string>> DLGetAvatarUrlsByFileIdsAsync(
        IReadOnlyCollection<Guid> fileIds,
        CancellationToken cancellationToken);
}
