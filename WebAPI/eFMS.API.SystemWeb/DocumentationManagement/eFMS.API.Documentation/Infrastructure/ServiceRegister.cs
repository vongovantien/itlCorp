using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using LocalizationCultureCore.StringLocalizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using eFMS.IdentityServer.DL.UserManager;
using eFMS.API.Shipment.Service.Contexts;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Services;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;
using eFMS.API.Shipment.Infrastructure.Filters;
using eFMS.API.Common;
using System.Reflection;
using System.IO;
using System;
using eFMS.API.Documentation.DL.Common;

namespace eFMS.API.Shipment.Infrastructure
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
            services.AddTransient<ITerminologyService, TerminologyService>();
            services.AddTransient<ICsTransactionService, CsTransactionService>();
            services.AddTransient<ICsTransactionDetailService, CsTransactionDetailService>();
            services.AddTransient<ICsMawbcontainerService, CsMawbcontainerService>();
            services.AddTransient<ICsShipmentSurchargeService, CsShipmentSurchargeService>();
            services.AddTransient<IAcctCDNoteServices, AcctCDNoteServices>();
            services.AddTransient<ICsManifestService, CsManifestService>();
            services.AddTransient<ICsShippingInstructionService, CsShippingInstructionService>();
            services.AddTransient<IOpsTransactionService, OpsTransactionService>();
            services.AddTransient<IAcctSOAService, AcctSOAService>();
            services.AddTransient<IAcctAdvancePaymentService, AcctAdvancePaymentService>();
            services.AddTransient<IShipmentService, ShipmentService>();
            services.AddTransient<IAcctSettlementPaymentService, AcctSettlementPaymentService>();
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
                DefaultRequestCulture = new RequestCulture(culture: "en-US"),
                SupportedCultures = supportedCultures,

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
        public static IServiceCollection AddSwagger(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(
                options =>
                {
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
                                Title = $"eFMS Document API {description.ApiVersion}",
                                Version = description.ApiVersion.ToString(),
                                Description = "eFMS Document API Document"
                            });
                    }
                    //options.DocumentFilter<SwaggerAddEnumDescriptions>();

                    options.AddSecurityDefinition("oauth2", new OAuth2Scheme
                    {
                        Flow = "implicit", // just get token via browser (suitable for swagger SPA)
                        AuthorizationUrl = "",
                        Scopes = new Dictionary<string, string> { { "apimobile", "Catalogue API" } }
                    });
                    options.DocumentFilter<SwaggerAddEnumDescriptions>();
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
            service.Configure<WebUrl>(option => {
                option.Url = configuration.GetSection("WebUrl").Value;
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
        //public static IServiceCollection AddCatelogueManagementApiServices(this IServiceCollection services)
        //{
        //    services.AddHttpClient<ICatStageApiService, CatStageApiService>()
        //            .AddTypedClient<ICatPlaceApiService, CatPlaceApiService>()
        //            .AddTypedClient<ICatPartnerApiService, CatPartnerApiService>()
        //           .SetHandlerLifetime(TimeSpan.FromMinutes(5));  //Sample. Default lifetime is 5 minutes;

        //    return services;
        //}
        //public static IServiceCollection AddSystemManagementApiServices(this IServiceCollection services)
        //{
        //    services.AddHttpClient<ISysUserApiService, SysUserApiService>()
        //           .SetHandlerLifetime(TimeSpan.FromMinutes(5));  //Sample. Default lifetime is 5 minutes;

        //    return services;
        //}
    }
}
