﻿using eFMS.API.SystemFileManagement.DL.IService;
using eFMS.API.SystemFileManagement.DL.Services;
using eFMS.API.SystemFileManagement.Infrastructure.Filters;
using eFMS.API.SystemFileManagement.Service.Contexts;
using ITL.NetCore.Connection.EF;
using LocalizationCultureCore.StringLocalizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace eFMS.API.SystemFileManagement.Infrastructure
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
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddTransient<IUserBaseService, UserBaseService>();
            services.AddTransient<IAWSS3Service, AWSS3Service>();
            services.AddTransient<IEDocService, EDocService>();
            services.AddTransient<IAttachFileTemplateService, AttachFilteTemplateService>();

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
                                Title = $"eFMS SystemFileManagement API {description.ApiVersion}",
                                Version = description.ApiVersion.ToString(),
                                Description = "eFMS SystemFileManagement API Document"
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
