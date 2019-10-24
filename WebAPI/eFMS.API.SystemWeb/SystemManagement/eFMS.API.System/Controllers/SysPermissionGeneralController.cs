using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Infrastructure.Common;
using eFMS.API.System.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.System.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SysPermissionGeneralController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ISysPermissionGeneralService permissionGeneralService;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="generalService"></param>
        /// <param name="localizer"></param>
        public SysPermissionGeneralController(ISysPermissionGeneralService generalService, IStringLocalizer<LanguageSub> localizer)
        {
            permissionGeneralService = generalService;
            stringLocalizer = localizer;
        }

        /// <summary>
        /// get all list of permission General
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            var results = permissionGeneralService.Get();
            return Ok(results);
        }

        /// <summary>
        /// get list of permission general by criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("Query")]
        public IActionResult Query(SysPermissionGeneralCriteria criteria)
        {
            var results = permissionGeneralService.Query(criteria);
            return Ok(results);
        }

        /// <summary>
        /// get an existed item by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get(short id)
        {
            var data = permissionGeneralService.Get(x => x.Id == id).FirstOrDefault();
            return Ok(data);
        }

        /// <summary>
        /// add new systemGeneral
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("Add")]
        public IActionResult Add(SysPermissionGeneralModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            
            var hs = permissionGeneralService.Add(model);

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
        public IActionResult Update(SysPermissionGeneralModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            var hs = permissionGeneralService.Update(model, x => x.Id == model.Id);

            var message = HandleError.GetMessage(hs, Crud.Update);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return Ok(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// delete an existed item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public IActionResult Delete(short id)
        {
            var hs = permissionGeneralService.Delete(x => x.Id == id);

            var message = HandleError.GetMessage(hs, Crud.Update);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return Ok(result);
            }
            return Ok(result);
        }
    }
}
