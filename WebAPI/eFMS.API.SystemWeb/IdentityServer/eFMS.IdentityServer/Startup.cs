using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using eFMS.IdentityServer.DL.IService;
using eFMS.IdentityServer.DL.Services;
using AutoMapper;
using ITL.NetCore.Connection.EF;
using eFMS.API.System.Service.Contexts;
using eFMS.IdentityServer;
using Microsoft.IdentityModel.Logging;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace AuthServer
{
    public class Startup
    {

        IHostingEnvironment _environment;
        public Startup(IHostingEnvironment environment)
        {
            _environment = environment;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940

        public void ConfigureServices(IServiceCollection services)
        {
          
            services.AddAutoMapper();
            var cert = new X509Certificate2(Path.Combine(_environment.ContentRootPath, "certs", "IdentityServer4Auth.pfx"));
            services.AddIdentityServer()
                //.AddDeveloperSigningCredential()
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryClients(Config.GetClients(null, 14400, 12600))
                .AddInMemoryPersistedGrants()
            .AddSigningCredential(cert)
                .AddValidationKey(cert);

            services.AddTransient<IAuthenUserService, AuthenticateService>();
            services.AddScoped(typeof(IContextBase<>), typeof(Base<>));

            //.AddTestUsers(Config.GetUsers());

            services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidator>();
            services.AddTransient<IProfileService, ProfileService>();
            services.AddTransient<ISysUserLogService, SysUserLogService>();

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
            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Identity Server is running ...");
            });
            app.UseCors(builder => builder
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowCredentials()
            .AllowAnyMethod());
        }
    }
}
