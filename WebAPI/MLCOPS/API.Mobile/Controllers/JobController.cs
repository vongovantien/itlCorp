using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Mobile.Models;
using API.Mobile.Repository;
using API.Mobile.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Mobile.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class JobController : ControllerBase
    {
        private readonly IJobRepository jobRepository;
        public JobController(IJobRepository job)
        {
            jobRepository = job;
        }

        [HttpPost]
        public JobViewModel Get(JobCriteria criteria, int? offset, int limit = 15)
        {
            return jobRepository.Get(criteria, offset, limit);
        }
    }
}