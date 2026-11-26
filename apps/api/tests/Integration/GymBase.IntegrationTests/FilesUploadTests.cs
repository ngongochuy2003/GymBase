using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GymBase.Entities.Biz.Enums;
using GymBase.TestDoubles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace GymBase.IntegrationTests;

/// <summary>
/// Upload hai bước qua HTTP: <c>POST /files/upload-url</c>, gửi file lên kho (giả lập bằng cách đặt object vào key tạm),
/// <c>POST /files/{id}/confirm</c>, rồi <c>GET /files/{id}/url</c>. Kiểm route, JSON snake_case, đăng nhập và mã lỗi.
/// </summary>
public sealed class FilesUploadTests : IAsyncLifetime
{
    private const string Files = "/api/v1/mob/files";
    private static readonly Guid Chu = Guid.Parse("0193f0a0-0000-7000-8000-000000000004");
    private static readonly Guid NguoiKhac = Guid.Parse("0193f0a0-0000-7000-8000-000000000002");
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0, 0x10, 0x4A, 0x46, 0x49, 0x46, 0, 1, 1, 0, 0, 1];

    private WebApplication _app = null!;

    public async Task InitializeAsync() => _app = await TestApp.StartAsync();

    public async Task DisposeAsync() => await _app.DisposeAsync();

    private HttpClient Client(Guid userId) => TestApp.ClientFor(_app, userId, "User");

    private FakeFileStorage Storage => _app.Services.GetRequiredService<FakeFileStorage>();

    private static object Xin(string muc = "avatar", string mime = "image/jpeg", long size = 16) =>
        new { muc_dich = muc, loai_mime = mime, kich_thuoc_byte = size, ten_tep = "anh.jpg" };

    private async Task<Guid> XinVaGuiAsync(HttpClient client, string muc = "avatar", string mime = "image/jpeg")
    {
        var response = await client.PostAsJsonAsync($"{Files}/upload-url", Xin(muc, mime, Jpeg.Length));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var id = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("file_id").GetGuid();
        Storage.Objects[$"tmp/{id}"] = (Jpeg, mime);
        return id;
    }

    [Fact]
    public async Task UploadUrl_WithoutLogin_Returns401()
    {
        var response = await _app.GetTestClient().PostAsJsonAsync($"{Files}/upload-url", Xin());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UploadUrl_ValidRequest_ReturnsSnakeCaseFormTargetingTmpKey()
    {
        var response = await Client(Chu).PostAsJsonAsync($"{Files}/upload-url", Xin());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var id = body.GetProperty("file_id").GetGuid();
        var upload = body.GetProperty("upload");
        Assert.Equal("POST", upload.GetProperty("method").GetString());
        Assert.Equal("file", upload.GetProperty("file_field").GetString());
        Assert.Equal($"tmp/{id}", upload.GetProperty("fields").GetProperty("key").GetString());
        Assert.True(upload.TryGetProperty("het_han", out _));
    }

    [Theory]
    [InlineData("avatar", "video/mp4", HttpStatusCode.BadRequest, "FILE_TYPE_NOT_ALLOWED")]
    [InlineData("khong_co", "image/jpeg", HttpStatusCode.BadRequest, "FILE_PURPOSE_INVALID")]
    public async Task UploadUrl_InvalidRequest_ReturnsProblemDetailsWithCode(string muc, string mime, HttpStatusCode status, string code)
    {
        var response = await Client(Chu).PostAsJsonAsync($"{Files}/upload-url", Xin(muc, mime));

        Assert.Equal(status, response.StatusCode);
        Assert.Equal(code, (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task UploadUrl_OverDailyQuota_Returns422WithQuotaCode()
    {
        var client = Client(Chu);
        for (var i = 0; i < 20; i++)
        {
            await client.PostAsJsonAsync($"{Files}/upload-url", Xin());
        }

        var response = await client.PostAsJsonAsync($"{Files}/upload-url", Xin());

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("FILE_QUOTA_EXCEEDED", (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task Confirm_BeforeUpload_Returns400NotUploaded()
    {
        var client = Client(Chu);
        var xin = await client.PostAsJsonAsync($"{Files}/upload-url", Xin());
        var id = (await xin.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("file_id").GetGuid();

        var response = await client.PostAsync($"{Files}/{id}/confirm", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("FILE_NOT_UPLOADED", (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
    }

    [Fact]
    public async Task Confirm_AfterUpload_MarksUploadedAndReturnsActualSize()
    {
        var client = Client(Chu);
        var id = await XinVaGuiAsync(client);

        var response = await client.PostAsync($"{Files}/{id}/confirm", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("da_tai_len", body.GetProperty("trang_thai").GetString());
        Assert.Equal(Jpeg.Length, body.GetProperty("kich_thuoc_byte").GetInt64());
        Assert.Equal("avatar", body.GetProperty("muc_dich").GetString());
    }

    [Fact]
    public async Task Confirm_OtherUsersFile_Returns404()
    {
        var id = await XinVaGuiAsync(Client(Chu));

        var response = await Client(NguoiKhac).PostAsync($"{Files}/{id}/confirm", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Url_OwnerGetsPresignedUrl_OtherUserGets404_UntilFileIsPubliclyLinked()
    {
        var owner = Client(Chu);
        var id = await XinVaGuiAsync(owner, "avatar");
        await owner.PostAsync($"{Files}/{id}/confirm", null);

        var own = await owner.GetAsync($"{Files}/{id}/url");
        Assert.Equal(HttpStatusCode.OK, own.StatusCode);
        var url = (await own.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("url").GetString();
        Assert.StartsWith("https://fake.test/get/avatar/", url);

        Assert.Equal(HttpStatusCode.NotFound, (await Client(NguoiKhac).GetAsync($"{Files}/{id}/url")).StatusCode);

        var store = _app.Services.GetRequiredService<InMemoryStore<GymBase.Entities.Biz.tep_tin>>();
        var entity = store.Find(id)!;
        entity.loai_doi_tuong = LoaiDoiTuongTepTin.ho_so_nguoi_dung;
        entity.doi_tuong_id = Chu;
        entity.trang_thai = TrangThaiTepTin.da_lien_ket;
        store.TryReplace(entity);
        Assert.Equal(HttpStatusCode.OK, (await Client(NguoiKhac).GetAsync($"{Files}/{id}/url")).StatusCode);
    }

    [Fact]
    public async Task Urls_ReturnsOnlyFilesTheViewerMaySee()
    {
        var owner = Client(Chu);
        var rieng = await XinVaGuiAsync(owner, "progress");
        await owner.PostAsync($"{Files}/{rieng}/confirm", null);

        var response = await Client(NguoiKhac).PostAsJsonAsync($"{Files}/urls", new { file_ids = new[] { rieng, Guid.NewGuid() } });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data").GetArrayLength());
    }
}
