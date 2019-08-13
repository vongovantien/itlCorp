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

namespace AuthServer
{
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

            IdentityModelEventSource.ShowPII = true;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
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
    static class CustomExtensionsMethods
    {
        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IHostingEnvironment environment, IAppConfig appConfig)
        {
            var cert = new X509Certificate2(Path.Combine(environment.ContentRootPath, "certs", "IdentityServer4Auth.pfx"));
            services.AddIdentityServer()
                //.AddDeveloperSigningCredential()
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryClients(Config.GetClients(appConfig.CrosConfig.Urls, appConfig.AuthConfig.RedirectUris, appConfig.AuthConfig.AccessTokenLifetime, appConfig.AuthConfig.SlidingRefreshTokenLifetime))
                .AddInMemoryPersistedGrants()
                .AddSigningCredential(cert)
                .AddValidationKey(cert);

            return services;
        }
        public static IServiceCollection AddCustomMvc(this IServiceCollection services, IAppConfig appConfig)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials()
                            .WithOrigins(appConfig.CrosConfig.Urls);
                    });
            });
            services.AddMvc();
            return services;
        }
    }
}
