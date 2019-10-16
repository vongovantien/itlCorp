using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models.Criteria;
using eFMS.API.Setting.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eFMS.API.Setting.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class TariffController : ControllerBase
    {
        private readonly ITariffService tariffService;
        public TariffController(ITariffService service)
        {
            tariffService = service;
        }

        [HttpPost]
        [Route("Query")]
        public IActionResult Get(TariffCriteria criteria)
        {
            var results = tariffService.Query(criteria);
            return Ok(results);
        }

        /// <summary>
        /// get and paging the list of tariff
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="page">page to retrieve data</param>
        /// <param name="size">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Paging")]
        //[Authorize]
        public IActionResult Paging(TariffCriteria criteria, int page, int size)
        {
            var data = tariffService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        [HttpGet]
        public IActionResult Get()
        {
            var data = tariffService.Get();
            return Ok(data);
        }

    }
}