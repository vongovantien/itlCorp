using System;
using System.Collections.Generic;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models.Accounting;
using eFMS.API.Accounting.Infrastructure.Middlewares;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using ITL.NetCore.Common;
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
        public IActionResult GetListAdvanceSyncData(List<Guid> Ids)
        {
            List<BravoAdvanceModel> result = accountingService.GetListAdvanceToSyncBravo(Ids);
            return Ok(result);
        }

        [HttpPost("GetListVoucherSyncData")]
        public IActionResult GetListVoucherSyncData(List<Guid> Ids)
        {
            List<BravoVoucherModel> result = accountingService.GetListVoucherToSyncBravo(Ids);
            return Ok(result);
        }

        [HttpPost("GetListSettlementSyncData")]
        public IActionResult GetListSettlementSyncData(List<Guid> ids)
        {
            List<BravoSettlementModel> result = accountingService.GetListSettlementToSyncBravo(ids);
            return Ok(result);
        }

        [HttpPost("GetListCdNoteToSync")]
        public IActionResult GetListCdNoteToSync(List<Guid> ids, string type)
        {
            var result = accountingService.GetListCdNoteToSync(ids, type);
            return Ok(result);
        }

        [HttpPost("GetListSoaToSync")]
        public IActionResult GetListSoaToSync(List<int> Ids, string type)
        {
            var result = accountingService.GetListSoaToSync(Ids, type);
            return Ok(result);
        }

        [HttpPut("SyncAdvanceToAccountantSystem")]
        [Authorize]
        public IActionResult SyncAdvanceToAccountantSystem(List<Guid> Ids)
        {
            if (!ModelState.IsValid) return BadRequest();

            HandleState hs = accountingService.SyncListAdvanceToBravo(Ids, out Ids);
            string message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = Ids };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut("SyncSettlementToAccountantSystem")]
        [Authorize]
        public IActionResult SyncSettlementToAccountantSystem(List<Guid> Ids)
        {
            if (!ModelState.IsValid) return BadRequest();

            HandleState hs = accountingService.SyncListSettlementToBravo(Ids, out Ids);
            string message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = Ids };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut("SyncVoucherToAccountantSystem")]
        [Authorize]
        public IActionResult SyncVoucherToAccountantSystem(List<Guid> Ids)
        {
            if (!ModelState.IsValid) return BadRequest();

            HandleState hs = accountingService.SyncListVoucherToBravo(Ids, out Ids);
            string message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = Ids };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut("SyncListCdNoteToAccountant")]
        [Authorize]
        public IActionResult SyncListCdNoteToAccountant(List<Guid> ids)
        {
            if (!ModelState.IsValid) return BadRequest();

            HandleState hs = accountingService.SyncListCdNoteToAccountant(ids);
            string message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = ids };
            if (!hs.Success)
            {
                result = new ResultHandle { Status = hs.Success, Message = hs.Message.ToString(), Data = ids };
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut("SyncListSoaToAccountant")]
        [Authorize]
        public IActionResult SyncListSoaToAccountant(List<int> ids)
        {
            if (!ModelState.IsValid) return BadRequest();

            HandleState hs = accountingService.SyncListSoaToAccountant(ids);
            string message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = ids };
            if (!hs.Success)
            {
                result = new ResultHandle { Status = hs.Success, Message = hs.Message.ToString(), Data = ids };
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}