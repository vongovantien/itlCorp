using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Mobile.Infrastructure;
using API.Mobile.Infrastructure.Middlewares;
using API.Mobile.Models;
using API.Mobile.Repository;
using API.Mobile.Resources;
using API.Mobile.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using static API.Mobile.Common.StatusEnum;

namespace API.Mobile.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class StageController : ControllerBase
    {
        private readonly IStageRepository stateRepository;
        private readonly ICommentRepository commentRepository;
        private IMemoryCache cache;
        private readonly IStringLocalizer stringLocalizer;

        public StageController(IStageRepository state, ICommentRepository comment, IMemoryCache memoryCache, IStringLocalizer<LanguageSub> localizer)
        {
            stateRepository = state;
            commentRepository = comment;
            cache = memoryCache;
            stringLocalizer = localizer;
        }

        /// <summary>
        /// Get by job Id
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get(string jobId)
        {
            var results = stateRepository.Get(jobId);
            return Ok(results);
        }

        [HttpGet]
        [Route("GetComments")]
        public IActionResult GetComments(string stageId)
        {
            var results = commentRepository.Get(stageId);
            return Ok(results);
        }

        [HttpPut]
        public IActionResult UpdateStatus(StageModel stage)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = stateRepository.UpdateStatus(stage);
            string message = HandleError.GetMessage(result, Crud.Update);
            if (result.Success)
            {
                return Ok(stringLocalizer[message]);
            }
            else
            {
                return BadRequest(stringLocalizer[message]);
            }
        }
    }
}