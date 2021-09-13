using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;


namespace eFMS.API.System.Controllers
{
    /// <summary>
    /// Controller EmailSetting
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SysEmailSettingController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ISysEmailSettingService sysEmailSettingService;
        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        public SysEmailSettingController(IStringLocalizer<LanguageSub> localizer, ISysEmailSettingService service)
        {
            stringLocalizer = localizer;
            sysEmailSettingService = service;
        }

        /// <summary>
        /// Get list EmailSetting
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var data = sysEmailSettingService.Get();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }


        /// <summary>
        /// Get EmailSetting by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        public IActionResult GetEmailSettingById(int id)
        {
            var result = sysEmailSettingService.GetEmailSettingById(id);
            return Ok(result);
        }

        /// <summary>
        /// Get List EmailSetting by Dept id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetEmailSettingByDeptId")]

        public IActionResult GetEmailSettingByDeptId(int id)
        {
            var result = sysEmailSettingService.GetEmailSettingsByDebtId(id);
            return Ok(result);
        }

        /// <summary>
        /// Insert EmailSetting
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        //[Authorize]
        [Route("Add")]
        public IActionResult Insert(SysEmailSettingModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            if (sysEmailSettingService.CheckExistsEmailInDeptInsert(model))
            {
                return Ok(new ResultHandle { Status = false, Message = stringLocalizer[SystemLanguageSub.MSG_EMAIL_EN_EXISTED_IN_DEPT].Value, Data = model });
            }

            if (!sysEmailSettingService.CheckValidEmail(model))
            {
                return Ok(new ResultHandle { Status = false, Message = stringLocalizer[SystemLanguageSub.MSG_EMAIL_EN_NOT_VALID].Value, Data = model });
            }

            var hs = sysEmailSettingService.AddEmailSetting(model);

            var message = HandleError.GetMessage(hs, Crud.Insert);

            ResultHandle result = new ResultHandle
            {
                Status = hs.Success,
                Message = stringLocalizer[message].Value,
                Data = model
            };

            if (!hs.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Update EmailSetting
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        //[Authorize]
        [Route("Update")]
        public IActionResult Update(SysEmailSettingModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            if (!sysEmailSettingService.CheckExistsEmailInDept(model))
            {
                return Ok(new ResultHandle { Status = false, Message = stringLocalizer[SystemLanguageSub.MSG_EMAIL_EN_NOT_EXISTED_IN_DEPT].Value, Data = model });
            }

            if (!sysEmailSettingService.CheckValidEmail(model))
            {
                return Ok(new ResultHandle { Status = false, Message = stringLocalizer[SystemLanguageSub.MSG_EMAIL_EN_NOT_VALID].Value, Data = model });
            }

            var hs = sysEmailSettingService.UpdateEmailInfo(model);

            var message = HandleError.GetMessage(hs, Crud.Update);

            ResultHandle result = new ResultHandle 
            { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Delete EmailSetting
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("Delete")]
        public IActionResult Delete(int id)
        {
            var hs = sysEmailSettingService.DeleteEmailSetting(id);

            var message = HandleError.GetMessage(hs, Crud.Delete);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return Ok(result);
            }
            return Ok(result);
        }

    }
}
