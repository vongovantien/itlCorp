using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Setting.DL.Common;
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
        private readonly IUnlockRequestApproveService unlockRequestApproveService;
        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        /// <param name="currUser"></param>
        public UnlockRequestController(IStringLocalizer<LanguageSub> localizer, IUnlockRequestService service, ICurrentUser currUser, IUnlockRequestApproveService unlockRequestApprove)
        {
            stringLocalizer = localizer;
            unlockRequestService = service;
            currentUser = currUser;
            unlockRequestApproveService = unlockRequestApprove;
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

        [HttpPost]
        [Route("SaveAndSendRequest")]
        [Authorize]
        public IActionResult SaveAndSendRequest(SetUnlockRequestModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            
            HandleState hs;
            var message = string.Empty;
            if (model.Id == null || model.Id == Guid.Empty) //Insert Unlock Request
            {
                #region -- Check Exist Setting Flow --
                var isExistSettingFlow = unlockRequestApproveService.CheckExistSettingFlow(model.UnlockType, currentUser.OfficeID);
                if (!isExistSettingFlow.Success)
                {
                    ResultHandle _result = new ResultHandle { Status = false, Message = isExistSettingFlow.Exception.Message };
                    return BadRequest(_result);
                }
                #endregion -- Check Exist Setting Flow --

                #region -- Check Exist User Approval --
                var isExistUserApproval = unlockRequestApproveService.CheckExistUserApproval(model.UnlockType, currentUser.GroupId, currentUser.DepartmentId, currentUser.OfficeID, currentUser.CompanyID);
                if (!isExistUserApproval.Success)
                {
                    ResultHandle _result = new ResultHandle { Status = false, Message = isExistUserApproval.Exception.Message };
                    return BadRequest(_result);
                }
                #endregion -- Check Exist User Approval --

                model.StatusApproval = SettingConstants.STATUS_APPROVAL_REQUESTAPPROVAL;
                model.RequestDate = DateTime.Now;
                model.RequestUser = currentUser.UserID;
                hs = unlockRequestService.AddUnlockRequest(model);
                message = HandleError.GetMessage(hs, Crud.Insert);
            }
            else //Update Unlock Request
            {
                var unlockRequestCurrent = unlockRequestService.Get(x => x.Id == model.Id).FirstOrDefault();
                #region -- Check Exist Unlock Request --
                if (unlockRequestCurrent == null)
                {
                    ResultHandle _result = new ResultHandle { Status = false, Message = "Not found unlock request" };
                    return BadRequest(_result);
                }
                #endregion -- Check Exist Unlock Request --

                #region -- Check Exist Setting Flow --
                var isExistSettingFlow = unlockRequestApproveService.CheckExistSettingFlow(unlockRequestCurrent.UnlockType, unlockRequestCurrent.OfficeId);
                if (!isExistSettingFlow.Success)
                {
                    ResultHandle _result = new ResultHandle { Status = false, Message = isExistSettingFlow.Exception.Message };
                    return BadRequest(_result);
                }
                #endregion -- Check Exist Setting Flow --

                #region -- Check Exist User Approval --
                var isExistUserApproval = unlockRequestApproveService.CheckExistUserApproval(model.UnlockType, unlockRequestCurrent.GroupId, unlockRequestCurrent.DepartmentId, unlockRequestCurrent.OfficeId, unlockRequestCurrent.CompanyId);
                if (!isExistUserApproval.Success)
                {
                    ResultHandle _result = new ResultHandle { Status = false, Message = isExistUserApproval.Exception.Message };
                    return BadRequest(_result);
                }
                #endregion -- Check Exist User Approval --

                #region -- Check Unlock Request Approving --
                if (!model.StatusApproval.Equals(SettingConstants.STATUS_APPROVAL_NEW) && !model.StatusApproval.Equals(SettingConstants.STATUS_APPROVAL_DENIED))
                {
                    ResultHandle _result = new ResultHandle { Status = false, Message = "Only allowed to edit the unlock request status is New or Deny" };
                    return BadRequest(_result);
                }
                #endregion -- Check Unlock Request Approving --

                model.StatusApproval = SettingConstants.STATUS_APPROVAL_REQUESTAPPROVAL;
                model.RequestDate = DateTime.Now;
                model.RequestUser = currentUser.UserID;
                hs = unlockRequestService.UpdateUnlockRequest(model);
                message = HandleError.GetMessage(hs, Crud.Update);
            }
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (hs.Success)
            {
                SetUnlockRequestApproveModel approve = new SetUnlockRequestApproveModel
                {
                    UnlockRequestId = model.Id
                };
                var resultInsertUpdateApproval = unlockRequestApproveService.InsertOrUpdateApproval(approve);
                if (!resultInsertUpdateApproval.Success)
                {
                    ResultHandle _result = new ResultHandle { Status = false, Message = resultInsertUpdateApproval.Exception.Message };
                    return BadRequest(_result);
                }
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
    }
}