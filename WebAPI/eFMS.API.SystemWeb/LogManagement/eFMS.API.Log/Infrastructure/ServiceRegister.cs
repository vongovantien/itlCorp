using eFMS.API.Log.DL.IService;
using eFMS.API.Log.DL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace eFMS.API.System.Infrastructure
{
    public static class ServiceRegister
    {

        public static void Register(IServiceCollection services)
        {
            //services.AddTransient<IStringLocalizer, JsonStringLocalizer>();
            //services.AddTransient<IStringLocalizerFactory, JsonStringLocalizerFactory>();
            //services.AddScoped(typeof(IContextBase<>), typeof(Base<>));

            //services.AddTransient<IUserGroupService, UserGroupService>();
            //services.AddTransient<ICatBranchService, CatBranchService>();
            //services.AddTransient<ICatPlaceService, CatPlaceService>();
            //services.AddTransient<ICatDepartmentService, CatDepartmentService>();
            //services.AddTransient<ISysUserService, SysUserService>();
            //services.AddTransient<ISysEmployeeService, SysEmployeeService>();
            services.AddTransient<ICatCurrencyService, CatCurrencyService>();
        }
    }
}
