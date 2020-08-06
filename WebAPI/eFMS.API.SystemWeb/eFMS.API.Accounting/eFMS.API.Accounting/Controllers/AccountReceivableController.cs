using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Infrastructure.Middlewares;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.Accounting.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class AccountReceivableController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IAccAccountReceivableService accountReceivableService;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="accountReceivable"></param>
        public AccountReceivableController(IStringLocalizer<LanguageSub> localizer,
            IAccAccountReceivableService accountReceivable)
        {
            stringLocalizer = localizer;
            accountReceivableService = accountReceivable;
        }

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            return Ok(accountReceivableService.Get());
        }

        /// <summary>
        /// Calculator Receivable
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("CalculatorReceivable")]
        [Authorize]
        public IActionResult CalculatorReceivable(AccAccountReceivableModel model)
        {
            var receivable = accountReceivableService.Get(x => x.PartnerId == model.PartnerId && x.Office == model.Office && x.Service == model.Service).FirstOrDefault();
            HandleState hs;
            var message = string.Empty;
            if (receivable == null)
            {
                hs = accountReceivableService.AddReceivable(model);
                message = HandleError.GetMessage(hs, Crud.Insert);
            }
            else
            {
                hs = accountReceivableService.UpdateReceivable(receivable);
                message = HandleError.GetMessage(hs, Crud.Update);
            }
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            return Ok(result);
        }
        
    }
}