using eFMS.API.Accounting.DL.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace eFMS.API.ForPartner.Infrastructure.Filters
{
    public class APIKeyAuthAttribute : Attribute, IAsyncActionFilter
    {
        private  string APIKeyHeaderName = AccountingConstants.API_KEY_HEADER;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(APIKeyHeaderName, out var clientAPIKey))
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            await next();
        }
    }
}
