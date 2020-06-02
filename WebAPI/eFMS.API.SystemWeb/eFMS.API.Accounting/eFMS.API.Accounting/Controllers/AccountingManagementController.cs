using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.Infrastructure.Middlewares;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Accounting.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class AccountingManagementController: ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IAccountingManagementService accountingService;
        public AccountingManagementController(IStringLocalizer<LanguageSub> localizer,
            IAccountingManagementService accService)
        {
            stringLocalizer = localizer;
            accountingService = accService;
        }

        [Authorize]
        [HttpGet("CheckAllowDelete")]
        public IActionResult CheckAllowDelete(Guid id)
        {
            var result = accountingService.CheckDeletePermission(id);
            if (result == 403)
            {
                return Ok(false);
            }
            return Ok(true);
        }

        [HttpDelete]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            HandleState hs = accountingService.Delete(id);
            if (hs.Code == 403)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
