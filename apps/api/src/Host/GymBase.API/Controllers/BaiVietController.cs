using GymBase.Base.Hosting;
using GymBase.Base.Paging;
using GymBase.BLBiz.BaiViet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymBase.API.Controllers;

/// <summary>
/// API bài viết mạng xã hội cho web: <c>/api/v1/web/baiviet</c>.
/// Bao gồm CRUD từ <see cref="BaseCrudController{TDto,TCreateDto,TUpdateDto}"/> và các thao tác bảng tin, thích, bình luận.
/// </summary>
public sealed class BaiVietController(BaiVietService baiVietService)
    : BaseCrudController<BaiVietDto, BaiVietCreateDto, BaiVietUpdateDto>(baiVietService)
{
    /// <summary>Lấy danh sách bài viết hiển thị trên bảng tin (hỗ trợ lọc theo tab: <c>for-you</c>, <c>trending</c>, <c>workout</c>, <c>knowledge</c>).</summary>
    [HttpGet("feed")]
    [AllowAnonymous]
    public async Task<ActionResult<PagingResult<BaiVietDto>>> GetFeed(
        [FromQuery] string? tab,
        [FromQuery] PagingRequest request,
        CancellationToken cancellationToken) =>
        Ok(await baiVietService.BLGetFeedAsync(tab, request, cancellationToken));

    /// <summary>Bật/tắt lượt thích trên bài viết.</summary>
    [HttpPost("{id:guid}/thich")]
    public async Task<ActionResult<KetQuaThichBaiVietDto>> ToggleLike(Guid id, CancellationToken cancellationToken) =>
        Ok(await baiVietService.BLToggleLikeAsync(id, cancellationToken));

    /// <summary>Lấy danh sách bình luận của bài viết.</summary>
    [HttpGet("{id:guid}/binh-luan")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<BinhLuanDto>>> GetComments(Guid id, CancellationToken cancellationToken) =>
        Ok(await baiVietService.BLGetCommentsAsync(id, cancellationToken));

    /// <summary>Thêm bình luận mới vào bài viết.</summary>
    [HttpPost("{id:guid}/binh-luan")]
    public async Task<ActionResult<BinhLuanDto>> AddComment(
        Guid id,
        [FromBody] BinhLuanCreateDto dto,
        CancellationToken cancellationToken) =>
        Ok(await baiVietService.BLAddCommentAsync(id, dto, cancellationToken));
}
