using eFMS.API.System.Service.Contexts;
using eFMS.IdentityServer.DL.IService;
using eFMS.IdentityServer.DL.Services;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.DependencyInjection;

namespace eFMS.IdentityServer.Infrastructure
{
    public static class ServiceRegister
    {
        public static void Register(IServiceCollection services)
        {
            services.AddTransient<IAuthenUserService, AuthenticateService>();
            services.AddScoped(typeof(IContextBase<>), typeof(Base<>));

            services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidator>();
            services.AddTransient<IProfileService, ProfileService>();
            services.AddTransient<ISysUserLogService, SysUserLogService>();
        }
    }
}
