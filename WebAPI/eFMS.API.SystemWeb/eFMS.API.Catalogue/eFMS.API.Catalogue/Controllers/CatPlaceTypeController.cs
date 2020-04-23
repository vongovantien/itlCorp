﻿using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Common.Globals;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.Catalogue.Controllers
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
    }
}