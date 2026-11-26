using GymBase.BLThongBao;
using GymBase.BLThongBao.BLDayThongBao;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace GymBase.UnitTests.ThongBao;

/// <summary>Chọn cách gửi push theo <c>ThongBao:PushProvider</c> và kiểm tra cấu hình ngay lúc khởi động.</summary>
public sealed class ThongBaoDiTests : IDisposable
{
    private readonly string _thuMuc = Directory.CreateTempSubdirectory("gymbase-fcm-").FullName;

    public void Dispose() => Directory.Delete(_thuMuc, recursive: true);

    private static IConfiguration Cau(params (string Khoa, string Gia)[] muc) =>
        new ConfigurationBuilder().AddInMemoryCollection(muc.ToDictionary(m => $"ThongBao:{m.Khoa}", m => (string?)m.Gia)).Build();

    private static ServiceDescriptor? DangKy(IServiceCollection services) =>
        services.LastOrDefault(d => d.ServiceType == typeof(IPushSender));

    private sealed class KhongLayToken : IFcmAccessTokenProvider
    {
        public Task<string> GetAccessTokenAsync(CancellationToken ct) => Task.FromResult("x");
    }

    private string TaoFile(string json)
    {
        var path = Path.Combine(_thuMuc, "service-account.json");
        System.IO.File.WriteAllText(path, json);
        return path;
    }

    [Fact]
    public void Default_IsLog()
    {
        var services = new ServiceCollection();

        services.AddThongBaoBusiness(Cau());

        Assert.Equal(typeof(LogPushSender), DangKy(services)!.ImplementationType);
    }

    [Fact]
    public void UnknownProvider_FailsAtStartup()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddThongBaoBusiness(Cau(("PushProvider", "APNS"))));

        Assert.Contains("LOG hoặc FCM", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("khong-co-file.json")]
    public void Fcm_WithoutAReadableCredentialsFile_FailsAtStartup(string path)
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            new ServiceCollection().AddThongBaoBusiness(Cau(("PushProvider", "FCM"), ("Fcm:CredentialsPath", path))));

        Assert.Contains("CredentialsPath", ex.Message);
    }

    [Fact]
    public void Fcm_RegistersTheFcmSender_AndTakesTheProjectIdFromTheFileWhenNotSet()
    {
        var path = TaoFile("""{"type":"service_account","project_id":"gymbase-dev-123"}""");
        var services = new ServiceCollection();

        services.AddThongBaoBusiness(Cau(("PushProvider", "fcm"), ("Fcm:CredentialsPath", path)));

        // Thay bộ lấy token thật (cần khóa hợp lệ) bằng bản giả để dựng được provider.
        services.AddLogging().AddSingleton<IFcmAccessTokenProvider, KhongLayToken>();
        using var provider = services.BuildServiceProvider();
        Assert.IsType<FcmPushSender>(provider.GetRequiredService<IPushSender>());
        Assert.Equal("gymbase-dev-123", provider.GetRequiredService<IOptions<ThongBaoOptions>>().Value.Fcm.ProjectId);
    }

    [Fact]
    public void Fcm_ExplicitProjectId_WinsOverTheFile()
    {
        var path = TaoFile("""{"project_id":"trong-file"}""");
        var services = new ServiceCollection();

        services.AddThongBaoBusiness(Cau(("PushProvider", "FCM"), ("Fcm:CredentialsPath", path), ("Fcm:ProjectId", "tu-cau-hinh")));

        using var provider = services.BuildServiceProvider();
        Assert.Equal("tu-cau-hinh", provider.GetRequiredService<IOptions<ThongBaoOptions>>().Value.Fcm.ProjectId);
    }

    [Fact]
    public void Fcm_FileWithoutProjectIdAndNoSetting_FailsAtStartup()
    {
        var path = TaoFile("""{"type":"service_account"}""");

        var ex = Assert.Throws<InvalidOperationException>(() =>
            new ServiceCollection().AddThongBaoBusiness(Cau(("PushProvider", "FCM"), ("Fcm:CredentialsPath", path))));

        Assert.Contains("project_id", ex.Message);
    }
}
