using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.DependencyInjection;
using eFMS.API.System.Service.Contexts;
using Microsoft.Extensions.Localization;
using LocalizationCultureCore.StringLocalizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;
using eFMS.API.System.Infrastructure.Filters;
using System.IO;
using System.Reflection;
using System;
using eFMS.API.System.DL.Services;
using eFMS.API.System.DL.IService;
using eFMS.IdentityServer.DL.UserManager;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Connection.Caching;
using eFMS.API.System.Infrastructure.Common;
using StackExchange.Redis;

namespace eFMS.API.System.Infrastructure
{
    public static class ServiceRegister
    {

        public static void Register(IServiceCollection services)
        {
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddTransient<IStringLocalizer, JsonStringLocalizer>();
            services.AddTransient<IStringLocalizerFactory, JsonStringLocalizerFactory>();
            services.AddScoped(typeof(IContextBase<>), typeof(Base<>));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddUserManager();
            services.AddTransient<ICurrentUser, CurrentUser>();
            services.AddTransient<ISysUserService, SysUserService>();
            services.AddTransient<ISysOfficeService, SysOfficeService>();
            services.AddTransient<ISysCompanyService, SysCompanyService>();
            services.AddTransient<ICatDepartmentService, CatDepartmentService>();
            services.AddTransient<ISysGroupService, SysGroupService>();
            services.AddTransient<ISysImageService, SysImageService>();
            services.AddTransient<ISysUserLevelService, SysUserLevelService>();
            services.AddTransient<ISysEmployeeService, SysEmployeeService>();
            services.AddTransient<ISysPermissionSampleService, SysPermissionSampleService>();
            services.AddTransient<ISysPermissionSampleGeneralService, SysPermissionSampleGeneralService>();
            services.AddTransient<ISysPermissionSampleSpecialService, SysPermissionSampleSpecialService>();
            services.AddTransient<ISysRoleService, SysRoleService>();
            services.AddTransient<ISysAuthorizationService, SysAuthorizationService>();
            services.AddTransient<ISysMenuService, SysMenuService>();
            services.AddTransient<ISysUserPermissionService, SysUserPermissionService>();
            services.AddTransient<ISysUserPermissionGeneralService, SysUserPermissionGeneralService>();
            services.AddTransient<ISysUserPermissionSpecialService, SysUserPermissionSpecialService>();
            services.AddTransient<ISysAuthorizedApprovalService, SysAuthorizedApprovalService>();
            services.AddTransient<ISysSettingFlowService, SysSettingFlowService>();


            services.AddSingleton<ICacheServiceBase<SysMenu>>(x =>
            new CacheServiceBase<SysMenu>(x.GetRequiredService<IConnectionMultiplexer>()
            , Enum.GetName(typeof(CacheEntity), CacheEntity.SysMenu)));

            services.AddSingleton<ICacheServiceBase<SysAuthorizedApproval>>(x =>
            new CacheServiceBase<SysAuthorizedApproval>(x.GetRequiredService<IConnectionMultiplexer>()
            , Enum.GetName(typeof(CacheEntity), CacheEntity.sysAuthorize)));

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
