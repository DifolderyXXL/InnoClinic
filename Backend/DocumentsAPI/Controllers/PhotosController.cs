using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using DocumentsAPI.Infrastructure;
using DocumentsAPI.Infrastructure.Photos;
using FluentValidation;
using MicroserviceApiKernel;
using MicroserviceApiKernel.Extensions.Endpoints;
using MicroserviceApiKernel.SharedControllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace DocumentsAPI.Controllers;

public static class CacheHelper
{
    private const int FromHours = 60 * 60;
    public const int PublicCacheTime = 6 * FromHours;
    public const int PublicRestCacheTime = 5 * FromHours;

    public const int SensitiveCacheTime = 3 * FromHours;
    public const int SensitiveRestCacheTime = 2 * FromHours;
}

public record PhotoCreatedResponse(Guid PhotoId);
public class PhotosController : BaseApiController
{
    [HttpGet("offices/{officeId}/avatar/{photoId:guid}")]
    [AllowAnonymous]
    [ResponseCache(Duration = CacheHelper.PublicRestCacheTime, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetPublicPhoto(
        [FromRoute] string officeId,
        [FromRoute] Guid photoId,
        [FromServices] IPublicPhotoStorage context,
        CancellationToken ct)
    {
        var client = context.Repository.GetPhotoClient(officeId, photoId);

        if (!await client.ExistsAsync(ct)) return NotFound();

        return Ok(new { url = client.Uri.ToString() });
    }

    [HttpGet("doctors/{doctorId:guid}/avatar/{photoId:guid}")]
    [AllowAnonymous]
    [ResponseCache(Duration = CacheHelper.PublicRestCacheTime, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetDoctorPhoto(
        [FromRoute] Guid doctorId,
        [FromRoute] Guid photoId,
        [FromServices] IUserPhotoStorage context,
        CancellationToken ct)
    {
        var client = context.Repository.GetPhotoClient(doctorId.ToString(), photoId);

        if (!await client.ExistsAsync(ct)) return NotFound();
        if (!await IsPhotoPublic(client, ct)) return Forbid();

        var expireTime = TimeSpan.FromSeconds(CacheHelper.PublicCacheTime);
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = client.BlobContainerName,
            BlobName = client.Name,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(expireTime)
        };
        sasBuilder.SetPermissions(BlobAccountSasPermissions.Read);

        var sasUri = client.GenerateSasUri(sasBuilder);

        return Ok(new { url = sasUri.ToString(), expireTimeMillis = expireTime.TotalMilliseconds });
    }

    [HttpGet("users/avatar/{photoId:guid}")]
    [HasPermission(Permissions.Accounts.ReadOwn)]
    [ResponseCache(Duration = CacheHelper.SensitiveRestCacheTime, Location = ResponseCacheLocation.Client)]
    public async Task<IActionResult> GetProfilePhoto(
        [FromRoute] Guid photoId,
        [FromServices] IUserPhotoStorage context,
        CancellationToken ct)
    {
        var user = await GetUserClaim();
        if (user == null || !Guid.TryParse(user.Id, out var guid)) return Unauthorized();

        var client = context.Repository.GetPhotoClient(guid.ToString(), photoId);

        if (!await client.ExistsAsync(ct)) return NotFound();

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = client.BlobContainerName,
            BlobName = client.Name,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddSeconds(CacheHelper.SensitiveCacheTime)
        };
        sasBuilder.SetPermissions(BlobAccountSasPermissions.Read);

        var sasUri = client.GenerateSasUri(sasBuilder);

        return Ok(new { url = sasUri.ToString() });
    }

    private async Task<bool> IsPhotoPublic(BlobClient blobClient, CancellationToken ct)
    {
        var tagsResponse = await blobClient.GetTagsAsync(cancellationToken: ct);
        var tags = tagsResponse.Value.Tags;

        return tags.TryGetValue("public", out var value) && value.Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    [HttpPost("users/avatar")]
    [HasPermission(Permissions.Photos.Manage)]
    [Produces<PhotoCreatedResponse>]
    public async Task<IActionResult> UploadProfilePhoto(
        IFormFile file,
        [FromServices] IValidator<IFormFile> validator,
        [FromServices] IUserPhotoStorage tempPhotoStorage,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(file, ct);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
        }
        
        var user = await GetUserClaim();
        if (user == null || !Guid.TryParse(user.Id, out var userId)) 
        {
            return Unauthorized();
        }
        
        await using var stream = file.OpenReadStream();
        var guid = await tempPhotoStorage.UploadTempAsync(userId.ToString(), stream, TimeSpan.FromHours(1), ct);
        
        return Ok(new PhotoCreatedResponse(guid));
    }

    [HttpPost("offices/{officeId}/avatar")]
    [HasPermission(Permissions.Offices.Manage)]
    [Produces<PhotoCreatedResponse>]
    public async Task<IActionResult> UploadOfficePhoto(
        IFormFile file,
        [FromRoute] string officeId,
        [FromServices] IValidator<IFormFile> validator,
        [FromServices] IPublicPhotoStorage tempPhotoStorage,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(file, ct);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
        }
        
        await using var stream = file.OpenReadStream();
        var guid = await tempPhotoStorage.UploadTempAsync(officeId, stream, TimeSpan.FromHours(1), ct);
        
        return Ok(new PhotoCreatedResponse(guid));
    }
    
    [HttpPost("offices/{officeId}/avatar/confirm")]
    [Authorize(Policy = RolePolicy.IdentityServer)]
    public async Task<IActionResult> ConfirmProfilePhoto(
        string officeId,
        [FromQuery] Guid photoId,
        [FromQuery] Guid? oldPhotoId,
        [FromServices] IPublicPhotoStorage photoStorage,
        [FromServices] ILogger<PhotosController> logger,
        CancellationToken ct)
    {
        if (photoId == Guid.Empty) return BadRequest();

        if (oldPhotoId != null)
        {
            await photoStorage.DeletePhotoAsync(officeId, oldPhotoId.Value, ct);
        }
        
        var isConfirmed = await photoStorage.ConfirmPhotoAsync(officeId, photoId, ct);
        if (!isConfirmed)
        {
            return NotFound();
        }
        
        return Ok(new{ photoId });
    }
}

public class UploadProfilePhotoValidator : AbstractValidator<IFormFile>
{
    public UploadProfilePhotoValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.Length)
            .LessThanOrEqualTo(2 * 1024 * 1024)
            .WithMessage("Max 2MB.");
    }
}