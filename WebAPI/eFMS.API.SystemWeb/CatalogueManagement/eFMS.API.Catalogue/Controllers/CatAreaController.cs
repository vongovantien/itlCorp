using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;
using SystemManagementAPI.Resources;

namespace eFMS.API.Catalogue.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatAreaController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatAreaService catAreaService;
        private readonly IMapper mapper;
        public CatAreaController(IStringLocalizer<LanguageSub> localizer, ICatAreaService service)
        {
            stringLocalizer = localizer;
            catAreaService = service;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(catAreaService.Get());
        }

        [HttpGet]
        [Route("GetByLanguage")]
        public IActionResult GetByLanguage()
        {
            var results = catAreaService.GetByLanguage();
            return Ok(results);
        }
    }
}