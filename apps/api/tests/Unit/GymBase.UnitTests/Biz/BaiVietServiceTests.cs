using System.Text.Encodings.Web;
using System.Text.Json;
using GymBase.Base.Exceptions;
using GymBase.Base.Paging;
using GymBase.BLBiz.BaiViet;
using GymBase.Entities.Biz;
using GymBase.Entities.Biz.Enums;
using GymBase.ModuleContracts.TepTin;
using GymBase.TestDoubles;

namespace GymBase.UnitTests.Biz;

/// <summary>Ghi lại các lần gắn tệp để test kiểm; có thể ép lỗi như khi tệp không hợp lệ.</summary>
internal sealed class FakeFileLienKet : IFileLienKet
{
    public List<(Guid NguoiDungId, IReadOnlyCollection<Guid> FileIds, string LoaiDoiTuong, Guid DoiTuongId, IReadOnlyCollection<string> MucDich)> Calls { get; } = [];

    public Exception? Throw { get; set; }

    public Task BLGanAsync(
        Guid nguoiDungId, IReadOnlyCollection<Guid> fileIds, string loaiDoiTuong, Guid doiTuongId,
        IReadOnlyCollection<string> mucDichChoPhep, CancellationToken ct = default)
    {
        if (Throw != null) throw Throw;
        Calls.Add((nguoiDungId, fileIds, loaiDoiTuong, doiTuongId, mucDichChoPhep));
        return Task.CompletedTask;
    }
}

internal sealed class FakeFileUrlProvider : IFileUrlProvider
{
    public Dictionary<Guid, FileUrlDto> Urls { get; } = new();

    public Task<IReadOnlyDictionary<Guid, FileUrlDto>> BLLayUrlsAsync(
        Guid nguoiXemId, IReadOnlyCollection<Guid> fileIds, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyDictionary<Guid, FileUrlDto>>(
            fileIds.Where(Urls.ContainsKey).ToDictionary(id => id, id => Urls[id]));
}

public sealed class BaiVietServiceTests
{
    private readonly FakeFileLienKet _fileLienKet = new();
    private readonly FakeFileUrlProvider _fileUrls = new();

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
        new BinhLuanCreateValidator(),
        _fileLienKet,
        _fileUrls);

    [Fact]
    public async Task Create_with_file_ids_links_files_to_the_new_post_and_returns_signed_urls()
    {
        var service = CreateService();
        var fileA = Guid.NewGuid();
        var fileB = Guid.NewGuid();
        _fileUrls.Urls[fileA] = new FileUrlDto(fileA, "https://minio.test/get/post/a.jpg?sig=1", DateTime.UtcNow, "image/jpeg");
        _fileUrls.Urls[fileB] = new FileUrlDto(fileB, "https://minio.test/get/post/b.mp4?sig=2", DateTime.UtcNow, "video/mp4");

        var created = await service.BLCreateAsync(
            new BaiVietCreateDto { noi_dung = "Buổi tập", file_ids = [fileA, fileB] },
            CancellationToken.None);

        var call = Assert.Single(_fileLienKet.Calls);
        Assert.Equal(TestUserId, call.NguoiDungId);
        Assert.Equal([fileA, fileB], call.FileIds);
        Assert.Equal("bai_viet", call.LoaiDoiTuong);
        Assert.Equal(created.Id, call.DoiTuongId);
        Assert.Equal(["post"], call.MucDich);
        Assert.NotNull(await _postRepository.DLGetByIdAsync(created.Id, CancellationToken.None));
    }

    [Fact]
    public async Task Create_with_only_file_ids_is_valid_and_is_typed_as_media_post()
    {
        var service = CreateService();

        var created = await service.BLCreateAsync(
            new BaiVietCreateDto { file_ids = [Guid.NewGuid()] }, CancellationToken.None);

        Assert.Equal("hinh_anh", created.loai_bai_viet);
    }

    [Fact]
    public async Task Create_when_linking_files_fails_does_not_create_the_post()
    {
        _fileLienKet.Throw = new ValidationFailedException("file_ids", "Tệp đính kèm không tồn tại hoặc không phải của bạn.");
        var service = CreateService();

        await Assert.ThrowsAsync<ValidationFailedException>(() => service.BLCreateAsync(
            new BaiVietCreateDto { noi_dung = "Có ảnh", file_ids = [Guid.NewGuid()] }, CancellationToken.None));

        Assert.Empty(_postStore.All());
    }

