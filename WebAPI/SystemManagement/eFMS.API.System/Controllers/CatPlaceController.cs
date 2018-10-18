using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models.Criteria;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;
using SystemManagementAPI.Resources;

namespace eFMS.API.System.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatPlaceController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatPlaceService catPlaceService;
        public CatPlaceController(IStringLocalizer<LanguageSub> localizer, ICatPlaceService service)
        {
            stringLocalizer = localizer;
            catPlaceService = service;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var results = catPlaceService.Get();
            return Ok(results);
        }

        [HttpGet]
        [Route("Query")]
        public IActionResult Get(CatPlaceCriteria criteria, string orderByProperty, bool isAscendingOrder)
        {
            return Ok();
        }
    }
}