using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace MicroserviceApiKernel.SharedControllers;


[ApiController]
[ApiVersion(1)]
[Route("/api/v{v:apiVersion}/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected async ValueTask<UserClaimParserResult?> GetUserClaim() => await UserClaimParser.Parse(HttpContext);
}