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
        public CsTransactionController(IStringLocalizer<LanguageSub> localizer, ICsTransactionService service)
        {
            stringLocalizer = localizer;
            csTransactionService = service;
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

        [HttpPost]
        public IActionResult Post(CsTransactionEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var hs = csTransactionService.AddCSTransaction(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut]
        public IActionResult Put(CsTransactionEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var hs = csTransactionService.UpdateCSTransaction(model);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
