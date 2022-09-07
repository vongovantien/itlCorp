using eFMS.API.Common.Helpers;
using eFMS.API.Common.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Common.Infrastructure.Common
{
    public static class HandleException
    {
        public static async Task HandleExceptionAsync(HttpContext context, Exception exception, HttpStatusCode code, string _webHookUrl)
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
            string _username = string.Empty;
            string _token = string.Empty;

            if(context.User.Claims != null && context.User.Claims.Count() > 0)
            {
                _username = context.User.Claims.Where(x => x.Type == "userName")?.FirstOrDefault().Value;
            }
            context.Request.Body.Position = 0;

            _token = context.Request.Headers["Authorization"].ToString();
            ResponseExModel log = new ResponseExModel
            {
                Code = (int)code,
                Message = exception.Message,
                Exception = body,
                Success = false,
                Source = exception.Source,
                Name = exception.GetType().Name,
                Body = body,
                Path = context.Request.Host + context.Request.Path,
                UserName = _username,
                Token = _token
            };

            await response.WriteAsync(JsonConvert.SerializeObject(log));

            LogHelper LogWebHook = new LogHelper();
            LogWebHook.LogWrite("eFMS_Log_Ex", JsonConvert.SerializeObject(log));

            if(!string.IsNullOrEmpty(_webHookUrl))
            {
                await LogWebHook.PushWebhook(_webHookUrl, log);
            }
        }
    }
}
