using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using eFMS.API.Accounting.Infrastructure.Common;
using eFMS.API.Common.Helpers;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace eFMS.API.Accounting.Infrastructure.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context /* other scoped dependencies */)
        {
            try
            {
                // must be awaited
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // if it's not one of the expected exception, set it to 500
            var code = HttpStatusCode.InternalServerError;

            //TODO:Mapping if (exception is NotFoundExe) code = HttpStatusCode.NotFound;
            if (exception is ArgumentNullException) code = HttpStatusCode.BadRequest;
            else if (exception is HttpRequestException) code = HttpStatusCode.BadRequest;
            else if (exception is UnauthorizedAccessException) code = HttpStatusCode.Unauthorized;

            await WriteExceptionAsync(context, exception, code);
        }

        private static async Task WriteExceptionAsync(HttpContext context, Exception exception, HttpStatusCode code)
        {
            var response = context.Response;
            response.ContentType = "application/json";
            response.StatusCode = (int)code;

            if (!context.Request.Body.CanSeek)
            {
                context.Request.EnableBuffering();
            }

            context.Request.Body.Position = 0;
            var reader = new StreamReader(context.Request.Body,  Encoding.UTF8);
            string body = await reader.ReadToEndAsync().ConfigureAwait(false);
            context.Request.Body.Position = 0;

            var log = JsonConvert.SerializeObject(new
            {
                error = new ResponseModel
                {
                    Code = (int)code,
                    Message = exception.GetType().Name + " " + exception.Message + " in " + exception.Source,
                    Exception = body,
                    Success = false,
                    Path = context.Request.Path
                }
            });

            new LogHelper("eFMS_Log_Exception", log);
            await response.WriteAsync(log);
        }
    }
}
