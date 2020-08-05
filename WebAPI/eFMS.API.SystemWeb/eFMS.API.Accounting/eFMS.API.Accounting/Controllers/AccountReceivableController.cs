using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Infrastructure.Middlewares;
using eFMS.API.Common.Globals;
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

        [HttpPost("CalculatorReceivable")]
        public IActionResult CalculatorReceivable(AccAccountReceivableModel model)
        {
            return Ok();
        }
        
    }
}