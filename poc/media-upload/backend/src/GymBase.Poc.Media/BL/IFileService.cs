using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace GymBase.Poc.Media.BL;

public interface IFileService
{
    Task<FileUploadResultDto> UploadAsync(
        ClaimsPrincipal user,
        IFormFile file,
        string purpose,
        CancellationToken ct = default);

    Task<FileContentStreamDto> OpenAuthorizedAsync(
        ClaimsPrincipal user,
        Guid id,
        CancellationToken ct = default);

    Task<TepTinMetadataDto?> GetMetadataAsync(
        ClaimsPrincipal user,
        Guid id,
        CancellationToken ct = default);

    Task DeleteAsync(
        ClaimsPrincipal user,
        Guid id,
        CancellationToken ct = default);
}
