using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Catalog.DL.IService;
using eFMS.API.Catalog.DL.Models.Criteria;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Resources;

namespace eFMS.API.Catalog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatPlaceTypeController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatPlaceTypeService catPlaceTypeService;
        public CatPlaceTypeController(IStringLocalizer<LanguageSub> localizer, ICatPlaceTypeService service)
        {
            stringLocalizer = localizer;
            catPlaceTypeService = service;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(catPlaceTypeService.Get());
        }

        [HttpGet]
        [Route("Query")]
        public IActionResult Get(CatPlaceTypeCriteria criteria, string orderByProperty, bool isAscendingOrder)
        {
            //var results = catPlaceTypeService.Query(criteria, orderByProperty, isAscendingOrder);
            //return Ok(results);
            return Ok();
        }
    }
}