using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.DependencyInjection;
using eFMS.API.Catalogue.Service.Contexts;
using Microsoft.Extensions.Localization;
using LocalizationCultureCore.StringLocalizer;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;
using eFMS.API.Catalogue.Infrastructure.Filters;
using System.IO;
using System.Reflection;
using System;
using StackExchange.Redis;
using ITL.NetCore.Connection.Caching;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Catalogue.Infrastructure.Common;

namespace eFMS.API.Catalogue.Infrastructure
{
    public static class ServiceRegister
    {

        public static void Register(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")));

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddTransient<IStringLocalizer, JsonStringLocalizer>();
            services.AddTransient<IStringLocalizerFactory, JsonStringLocalizerFactory>();
            services.AddScoped(typeof(IContextBase<>), typeof(Base<>));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddSingleton<ICacheServiceBase<CatCountry>>(x =>
            new CacheServiceBase<CatCountry>(x.GetRequiredService<IConnectionMultiplexer>()
            , Enum.GetName(typeof(CacheEntity), CacheEntity.CatCountry)));

            services.AddSingleton<ICacheServiceBase<CatArea>>(x =>
            new CacheServiceBase<CatArea>(x.GetRequiredService<IConnectionMultiplexer>()
            , Enum.GetName(typeof(CacheEntity), CacheEntity.CatArea)));

            services.AddSingleton<ICacheServiceBase<CatCurrency>>(x =>
            new CacheServiceBase<CatCurrency>(x.GetRequiredService<IConnectionMultiplexer>()
            , Enum.GetName(typeof(CacheEntity), CacheEntity.CatCurrency)));

            services.AddSingleton<ICacheServiceBase<CatChargeDefaultAccount>>(x =>
            new CacheServiceBase<CatChargeDefaultAccount>(x.GetRequiredService<IConnectionMultiplexer>()
            , Enum.GetName(typeof(CacheEntity), CacheEntity.CatCharge)));

            services.AddSingleton<ICacheServiceBase<CatCharge>>(x =>
            new CacheServiceBase<CatCharge>(x.GetRequiredService<IConnectionMultiplexer>()
            , Enum.GetName(typeof(CacheEntity), CacheEntity.CatCharge)));

            services.AddSingleton<ICacheServiceBase<CatPartner>>(x =>
            new CacheServiceBase<CatPartner>(x.GetRequiredService<IConnectionMultiplexer>()
            , Enum.GetName(typeof(CacheEntity), CacheEntity.CatPartner)));

            services.AddSingleton<ICacheServiceBase<CatCommodityGroup>>(x =>
            new CacheServiceBase<CatCommodityGroup>(x.GetRequiredService<IConnectionMultiplexer>()
            , Enum.GetName(typeof(CacheEntity), CacheEntity.CatCommodityGroup)));

            services.AddSingleton<ICacheServiceBase<CatCommodity>>(x =>
            new CacheServiceBase<CatCommodity>(x.GetRequiredService<IConnectionMultiplexer>()
            , Enum.GetName(typeof(CacheEntity), CacheEntity.CatCommodity)));

            services.AddSingleton<ICacheServiceBase<CatPlace>>(x =>
            new CacheServiceBase<CatPlace>(x.GetRequiredService<IConnectionMultiplexer>()
            , Enum.GetName(typeof(CacheEntity), CacheEntity.CatPlace)));

            services.AddSingleton<ICacheServiceBase<CatStage>>(x =>
            new CacheServiceBase<CatStage>(x.GetRequiredService<IConnectionMultiplexer>()
            , Enum.GetName(typeof(CacheEntity), CacheEntity.CatStage)));

            services.AddSingleton<ICacheServiceBase<CatUnit>>(x =>
            new CacheServiceBase<CatUnit>(x.GetRequiredService<IConnectionMultiplexer>()
            , Enum.GetName(typeof(CacheEntity), CacheEntity.CatStage)));

            services.AddSingleton<ICacheServiceBase<CatChargeGroup>>(x =>
            new CacheServiceBase<CatChargeGroup>(x.GetRequiredService<IConnectionMultiplexer>()
            , Enum.GetName(typeof(CacheEntity), CacheEntity.CatChargeGroup)));

            services.AddSingleton<ICacheServiceBase<CatContract>>(x =>
            new CacheServiceBase<CatContract>(x.GetRequiredService<IConnectionMultiplexer>()
            , Enum.GetName(typeof(CacheEntity), CacheEntity.CatContract)));


            services.AddSingleton<ICacheServiceBase<CatChartOfAccounts>>(x =>
            new CacheServiceBase<CatChartOfAccounts>(x.GetRequiredService<IConnectionMultiplexer>()
            , Enum.GetName(typeof(CacheEntity), CacheEntity.catChartOfAccounts)));

            services.AddTransient<ICurrentUser, CurrentUser>();
            //services.AddTransient<ICatBranchService, CatBranchService>();
            services.AddTransient<ICatPlaceService, CatPlaceService>();
            services.AddTransient<ICatCountryService, CatCountryService>();
            services.AddTransient<ICatStageService, CatStageService>();
            services.AddTransient<ICatAreaService, CatAreaService>();
            services.AddTransient<ICatCommodityGroupService, CatCommodityGroupService>();
            services.AddTransient<ICatCommodityService, CatCommodityService>();
            services.AddTransient<ICatPartnerService, CatPartnerService>();
            services.AddTransient<ICatPartnerGroupService, CatPartnerGroupService>();
            services.AddTransient<ICatUnitService, CatUnitService>();
            services.AddTransient<ICatChargeService, CatChargeService>();
            services.AddTransient<ICatChargeDefaultAccountService, CatChargeDefaultService>();
            services.AddTransient<ICatCurrencyService, CatCurrencyService>();
            services.AddTransient<ICatCurrencyExchangeService, CatCurrencyExchangeService>();
            services.AddTransient<ICatContractService, CatContractService>();
            services.AddTransient<ICatPartnerChargeService, CatPartnerChargeService>();
            services.AddTransient<ICatChargeGroupService, CatChargeGroupService>();
            services.AddTransient<ICatChartOfAccountsService, CatChartOfAccountsService>();
            services.AddTransient<ICatIncotermService, CatIncotermService>();

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
                                Title = $"eFMS Catalogue API {description.ApiVersion}",
                                Version = description.ApiVersion.ToString(),
                                Description = "eFMS Catalogue API Document"
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

        #region For Demo
        //public static IServiceCollection AddAuthorize(this IServiceCollection services, IConfiguration configuration)
        //{
        //    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        //    services.AddAuthentication(options =>
        //    {
        //        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        //    })
        //    .AddJwtBearer(options =>
        //    {
        //        options.Authority = configuration["Authentication:Authority"];
        //        options.RequireHttpsMetadata = bool.Parse(configuration["Authentication:RequireHttpsMetadata"]);
        //        options.Audience = configuration["Authentication:ApiName"];
        //        options.SaveToken = true;
        //        //options.Events = new JwtBearerEvents() //for test
        //        //{
        //        //    OnTokenValidated = async context =>
        //        //    {
        //        //        try
        //        //        {
        //        //            String userID = context.Principal.FindFirstValue("id");

        //        //                List<Claim> lstClaim = new List<Claim>();
        //        //                lstClaim.Add(new Claim(JwtClaimTypes.Role, "ABC"));
        //        //                context.Principal.AddIdentity(new ClaimsIdentity(lstClaim, JwtBearerDefaults.AuthenticationScheme, "name", "role"));
        //        //        }
        //        //        catch { }
        //        //    }
        //        //};
        //    });
        //    return services;
        //}
        #endregion
    }
}