    [Fact]
    public async Task Feed_returns_signed_urls_for_uploaded_files_and_skips_files_the_viewer_may_not_see()
    {
        var service = CreateService();
        var visible = Guid.NewGuid();
        var hidden = Guid.NewGuid();
        var created = await service.BLCreateAsync(
            new BaiVietCreateDto { noi_dung = "Bài có hai tệp", file_ids = [visible, hidden] }, CancellationToken.None);
        _postRepository.AddUploadedFile(created.Id, visible, 1);
        _postRepository.AddUploadedFile(created.Id, hidden, 2);
        _fileUrls.Urls[visible] = new FileUrlDto(visible, "https://minio.test/get/post/a.mp4?sig=1", DateTime.UtcNow, "video/mp4");

        var detail = await service.BLGetByIdAsync(created.Id, CancellationToken.None);

        Assert.Equal(["https://minio.test/get/post/a.mp4?sig=1"], detail.hinh_anh_urls);
        // hinh_anh_meta cùng thứ tự với hinh_anh_urls: tệp không được xem không để lại dấu ở cả hai danh sách.
        var meta = Assert.Single(detail.hinh_anh_meta!);
        Assert.Equal(visible, meta.file_id);
    }

    [Fact]
    public async Task Feed_returns_file_id_and_blurhash_alongside_each_uploaded_image()
    {
        var service = CreateService();
        var image = Guid.NewGuid();
        var created = await service.BLCreateAsync(
            new BaiVietCreateDto { noi_dung = "Bài có ảnh", file_ids = [image] }, CancellationToken.None);
        _postRepository.AddUploadedFile(created.Id, image, 1);
        _fileUrls.Urls[image] = new FileUrlDto(image, "https://minio.test/get/post/a.jpg?sig=1", DateTime.UtcNow, "image/jpeg", "LEHV6nWB2yk8pyo0adR*.7kCMdnj");

        var detail = await service.BLGetByIdAsync(created.Id, CancellationToken.None);

        var meta = Assert.Single(detail.hinh_anh_meta!);
        Assert.Equal(image, meta.file_id);
        Assert.Equal("LEHV6nWB2yk8pyo0adR*.7kCMdnj", meta.blurhash);
    }

    [Theory]
    [InlineData("/api/v1/mob/files/abc/content")]
    [InlineData("file:///local/anh.jpg")]
    public async Task Create_rejects_non_http_media_urls(string url)
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ValidationFailedException>(() => service.BLCreateAsync(
            new BaiVietCreateDto { noi_dung = "x", hinh_anh_urls = [url] }, CancellationToken.None));
    }

    [Fact]
    public async Task Create_rejects_more_than_ten_attachments_across_urls_and_file_ids()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ValidationFailedException>(() => service.BLCreateAsync(
            new BaiVietCreateDto
            {
                noi_dung = "x",
                hinh_anh_urls = Enumerable.Range(0, 6).Select(i => $"https://cdn.test/{i}.jpg").ToList(),
                file_ids = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToList(),
            }, CancellationToken.None));
    }

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
    public async Task Public_post_responses_never_expose_private_profile_fields()
    {
        // Tác giả có đủ thông tin riêng tư; bảng tin, chi tiết và bình luận chỉ được lộ id, tên hiển thị, ảnh.
        _postRepository.SeedProfile(new ho_so_nguoi_dung
        {
            tai_khoan_id = TestUserId,
            loai_nguoi_dung = LoaiNguoiDung.hoc_vien,
            ten_hien_thi = "Minh Khôi",
            ngay_sinh = new DateOnly(1998, 5, 20),
            gioi_tinh = GioiTinh.nam,
            trinh_do_tap = "lau_nam",
            trang_thai_xac_minh = TrangThaiXacMinh.chua_xac_minh,
            ngay_tao = DateTime.UtcNow,
        });
        var service = CreateService();
        var post = await service.BLCreateAsync(new BaiVietCreateDto { noi_dung = "Hôm nay tập chân" }, CancellationToken.None);
        await service.BLAddCommentAsync(post.Id, new BinhLuanCreateDto { noi_dung = "Tập tốt" }, CancellationToken.None);

        var responses = new object[]
        {
            await service.BLGetFeedAsync("for-you", new PagingRequest(), CancellationToken.None),
            await service.BLGetByIdAsync(post.Id, CancellationToken.None),
            await service.BLGetCommentsAsync(post.Id, CancellationToken.None),
        };

        foreach (var response in responses)
        {
            var json = JsonSerializer.Serialize(
                response, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            Assert.Contains("Minh Khôi", json);
            foreach (var truong in new[] { "ngay_sinh", "tuoi", "gioi_tinh", "1998", "trinh_do_tap" })
            {
                Assert.DoesNotContain(truong, json, StringComparison.OrdinalIgnoreCase);
            }
        }
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
