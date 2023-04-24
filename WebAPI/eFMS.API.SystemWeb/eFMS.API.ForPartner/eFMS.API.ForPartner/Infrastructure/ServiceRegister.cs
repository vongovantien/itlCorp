
using eFMS.API.ForPartner.DL.IService;
using eFMS.API.ForPartner.DL.Service;
using eFMS.API.ForPartner.Infrastructure.Filters;
using eFMS.API.ForPartner.Service.Contexts;
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

namespace eFMS.API.ForPartner.Infrastructure
{
    public static class ServiceRegister
    {

        public static void Register(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IStringLocalizer, JsonStringLocalizer>();
            services.AddTransient<IStringLocalizerFactory, JsonStringLocalizerFactory>();
            services.AddScoped(typeof(IContextBase<>), typeof(Base<>));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            //services.AddTransient<IUserService, UserService>();
            services.AddTransient<ICurrencyExchangeService, CurrencyExchangeService>();
            services.AddTransient<IAccountingManagementService, AccAccountingManagementService>();
            services.AddTransient<IActionFuncLogService, ActionFuncLogService>();
            services.AddTransient<IAccountPayableService, AccountPayableService>();
            services.AddTransient<IAccountingPaymentService, AccAccountingPaymentService>();
            services.AddTransient<IAccountReceivableService, AccountReceivableService>();
            services.AddTransient<ICatPartnerBankService, CatPartnerBankService>();
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
                                Title = $"eFMS Partner API {description.ApiVersion}",
                                Version = description.ApiVersion.ToString(),
                                Description = "eFMS Partner API Document",
                                Contact = new Contact()
                                {
                                    Name = "LogTecHub",
                                    Email = @"efms-team@logtechub.com",
                                    Url = @"https://logtechub.com"
                                },
                                License = new License()
                                {
                                    Name = "LogTecHub 1.0",
                                    Url = @"https://logtechub.com"
                                },
                                TermsOfService = @"https://logtechub.com"
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
