using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.DL.Models.Criteria;
using eFMS.API.Setting.Infrastructure.Middlewares;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.Setting.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class UnlockRequestController : ControllerBase
    {
        readonly ICurrentUser currentUser;
        private readonly IUnlockRequestService unlockRequestService;
        private readonly IStringLocalizer stringLocalizer;
        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        /// <param name="currUser"></param>
        public UnlockRequestController(IStringLocalizer<LanguageSub> localizer, IUnlockRequestService service, ICurrentUser currUser)
        {
            stringLocalizer = localizer;
            unlockRequestService = service;
            currentUser = currUser;
        }

        [HttpGet()]
        public IActionResult Get()
        {
            return Ok(unlockRequestService.Get());
        }

        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult AddUnlockRequest(SetUnlockRequestModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var hs = unlockRequestService.AddUnlockRequest(model);

            var message = HandleError.GetMessage(hs, Crud.Insert);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            return Ok(result);
        }

        [HttpDelete]
        [Route("Delete")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            var hs = unlockRequestService.DeleteUnlockRequest(id);
            
            ResultHandle result = new ResultHandle();
            if (!hs.Success)
            {
                result = new ResultHandle { Status = hs.Success, Message = hs.Message?.ToString() };
                return BadRequest(result);
            }
            else
            {
                var message = HandleError.GetMessage(hs, Crud.Delete);
                result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
                return Ok(result);
            }
        }

        [HttpPut]
        [Route("Update")]
        [Authorize]
        public IActionResult UpdateUnlockRequest(SetUnlockRequestModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var hs = unlockRequestService.UpdateUnlockRequest(model);

            var message = HandleError.GetMessage(hs, Crud.Update);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            return Ok(result);
        }

        [HttpPost("GetJobToUnlockRequest")]
        public IActionResult GetJobToUnlockRequest(UnlockJobCriteria criteria)
        {
            var data = unlockRequestService.GetJobToUnlockRequest(criteria);
            return Ok(data);
        }

        [HttpPost]
        [Route("Paging")]
        [Authorize]
        public IActionResult Paging(UnlockRequestCriteria criteria, int pageNumber, int pageSize)
        {
            var data = unlockRequestService.Paging(criteria, pageNumber, pageSize, out int totalItems);
            var result = new { data, totalItems, pageNumber, pageSize };
            return Ok(result);
        }

        [HttpGet("GetById")]
        public IActionResult GetDetailById(Guid id)
        {            
            var detail = unlockRequestService.GetDetailUnlockRequest(id);
            if (detail == null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND].Value });
            }
            return Ok(detail);
        }

        [HttpPost("CheckExistVoucherInvoiceOfSettlementAdvance")]
        public IActionResult CheckExistVoucherInvoiceOfSettlementAdvance(UnlockJobCriteria criteria)
        {
            if (criteria.UnlockTypeNum == UnlockTypeEnum.ADVANCE)
            {
                return Ok(unlockRequestService.CheckExistVoucherNoOfAdvance(criteria));
            }
            else if (criteria.UnlockTypeNum == UnlockTypeEnum.SETTLEMENT)
            {
                return Ok(unlockRequestService.CheckExistInvoiceNoOfSettlement(criteria));
            }
            return Ok(new HandleState());
        }
        
    }
}