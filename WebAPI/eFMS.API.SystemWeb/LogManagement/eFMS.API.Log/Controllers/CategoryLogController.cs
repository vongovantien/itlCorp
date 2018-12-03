using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Log.DL.Common;
using eFMS.API.Log.DL.IService;
using eFMS.API.Log.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eFMS.API.Log.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
                var data = catLogService.Paging(categoryCriteria.TableType, categoryCriteria.Query, page, size, out long rowCount);
                var result = new { data, totalItems = rowCount, page, size };
                return Ok(result);
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }
    }
}