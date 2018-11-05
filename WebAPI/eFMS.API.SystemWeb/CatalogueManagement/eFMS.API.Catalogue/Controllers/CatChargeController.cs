using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Infrastructure.Common;
using eFMS.API.Common;
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
    public class CatChargeController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatChargeService catChargeService;

        public CatChargeController(IStringLocalizer<LanguageSub> localizer, ICatChargeService service)
        {
            stringLocalizer = localizer;
            catChargeService = service;
        }

        [HttpPost]
        [Route("paging/{pageNumber}/{pageSize}")]
        public IActionResult Get(CatChargeCriteria criteria,int pageNumber,int pageSize)
        {
            var data = catChargeService.GetCharges(criteria, pageNumber, pageSize, out int rowCount);
            var result = new { data, totalItems = rowCount, pageNumber, pageSize };
            return Ok(result);
        }

        [HttpGet]
        [Route("getById/{id}")]
        public IActionResult Get(string id)
        {
            var result = catChargeService.Get(x => x.Id == id).FirstOrDefault();
            return Ok(result);
        }

        [HttpGet]
        [Route("getAll")]
        public IActionResult All()
        {
            var data = catChargeService.Get();
            return Ok(data);
        }






    }
}