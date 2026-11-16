using FluentValidation;
using GymBase.Base.Auth;
using GymBase.Base.Crud;
using GymBase.Base.Entities;
using GymBase.Base.Exceptions;
using GymBase.Base.Paging;
using GymBase.Base.Validation;
using GymBase.DLBiz.Contract;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;

namespace GymBase.BLBiz.BaiViet;

/// <summary>
/// Nghiệp vụ bài viết và tương tác mạng xã hội (bảng tin, đăng bài, thích, bình luận),
/// dùng chung cho controller web (<c>Controllers/</c>) và mobile (<c>Mob/Controllers/</c>).
/// </summary>
public sealed class BaiVietService
    : BaseService<bai_viet, BaiVietDto, BaiVietCreateDto, BaiVietUpdateDto>
{
    private readonly IBaiVietRepository _baiVietRepository;
    private readonly ITuongTacBaiVietRepository _tuongTacRepository;
    private readonly IValidator<BaiVietCreateDto> _createValidator;
    private readonly IValidator<BinhLuanCreateDto> _commentValidator;

    public BaiVietService(
        IBaiVietRepository baiVietRepository,
        ITuongTacBaiVietRepository tuongTacRepository,
        TimeProvider timeProvider,
        ICurrentUser currentUser,
        IValidator<BaiVietCreateDto> createValidator,
        IValidator<BaiVietUpdateDto> updateValidator,
        IValidator<BinhLuanCreateDto> commentValidator)
        : base(baiVietRepository, timeProvider, currentUser, createValidator, updateValidator)
    {
        _baiVietRepository = baiVietRepository;
        _tuongTacRepository = tuongTacRepository;
        _createValidator = createValidator;
        _commentValidator = commentValidator;
    }
    private static readonly HashSet<string> SystemSeedUserNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "seed",
        "admin",
        "pt.minh",
        "pt.lan",
        "hv.huy",
        "hv.phuong",
    };

    protected override string EntityDisplayName => "bài viết";

    /// <summary>Lấy danh sách bài viết cho bảng tin (hỗ trợ lọc theo tab và làm giàu thông tin tác giả, ảnh, lượt thích/bình luận).</summary>
    public async Task<PagingResult<BaiVietDto>> BLGetFeedAsync(
        string? tab,
        PagingRequest request,
        CancellationToken cancellationToken)
    {
        var page = await _baiVietRepository.DLGetFeedPagingAsync(
            tab,
            CurrentUser.UserId,
            request.Normalize(),
            cancellationToken);

        var dtos = await EnrichPostsAsync(page.data, cancellationToken);
        return new PagingResult<BaiVietDto>(dtos, page.meta.page, page.meta.page_size, page.meta.total);
    }

    public override async Task<PagingResult<BaiVietDto>> BLGetPagingAsync(
        PagingRequest request,
        CancellationToken cancellationToken)
    {
        var page = await _baiVietRepository.DLGetPagingAsync(request.Normalize(), cancellationToken);
        var dtos = await EnrichPostsAsync(page.data, cancellationToken);
        return new PagingResult<BaiVietDto>(dtos, page.meta.page, page.meta.page_size, page.meta.total);
    }

    public override async Task<BaiVietDto> BLGetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await BLGetEntityOrThrowAsync(id, cancellationToken);
        var dtos = await EnrichPostsAsync([entity], cancellationToken);
        return dtos[0];
    }

    public override async Task<BaiVietDto> BLCreateAsync(BaiVietCreateDto dto, CancellationToken cancellationToken)
    {
        await _createValidator.ValidateOrThrowAsync(dto, cancellationToken);
        await BLBeforeCreateAsync(dto, cancellationToken);

        var utcNow = TimeProvider.GetUtcNow().UtcDateTime;
        var entity = BLCreateEntity(dto);
        var creatorName = CurrentUser.UserName;
        entity.MarkCreated(creatorName, utcNow);

        await _baiVietRepository.DLInsertAsync(entity, cancellationToken);

        var mediaUrls = dto.hinh_anh_urls?
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Select(u => u.Trim())
            .ToList() ?? [];

        if (mediaUrls.Count > 0)
        {
            await _baiVietRepository.DLAttachMediaUrlsAsync(
                entity,
                mediaUrls,
                dto.chu_de,
                utcNow,
                cancellationToken);
        }

        var enriched = await EnrichPostsAsync([entity], cancellationToken, dto.chu_de);
        return enriched[0];
    }

    /// <summary>Bật hoặc tắt lượt thích của người dùng hiện tại trên một bài viết.</summary>
    public async Task<KetQuaThichBaiVietDto> BLToggleLikeAsync(Guid baiVietId, CancellationToken cancellationToken)
    {
        _ = await BLGetEntityOrThrowAsync(baiVietId, cancellationToken);

        var actorId = ResolveActorId();
        var utcNow = TimeProvider.GetUtcNow().UtcDateTime;
        var existing = await _tuongTacRepository.DLFindLikeByUserAsync(baiVietId, actorId, cancellationToken);

        bool liked;
        if (existing is null)
        {
            var interaction = new tuong_tac_bai_viet
            {
                bai_viet_id = baiVietId,
                tai_khoan_id = actorId,
                loai_tuong_tac = LoaiTuongTac.thich,
                noi_dung = null,
                tuong_tac_cha_id = null,
                ngay_tao = utcNow,
                ngay_xoa = null,
            };
            await _tuongTacRepository.DLInsertAsync(interaction, cancellationToken);
            liked = true;
        }
        else if (existing.ngay_xoa is null)
        {
            existing.ngay_xoa = utcNow;
            await _tuongTacRepository.DLUpdateAsync(existing, cancellationToken);
            liked = false;
        }
        else
        {
            existing.ngay_xoa = null;
            existing.ngay_tao = utcNow;
            await _tuongTacRepository.DLUpdateAsync(existing, cancellationToken);
            liked = true;
        }

        var summaries = await _tuongTacRepository.DLGetSummariesByPostIdsAsync([baiVietId], actorId, cancellationToken);
        var totalLikes = summaries.TryGetValue(baiVietId, out var summary) ? summary.so_luot_thich : (liked ? 1 : 0);

        return new KetQuaThichBaiVietDto(baiVietId, liked, totalLikes);
    }

    /// <summary>Lấy danh sách bình luận còn hiệu lực của một bài viết.</summary>
    public async Task<IReadOnlyList<BinhLuanDto>> BLGetCommentsAsync(Guid baiVietId, CancellationToken cancellationToken)
    {
        _ = await BLGetEntityOrThrowAsync(baiVietId, cancellationToken);

        var comments = await _tuongTacRepository.DLGetCommentsByPostIdAsync(baiVietId, cancellationToken);
        if (comments.Count == 0)
        {
            return [];
        }

        var accountIds = comments.Select(c => c.tai_khoan_id).Distinct().ToList();
        var profiles = await _baiVietRepository.DLGetProfilesByAccountIdsAsync(accountIds, cancellationToken);
        var avatarFileIds = profiles.Values
            .Where(p => p.anh_dai_dien_tep_tin_id.HasValue)
            .Select(p => p.anh_dai_dien_tep_tin_id!.Value)
            .Distinct()
            .ToList();
        var avatarUrls = await _baiVietRepository.DLGetAvatarUrlsByFileIdsAsync(avatarFileIds, cancellationToken);

        return comments.Select(c =>
        {
            var author = ResolveAuthorInfo(c.tai_khoan_id, null, profiles, avatarUrls);
            return new BinhLuanDto(
                c.Id,
                c.bai_viet_id,
                c.tai_khoan_id,
                author.ten_hien_thi,
                author.anh_dai_dien_url,
                c.noi_dung ?? string.Empty,
                c.tuong_tac_cha_id,
                c.ngay_tao);
        }).ToList();
    }

    /// <summary>Thêm bình luận mới vào bài viết.</summary>
    public async Task<BinhLuanDto> BLAddCommentAsync(
        Guid baiVietId,
        BinhLuanCreateDto dto,
        CancellationToken cancellationToken)
    {
        await _commentValidator.ValidateOrThrowAsync(dto, cancellationToken);
        _ = await BLGetEntityOrThrowAsync(baiVietId, cancellationToken);

        if (dto.tuong_tac_cha_id is { } parentId)
        {
            var parent = await _tuongTacRepository.DLGetByIdAsync(parentId, cancellationToken);
            if (parent is null
                || parent.ngay_xoa is not null
                || parent.bai_viet_id != baiVietId
                || parent.loai_tuong_tac != LoaiTuongTac.binh_luan)
            {
                throw new BusinessRuleException(
                    "Bình luận cha không tồn tại hoặc không thuộc cùng bài viết.",
                    "INVALID_PARENT_COMMENT");
            }
        }

        var actorId = ResolveActorId();
        var utcNow = TimeProvider.GetUtcNow().UtcDateTime;
        var entity = new tuong_tac_bai_viet
        {
            bai_viet_id = baiVietId,
            tai_khoan_id = actorId,
            loai_tuong_tac = LoaiTuongTac.binh_luan,
            noi_dung = dto.noi_dung.Trim(),
            tuong_tac_cha_id = dto.tuong_tac_cha_id,
            ngay_tao = utcNow,
            ngay_xoa = null,
        };

        await _tuongTacRepository.DLInsertAsync(entity, cancellationToken);

        var profiles = await _baiVietRepository.DLGetProfilesByAccountIdsAsync([actorId], cancellationToken);
        var avatarFileIds = profiles.Values
            .Where(p => p.anh_dai_dien_tep_tin_id.HasValue)
            .Select(p => p.anh_dai_dien_tep_tin_id!.Value)
            .ToList();
        var avatarUrls = await _baiVietRepository.DLGetAvatarUrlsByFileIdsAsync(avatarFileIds, cancellationToken);
        var author = ResolveAuthorInfo(actorId, null, profiles, avatarUrls);

        return new BinhLuanDto(
            entity.Id,
            entity.bai_viet_id,
            entity.tai_khoan_id,
            author.ten_hien_thi,
            author.anh_dai_dien_url,
            entity.noi_dung,
            entity.tuong_tac_cha_id,
            entity.ngay_tao);
    }

    protected override BaiVietDto BLToDto(bai_viet entity) => new(
        entity.Id,
        entity.tac_gia_id,
        new TacGiaBaiVietDto(entity.tac_gia_id, entity.nguoi_tao ?? "Người dùng GymBase", null),
        entity.noi_dung,
        entity.loai_bai_viet.ToString(),
        entity.che_do_hien_thi.ToString(),
        entity.trang_thai.ToString(),
        InferTopic(entity.noi_dung, null),
        [],
        0,
        0,
        false,
        entity.ngay_dang,
        entity.ngay_tao,
        entity.ngay_chinh_sua);

    protected override bai_viet BLCreateEntity(BaiVietCreateDto dto)
    {
        var utcNow = TimeProvider.GetUtcNow().UtcDateTime;
        var hasText = !string.IsNullOrWhiteSpace(dto.noi_dung);
        var hasMedia = dto.hinh_anh_urls is { Count: > 0 } && dto.hinh_anh_urls.Any(u => !string.IsNullOrWhiteSpace(u));

        var postType = ParsePostType(dto.loai_bai_viet, hasText, hasMedia);
        var visibility = ParseVisibility(dto.che_do_hien_thi);
        var content = NormalizeContentWithTopic(dto.noi_dung, dto.chu_de, hasMedia);

        return new bai_viet
        {
            tac_gia_id = ResolveActorId(),
            noi_dung = content,
            loai_bai_viet = postType,
            che_do_hien_thi = visibility,
            trang_thai = TrangThaiBaiViet.da_dang,
            ngay_dang = utcNow,
        };
    }

    protected override void BLApplyUpdate(bai_viet entity, BaiVietUpdateDto dto)
    {
        entity.noi_dung = string.IsNullOrWhiteSpace(dto.noi_dung) ? null : dto.noi_dung.Trim();
        entity.che_do_hien_thi = ParseVisibility(dto.che_do_hien_thi);
        if (Enum.TryParse<TrangThaiBaiViet>(dto.trang_thai.Trim(), ignoreCase: true, out var status))
        {
            entity.trang_thai = status;
        }
    }

    private Guid ResolveActorId() => CurrentUser.UserId ?? throw new UnauthorizedException();

    private async Task<IReadOnlyList<BaiVietDto>> EnrichPostsAsync(
        IReadOnlyList<bai_viet> posts,
        CancellationToken cancellationToken,
        string? explicitTopic = null)
    {
        if (posts.Count == 0)
        {
            return [];
        }

        var postIds = posts.Select(p => p.Id).Distinct().ToList();
        var authorIds = posts.Select(p => p.tac_gia_id).Distinct().ToList();
        var viewerId = CurrentUser.UserId ?? throw new UnauthorizedException();

        var mediaByPost = await _baiVietRepository.DLGetMediaByPostIdsAsync(postIds, cancellationToken);
        var summariesByPost = await _tuongTacRepository.DLGetSummariesByPostIdsAsync(postIds, viewerId, cancellationToken);
        var profilesByAccount = await _baiVietRepository.DLGetProfilesByAccountIdsAsync(authorIds, cancellationToken);

        var avatarFileIds = profilesByAccount.Values
            .Where(p => p.anh_dai_dien_tep_tin_id.HasValue)
            .Select(p => p.anh_dai_dien_tep_tin_id!.Value)
            .Distinct()
            .ToList();
        var avatarUrlsByFile = await _baiVietRepository.DLGetAvatarUrlsByFileIdsAsync(avatarFileIds, cancellationToken);

        var result = new List<BaiVietDto>(posts.Count);
        foreach (var post in posts)
        {
            var files = mediaByPost.TryGetValue(post.Id, out var attached) ? attached : [];
            var imageUrls = files.Select(ExtractCleanUrl).ToList();
            var fileTopic = files.FirstOrDefault(f => !string.IsNullOrWhiteSpace(f.muc_dich))?.muc_dich;
            var topic = explicitTopic ?? fileTopic ?? InferTopic(post.noi_dung, null);

            var summary = summariesByPost.TryGetValue(post.Id, out var s)
                ? s
                : new PostInteractionSummary(0, 0, false);

            var author = ResolveAuthorInfo(post.tac_gia_id, post.nguoi_tao, profilesByAccount, avatarUrlsByFile);

            result.Add(new BaiVietDto(
                post.Id,
                post.tac_gia_id,
                author,
                post.noi_dung,
                post.loai_bai_viet.ToString(),
                post.che_do_hien_thi.ToString(),
                post.trang_thai.ToString(),
                topic,
                imageUrls,
                summary.so_luot_thich,
                summary.so_binh_luan,
                summary.da_thich,
                post.ngay_dang,
                post.ngay_tao,
                post.ngay_chinh_sua));
        }

        return result;
    }

    private static TacGiaBaiVietDto ResolveAuthorInfo(
        Guid accountId,
        string? customCreatorName,
        IReadOnlyDictionary<Guid, ho_so_nguoi_dung> profiles,
        IReadOnlyDictionary<Guid, string> avatarUrls)
    {
        var hasCustomName = !string.IsNullOrWhiteSpace(customCreatorName)
                            && !SystemSeedUserNames.Contains(customCreatorName.Trim());

        if (profiles.TryGetValue(accountId, out var profile))
        {
            var avatarUrl = profile.anh_dai_dien_tep_tin_id is { } fileId
                            && avatarUrls.TryGetValue(fileId, out var url)
                ? url
                : null;

            var displayName = hasCustomName ? customCreatorName!.Trim() : profile.ten_hien_thi;
            return new TacGiaBaiVietDto(accountId, displayName, avatarUrl);
        }

        var fallbackName = hasCustomName ? customCreatorName!.Trim() : "Người dùng GymBase";
        return new TacGiaBaiVietDto(accountId, fallbackName, null);
    }

    private static string ExtractCleanUrl(tep_tin file)
    {
        var key = file.s3_key;
        var hashIndex = key.IndexOf("#tep=", StringComparison.Ordinal);
        if (hashIndex > 0)
        {
            key = key[..hashIndex];
        }

        if (key.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || key.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return key;
        }

        return $"/api/v1/mob/files/{file.Id}/content";
    }

    private static CheDoHienThi ParseVisibility(string raw) =>
        raw.Trim().ToLowerInvariant() switch
        {
            "public" or nameof(CheDoHienThi.cong_khai) => CheDoHienThi.cong_khai,
            "followers" or nameof(CheDoHienThi.nguoi_theo_doi) => CheDoHienThi.nguoi_theo_doi,
            "private" or nameof(CheDoHienThi.rieng_tu) => CheDoHienThi.rieng_tu,
            _ => CheDoHienThi.cong_khai,
        };

    private static LoaiBaiViet ParsePostType(string? raw, bool hasText, bool hasMedia)
    {
        if (!string.IsNullOrWhiteSpace(raw)
            && Enum.TryParse<LoaiBaiViet>(raw.Trim(), ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        if (hasText && hasMedia)
        {
            return LoaiBaiViet.hon_hop;
        }

        if (hasMedia)
        {
            return LoaiBaiViet.hinh_anh;
        }

        return LoaiBaiViet.van_ban;
    }

    private static string? NormalizeContentWithTopic(string? content, string? topic, bool hasMedia)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        var trimmed = content.Trim();
        if (hasMedia || string.IsNullOrWhiteSpace(topic))
        {
            return trimmed;
        }

        var tag = topic.Trim().ToLowerInvariant() switch
        {
            "meal-prep" => "#mealprep",
            "tips" => "#meohay",
            "knowledge" => "#kienthuc",
            "progress" => "#tiendo",
            "workout" => "#tapluyen",
            _ => null,
        };

        if (tag is not null && !trimmed.Contains(tag, StringComparison.OrdinalIgnoreCase))
        {
            return $"{trimmed} {tag}";
        }

        return trimmed;
    }

    private static string InferTopic(string? content, string? fallback)
    {
        if (!string.IsNullOrWhiteSpace(fallback))
        {
            return fallback.Trim();
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return "workout";
        }

        if (content.Contains("Meal prep", StringComparison.OrdinalIgnoreCase)
            || content.Contains("#mealprep", StringComparison.OrdinalIgnoreCase)
            || content.Contains("siết mỡ", StringComparison.OrdinalIgnoreCase))
        {
            return "meal-prep";
        }

        if (content.Contains("#meohay", StringComparison.OrdinalIgnoreCase)
            || content.Contains("#kienthuc", StringComparison.OrdinalIgnoreCase)
            || content.Contains("Mẹo", StringComparison.OrdinalIgnoreCase)
            || content.Contains("protein", StringComparison.OrdinalIgnoreCase))
        {
            return "knowledge";
        }

        return "workout";
    }
}
