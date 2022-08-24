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
using eFMS.API.Common.Models;
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
    public class AccountingPrePaidPaymentController : ControllerBase
    {
        private readonly IAccountingPrePaidPaymentService prepaidService;
        private readonly IStringLocalizer stringLocalizer;

        public AccountingPrePaidPaymentController(IAccountingPrePaidPaymentService prepaidService,
            IStringLocalizer<LanguageSub> localizer)
        {
            this.prepaidService = prepaidService;
            this.stringLocalizer = localizer;
        }

        [HttpPost]
        [Route("Paging")]
        [Authorize]
        public IActionResult Paging(AccountingPrePaidPaymentCriteria criteria, int page, int size)
        {
            IQueryable<AccPrePaidPaymentResult> data = prepaidService.Paging(criteria, page, size, out int rowsCount);
            var result = new ResponsePagingModel<AccPrePaidPaymentResult> { Data = data, Page = page, Size = size, TotalItems = rowsCount };
            return Ok(result);
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Put([FromBody] List<AccountingPrePaidPaymentUpdateModel> model)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest();
            }

            if(model.First().Status == "Unpaid")
            {
                var isValid = prepaidService.ValidateRevertPayment(model.First().Id);
                if(!isValid)
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "SOA/DEBIT was synced, you cannot revert this payment" });
                }
            }

            HandleState hs = await prepaidService.UpdatePrePaidPayment(model);

            if (!hs.Success)
            {
               ResultHandle _result = new ResultHandle { Status = hs.Success, Message = hs.Message.ToString() };
               return BadRequest(_result);
            }

            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            return Ok(result);
        }
    }
}