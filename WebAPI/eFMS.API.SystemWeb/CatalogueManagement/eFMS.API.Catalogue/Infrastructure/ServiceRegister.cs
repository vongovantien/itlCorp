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

namespace eFMS.API.Catalogue.Infrastructure
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
            services.AddTransient<ICatBranchService, CatBranchService>();
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
        }
    }
}
