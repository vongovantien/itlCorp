using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
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
        private readonly ISysPermissionSampleService permissionGeneralService;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="generalService"></param>
        /// <param name="localizer"></param>
        public SysPermissionGeneralController(ISysPermissionSampleService generalService, IStringLocalizer<LanguageSub> localizer)
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

        [HttpPost]
        [Route("Paging")]
        public IActionResult Paging(SysPermissionGeneralCriteria criteria, int page, int size)
        {
            var data = permissionGeneralService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

      
        /// <summary>
        /// get data combobox
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetDataCombobox")]
        public IActionResult GetDataCombobox()
        {
            var data = permissionGeneralService.Get().Select(x => new { x.Id, x.Name });
            return Ok(data);
        }

        /// <summary>
        /// get an existed item by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Get")]
        public IActionResult Get(Guid? id)
        {
            var data = permissionGeneralService.GetBy(id);
            return Ok(data);
        }

        /// <summary>
        /// add new systemGeneral
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("Add")]
        [Authorize]
        public IActionResult Add(SysPermissionSampleModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            model.Id = Guid.NewGuid();
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
        [Authorize]
        public IActionResult Update(SysPermissionSampleModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            var hs = permissionGeneralService.Update(model);

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
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            var permission = permissionGeneralService.Get(x => x.Id == id);
            if (permission != null && permission.FirstOrDefault(x => x.Active == true) != null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[SystemLanguageSub.MSG_ITEM_IS_ACTIVE_NOT_ALLOW_DELETED].Value });
            }
            var hs = permissionGeneralService.Delete(id);

            var message = HandleError.GetMessage(hs, Crud.Delete);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return Ok(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// get all level permission
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetLevelPermissions")]
        public IActionResult GetLevelPermissions()
        {
            var results = permissionGeneralService.GetLevelPermissions();
            return Ok(results);
        }
    }
}
