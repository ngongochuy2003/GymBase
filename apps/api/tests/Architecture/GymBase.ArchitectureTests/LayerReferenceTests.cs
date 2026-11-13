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

    [GeneratedRegex(@"GymBase\.Entities\.(?<group>\w+)")]
    private static partial Regex EntityNamespace();

    private const string Entities = "GymBase.Entities";

    /// <summary>AppDbContext, BaseRepository và migrations dùng chung cho mọi nhóm (quyết định Q9).</summary>
    private const string SharedPersistence = "GymBase.Persistence";

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

    /// <summary>
    /// Mọi nhóm cùng tham chiếu GymBase.Entities, nên ranh giới giữa các nhóm được giữ ở đây: BL và DL của một nhóm
    /// chỉ dùng entity của chính nhóm đó. Cần dữ liệu nhóm khác thì đi qua ModuleContracts.
    /// </summary>
    [Theory]
    [MemberData(nameof(SourceProjects))]
    public void Group_projects_use_only_their_own_entities(string projectName)
    {
        var group = GroupOf(projectName);
        if (group is null)
        {
            return;
        }

        var directory = Path.GetDirectoryName(SourceProjectFiles()[projectName])!;
        var violations = Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Split(Path.DirectorySeparatorChar).Any(part => part is "bin" or "obj"))
            .SelectMany(f => EntityNamespace().Matches(File.ReadAllText(f))
                .Where(match => !match.Groups["group"].Value.Equals(group, StringComparison.OrdinalIgnoreCase))
                .Select(match => $"{Path.GetFileName(f)}: {match.Value}"))
            .ToList();

        Assert.True(violations.Count == 0, $"{projectName} dùng entity của nhóm khác: {string.Join(", ", violations)}");
    }

    private static string? GroupOf(string project) =>
        new[] { BusinessProject(), DataContractProject(), DataMySqlProject() }
            .Select(regex => regex.Match(project))
            .FirstOrDefault(match => match.Success)?.Groups["group"].Value;

    /// <summary>Quy tắc cho từng loại project; project không thuộc loại nào thì không được tham chiếu gì.</summary>
    private static Func<string, bool> AllowedReferences(string project)
    {
        if (project == "GymBase.API")
        {
            // Composition root: được tham chiếu mọi BL, Base.Hosting, DL.*.MySql và Persistence (chỉ để đăng ký dịch vụ).
            return r => r == "GymBase.Base.Hosting" || r == SharedPersistence || BusinessProject().IsMatch(r) || DataMySqlProject().IsMatch(r);
        }

        if (project == Entities)
        {
            return r => r == "GymBase.Base";
        }

        if (project == SharedPersistence)
        {
            return r => r is "GymBase.Base" or Entities;
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
            return r => r is "GymBase.Base" or Entities;
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

    internal static Dictionary<string, string> SourceProjectFiles() =>
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
