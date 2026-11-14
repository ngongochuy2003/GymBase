namespace GymBase.BLFile.BLFiles;

public interface IFileMigrationService
{
    Task<MigrationResultDto> BLMigrateLocalToS3Async(int batchSize = 50, CancellationToken ct = default);
}
