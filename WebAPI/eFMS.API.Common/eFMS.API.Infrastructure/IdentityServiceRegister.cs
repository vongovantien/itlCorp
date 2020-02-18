using eFMS.API.Infrastructure.Authorizations;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace eFMS.API.Infrastructure
{
    public class IdentityServiceRegister
    {
        public static void IdentityRegister(IServiceCollection services)
        {
            services.AddTransient<IClaimsTransformation, ClaimsExtender>();
            services.AddUserManager();
        }
    }
}
