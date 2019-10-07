using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.DependencyInjection;
using eFMS.API.System.Service.Contexts;
using Microsoft.Extensions.Localization;
using LocalizationCultureCore.StringLocalizer;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Services;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Localization;
using Swashbuckle.AspNetCore.Swagger;
using eFMS.API.System.Infrastructure.Filters;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace eFMS.API.System.Infrastructure
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
            //services.AddTransient<IUserGroupService, UserGroupService>();
            //services.AddTransient<ICatBranchService, CatBranchService>();
            services.AddTransient<ICatPlaceService, CatPlaceService>();
            services.AddTransient<ICatDepartmentService, CatDepartmentService>();
            services.AddTransient<ISysUserService, SysUserService>();
            services.AddTransient<ISysEmployeeService, SysEmployeeService>();
            services.AddTransient<ISysBuService, SysBuService>();

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

                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerDoc(
                            description.GroupName,
                            new Info()
                            {
                                Title = $"eFMS System API {description.ApiVersion}",
                                Version = description.ApiVersion.ToString(),
                                Description = "eFMS System API Document"
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
    }
}
