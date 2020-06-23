using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.DependencyInjection;
using eFMS.API.Setting.DL.Services;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Catalogue.Service.Contexts;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;
using eFMS.API.Setting.Infrastructure.Filters;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System;
using eFMS.API.Common;
using eFMS.API.Provider.Services.IService;
using eFMS.API.Provider.Services.ServiceImpl;
using System.IO;
using System.Reflection;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Localization;
using LocalizationCultureCore.StringLocalizer;

namespace eFMS.API.Setting.Infrastructure
{
    public static class ServiceRegister
    {

        public static void Register(IServiceCollection services)
        {
            services.AddTransient<IStringLocalizer, JsonStringLocalizer>();
            services.AddTransient<IStringLocalizerFactory, JsonStringLocalizerFactory>();
            services.AddScoped(typeof(IContextBase<>), typeof(Base<>));
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IActionContextAccessor, ActionContextAccessor>();
            services.AddTransient<ITariffService, TariffService>();
            services.AddTransient<IUnlockRequestService, UnlockRequestService>();
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
                                Title = $"eFMS Setting API {description.ApiVersion}",
                                Version = description.ApiVersion.ToString(),
                                Description = "eFMS Setting API Document"
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
    }
}
