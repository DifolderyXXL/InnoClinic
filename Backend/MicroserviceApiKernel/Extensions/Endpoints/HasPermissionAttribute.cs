using Microsoft.AspNetCore.Authorization;

namespace MicroserviceApiKernel.Extensions.Endpoints;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission)
    {
        Policy = permission;
    }
}