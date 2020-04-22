using Microsoft.AspNetCore.Builder;

namespace eFMS.API.Operation.Infrastructure.Middlewares
{
    public class LocalizationMiddleware
    {
        public void Configure(IApplicationBuilder app, RequestLocalizationOptions options)
        {
            app.UseRequestLocalization(options);
        }
    }
}
