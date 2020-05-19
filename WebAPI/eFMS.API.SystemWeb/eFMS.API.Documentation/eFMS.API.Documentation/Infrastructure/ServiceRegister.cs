using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using LocalizationCultureCore.StringLocalizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using eFMS.API.Shipment.Service.Contexts;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Services;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;
using eFMS.API.Shipment.Infrastructure.Filters;
using System.Reflection;
using System.IO;
using System;

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
            
            services.AddTransient<ITerminologyService, TerminologyService>();
            services.AddTransient<ICsTransactionService, CsTransactionService>();
            services.AddTransient<ICsTransactionDetailService, CsTransactionDetailService>();
            services.AddTransient<ICsMawbcontainerService, CsMawbcontainerService>();
            services.AddTransient<ICsShipmentSurchargeService, CsShipmentSurchargeService>();
            services.AddTransient<IAcctCDNoteServices, AcctCDNoteServices>();
            services.AddTransient<ICsManifestService, CsManifestService>();
            services.AddTransient<ICsShippingInstructionService, CsShippingInstructionService>();
            services.AddTransient<ICsBookingNoteService, CsBookingNoteService>();

            services.AddTransient<IOpsTransactionService, OpsTransactionService>();
            services.AddTransient<IShipmentService, ShipmentService>();
            services.AddTransient<ICsArrivalFrieghtChargeService, CsArrivalFrieghtChargeService>();
            services.AddTransient<ICsDimensionDetailService, CsDimensionDetailService>();
            services.AddTransient<ISysImageService, SysImageService>();
            services.AddTransient<ICsAirWayBillService, CsAirWayBillService>();
            services.AddTransient<ICsShipmentOtherChargeService, CsShipmentOtherChargeService>();
            services.AddTransient<ISaleReportService, SaleReportService>();
            services.AddTransient<ICurrencyExchangeService, CurrencyExchangeService>();
            services.AddTransient<IDocSendMailService, DocSendMailService>();
        }
        public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
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
