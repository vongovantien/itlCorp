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
using eFMS.IdentityServer.DL.UserManager;
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

        [HttpPost]
        public async Task<IActionResult> Post(WorkOrderRequest model)
        {
            var hs = await workOrderService.SaveWorkOrder(model);
            string message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = null };
            if (!hs.Success)
                return BadRequest(result);
            return Ok(result);
        }

    }
}