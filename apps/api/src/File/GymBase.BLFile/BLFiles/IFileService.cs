using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace GymBase.BLFile.BLFiles;

public interface IFileService
{
    Task<FileUploadResultDto> BLUploadAsync(ClaimsPrincipal user, IFormFile file, string purpose, CancellationToken ct = default);
    Task<FileContentStreamDto> BLOpenAuthorizedAsync(ClaimsPrincipal user, Guid id, CancellationToken ct = default);
    Task<TepTinMetadataDto?> BLGetMetadataAsync(ClaimsPrincipal user, Guid id, CancellationToken ct = default);
    Task BLDeleteAsync(ClaimsPrincipal user, Guid id, CancellationToken ct = default);
}

