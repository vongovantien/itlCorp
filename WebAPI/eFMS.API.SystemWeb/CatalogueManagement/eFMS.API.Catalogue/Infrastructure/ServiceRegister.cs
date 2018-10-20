using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.DependencyInjection;
using eFMS.API.Catalogue.Service.Contexts;
using Microsoft.Extensions.Localization;
using LocalizationCultureCore.StringLocalizer;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Services;

namespace eFMS.API.Catalogue.Infrastructure
{
    public static class ServiceRegister
    {

        public static void Register(IServiceCollection services)
        {
            services.AddTransient<IStringLocalizer, JsonStringLocalizer>();
            services.AddTransient<IStringLocalizerFactory, JsonStringLocalizerFactory>();
            services.AddScoped(typeof(IContextBase<>), typeof(Base<>));

            services.AddTransient<ICatBranchService, CatBranchService>();
            services.AddTransient<ICatPlaceService, CatPlaceService>();
            services.AddTransient<ICatCountryService, CatCountryService>();
            services.AddTransient<ICatStageService, CatStageService>();
        }
    }
}
