using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace GymBase.ArchitectureTests;

/// <summary>
/// Giữ ma trận ProjectReference của chuẩn kiến trúc (docs/api/cau-truc-thu-muc.md, mục "Ma trận tham chiếu"):
/// mỗi loại project chỉ được tham chiếu các project trong danh sách cho phép. Tự quét mọi .csproj trong
/// src/, nên nhóm hoặc project mới thêm vào không cần sửa test này.
/// </summary>
public partial class LayerReferenceTests
{
    [GeneratedRegex(@"^GymBase\.BL(?<group>\w+)$")]
    private static partial Regex BusinessProject();

    [GeneratedRegex(@"^GymBase\.DL(?<group>\w+)\.Contract$")]
    private static partial Regex DataContractProject();

    [GeneratedRegex(@"^GymBase\.DL(?<group>\w+)\.MySql$")]
    private static partial Regex DataMySqlProject();

    /// <summary>
    /// DbContext và DL mock dùng chung đặt trong DLBiz.MySql (quyết định Q9), nên DL.MySql của nhóm khác
    /// được tham chiếu riêng project này.
    /// </summary>
    private const string SharedPersistence = "GymBase.DLBiz.MySql";

    public static TheoryData<string> SourceProjects() => new(SourceProjectFiles().Keys.Order());

    [Fact]
    public void Discovers_projects_of_every_layer()
    {
        var names = SourceProjectFiles().Keys.ToList();

        Assert.Contains("GymBase.API", names);
        Assert.Contains("GymBase.Base", names);
        Assert.Contains("GymBase.BLAuth", names);
        Assert.Contains("GymBase.DLBiz.Contract", names);
        Assert.Contains("GymBase.DLDI.MySql", names);
    }

    [Theory]
    [MemberData(nameof(SourceProjects))]
    public void Project_references_only_allowed_layers(string projectName)
    {
        var allowed = AllowedReferences(projectName);

        var violations = ProjectReferences(SourceProjectFiles()[projectName])
            .Where(reference => !allowed(reference))
            .ToList();

        Assert.True(violations.Count == 0,
            $"{projectName} không được tham chiếu: {string.Join(", ", violations)}");
    }

    /// <summary>Quy tắc cho từng loại project; project không thuộc loại nào thì không được tham chiếu gì.</summary>
    private static Func<string, bool> AllowedReferences(string project)
    {
        if (project == "GymBase.API")
        {
            // Composition root: được tham chiếu mọi BL, Base.Hosting và DL.*.MySql (chỉ để đăng ký dịch vụ).
            return r => r == "GymBase.Base.Hosting" || BusinessProject().IsMatch(r) || DataMySqlProject().IsMatch(r);
        }

        if (project is "GymBase.Base.Hosting" or "GymBase.ModuleContracts")
        {
            return r => r == "GymBase.Base";
        }

        if (BusinessProject().Match(project) is { Success: true } bl)
        {
            var group = bl.Groups["group"].Value;
            return r => r is "GymBase.Base" or "GymBase.ModuleContracts" || r == $"GymBase.DL{group}.Contract";
        }

        if (DataContractProject().IsMatch(project))
        {
            return r => r == "GymBase.Base";
        }

        if (DataMySqlProject().Match(project) is { Success: true } dl)
        {
            var group = dl.Groups["group"].Value;
            return r => r == "GymBase.Base" || r == $"GymBase.DL{group}.Contract" || r == SharedPersistence;
        }

        return _ => false;
    }

    private static List<string> ProjectReferences(string csproj) =>
        XDocument.Load(csproj)
            .Descendants("ProjectReference")
            .Select(r => Path.GetFileNameWithoutExtension(((string?)r.Attribute("Include") ?? string.Empty).Replace('\\', '/')))
            .ToList();

    private static Dictionary<string, string> SourceProjectFiles() =>
        Directory.EnumerateFiles(Path.Combine(FindApiRoot(), "src"), "*.csproj", SearchOption.AllDirectories)
            .Where(f => !f.Split(Path.DirectorySeparatorChar).Any(part => part is "bin" or "obj"))
            .ToDictionary(f => Path.GetFileNameWithoutExtension(f), f => f);

    private static string FindApiRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "GymBase.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("Không tìm thấy GymBase.slnx.");
    }
}
