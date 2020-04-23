using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Setting.DL.Common;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using eFMS.API.Setting.Infrastructure.Middlewares;

namespace eFMS.API.Setting.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CategoryLogController : ControllerBase
    {
        private readonly ICategoryLogService catLogService;
        public CategoryLogController(ICategoryLogService service)
        {
            catLogService = service;
        }

        [HttpPost]
        [Route("Paging")]
        public IActionResult Paging(CategoryCriteria categoryCriteria, int page, int size)
        {
            try
            {
                var data = catLogService.Paging(categoryCriteria, page, size, out long rowCount);
                var result = new { data, totalItems = rowCount, page, size };
                return Ok(result);
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        [HttpGet]
        [Route("GetCategory")]
        public IActionResult GetCategory()
        {
            var data = catLogService.GetCollectionName();
            return Ok(data);
        }
    }
}