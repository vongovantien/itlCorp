using System;
using System.Threading.Tasks;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.Infrastructure.Middlewares;
using eFMS.API.Common;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Mvc;

namespace eFMS.API.Accounting.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class AcctDebitManagementArController : ControllerBase
    {
        private readonly IAcctDebitManagementARService acctDebitArService;

        public AcctDebitManagementArController(IAcctDebitManagementARService acctDebitArService)
        {
            this.acctDebitArService = acctDebitArService;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Guid Id)
        {
            HandleState hs = await acctDebitArService.AddAndUpdate(Id);

            return Ok(hs);
        }

        [HttpDelete]
        public IActionResult Delete(Guid Id)
        {
            acctDebitArService.DeleteDebit(Id);

            return Ok(new ResultHandle());
        }
    }
}