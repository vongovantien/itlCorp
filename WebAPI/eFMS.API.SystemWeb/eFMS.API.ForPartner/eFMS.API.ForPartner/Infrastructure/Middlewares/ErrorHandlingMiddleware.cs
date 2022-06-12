using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Models;
using eFMS.API.ForPartner.Infrastructure.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace eFMS.API.ForPartner.Infrastructure.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        readonly IOptions<MsWebHookUrl> webHookUrl;

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
            HttpResponse response = context.Response;
            response.ContentType = "application/json";
            response.StatusCode = (int)code;

            if (!context.Request.Body.CanSeek)
            {
                context.Request.EnableBuffering();
            }

            context.Request.Body.Position = 0;
            StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8);
            string body = await reader.ReadToEndAsync().ConfigureAwait(false);
            context.Request.Body.Position = 0;

            ResponseExModel log = new ResponseExModel
            {
                Code = (int)code,
                Message = exception.Message,
                Exception = body,
                Success = false,
                Source = exception.Source,
                Name = exception.GetType().Name,
                Body = body,
                Path = context.Request.Path
            };

            // await response.WriteAsync(JsonConvert.SerializeObject(log));
            await response.WriteAsync(JsonConvert.SerializeObject(new
            {
                error = new ResponseModel
                {
                    Code = (int)code,
                    Message = exception.Message,
                    Exception = exception.GetType().Name,
                    Success = false
                }
            }));

            LogHelper LogWebHook = new LogHelper();
            LogWebHook.LogWrite("eFMS_Log_Ex", JsonConvert.SerializeObject(log));
            await LogWebHook.PushWebhook(_webHookUrl.Value.Url.ToString(), log);
        }
    }
}
