using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Shipment.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;
using SystemManagementAPI.Resources;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Documentation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class OpsTransactionController : Controller
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IOpsTransactionService transactionService;

        public OpsTransactionController(IStringLocalizer<LanguageSub> localizer, ICurrentUser user, IOpsTransactionService service)
        {
            stringLocalizer = localizer;
            currentUser = user;
            transactionService = service;
        }

        [HttpPost("Query")]
        public IActionResult Query(OpsTransactionCriteria criteria)
        {
            var results = transactionService.Query(criteria);
            return Ok(results);
        }
        [HttpGet]
        public IActionResult Get(Guid id)
        {
            var result = transactionService.GetDetails(id); //transactionService.First(x => x.Id == id);
            return Ok(result);
        }
        [HttpPost("Paging")]
        public IActionResult Paging(OpsTransactionCriteria criteria, int page, int size)
        {
            var data = transactionService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        [HttpPost]
        [Route("Add")]
        public IActionResult Add(OpsTransactionModel model)
        {
            var existedMessage = CheckExist(model);
            if (existedMessage != null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedMessage });
            }
            
            var hs = transactionService.Add(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model.Id };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut]
        [Route("Update")]       
        public IActionResult Update(OpsTransactionModel model)
        {
            var existedMessage = CheckExist(model);
            if (existedMessage != null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = existedMessage });
            }

            model.ModifiedDate = DateTime.Now;
            model.UserModified = "admin"; //currentUser.UserID
            var hs = transactionService.Update(model,x=>x.Id==model.Id);
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
        public IActionResult Delete(Guid id)
        {
            var hs = transactionService.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        private string CheckExist(OpsTransactionModel model)
        {
            var existedHBL = transactionService.Any(x => x.Id != model.Id && x.Hblno == model.Hblno);
            var existedMBL = transactionService.Any(x => x.Id != model.Id && x.Mblno == model.Mblno);
            if (existedHBL)
            {
                return "HBL no is existed !";
            }
            if (existedMBL)
            {
                return "MBL no is existed !";
            }
            return null;
        }
    }
}
