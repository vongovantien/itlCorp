using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using LocalizationCultureCore.StringLocalizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;
using eFMS.API.Operation.Infrastructure.Filters;
using System.IO;
using System.Reflection;
using System;
using eFMS.API.Operation.Service.Contexts;
using eFMS.API.Operation.DL.IService;
using eFMS.API.Operation.DL.Services;
using eFMS.API.Provider.Services.ServiceImpl;
using eFMS.API.Provider.Services.IService;
using StackExchange.Redis;
using eFMS.API.Operation.Service.Models;
using ITL.NetCore.Connection.Caching;
using eFMS.API.Operation.Infrastructure.Common;

namespace eFMS.API.Operation.Infrastructure
{
    public static class ServiceRegister
    {

        public static void Register(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")));

            services.AddTransient<IStringLocalizer, JsonStringLocalizer>();
            services.AddTransient<IStringLocalizerFactory, JsonStringLocalizerFactory>();
            services.AddScoped(typeof(IContextBase<>), typeof(Base<>));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddSingleton<ICacheServiceBase<CustomsDeclaration>>(x =>
            new CacheServiceBase<CustomsDeclaration>(x.GetRequiredService<IConnectionMultiplexer>()
            , Enum.GetName(typeof(CacheEntity), CacheEntity.CustomsDeclaration)));

            services.AddSingleton<ICacheServiceBase<SetEcusconnection>>(x =>
            new CacheServiceBase<SetEcusconnection>(x.GetRequiredService<IConnectionMultiplexer>()
            , Enum.GetName(typeof(CacheEntity), CacheEntity.SetEcusconnection)));


            //services.AddTransient<ICurrentUser, CurrentUser>();
            services.AddTransient<IOpsStageAssignedService, OpsStageAssignedService>();
            services.AddTransient<IEcusConnectionService, EcusConnectionService>();
            services.AddTransient<ICustomsDeclarationService, CustomsDeclarationService>();
        }
        public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
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
                                Description = "eFMS Operation API Document"
                            });
                    }
                    options.DocumentFilter<SwaggerAddEnumDescriptions>();

                    var security = new Dictionary<string, IEnumerable<string>>{
                        { "Bearer", new string[] { }},
                    };

                    options.AddSecurityDefinition("Bearer", new ApiKeyScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                        Name = "Authorization",
                        In = "header",
                        Type = "apiKey"
                    });

                    options.OperationFilter<AuthorizeCheckOperationFilter>(); // Required to use access token
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
