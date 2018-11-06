using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
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
    public class CatPartnerGroupController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatPartnerGroupService catPartnerGroupService;
        public CatPartnerGroupController(IStringLocalizer<LanguageSub> localizer, ICatPartnerGroupService service)
        {
            stringLocalizer = localizer;
            catPartnerGroupService = service;
        }

        [HttpGet]
        public IActionResult Get()
        {
            //return Ok(catPartnerGroupService.Get());
            return Ok(DataEnums.CatPartnerGroups);
        }
    }
}