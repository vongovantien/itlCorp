using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using API.Mobile.Common;
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
        private readonly IStageRepository stateRepository;
        public JobController(IJobRepository job, IStringLocalizer<LanguageSub> localizer, IStageRepository stateRepo)
        {
            jobRepository = job;
            stringLocalizer = localizer;
            stateRepository = stateRepo;
        }

        [HttpPost]
        [Route("GetBy")]
        public JobViewModel Get(JobCriteria criteria, int? offset, int limit = 15)
        {
            var userId = User.FindFirst("UserId")?.Value;
            //var userId = FakeData.user.UserId;
            return jobRepository.Get(criteria, userId, offset, limit);
        }

        /// <summary>
        /// Get by job Id
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet]
        public JobDetailModel Get([Required]string jobId)
        {
            var job = jobRepository.Get(jobId);
            var stages = stateRepository.Get(jobId);
            var results = new JobDetailModel { Job = job, Stages = stages };
            return results;
        }

        [HttpPost]
        [Route("PerformanceReports")]
        public List<JobPerformance> Get(JobPerformanceCriteria criteria)
        {
            return jobRepository.Get(criteria);
        }
    }
}