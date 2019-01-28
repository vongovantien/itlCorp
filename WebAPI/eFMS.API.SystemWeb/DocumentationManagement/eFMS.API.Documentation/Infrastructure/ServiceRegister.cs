using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using LocalizationCultureCore.StringLocalizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using eFMS.IdentityServer.DL.UserManager;
using eFMS.API.Shipment.Service.Contexts;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Services;

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

            services.AddTransient<ICurrentUser, CurrentUser>();
            services.AddTransient<ITerminologyService, TerminologyService>();
            services.AddTransient<ICsTransactionService, CsTransactionService>();
        }
    }
}
