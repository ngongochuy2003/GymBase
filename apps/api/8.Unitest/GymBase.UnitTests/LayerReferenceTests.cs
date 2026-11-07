using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace GymBase.UnitTests;

/// <summary>
/// Giữ đúng quy tắc phân tầng: BL và DL.Contract không được phụ thuộc vào tầng truy cập DB cụ thể (*.Mysql).
/// Tự tìm mọi project BL (GymBase.BL*, GymBase.AMISBase.BL) và DL.Contract (*.Contract) trong apps/api,
/// nên nhóm mới thêm vào Management không cần sửa test này.
/// </summary>
public partial class LayerReferenceTests
{
    [GeneratedRegex(@"^GymBase\.(BL\w+|AMISBase\.BL|DL\w+\.Contract|AMISBase\.DL\.Contract)$")]
    private static partial Regex DbAgnosticProjectName();

    public static TheoryData<string> DbAgnosticProjects() => new(DbAgnosticProjectNames());

    [Fact]
    public void Discovers_bl_and_contract_projects()
    {
        var names = DbAgnosticProjectNames();

        Assert.Contains("GymBase.BLBIZ", names);
        Assert.Contains("GymBase.DLDI.Contract", names);
        Assert.Contains("GymBase.AMISBase.BL", names);
    }

    private static List<string> DbAgnosticProjectNames() =>
        AllProjectFiles()
            .Select(Path.GetFileNameWithoutExtension)
            .OfType<string>()
            .Where(name => DbAgnosticProjectName().IsMatch(name))
            .Order()
            .ToList();

    [Theory]
    [MemberData(nameof(DbAgnosticProjects))]
    public void Project_does_not_reference_mysql_layer(string projectName)
    {
        var csproj = AllProjectFiles().Single(f => Path.GetFileNameWithoutExtension(f) == projectName);

        var mysqlReferences = XDocument.Load(csproj)
            .Descendants("ProjectReference")
            .Select(r => (string?)r.Attribute("Include") ?? string.Empty)
            .Where(include => include.Contains(".Mysql", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.Empty(mysqlReferences);
    }

    private static IEnumerable<string> AllProjectFiles() =>
        Directory.EnumerateFiles(FindApiRoot(), "*.csproj", SearchOption.AllDirectories)
            .Where(f => !f.Split(Path.DirectorySeparatorChar).Any(part => part is "bin" or "obj"));

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
