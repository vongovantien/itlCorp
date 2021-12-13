using System;
using System.Collections.Generic;
using System.Linq;
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
    public class AccountPayableController : ControllerBase
    {
        private readonly IAccountPayableService accPayableService;

        public AccountPayableController(IAccountPayableService accPayable)
        {
            accPayableService = accPayable;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            HandleState hs = await accPayableService.InsertAccountPayable();
            return Ok(new ResultHandle());
        }
    }

    
}