using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace eFMS.API.System.Infrastructure.Extensions
{
    public static class SQLTableDependencyBuilderExtensions
    {
        public static void UseSqlTableDependency<T>(this IApplicationBuilder services, string connectionString)
            where T : IDatabaseSubscription
        {
            var serviceProvider = services.ApplicationServices;
            var subscription = serviceProvider.GetService<T>();
            subscription.Configure(connectionString);
        }
    }
}
