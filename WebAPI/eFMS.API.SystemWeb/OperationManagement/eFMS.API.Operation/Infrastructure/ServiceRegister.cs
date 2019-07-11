using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using LocalizationCultureCore.StringLocalizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;
using eFMS.API.Operation.Infrastructure.Filters;
using eFMS.API.Common;
using System.IO;
using System.Reflection;
using System;
using eFMS.API.Operation.Service.Contexts;
using eFMS.API.Operation.DL.IService;
using eFMS.API.Operation.DL.Services;
using eFMS.API.Provider.Services.ServiceImpl;
using eFMS.API.Provider.Services.IService;
using eFMS.IdentityServer.DL.UserManager;

namespace eFMS.API.Operation.Infrastructure
{
    public static class ServiceRegister
    {

        public static void Register(IServiceCollection services)
        {
            services.AddTransient<IStringLocalizer, JsonStringLocalizer>();
            services.AddTransient<IStringLocalizerFactory, JsonStringLocalizerFactory>();
            services.AddScoped(typeof(IContextBase<>), typeof(Base<>));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddTransient<ICurrentUser, CurrentUser>();
            services.AddTransient<IOpsStageAssignedService, OpsStageAssignedService>();
            services.AddTransient<IEcusConnectionService, EcusConnectionService>();
            services.AddTransient<ICustomsDeclarationService, CustomsDeclarationService>();
            services.AddTransient<IOpsTransactionService, OpsTransactionSevice>();
        }

        public static IServiceCollection AddCulture(this IServiceCollection services, IConfiguration configuration)
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

        public static IServiceCollection AddAuthorize(this IServiceCollection services, IConfiguration configuration)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddIdentityServerAuthentication(options =>
            {
                options.Authority = configuration["Authentication:Authority"];
                options.RequireHttpsMetadata = bool.Parse(configuration["Authentication:RequireHttpsMetadata"]);
                options.ApiName = configuration["Authentication:ApiName"];
                options.ApiSecret = configuration["Authentication:ApiSecret"];
            });
            return services;
        }
        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(
                options =>
                {
                    //options.DescribeAllEnumsAsStrings();
                    var provider = services.BuildServiceProvider()
                    .GetRequiredService<IApiVersionDescriptionProvider>();
                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    if (xmlPath != null)
                    {
                        options.IncludeXmlComments(xmlPath);
                    }

                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerDoc(
                            description.GroupName,
                            new Info()
                            {
                                Title = $"eFMS Operation API {description.ApiVersion}",
                                Version = description.ApiVersion.ToString(),
                                Description = "eFMS Catalogue API Document"
                            });
                    }
                    options.DocumentFilter<SwaggerAddEnumDescriptions>();

                    options.AddSecurityDefinition("oauth2", new OAuth2Scheme
                    {
                        Flow = "implicit", // just get token via browser (suitable for swagger SPA)
                        AuthorizationUrl = "",
                        Scopes = new Dictionary<string, string> { { "apimobile", "Operation API" } }
                    });
                    options.OperationFilter<AuthorizeCheckOperationFilter>(); // Required to use access token
                });
            return services;
        }
        public static IServiceCollection AddConfigureSetting(this IServiceCollection service, IConfiguration configuration)
        {
            service.Configure<Settings>(options =>
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
            return service;
        }
        public static IServiceCollection AddCrossOrigin(this IServiceCollection services)
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

        public static IServiceCollection AddCatelogueManagementApiServices(this IServiceCollection services)
        {
            services.AddHttpClient<ICatStageApiService, CatStageApiService>()
                   .SetHandlerLifetime(TimeSpan.FromMinutes(5));
            services.AddHttpClient<ICatPartnerApiService, CatPartnerApiService>()
                   .SetHandlerLifetime(TimeSpan.FromMinutes(5));  //Sample. Default lifetime is 5 minutes;
            services.AddHttpClient<ICatPlaceApiService, CatPlaceApiService>()
                   .SetHandlerLifetime(TimeSpan.FromMinutes(5));  //Sample. Default lifetime is 5 minutes;
            services.AddHttpClient<ICatCountryApiService, CatCountryApiService>()
                  .SetHandlerLifetime(TimeSpan.FromMinutes(5));  //Sample. Default lifetime is 5 minutes;
            services.AddHttpClient<ICatCommodityApiService, CatCommodityApiService>()
                  .SetHandlerLifetime(TimeSpan.FromMinutes(5));  //Sample. Default lifetime is 5 minutes;
            return services;
        }

        public static IServiceCollection AddSystemManagementApiServices(this IServiceCollection services)
        {
            services.AddHttpClient<ICatDepartmentApiService, CatDepartmentApiService>()
                   .SetHandlerLifetime(TimeSpan.FromMinutes(5));
            return services;
        }
    }
}
