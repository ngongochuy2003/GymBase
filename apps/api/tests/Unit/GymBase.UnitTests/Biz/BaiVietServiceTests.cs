using GymBase.Base.Exceptions;
using GymBase.Base.Paging;
using GymBase.BLBiz.BaiViet;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using GymBase.TestDoubles;

namespace GymBase.UnitTests.Biz;

public sealed class BaiVietServiceTests
{
    private static readonly Guid TestUserId = Guid.Parse("0193f0a0-0000-7000-8000-000000000002");
    private readonly InMemoryStore<bai_viet> _postStore = new();
    private readonly InMemoryStore<tuong_tac_bai_viet> _interactionStore = new();
    private readonly InMemoryBaiVietRepository _postRepository;
    private readonly InMemoryTuongTacBaiVietRepository _interactionRepository;

    public BaiVietServiceTests()
    {
        _postRepository = new InMemoryBaiVietRepository(_postStore);
        _interactionRepository = new InMemoryTuongTacBaiVietRepository(_interactionStore);
    }

    private BaiVietService CreateService(Guid? userId = null, string? userName = "pt.minh") => new(
        _postRepository,
        _interactionRepository,
        TimeProvider.System,
        new FakeCurrentUser(userName, userId ?? TestUserId),
        new BaiVietCreateValidator(),
        new BaiVietUpdateValidator(),
        new BinhLuanCreateValidator());

    [Fact]
    public async Task Create_with_text_and_media_saves_post_and_returns_enriched_dto()
    {
        _postRepository.SeedProfile(new ho_so_nguoi_dung
        {
            tai_khoan_id = TestUserId,
            loai_nguoi_dung = LoaiNguoiDung.pt,
            ten_hien_thi = "Minh Khôi",
            trang_thai_xac_minh = TrangThaiXacMinh.da_xac_minh,
            ngay_tao = DateTime.UtcNow,
        });

        var service = CreateService();
        var created = await service.BLCreateAsync(
            new BaiVietCreateDto
            {
                noi_dung = "Buổi Push hôm nay.\nTập trung ngực – vai – tay sau, cảm giác rất ổn!",
                che_do_hien_thi = "public",
                chu_de = "workout",
                hinh_anh_urls = ["https://cdn.test/gymbase-push.jpg"],
            },
            CancellationToken.None);

        Assert.Equal("Minh Khôi", created.tac_gia.ten_hien_thi);
        Assert.Equal("hon_hop", created.loai_bai_viet);
        Assert.Equal("cong_khai", created.che_do_hien_thi);
        Assert.Equal("workout", created.chu_de);
        Assert.Single(created.hinh_anh_urls);
        Assert.Equal("https://cdn.test/gymbase-push.jpg", created.hinh_anh_urls[0]);
    }

    [Fact]
    public async Task Create_without_content_or_media_throws_validation_exception()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ValidationFailedException>(() =>
            service.BLCreateAsync(
                new BaiVietCreateDto
                {
                    noi_dung = "   ",
                    hinh_anh_urls = [],
                },
                CancellationToken.None));
    }

    [Fact]
    public async Task ToggleLike_switches_like_state_and_updates_count()
    {
        var service = CreateService();
        var post = await service.BLCreateAsync(
            new BaiVietCreateDto { noi_dung = "Hôm nay tập chân" },
            CancellationToken.None);

        var firstToggle = await service.BLToggleLikeAsync(post.Id, CancellationToken.None);
        Assert.True(firstToggle.da_thich);
        Assert.Equal(1, firstToggle.so_luot_thich);

        var secondToggle = await service.BLToggleLikeAsync(post.Id, CancellationToken.None);
        Assert.False(secondToggle.da_thich);
        Assert.Equal(0, secondToggle.so_luot_thich);

        var thirdToggle = await service.BLToggleLikeAsync(post.Id, CancellationToken.None);
        Assert.True(thirdToggle.da_thich);
        Assert.Equal(1, thirdToggle.so_luot_thich);
    }

    [Fact]
    public async Task GetFeed_filters_by_workout_and_knowledge_tabs()
    {
        var service = CreateService();

        await service.BLCreateAsync(
            new BaiVietCreateDto
            {
                noi_dung = "Buổi Push hôm nay.",
                chu_de = "workout",
                hinh_anh_urls = ["https://cdn.test/gymbase-push.jpg"],
            },
            CancellationToken.None);

        await service.BLCreateAsync(
            new BaiVietCreateDto
            {
                noi_dung = "Meal prep đơn giản cho 1 ngày siết mỡ.",
                chu_de = "meal-prep",
                hinh_anh_urls = ["https://cdn.test/gymbase-mealprep.jpg"],
            },
            CancellationToken.None);

        var allFeed = await service.BLGetFeedAsync("for-you", new PagingRequest(), CancellationToken.None);
        var workoutFeed = await service.BLGetFeedAsync("workout", new PagingRequest(), CancellationToken.None);
        var knowledgeFeed = await service.BLGetFeedAsync("knowledge", new PagingRequest(), CancellationToken.None);

        Assert.Equal(2, allFeed.data.Count);
        Assert.Single(workoutFeed.data);
        Assert.Contains("Buổi Push", workoutFeed.data[0].noi_dung);
        Assert.Single(knowledgeFeed.data);
        Assert.Contains("Meal prep", knowledgeFeed.data[0].noi_dung);
    }

    [Fact]
    public async Task AddComment_creates_comment_and_increments_post_comment_count()
    {
        var service = CreateService();
        var post = await service.BLCreateAsync(
            new BaiVietCreateDto { noi_dung = "Bài viết thảo luận" },
            CancellationToken.None);

        var comment = await service.BLAddCommentAsync(
            post.Id,
            new BinhLuanCreateDto { noi_dung = "Quá tuyệt vời!" },
            CancellationToken.None);

        Assert.Equal("Quá tuyệt vời!", comment.noi_dung);

        var comments = await service.BLGetCommentsAsync(post.Id, CancellationToken.None);
        Assert.Single(comments);

        var updatedPost = await service.BLGetByIdAsync(post.Id, CancellationToken.None);
        Assert.Equal(1, updatedPost.so_binh_luan);
    }
}
