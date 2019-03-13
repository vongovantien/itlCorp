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
using Microsoft.AspNetCore.Authorization;
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
    public class CsTransactionController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICsTransactionService csTransactionService;
        private readonly ICurrentUser currentUser;
        public CsTransactionController(IStringLocalizer<LanguageSub> localizer, ICsTransactionService service, ICurrentUser user)
        {
            stringLocalizer = localizer;
            csTransactionService = service;
            currentUser = user;
        }

        [HttpGet("CountJobByDate/{{date}}")]
        public IActionResult CountJob(DateTime date)
        {
            var result = csTransactionService.Count(x => x.CreatedDate == date);
            return Ok(result);
        }
        [HttpPost]
        [Route("Query")]
        public IActionResult Query(CsTransactionCriteria criteria)
        {
            return Ok(csTransactionService.Query(criteria));
        }
        
        [HttpPost]
        [Route("Paging")]
        public IActionResult Paging(CsTransactionCriteria criteria, int page, int size)
        {
            var data = csTransactionService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var data = csTransactionService.GetById(id);
            return Ok(data);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Post(CsTransactionEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            string checkExistMessage = CheckExist(model.Id, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            model.UserCreated = currentUser.UserID;
            var result = csTransactionService.AddCSTransaction(model);
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        [Route("Import")]
        public IActionResult Import(CsTransactionEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            string checkExistMessage = CheckExist(model.Id, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            model.UserCreated = currentUser.UserID;
            var result = csTransactionService.ImportCSTransaction(model);
            return Ok(result);
        }

        [HttpPut]
        [Authorize]
        public IActionResult Put(CsTransactionEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            model.UserModified = currentUser.UserID;
            var hs = csTransactionService.UpdateCSTransaction(model);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        private string CheckExist(Guid id, CsTransactionEditModel model)
        {
            string message = string.Empty;
            if (id == Guid.Empty)
            {
                if (csTransactionService.Any(x => x.Mawb.ToLower() == model.Mawb.ToLower()))
                {
                    message = stringLocalizer[LanguageSub.MSG_MAWB_EXISTED].Value;
                }
            }
            else
            {
                if (csTransactionService.Any(x => (x.Mawb.ToLower() == model.Mawb.ToLower() && x.Id != id)))
                {
                    message = stringLocalizer[LanguageSub.MSG_MAWB_EXISTED].Value;
                }
            }
            return message;
        }
    }
}
