using System;
using System.Collections.Generic;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models.Accounting;
using eFMS.API.Accounting.DL.Services;
using eFMS.API.Accounting.Infrastructure.Middlewares;
using eFMS.API.Common.Globals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.Accounting.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class AccountingController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IAccountingService accountingService;

        public AccountingController(
            IStringLocalizer<LanguageSub> localizer,
            IAccountingService service
            )
        {
            stringLocalizer = localizer;
            accountingService = service;
        }

        [HttpPost("GetListAdvanceSyncData")]
        public IActionResult CheckAllowDelete(List<Guid> Ids)
        {
            List<BravoAdvanceModel> result = accountingService.GetListAdvanceToSyncBravo(Ids);
            return Ok(result);
        }

    }
}