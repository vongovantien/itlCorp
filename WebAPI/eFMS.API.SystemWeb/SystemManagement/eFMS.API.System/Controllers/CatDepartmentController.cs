using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.System.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatDepartmentController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatDepartmentService catDepartmentService;
        private readonly IMapper mapper;
        public CatDepartmentController(IStringLocalizer<LanguageSub> localizer, IMapper mapper, ICatDepartmentService service)
        {
            stringLocalizer = localizer;
            catDepartmentService = service;
            mapper = this.mapper;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(catDepartmentService.Get());

        }

        /// <summary>
        /// Query Data
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("QueryData")]
        public IActionResult QueryData(CatDepartmentCriteria criteria)
        {
            var _criteria = new CatDepartmentCriteria {
                Type = !string.IsNullOrEmpty(criteria.Type) ? criteria.Type.Trim() : criteria.Type,
                Keyword = !string.IsNullOrEmpty(criteria.Keyword) ? criteria.Keyword.Trim() : criteria.Keyword,
            };            
            var data = catDepartmentService.QueryData(_criteria);
            return Ok(data);
        }

        /// <summary>
        /// Paging
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpPost("Paging")]
        public IActionResult Paging(CatDepartmentCriteria criteria, int page, int size)
        {
            var data = catDepartmentService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        /// <summary>
        /// Get department by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        public IActionResult GetDepartmentById(int id)
        {
            var result = catDepartmentService.GetDepartmentById(id);
            return Ok(result);
        }

    }

}
