using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Setting.DL.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using eFMS.API.Setting.DL.Common;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;
using ITL.NetCore.Common.Items;
using ITL.NetCore.Common;
using eFMS.API.Setting.DL.Models;
using eFMS.API.System.Infrastructure.Common;
using eFMS.API.Common;
using eFMS.API.Setting.DL.Models.Criteria;

namespace eFMS.API.Setting.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class EcusConnectionController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IEcusConnectionService ecusConnectionService;

        public EcusConnectionController(IStringLocalizer<LanguageSub> localizer, IEcusConnectionService service)
        {
            stringLocalizer = localizer;
            ecusConnectionService = service;
        }

        [HttpPost]
        [Route("Add")]
        public IActionResult AddNew(SetEcusConnectionModel model)
        {
            var existedMessage = CheckExist(model);
            if (existedMessage != null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedMessage });
            }
            model.DatetimeCreated = DateTime.Now;
            model.UserCreated = "admin";
            var hs = ecusConnectionService.Add(model);
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
        public IActionResult Update(SetEcusConnectionModel model)
        {
            var existedMessage = CheckExist(model);
            if (existedMessage != null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedMessage });
            }
            model.DatetimeModified = DateTime.Now;
            model.UserModified = "admin";
            var hs = ecusConnectionService.Update(model, x => x.Id == model.Id);
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
        public IActionResult Delete(int id_connection)
        {
            var hs = ecusConnectionService.Delete(x => x.Id == id_connection);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("Paging")]
        public IActionResult Paging(SetEcusConnectionCriteria criteria,int page_number, int page_size)
        {
            var data = ecusConnectionService.Paging(criteria, page_number, page_size, out int total_item);
            var result = new { data, totalItems = total_item, page_number, page_size };
            return Ok(result);
        }

        [HttpGet]
        [Route("GetAll")]
        public List<SetEcusConnectionModel> GetAll()
        {
            return ecusConnectionService.getConnections();
        }

        [HttpGet]
        [Route("GetDetails")]
        public SetEcusConnectionModel GetDetails(int id_connection)
        {
            return ecusConnectionService.getConnectionDetails(id_connection);
        }
        private string CheckExist(SetEcusConnectionModel model)
        {
            string message = string.Empty;
            var existed = ecusConnectionService.Any(x => x.Id!=model.Id && x.UserId == model.UserId && x.ServerName == model.ServerName && x.Dbname == model.Dbname);
            message = existed ? "This connection of "+model.username+" has already existed, Please check again!" : null;
            return message;
        }


    }
}