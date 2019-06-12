using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.Infrastructure.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using eFMS.API.Setting.Infrastructure.Middlewares;
using eFMS.API.Setting.Resources;
using System.Diagnostics.Contracts;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Setting.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CustomsDeclarationController : Controller
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICustomsDeclarationService customsDeclarationService;

        public CustomsDeclarationController(IStringLocalizer<LanguageSub> localizer, ICustomsDeclarationService service)
        {
            stringLocalizer = localizer;
            customsDeclarationService = service;
        }

        [HttpPost]
        [Route("Add")]
        public IActionResult AddNew(CustomsDeclarationModel model)
        {
            var existedMessage = CheckExist(model, model.Id);
            if (existedMessage != null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedMessage });
            }
            model.DatetimeCreated = DateTime.Now;
            model.DatetimeModified = DateTime.Now;
            model.UserCreated = model.UserModified = "admin";
            var hs = customsDeclarationService.Add(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPut]
        [Route("Update")]
        public IActionResult Update(CustomsDeclarationModel model)
        {
            var existedMessage = CheckExist(model, model.Id);
            if (existedMessage != null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedMessage });
            }
            model.DatetimeModified = DateTime.Now;
            model.UserModified = "admin";
            var hs = customsDeclarationService.Update(model, x => x.Id == model.Id);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpDelete]
        [Route("Delete")]
        public IActionResult Delete(int id)
        {
            var hs = customsDeclarationService.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        //[HttpGet("ImportClearancesFromEcus")]
        //public IActionResult ImportClearancesFromEcus()
        //{
        //    ResultHandle result = customsDeclarationService.ImportClearancesFromEcus();
        //}
        private string CheckExist(CustomsDeclarationModel model, decimal id)
        {
            string message = string.Empty;
            if (id > 0)
            {
                if (customsDeclarationService.Any(x => x.ClearanceNo == model.ClearanceNo))
                {
                    message = stringLocalizer[LanguageSub.MSG_CLEARANCENO_EXISTED].Value;
                }
            }
            else
            {
                if (customsDeclarationService.Any(x => (x.ClearanceNo == model.ClearanceNo && x.Id != id)))
                {
                    message = stringLocalizer[LanguageSub.MSG_CLEARANCENO_EXISTED].Value;
                }
            }
            return message;
        }
    }
}
