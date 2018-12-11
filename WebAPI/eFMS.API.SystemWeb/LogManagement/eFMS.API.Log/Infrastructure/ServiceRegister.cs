using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.DependencyInjection;
using eFMS.API.Log.DL.Services;
using eFMS.API.Log.DL.IService;
using eFMS.API.Log.Service.Contexts;

namespace eFMS.API.System.Infrastructure
{
    public static class ServiceRegister
    {

        public static void Register(IServiceCollection services)
        {
            services.AddScoped(typeof(IContextBase<>), typeof(Base<>));
            services.AddTransient<ICategoryLogService, CategoryLogService>();
        }
    }
}
