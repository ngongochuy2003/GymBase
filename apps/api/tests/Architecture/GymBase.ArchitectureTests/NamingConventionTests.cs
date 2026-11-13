using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using GymBase.Base.Crud;
using GymBase.Persistence;

namespace GymBase.ArchitectureTests;

/// <summary>
/// Quy tắc đặt tên của hai tầng (docs/api/cau-truc-thu-muc.md, mục "Quy ước đặt tên"):
/// hàm của Service và thư mục chức năng trong project BL bắt đầu bằng <c>BL</c>; hàm của Repository và thư mục chức năng
/// trong project DL bắt đầu bằng <c>DL</c>. Tự quét project BL/DL trong src/, nhưng assembly phải được test này
/// tham chiếu (xem csproj) để nạp được.
/// </summary>
public partial class NamingConventionTests
{
    [GeneratedRegex(@"^GymBase\.(?<layer>BL|DL)[\w.]+$")]
    private static partial Regex LayerProject();

    private const BindingFlags Declared =
        BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    /// <summary>Lớp nền dùng chung: tên không nằm trong project BL/DL nên liệt kê tường minh.</summary>
    private static readonly Type[] BaseBusinessTypes = [typeof(BaseService<,,,>), typeof(ICrudService<,,>)];

    private static readonly Type[] BaseDataTypes = [typeof(IBaseRepository<>), typeof(BaseRepository<>)];

    public static TheoryData<string> LayerProjects() =>
        new(LayerProjectNames().Order());

    [Fact]
    public void Business_service_methods_start_with_BL()
    {
        var types = LayerProjectNames()
            .Where(name => name.StartsWith("GymBase.BL", StringComparison.Ordinal))
            .SelectMany(name => LoadTypes(name).Where(t => t.Name.EndsWith("Service", StringComparison.Ordinal)))
            .Concat(BaseBusinessTypes);

        AssertMethodsStartWith(types, "BL");
    }

    [Fact]
    public void Data_repository_methods_start_with_DL()
    {
        var types = LayerProjectNames()
            .Where(name => name.StartsWith("GymBase.DL", StringComparison.Ordinal))
            .SelectMany(name => LoadTypes(name).Where(t => t.Name.EndsWith("Repository", StringComparison.Ordinal)))
            .Concat(BaseDataTypes);

        AssertMethodsStartWith(types, "DL");
    }

    [Theory]
    [MemberData(nameof(LayerProjects))]
    public void Feature_folders_in_layer_projects_start_with_layer_prefix(string projectName)
    {
        var prefix = LayerProject().Match(projectName).Groups["layer"].Value;
        var directory = Path.GetDirectoryName(LayerReferenceTests.SourceProjectFiles()[projectName])!;

        var violations = Directory.EnumerateDirectories(directory)
            .Select(Path.GetFileName)
            .Where(name => name is not ("bin" or "obj" or "Properties"))
            .Where(name => !name!.StartsWith(prefix, StringComparison.Ordinal))
            .ToList();

        Assert.True(
            violations.Count == 0,
            $"{projectName}: thư mục chức năng phải bắt đầu bằng {prefix}: {string.Join(", ", violations)}");
    }

    /// <summary>
    /// Mỗi Service và Repository nằm trong file cùng tên (<c>TaiKhoanService.cs</c>, <c>ITaiKhoanRepository.cs</c>), để người
    /// mới tìm theo tên lớp là ra file.
    /// </summary>
    [Theory]
    [MemberData(nameof(LayerProjects))]
    public void Service_and_repository_types_have_a_file_with_the_same_name(string projectName)
    {
        var directory = Path.GetDirectoryName(LayerReferenceTests.SourceProjectFiles()[projectName])!;
        var files = Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Split(Path.DirectorySeparatorChar).Any(part => part is "bin" or "obj"))
            .Select(Path.GetFileNameWithoutExtension)
            .ToHashSet();

        var missing = LoadTypes(projectName)
            .Where(t => t.IsPublic && !t.IsNested)
            .Where(t => t.Name.EndsWith("Service", StringComparison.Ordinal) || t.Name.EndsWith("Repository", StringComparison.Ordinal))
            .Select(t => t.Name.Split('`')[0])
            .Where(name => !files.Contains(name))
            .ToList();

        Assert.True(missing.Count == 0, $"{projectName}: không có file cùng tên cho: {string.Join(", ", missing)}");
    }

    private static void AssertMethodsStartWith(IEnumerable<Type> types, string prefix)
    {
        var methods = types
            .SelectMany(type => type.GetMethods(Declared)
                .Where(m => !m.IsSpecialName && !m.IsPrivate && !m.IsAssembly && m.GetCustomAttribute<CompilerGeneratedAttribute>() is null)
                .Select(m => (Type: type.Name, Method: m.Name)))
            .ToList();
        var violations = methods.Where(m => !m.Method.StartsWith(prefix, StringComparison.Ordinal)).Select(m => $"{m.Type}.{m.Method}").ToList();

        // Chặn trường hợp test chạy rỗng vì không tìm thấy kiểu nào (assembly chưa được nạp, tên lớp đổi).
        Assert.True(methods.Count > 0, $"Không tìm thấy hàm nào để kiểm tra tiền tố {prefix}.");
        Assert.True(violations.Count == 0, $"Hàm phải bắt đầu bằng {prefix}: {string.Join(", ", violations)}");
    }

    private static IEnumerable<Type> LoadTypes(string assemblyName)
    {
        Assembly assembly;
        try
        {
            assembly = Assembly.Load(assemblyName);
        }
        catch (FileNotFoundException ex)
        {
            throw new InvalidOperationException(
                $"Không nạp được {assemblyName}: thêm ProjectReference tới project này vào GymBase.ArchitectureTests.csproj.", ex);
        }

        return assembly.GetTypes().Where(t => t.GetCustomAttribute<CompilerGeneratedAttribute>() is null);
    }

    private static List<string> LayerProjectNames() =>
        LayerReferenceTests.SourceProjectFiles().Keys.Where(name => LayerProject().IsMatch(name)).Order().ToList();
}
