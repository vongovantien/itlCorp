using AuthServer;
using eFMS.API.Common.Globals.Configs;
using eFMS.API.System.Service.Contexts;
using eFMS.IdentityServer.Configuration;
using eFMS.IdentityServer.DL.Infrastructure;
using eFMS.IdentityServer.DL.IService;
using eFMS.IdentityServer.DL.Services;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using ITL.NetCore.Connection.EF;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace eFMS.IdentityServer.Infrastructure
{
    public static class ServiceRegister
    {
        public static void Register(IServiceCollection services)
        {
            services.AddTransient<IAuthenUserService, AuthenticateService>();
            services.AddScoped(typeof(IContextBase<>), typeof(Base<>));

            services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidator>();
            services.AddTransient<IProfileService, ProfileService>();
            services.AddTransient<ISysUserLogService, SysUserLogService>();
            services.AddTransient<ISysEmployeeService, SysEmployeeService>();
        }
        public static IServiceCollection AddConfigureSetting(this IServiceCollection service, IConfiguration configuration)
        {
            service.Configure<ConnectionString>(options =>
            {
                options.MongoConnection
                    = configuration.GetSection("ConnectionStrings:MongoConnection").Value;
                options.MongoDatabase
                    = configuration.GetSection("ConnectionStrings:Database").Value;
                options.RedisConnection
                    = configuration.GetSection("ConnectionStrings:Redis").Value;
                options.eFMSConnection
                    = configuration.GetSection("ConnectionStrings:eFMSConnection").Value;
            });
            service.Configure<LDAPConfig>(options =>
            {
                options.LdapPaths
                    = configuration.GetSection("Ldap:Path").GetChildren().Select(sl => sl.Value).ToArray();
                options.Domain
                    = configuration.GetSection("Ldap:Domain").Value;
            });
            return service;
        }

        public static IServiceCollection AddCustomMvc(this IServiceCollection services, IAppConfig appConfig)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder
                            //.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials()
                            .WithOrigins(appConfig.CrosConfig.Urls);
                    });
            });
            services.AddMvc();
            return services;
        }

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
    }
}
