using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Infrastructure.Common;
using eFMS.API.System.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.System.Controllers
{
    /// <summary>
    /// Controller Department
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatDepartmentController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatDepartmentService catDepartmentService;
        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        public CatDepartmentController(IStringLocalizer<LanguageSub> localizer, ICatDepartmentService service)
        {
            stringLocalizer = localizer;
            catDepartmentService = service;
        }

        /// <summary>
        /// Get list department
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            var data = catDepartmentService.Get();
            return Ok(data);
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
            var _criteria = new CatDepartmentCriteria
            {
                Type = !string.IsNullOrEmpty(criteria.Type) ? criteria.Type.Trim() : criteria.Type,
                Keyword = !string.IsNullOrEmpty(criteria.Keyword) ? criteria.Keyword.Trim() : criteria.Keyword,
            };
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

        /// <summary>
        /// Insert department
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        //[Authorize]
        public IActionResult Insert(CatDepartmentModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            var hs = catDepartmentService.Insert(model);

            var message = HandleError.GetMessage(hs, Crud.Insert);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return Ok(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Update department
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("Update")]
        //[Authorize]
        public IActionResult Update(CatDepartmentModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            var hs = catDepartmentService.Update(model);

            var message = HandleError.GetMessage(hs, Crud.Update);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return Ok(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Delete department
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("Delete")]
        //[Authorize]
        public IActionResult Delete(int id)
        {
            var hs = catDepartmentService.Delete(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return Ok(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Get list department by office id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetDepartmentByOfficeId")]
        public IActionResult GetDepartmentByOfficeId(Guid id)
        {
            var data = catDepartmentService.GetDepartmentsByOfficeId(id);
            return Ok(data);
        }
    }

}
