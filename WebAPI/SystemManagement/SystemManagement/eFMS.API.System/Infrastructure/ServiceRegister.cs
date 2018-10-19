using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.DependencyInjection;
using eFMS.API.Catalog.Service.Contexts;
using Microsoft.Extensions.Localization;
using LocalizationCultureCore.StringLocalizer;
using eFMS.API.Catalog.DL.IService;
using eFMS.API.Catalog.DL.Services;

namespace eFMS.API.Catalog.Infrastructure
{
    public static class ServiceRegister
    {

        public static void Register(IServiceCollection services)
        {
            services.AddTransient<IStringLocalizer, JsonStringLocalizer>();
            services.AddTransient<IStringLocalizerFactory, JsonStringLocalizerFactory>();
            services.AddScoped(typeof(IContextBase<>), typeof(Base<>));

            services.AddTransient<IUserGroupService, UserGroupService>();
            services.AddTransient<ICatBranchService, CatBranchService>();
            services.AddTransient<ICatPlaceService, CatPlaceService>();
        }
    }
}
