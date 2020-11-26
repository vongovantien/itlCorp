using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.IdentityModel.Logging;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using eFMS.IdentityServer.Configuration;
using eFMS.IdentityServer.Infrastructure;
using eFMS.API.Common.Globals.Configs;
using Microsoft.Extensions.Configuration;

namespace AuthServer
{
    public class HttpContextUtils
    {
        private static IHttpContextAccessor m_httpContextAccessor;

        public static HttpContext Current => m_httpContextAccessor.HttpContext;

        public static string AppBaseUrl => $"{Current.Request.Scheme}://{Current.Request.Host}{Current.Request.PathBase}";

        internal static void Configure(IHttpContextAccessor contextAccessor)
        {
            m_httpContextAccessor = contextAccessor;
        }
    }
    public static class HttpContextEx
    {
        //public static void AddHttpContextAccessor(this IServiceCollection services)
        //{
        //    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        //}

        public static IApplicationBuilder UseHttpContext(this IApplicationBuilder app)
        {
            HttpContextUtils.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());
            return app;
        }
    }
    public class Startup
    {
        protected readonly IHostingEnvironment _environment;
        protected readonly IAppConfig _appConfig;

        public Startup(IHostingEnvironment environment, IAppConfig appConfig)
        {
            _environment = environment;
            _appConfig = appConfig;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        
        public void ConfigureServices(IServiceCollection services)
        {          
            services.AddAutoMapper();
            services.AddCustomMvc(_appConfig)
                .AddCustomAuthentication(_environment, _appConfig);
            ServiceRegister.Register(services);
            ServiceRegister.AddConfigureSetting(services, _appConfig.Configuration);
            
            IdentityModelEventSource.ShowPII = true;
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHttpContext();
            app.UseIdentityServer();
            //app.Run(async (context) =>
            //{
            //    string url = string.Concat(
            //          context.Request.Scheme,
            //          "://",
            //          context.Request.Host.ToUriComponent(),
            //          context.Request.PathBase.ToUriComponent(),
            //         context.Request.PathBase.HasValue 
            //         && context.Request.PathBase.ToUriComponent().EndsWith('/')
            //         ? "" : @"/");
            //    if(context.Request.Path.HasValue && context.Request.Path.ToUriComponent())
            //    await
            //    context.Response.WriteAsync("Identity Server is running ..."
            //         + Environment.NewLine
            //         + "Check working: " + url
            //         + @".well-known/openid-configuration");
            //});

            app.UseMvc();
            app.Run(async (context) =>
            {
                string url = string.Concat(
                       context.Request.Scheme,
                       "://",
                       context.Request.Host.ToUriComponent(),
                       context.Request.PathBase.ToUriComponent(),
                      context.Request.Path.HasValue && context.Request.Path.ToUriComponent().EndsWith('/')
                      ? "" : @"/");
                await
                context.Response.WriteAsync("Identity Server is running ..."
                     + Environment.NewLine
                     + "Check working: " + url
                     + @".well-known/openid-configuration");
            });
            app.UseCors("AllowAllOrigins");
        }
    }
}
