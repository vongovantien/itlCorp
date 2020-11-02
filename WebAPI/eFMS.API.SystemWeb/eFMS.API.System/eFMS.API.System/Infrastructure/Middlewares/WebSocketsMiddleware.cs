using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.System.Infrastructure.Middlewares
{
    public class WebSocketsMiddleware
    {

        private readonly RequestDelegate _next;

        public WebSocketsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var request = httpContext.Request;
            var auth = request.Query.TryGetValue("access_token", out var accessToken);
            // web sockets cannot pass headers so we must take the access token from query param and
            // add it to the header before authentication middleware runs
            if (request.Path.StartsWithSegments("/notification", StringComparison.OrdinalIgnoreCase))
            {
                request.Headers.Add("Authorization", $"Bearer {accessToken}");
            }

            await _next(httpContext);
        }
    }
}
