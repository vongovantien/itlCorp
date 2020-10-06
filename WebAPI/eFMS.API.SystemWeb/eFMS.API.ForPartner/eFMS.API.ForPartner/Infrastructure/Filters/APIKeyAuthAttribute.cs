using eFMS.API.ForPartner.DL.Common;
using eFMS.API.ForPartner.DL.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.IO;
using System.Threading.Tasks;

namespace eFMS.API.ForPartner.Infrastructure.Filters
{
    public class APIKeyAuthAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            IQueryCollection query = context.HttpContext.Request.Query;
            var data = context.HttpContext.Request;

            if (query.ContainsKey("apiKey") && query.ContainsKey("hash"))
            {
                string apiKey = query["apiKey"].ToString();
                string hash = query["hash"].ToString();

                await next();
            }
            else
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            
        }
    }
}
