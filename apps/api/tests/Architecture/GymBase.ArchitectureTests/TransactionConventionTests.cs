namespace GymBase.ArchitectureTests;

/// <summary>
/// DbContext bật <c>EnableRetryOnFailure</c> (Pomelo), nên EF từ chối transaction do code tự mở trừ khi chạy trong
/// <c>CreateExecutionStrategy().ExecuteAsync</c>. Test tích hợp dùng repository giả nên không bắt được lỗi này, chỉ lộ khi
/// chạy với MySQL thật (<c>InvalidOperationException ... does not support user-initiated transactions</c>).
/// </summary>
public sealed class TransactionConventionTests
{
    [Fact]
    public void Files_that_begin_a_transaction_run_it_inside_an_execution_strategy()
    {
        var violations = LayerReferenceTests.SourceProjectFiles().Values
            .Select(Path.GetDirectoryName)
            .SelectMany(directory => Directory.EnumerateFiles(directory!, "*.cs", SearchOption.AllDirectories))
            .Where(f => !f.Split(Path.DirectorySeparatorChar).Any(part => part is "bin" or "obj" or "Migrations"))
            .Select(f => (File: f, Text: File.ReadAllText(f)))
            .Where(x => x.Text.Contains("BeginTransaction", StringComparison.Ordinal)
                        && !x.Text.Contains("CreateExecutionStrategy", StringComparison.Ordinal))
            .Select(x => Path.GetFileName(x.File))
            .ToList();

        Assert.True(
            violations.Count == 0,
            $"Mở transaction mà không dùng db.Database.CreateExecutionStrategy(): {string.Join(", ", violations)}");
    }
}
