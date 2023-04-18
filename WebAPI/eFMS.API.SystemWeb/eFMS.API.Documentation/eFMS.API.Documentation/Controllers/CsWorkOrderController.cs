using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Common.Models;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;

namespace eFMS.API.Documentation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Authorize]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CsWorkOrderController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IWorkOrderService workOrderService;


        public CsWorkOrderController(
            IStringLocalizer<LanguageSub> stringLocalizer,
            IWorkOrderService worOrder,
            ICurrentUser currentUser)
        {
            this.stringLocalizer = stringLocalizer;
            this.currentUser = currentUser;
            workOrderService = worOrder;
        }

        [HttpPost]
        [Route("Query")]
        public async Task<IActionResult> Query(WorkOrderCriteria criteria)
        {
            IQueryable<CsWorkOrder> result = await workOrderService.QueryAsync(criteria);
            return Ok(result);
        }

        [HttpPost]
        [Route("Paging")]
        public async Task<IActionResult> Paging(WorkOrderCriteria criteria, int page, int size)
        {
            ResponsePagingModel<CsWorkOrderViewModel> data = await workOrderService.PagingAsync(criteria, page, size);
            return Ok(data);
        }

        [HttpGet("CheckAllowDetail/{id}")]
        [Authorize]
        public IActionResult CheckAllowDetail(Guid id)
        {
            var charge = workOrderService.First(x => x.Id == id);
            if (charge == null)
            {
                return Ok(false);
            }

            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialWorkOrder);
            PermissionRange permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Detail);

            return Ok(workOrderService.CheckAllowPermissionAction(id, permissionRange));
        }

        [HttpGet("CheckAllowDelete/{id}")]
        [Authorize]
        public IActionResult CheckAllowDelete(Guid id)
        {
            var charge = workOrderService.First(x => x.Id == id);
            if (charge == null)
            {
                return Ok(false);
            }
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialWorkOrder);
            PermissionRange permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);

            return Ok(workOrderService.CheckAllowPermissionAction(id, permissionRange));
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize]
        public IActionResult Get(Guid id)
        {
            CsWorkOrderViewUpdateModel result = workOrderService.GetById(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post(WorkOrderRequest model)
        {
            if (!ModelState.IsValid) return BadRequest();
            string checkExistMessage = CheckExist(model, out CsWorkOrder workOrderDuplicate);
            if (checkExistMessage.Length > 0)
            {
                return Ok(new ResultHandle { Status = false, Message = string.Format(@"Work Order information was duplicated with {0}", workOrderDuplicate.Code) });
            }

            var hs = await workOrderService.SaveWorkOrder(model);
            string message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Put(WorkOrderRequest model)
        {
            if (!ModelState.IsValid) return BadRequest();
            string checkExistMessage = CheckExist(model, out CsWorkOrder workOrderDuplicate);
            if (checkExistMessage.Length > 0)
            {
                return Ok(new ResultHandle { Status = false, Message = string.Format(@"Work Order information was duplicated with {0}", workOrderDuplicate.Code) });
            }
            var hs = await workOrderService.UpdateWorkOrder(model);
            string message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("SetActiveInactive")]
        public async Task<IActionResult> SetActiveInactive(ActiveInactiveRequest model)
        {
            var hs = await workOrderService.SetActiveInactive(model);
            string message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialWorkOrder);
            PermissionRange permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);

            if (!workOrderService.CheckAllowPermissionAction(id, permissionRange))
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            HandleState hs = await workOrderService.Delete(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete]
        [Route("Price/{id}")]
        public async Task<IActionResult> DeletePrice(Guid id)
        {
            HandleState hs = await workOrderService.DeletePrice(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        private string CheckExist(WorkOrderRequest model, out CsWorkOrder workOrderDuplicate)
        {
            var checkExist = workOrderService.CheckExist(model, out CsWorkOrder workOrder);
            workOrderDuplicate = workOrder;
            if (checkExist)
            {
                return stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
            }
            return string.Empty;
        }

    }
}