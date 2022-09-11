using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Infrastructure.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace eFMS.API.Accounting.Infrastructure.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        IOptions<MsWebHookUrl> webHookUrl;

        public ErrorHandlingMiddleware(RequestDelegate next, IOptions<MsWebHookUrl> _webHookUrl)
        {
            this.next = next;
            webHookUrl = _webHookUrl;
        }

        public async Task Invoke(HttpContext context /* other scoped dependencies */, IOptions<MsWebHookUrl> _webHookUrl)
        {
            try
            {
                // must be awaited
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, _webHookUrl);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception, IOptions<MsWebHookUrl> _webHookUrl)
        {
            // if it's not one of the expected exception, set it to 500
            var code = HttpStatusCode.InternalServerError;

            //TODO:Mapping if (exception is NotFoundExe) code = HttpStatusCode.NotFound;
            if (exception is ArgumentNullException) code = HttpStatusCode.BadRequest;
            else if (exception is HttpRequestException) code = HttpStatusCode.BadRequest;
            else if (exception is UnauthorizedAccessException) code = HttpStatusCode.Unauthorized;

            await WriteExceptionAsync(context, exception, code, _webHookUrl);
        }

        private static async Task WriteExceptionAsync(HttpContext context, Exception exception, HttpStatusCode code, IOptions<MsWebHookUrl> _webHookUrl)
        {
            await HandleException.HandleExceptionAsync(context, exception, code, _webHookUrl.Value.Url.ToString());
        }
    }
}
