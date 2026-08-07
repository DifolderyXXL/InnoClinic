using DocumentsAPI.Application;
using FluentValidation;
using MicroserviceApiKernel;
using MicroserviceApiKernel.Extensions.Endpoints;
using MicroserviceApiKernel.SharedControllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocumentsAPI.Controllers;

public record PhotoCreatedResponse(Guid PhotoId);

public class PhotosController(IPhotoFacade photoFacade) : BaseApiController
{
    [HttpGet("offices/{officeId}/avatar/{photoId:guid}")]
    [AllowAnonymous]
    [ResponseCache(Duration = CacheHelper.PublicRestCacheTime, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetPublicPhoto(
        [FromRoute] string officeId,
        [FromRoute] Guid photoId,
        CancellationToken ct)
    {
        var photo = await photoFacade.GetPublicPhoto(officeId, photoId, ct);
        if (photo == null) return NotFound();

        return Ok(new { url = photo });
    }

    [HttpGet("doctors/{doctorId:guid}/avatar/{photoId:guid}")]
    [AllowAnonymous]
    [ResponseCache(Duration = CacheHelper.PublicRestCacheTime, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetDoctorPhoto(
        [FromRoute] Guid doctorId,
        [FromRoute] Guid photoId,
        CancellationToken ct)
    {
        var response = await photoFacade.GetDoctorPhoto(doctorId, photoId, ct);

        return response.Status switch
        {
            DoctorPhotoStatus.NotFound => NotFound(),
            DoctorPhotoStatus.Forbidden => Forbid(),
            DoctorPhotoStatus.Success => Ok(new 
            { 
                url = response.Result!.Url, 
                expireTimeMillis = response.Result.ExpireTimeMillis 
            }),
            _ => BadRequest()
        };
    }

    [HttpGet("users/avatar/{photoId:guid}")]
    [HasPermission(Permissions.Accounts.ReadOwn)]
    [ResponseCache(Duration = CacheHelper.SensitiveRestCacheTime, Location = ResponseCacheLocation.Client)]
    public async Task<IActionResult> GetProfilePhoto(
        [FromRoute] Guid photoId,
        CancellationToken ct)
    {
        var user = await GetUserClaim();
        if (user == null || !Guid.TryParse(user.Id, out var guid)) return Unauthorized();

        var photo = await photoFacade.GetProfilePhoto(guid, photoId, ct);
        if (photo == null) return NotFound();

        return Ok(new { url = photo });
    }
    
    [HttpGet("users/{userId:guid}/avatar/{photoId:guid}")]
    [HasPermission(Permissions.Accounts.Read)]
    [ResponseCache(Duration = CacheHelper.SensitiveRestCacheTime, Location = ResponseCacheLocation.Client)]
    public async Task<IActionResult> GetProfilePhoto(
        [FromRoute] Guid userId,
        [FromRoute] Guid photoId,
        CancellationToken ct)
    {
        var photo = await photoFacade.GetProfilePhoto(userId, photoId, ct);
        if (photo == null) return NotFound();

        return Ok(new { url = photo });
    }

    [HttpPost("users/avatar")]
    [HasPermission(Permissions.Photos.Manage)]
    [Produces<PhotoCreatedResponse>]
    public async Task<IActionResult> UploadProfilePhoto(
        [FromForm] IFormFile file,
        [FromServices] IValidator<IFormFile> validator,
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
        var guid = await photoFacade.UploadProfilePhoto(userId, stream, ct);
        
        return Ok(new PhotoCreatedResponse(guid));
    }

    [HttpPost("offices/{officeId}/avatar")]
    [HasPermission(Permissions.Offices.Manage)]
    [Produces<PhotoCreatedResponse>]
    public async Task<IActionResult> UploadOfficePhoto(
        [FromForm] IFormFile file,
        [FromRoute] string officeId,
        [FromServices] IValidator<IFormFile> validator,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(file, ct);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
        }
        
        await using var stream = file.OpenReadStream();
        var guid = await photoFacade.UploadOfficePhoto(officeId, stream, ct);
        
        return Ok(new PhotoCreatedResponse(guid));
    }
    
    [HttpPost("offices/{officeId}/avatar/confirm")]
    [Authorize(Policy = RolePolicy.IdentityServer)]
    public async Task<IActionResult> ConfirmProfilePhoto(
        [FromRoute] string officeId,
        [FromQuery] Guid photoId,
        [FromQuery] Guid? oldPhotoId,
        CancellationToken ct)
    {
        if (photoId == Guid.Empty) return BadRequest();
        
        var isConfirmed = await photoFacade.ConfirmOfficePhotoAsync(officeId, photoId, oldPhotoId, ct);
        if (!isConfirmed)
        {
            return NotFound();
        }
        
        return Ok(new { photoId });
    }
}