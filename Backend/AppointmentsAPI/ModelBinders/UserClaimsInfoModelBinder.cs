using MicroserviceApiKernel;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AppointmentsAPI.ModelBinders;

public class UserClaimsInfoModelBinder: IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }
        
        var context = bindingContext.HttpContext;

        var result = await UserClaimParser.Parse(context);

        bindingContext.Result = ModelBindingResult.Success(result);
    }
}