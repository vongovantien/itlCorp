using eFMS.API.Report.DL.IService;
using eFMS.API.Report.DL.Services;
using eFMS.API.Report.Infrastructure.Filters;
using eFMS.API.Report.Service.Context;
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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace eFMS.API.Report.Infrastructure
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

            services.AddTransient<IReportDocumentService, ReportDocumentService>();
            services.AddTransient<ISaleReportService, SaleReportService>();
            services.AddTransient<IReportLogService, ReportLogService>();
            services.AddTransient<IUserBaseService, UserBaseService>();

        }
        public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(
                options =>
                {
                    options.DescribeAllEnumsAsStrings();
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
                                Title = $"eFMS Report Management API {description.ApiVersion}",
                                Version = description.ApiVersion.ToString(),
                                Description = "eFMS Report API Document"
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
