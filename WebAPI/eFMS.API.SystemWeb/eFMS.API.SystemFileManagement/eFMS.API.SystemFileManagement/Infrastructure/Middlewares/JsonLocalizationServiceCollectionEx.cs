using System;
using LocalizationCultureCore;
using LocalizationCultureCore.StringLocalizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;

namespace SystemManagementAPI.Infrastructure.Middlewares
{
    public static class JsonLocalizationServiceCollectionEx
    {
        public static IServiceCollection AddJsonLocalization(this IServiceCollection services)
        {
            return AddJsonLocalization(services, setupAction: null);
        }
        public static IServiceCollection AddJsonLocalization(this IServiceCollection services, Action<JsonLocalizationOptions> setupAction)
        {
            services.TryAdd(new ServiceDescriptor(typeof(IStringLocalizerFactory),
                typeof(JsonStringLocalizerFactory), ServiceLifetime.Singleton));
            services.TryAdd(new ServiceDescriptor(typeof(IStringLocalizer),
                typeof(JsonStringLocalizer), ServiceLifetime.Singleton));

            if (setupAction != null)
            {
                services.Configure(setupAction);
            }
            return services;
        }
    }


    public interface IStringLocalizerFactory
    {
        IStringLocalizer Create(Type resourceSource);
        IStringLocalizer Create(string baseName, string location);
    }
}
