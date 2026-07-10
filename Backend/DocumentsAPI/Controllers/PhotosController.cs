using Azure.Storage.Sas;
using DocumentsAPI.Infrastructure;
using FluentValidation;
using MicroserviceApiKernel;
using MicroserviceApiKernel.SharedControllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocumentsAPI.Controllers;

public class PhotosController(PhotoRepository context) : BaseApiController
{
    [HttpGet("users/avatar/{photoId:guid}")]
    [Authorize(Policy = RolePolicy.Client)]
    public async Task<IActionResult> GetProfilePhoto(
        [FromRoute] Guid photoId,
        CancellationToken ct)
    {
        var user = await GetUserClaim();
        if (user == null || !Guid.TryParse(user.Id, out var guid)) return Unauthorized();
        
        var client = context.GetProfilePhotoClient(guid, photoId);

        if (!await client.ExistsAsync(ct)) return NotFound();

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = client.BlobContainerName,
            BlobName = client.Name,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(15)
        };
        sasBuilder.SetPermissions(BlobAccountSasPermissions.Read);

        var sasUri = client.GenerateSasUri(sasBuilder);
        
        return Ok(new { url = sasUri.ToString() });
    }
    
    [HttpPost("users/avatar")]
    [Authorize(Policy = RolePolicy.Client)]
    public async Task<IActionResult> UploadProfilePhoto(
        IFormFile file,
        [FromServices] IValidator<IFormFile> validator,
        [FromServices] ITempPhotoStorage tempPhotoStorage,
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
        await tempPhotoStorage.UploadAsync(userId, stream, TimeSpan.FromHours(1), ct);
        
        return Ok();
    }
    
    [HttpPost("users/{userId:guid}/avatar/confirm")]
    [Authorize(Policy = RolePolicy.IdentityServer)]
    public async Task<IActionResult> ConfirmProfilePhoto(
        Guid userId,
        [FromQuery] Guid photoId,
        [FromQuery] Guid oldPhotoId,
        [FromServices] IProfilePhotoStorage photoStorage,
        [FromServices] ILogger<PhotosController> logger,
        CancellationToken ct)
    {
        logger.LogWarning("SUCCESS");
        if (photoId == Guid.Empty) return BadRequest();

        await photoStorage.DeletePhotoAsync(userId, oldPhotoId, ct);
        var isConfirmed = await photoStorage.ConfirmPhotoAsync(userId, photoId, ct);

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