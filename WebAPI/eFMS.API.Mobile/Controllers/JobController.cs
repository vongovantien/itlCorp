using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Mobile.Infrastructure.Middlewares;
using API.Mobile.Models;
using API.Mobile.Repository;
using API.Mobile.Resources;
using API.Mobile.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace API.Mobile.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    [Authorize]
    public class JobController : ControllerBase
    {
        private readonly IJobRepository jobRepository;
        private readonly IStringLocalizer stringLocalizer;
        public JobController(IJobRepository job, IStringLocalizer<LanguageSub> localizer)
        {
            jobRepository = job;
            stringLocalizer = localizer;
        }

        [HttpPost]
        public JobViewModel Get(JobCriteria criteria, int? offset, int limit = 15)
        {
            var userId = User.FindFirst("UserId")?.Value;
            return jobRepository.Get(criteria, userId, offset, limit);
        }
    }
}