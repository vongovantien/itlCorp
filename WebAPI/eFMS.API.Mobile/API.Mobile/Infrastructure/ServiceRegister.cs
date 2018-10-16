using API.Mobile.Repository;
using LocalizationCultureCore.StringLocalizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Mobile.Infrastructure
{
    public static class ServiceRegister
    {
        public static void Register(IServiceCollection services)
        {
            services.AddTransient<IStringLocalizer, JsonStringLocalizer>();
            services.AddTransient<IStringLocalizerFactory, JsonStringLocalizerFactory>();
            services.AddTransient<IStageRepository, StageRepositoryImpl>();
            services.AddTransient<ICommentRepository, CommentRepositoryImpl>();
            services.AddTransient<IJobRepository, JobRepositoryImpl>();
            services.AddTransient<IUserRepository, UserRepositoryImpl>();
        }
    }
}
