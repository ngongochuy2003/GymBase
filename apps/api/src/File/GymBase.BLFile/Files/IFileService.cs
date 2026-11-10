using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace GymBase.BLFile.Files;

public interface IFileService
{
    Task<FileUploadResultDto> UploadAsync(ClaimsPrincipal user, IFormFile file, string purpose, CancellationToken ct = default);
    Task<FileContentStreamDto> OpenAuthorizedAsync(ClaimsPrincipal user, Guid id, CancellationToken ct = default);
    Task<TepTinMetadataDto?> GetMetadataAsync(ClaimsPrincipal user, Guid id, CancellationToken ct = default);
    Task DeleteAsync(ClaimsPrincipal user, Guid id, CancellationToken ct = default);
}

public interface IFileMigrationService
{
    Task<MigrationResultDto> MigrateLocalToS3Async(int batchSize = 50, CancellationToken ct = default);
}
