using API.Mobile.Infrastructure;
using API.Mobile.Infrastructure.Middlewares;
using API.Mobile.Models;
using API.Mobile.Repository;
using API.Mobile.Resources;
using API.Mobile.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;

namespace API.Mobile.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    [Authorize]
    public class StageController : ControllerBase
    {
        private readonly IStageRepository stateRepository;
        private readonly ICommentRepository commentRepository;
        private IMemoryCache cache;
        private readonly IStringLocalizer stringLocalizer;

        public StageController(IStageRepository state,  ICommentRepository comment, IMemoryCache memoryCache, IStringLocalizer<LanguageSub> localizer)
        {
            stateRepository = state;
            commentRepository = comment;
            cache = memoryCache;
            stringLocalizer = localizer;
        }

        [HttpGet]
        [Route("GetComments")]
        public List<Comment> GetComments(string stageId)
        {
            var results = commentRepository.Get(stageId);
            return results;
        }

        [HttpPut]
        public LocalizedString UpdateStatus(StageComment stage)
        {
            if (!ModelState.IsValid)
            {
                return stringLocalizer[LanguageSub.DATA_NOT_EMPTY];
            }
            var result = stateRepository.UpdateStatus(stage);
            string message = HandleError.GetMessage(result, Crud.Update);
            if (result.Success)
            {
                return stringLocalizer[message];
            }
            else
            {
                return stringLocalizer[message];
            }
        }
    }
}