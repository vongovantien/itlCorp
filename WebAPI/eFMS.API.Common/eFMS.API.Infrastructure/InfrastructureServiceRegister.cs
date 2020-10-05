using eFMS.API.Common;
using eFMS.API.Infrastructure.Authorizations;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;

namespace eFMS.API.Infrastructure
{
    public static class InfrastructureServiceRegister
    {
        private static IConfiguration configuration;
        public static IServiceCollection AddInfrastructure<TLanguage>(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .BindingSettingConfig(configuration)
                .AddCrossOrigin()
                .AddCustomCulture()
                .ApiVersionningconfig()
                .AddCustomAuthentication();

            return services;
        }
        static IServiceCollection ApiVersionningconfig(this IServiceCollection services)
        {
            services.AddApiVersioning(config =>
            {
                config.ReportApiVersions = true;
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.ApiVersionReader = new HeaderApiVersionReader("api-version");
            });
            return services;
        }
        static IServiceCollection BindingSettingConfig(this IServiceCollection services, IConfiguration configuration)
        {
            InfrastructureServiceRegister.configuration = configuration;
            services.Configure<Settings>(options =>
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
            services.Configure<WebUrl>(option => {
                option.Url = configuration.GetSection("WebUrl").Value;
            });
            services.Configure<ApiUrl>(option => {
                option.Url = configuration.GetSection("ApiUrl").Value;
            });
            services.Configure<AuthenticationSetting>(authSetting => {

                authSetting.PartnerShareKey
                    = configuration.GetSection("Authentication:PartnerShareKey").Value;
                //authSetting.Authority = configuration["Authentication.Authority").Value;
                //authSetting.ApiSecret = configuration"Authentication.ApiSecret").Value;
                //authSetting.ApiName = configuration.GetSection("Authentication.ApiName").Value;
                //authSetting.RequireHttpsMetadata = configuration.GetSection("Authentication.RequireHttpsMetadata").Value;
            });
            return services;
        }
        static IServiceCollection AddCrossOrigin(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder
                            .WithHeaders("accept", "content-type", "origin", "x-custom-header")
                            .AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
            });
            return services;
        }
        static IServiceCollection AddCustomAuthentication(this IServiceCollection services)
        {
            services.AddUserManager();
            services.AddTransient<IClaimsTransformation, ClaimsExtender>();

            services.AddMvcCore()
                .AddAuthorization();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Authentication:Authority"];
                options.RequireHttpsMetadata = bool.Parse(configuration["Authentication:RequireHttpsMetadata"]);
                options.Audience = configuration["Authentication:ApiName"];
                options.SaveToken = true;
            });
            return services;
        }
        static IServiceCollection AddCustomCulture(this IServiceCollection services)
        {
            services.AddJsonLocalization(opts => opts.ResourcesPath = configuration["LANGUAGE_PATH"]);
            //Multiple language setting
            var supportedCultures = new[]
            {
                new CultureInfo("en-US"),
                new CultureInfo("vi-VN")
            };
            var localizationOptions = new RequestLocalizationOptions()
            {
                DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };

            localizationOptions.RequestCultureProviders = new[]
            {
                 new RouteDataRequestCultureProvider()
                 {
                     RouteDataStringKey = "lang",
                     Options = localizationOptions
                 }
            };

            services.AddSingleton(localizationOptions);
            return services;
        }
    }
}
